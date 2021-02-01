// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable disable

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Internal.Log;
using Microsoft.CodeAnalysis.LanguageServices;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;
using static Microsoft.CodeAnalysis.CodeActions.CodeAction;

namespace Microsoft.CodeAnalysis.IntroduceVariable
{
    internal abstract partial class AbstractIntroduceVariableService<TService, TExpressionSyntax, TTypeSyntax, TTypeDeclarationSyntax, TQueryExpressionSyntax, TNameSyntax> : IIntroduceVariableService
        where TService : AbstractIntroduceVariableService<TService, TExpressionSyntax, TTypeSyntax, TTypeDeclarationSyntax, TQueryExpressionSyntax, TNameSyntax>
        where TExpressionSyntax : SyntaxNode
        where TTypeSyntax : TExpressionSyntax
        where TTypeDeclarationSyntax : SyntaxNode
        where TQueryExpressionSyntax : TExpressionSyntax
        where TNameSyntax : TTypeSyntax
    {
        protected abstract bool IsInNonFirstQueryClause(TExpressionSyntax expression);
        protected abstract bool IsInFieldInitializer(TExpressionSyntax expression);
        protected abstract bool IsInParameterInitializer(TExpressionSyntax expression);
        protected abstract bool IsInConstructorInitializer(TExpressionSyntax expression);
        protected abstract bool IsInAttributeArgumentInitializer(TExpressionSyntax expression);
        protected abstract bool IsInAutoPropertyInitializer(TExpressionSyntax expression);
        protected abstract bool IsInExpressionBodiedMember(TExpressionSyntax expression);

        protected abstract IEnumerable<SyntaxNode> GetContainingExecutableBlocks(TExpressionSyntax expression);
        protected abstract IList<bool> GetInsertionIndices(TTypeDeclarationSyntax destination, CancellationToken cancellationToken);

        protected abstract bool CanIntroduceVariableFor(TExpressionSyntax expression);
        protected abstract bool CanReplace(TExpressionSyntax expression);

        protected abstract bool IsExpressionInStaticLocalFunction(TExpressionSyntax expression);

        protected abstract Task<Document> IntroduceQueryLocalAsync(SemanticDocument document, TExpressionSyntax expression, bool allOccurrences, CancellationToken cancellationToken);
        protected abstract Task<Document> IntroduceLocalAsync(SemanticDocument document, TExpressionSyntax expression, bool allOccurrences, bool isConstant, CancellationToken cancellationToken);
        protected abstract Task<Document> IntroduceFieldAsync(SemanticDocument document, TExpressionSyntax expression, bool allOccurrences, bool isConstant, CancellationToken cancellationToken);
        protected abstract Task<Document> IntroduceParameterAsync(SemanticDocument document, TExpressionSyntax expression, bool allOccurrences, CancellationToken cancellationToken);

        protected abstract int DetermineFieldInsertPosition(TTypeDeclarationSyntax oldDeclaration, TTypeDeclarationSyntax newDeclaration);
        protected abstract int DetermineConstantInsertPosition(TTypeDeclarationSyntax oldDeclaration, TTypeDeclarationSyntax newDeclaration);

        protected virtual bool BlockOverlapsHiddenPosition(SyntaxNode block, CancellationToken cancellationToken)
            => block.OverlapsHiddenPosition(cancellationToken);

        public async Task<ImmutableArray<CodeAction>> IntroduceVariableAsync(
            Document document,
            TextSpan textSpan,
            CancellationToken cancellationToken)
        {
            using (Logger.LogBlock(FunctionId.Refactoring_IntroduceVariable, cancellationToken))
            {
                var semanticDocument = await SemanticDocument.CreateAsync(document, cancellationToken).ConfigureAwait(false);

                var state = await State.GenerateAsync((TService)this, semanticDocument, textSpan, cancellationToken).ConfigureAwait(false);
                var codeActionWithNestedActionsList = new ArrayBuilder<CodeAction>();
                if (state != null)
                {
                    var titlesAndActions = AddActionsAndGetTitle(state, cancellationToken);
                    foreach (var titleAndAction in titlesAndActions)
                    {
                        if (titleAndAction.Value.Length > 0)
                        {
                            codeActionWithNestedActionsList.Add(new CodeActionWithNestedActions(titleAndAction.Key, titleAndAction.Value, isInlinable: true));
                        }
                    }
                    return codeActionWithNestedActionsList.ToImmutable();
                }
                return codeActionWithNestedActionsList.ToImmutable();
            }
        }

        private static void AddCodeActions(string key, Dictionary<string, ImmutableArray<CodeAction>> titleAndActionsDictionary, params CodeAction[] codeActions)
        {
            var arrayBuilder = new ArrayBuilder<CodeAction>();
            arrayBuilder.AddRange(codeActions);
            titleAndActionsDictionary[key] = arrayBuilder.ToImmutable();
        }

        private Dictionary<string, ImmutableArray<CodeAction>> AddActionsAndGetTitle(State state, CancellationToken cancellationToken)
        {
            var titleAndActionsDictionary = new Dictionary<string, ImmutableArray<CodeAction>>();
            if (state.InQueryContext)
            {
                AddCodeActions(FeaturesResources.Introduce_query_variable, titleAndActionsDictionary, CreateAction(state, allOccurrences: false, isConstant: false, isLocal: false, isQueryLocal: true, isParameter: false),
                    CreateAction(state, allOccurrences: true, isConstant: false, isLocal: false, isQueryLocal: true, isParameter: false));
            }
            else if (state.InParameterContext)
            {
                AddCodeActions(FeaturesResources.Introduce_constant, titleAndActionsDictionary, CreateAction(state, allOccurrences: false, isConstant: true, isLocal: false, isQueryLocal: false, isParameter: false),
                    CreateAction(state, allOccurrences: true, isConstant: true, isLocal: false, isQueryLocal: false, isParameter: false));
            }
            else if (state.InFieldContext)
            {
                var title = GetConstantOrFieldResource(state.IsConstant);

                AddCodeActions(title, titleAndActionsDictionary, CreateAction(state, allOccurrences: false, isConstant: state.IsConstant, isLocal: false, isQueryLocal: false, isParameter: false),
                    CreateAction(state, allOccurrences: true, isConstant: state.IsConstant, isLocal: false, isQueryLocal: false, isParameter: false));
            }
            else if (state.InConstructorInitializerContext)
            {
                var title = GetConstantOrFieldResource(state.IsConstant);

                AddCodeActions(title, titleAndActionsDictionary, CreateAction(state, allOccurrences: false, isConstant: state.IsConstant, isLocal: false, isQueryLocal: false, isParameter: false),
                    CreateAction(state, allOccurrences: true, isConstant: state.IsConstant, isLocal: false, isQueryLocal: false, isParameter: false));
            }
            else if (state.InAutoPropertyInitializerContext)
            {
                var title = GetConstantOrFieldResource(state.IsConstant);

                AddCodeActions(title, titleAndActionsDictionary, CreateAction(state, allOccurrences: false, isConstant: state.IsConstant, isLocal: false, isQueryLocal: false, isParameter: false),
                    CreateAction(state, allOccurrences: true, isConstant: state.IsConstant, isLocal: false, isQueryLocal: false, isParameter: false));
            }
            else if (state.InAttributeContext)
            {
                AddCodeActions(FeaturesResources.Introduce_constant, titleAndActionsDictionary, CreateAction(state, allOccurrences: false, isConstant: true, isLocal: false, isQueryLocal: false, isParameter: false),
                    CreateAction(state, allOccurrences: true, isConstant: true, isLocal: false, isQueryLocal: false, isParameter: false));
            }
            else if (state.InBlockContext)
            {
                CreateConstantFieldActions(state, titleAndActionsDictionary, cancellationToken);

                var blocks = GetContainingExecutableBlocks(state.Expression);
                var block = blocks.FirstOrDefault();

                if (!BlockOverlapsHiddenPosition(block, cancellationToken))
                {
                    var localAction = CreateAction(state, allOccurrences: false, isConstant: state.IsConstant, isLocal: true, isQueryLocal: false, isParameter: false);
                    var paramaterAction = CreateAction(state, allOccurrences: false, isConstant: false, isLocal: false, isQueryLocal: false, isParameter: true);
                    var title = GetConstantOrLocalResource(state.IsConstant);

                    if (blocks.All(b => !BlockOverlapsHiddenPosition(b, cancellationToken)))
                    {
                        var localActionAllOccurrences = CreateAction(state, allOccurrences: true, isConstant: state.IsConstant, isLocal: true, isQueryLocal: false, isParameter: false);
                        var parameterActionAllOccurrences = CreateAction(state, allOccurrences: true, isConstant: false, isLocal: false, isQueryLocal: false, isParameter: true);

                        AddCodeActions(title, titleAndActionsDictionary, localAction, localActionAllOccurrences);
                        AddCodeActions(FeaturesResources.Introduce_parameter, titleAndActionsDictionary, paramaterAction, parameterActionAllOccurrences);
                    }
                    else
                    {
                        AddCodeActions(title, titleAndActionsDictionary, localAction);
                        AddCodeActions(FeaturesResources.Introduce_parameter, titleAndActionsDictionary, paramaterAction);
                    }
                }
            }
            else if (state.InExpressionBodiedMemberContext)
            {
                CreateConstantFieldActions(state, titleAndActionsDictionary, cancellationToken);

                var title = GetConstantOrLocalResource(state.IsConstant);

                AddCodeActions(title, titleAndActionsDictionary, CreateAction(state, allOccurrences: false, isConstant: state.IsConstant, isLocal: true, isQueryLocal: false, isParameter: false),
                    CreateAction(state, allOccurrences: true, isConstant: state.IsConstant, isLocal: true, isQueryLocal: false, isParameter: false));

                AddCodeActions(FeaturesResources.Introduce_parameter, titleAndActionsDictionary, CreateAction(state, allOccurrences: false, isConstant: state.IsConstant, isLocal: false, isQueryLocal: false, isParameter: true),
                    CreateAction(state, allOccurrences: true, isConstant: state.IsConstant, isLocal: false, isQueryLocal: false, isParameter: true));
            }
            else
            {
                return null;
            }
            return titleAndActionsDictionary;
        }

        private static string GetConstantOrFieldResource(bool isConstant)
            => isConstant ? FeaturesResources.Introduce_constant : FeaturesResources.Introduce_field;

        private static string GetConstantOrLocalResource(bool isConstant)
            => isConstant ? FeaturesResources.Introduce_constant : FeaturesResources.Introduce_local;

        private void CreateConstantFieldActions(State state, Dictionary<string, ImmutableArray<CodeAction>> titleAndActionsDictionary, CancellationToken cancellationToken)
        {
            if (state.IsConstant &&
                !state.GetSemanticMap(cancellationToken).AllReferencedSymbols.OfType<ILocalSymbol>().Any() &&
                !state.GetSemanticMap(cancellationToken).AllReferencedSymbols.OfType<IParameterSymbol>().Any())
            {
                // If something is a constant, and it doesn't access any other locals constants,
                // then we prefer to offer to generate a constant field instead of a constant
                // local.
                if (CanGenerateIntoContainer(state, cancellationToken))
                {
                    AddCodeActions(FeaturesResources.Introduce_constant, titleAndActionsDictionary, CreateAction(state, allOccurrences: false, isConstant: true, isLocal: false, isQueryLocal: false, isParameter: false),
                        CreateAction(state, allOccurrences: true, isConstant: true, isLocal: false, isQueryLocal: false, isParameter: false));
                }
            }
        }

        protected int GetFieldInsertionIndex(
            bool isConstant, TTypeDeclarationSyntax oldType, TTypeDeclarationSyntax newType, CancellationToken cancellationToken)
        {
            var preferredInsertionIndex = isConstant
                ? DetermineConstantInsertPosition(oldType, newType)
                : DetermineFieldInsertPosition(oldType, newType);

            var legalInsertionIndices = GetInsertionIndices(oldType, cancellationToken);
            if (legalInsertionIndices[preferredInsertionIndex])
            {
                return preferredInsertionIndex;
            }

            // location we wanted to insert into isn't legal (i.e. it's hidden).  Try to find a
            // non-hidden location.
            var legalIndex = legalInsertionIndices.IndexOf(true);
            if (legalIndex >= 0)
            {
                return legalIndex;
            }

            // Couldn't find a viable non-hidden position.  Fall back to the computed position we
            // wanted originally.
            return preferredInsertionIndex;
        }

        private bool CanGenerateIntoContainer(State state, CancellationToken cancellationToken)
        {
            var destination = state.Expression.GetAncestor<TTypeDeclarationSyntax>() ?? state.Document.Root;
            if (!destination.OverlapsHiddenPosition(cancellationToken))
            {
                return true;
            }

            if (destination is TTypeDeclarationSyntax typeDecl)
            {
                var insertionIndices = GetInsertionIndices(typeDecl, cancellationToken);
                // We can generate into a containing type as long as there is at least one non-hidden location in it.
                if (insertionIndices.Contains(true))
                {
                    return true;
                }
            }

            return false;
        }

        private CodeAction CreateAction(State state, bool allOccurrences, bool isConstant, bool isLocal, bool isQueryLocal, bool isParameter)
        {
            if (allOccurrences)
            {
                return new IntroduceVariableAllOccurrenceCodeAction((TService)this, state.Document, state.Expression, allOccurrences, isConstant, isLocal, isQueryLocal, isParameter);
            }

            return new IntroduceVariableCodeAction((TService)this, state.Document, state.Expression, allOccurrences, isConstant, isLocal, isQueryLocal, isParameter);
        }

        protected static SyntaxToken GenerateUniqueFieldName(
            SemanticDocument semanticDocument,
            TExpressionSyntax expression,
            bool isConstant,
            CancellationToken cancellationToken)
        {
            var semanticFacts = semanticDocument.Document.GetLanguageService<ISemanticFactsService>();

            var semanticModel = semanticDocument.SemanticModel;
            var baseName = semanticFacts.GenerateNameForExpression(
                semanticModel, expression, isConstant, cancellationToken);

            // A field can't conflict with any existing member names.
            var declaringType = semanticModel.GetEnclosingNamedType(expression.SpanStart, cancellationToken);
            var reservedNames = declaringType.GetMembers().Select(m => m.Name);

            return semanticFacts.GenerateUniqueName(baseName, reservedNames);
        }

        protected static SyntaxToken GenerateUniqueLocalName(
            SemanticDocument semanticDocument,
            TExpressionSyntax expression,
            bool isConstant,
            SyntaxNode containerOpt,
            CancellationToken cancellationToken)
        {
            var semanticModel = semanticDocument.SemanticModel;

            var semanticFacts = semanticDocument.Document.GetLanguageService<ISemanticFactsService>();
            var baseName = semanticFacts.GenerateNameForExpression(
                semanticModel, expression, capitalize: isConstant, cancellationToken: cancellationToken);

            return semanticFacts.GenerateUniqueLocalName(
                semanticModel, expression, containerOpt, baseName, cancellationToken);
        }

        protected ISet<TExpressionSyntax> FindMatches(
            SemanticDocument originalDocument,
            TExpressionSyntax expressionInOriginal,
            SemanticDocument currentDocument,
            SyntaxNode withinNodeInCurrent,
            bool allOccurrences,
            CancellationToken cancellationToken)
        {
            var syntaxFacts = currentDocument.Project.LanguageServices.GetService<ISyntaxFactsService>();
            var originalSemanticModel = originalDocument.SemanticModel;
            var currentSemanticModel = currentDocument.SemanticModel;

            var result = new HashSet<TExpressionSyntax>();
            var matches = from nodeInCurrent in withinNodeInCurrent.DescendantNodesAndSelf().OfType<TExpressionSyntax>()
                          where NodeMatchesExpression(originalSemanticModel, currentSemanticModel, expressionInOriginal, nodeInCurrent, allOccurrences, cancellationToken)
                          select nodeInCurrent;
            result.AddRange(matches.OfType<TExpressionSyntax>());

            return result;
        }

        private bool NodeMatchesExpression(
            SemanticModel originalSemanticModel,
            SemanticModel currentSemanticModel,
            TExpressionSyntax expressionInOriginal,
            TExpressionSyntax nodeInCurrent,
            bool allOccurrences,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (nodeInCurrent == expressionInOriginal)
            {
                return true;
            }

            if (allOccurrences && CanReplace(nodeInCurrent))
            {
                // Original expression and current node being semantically equivalent isn't enough when the original expression 
                // is a member access via instance reference (either implicit or explicit), the check only ensures that the expression
                // and current node are both backed by the same member symbol. So in this case, in addition to SemanticEquivalence check, 
                // we also check if expression and current node are both instance member access.
                //
                // For example, even though the first `c` binds to a field and we are introducing a local for it,
                // we don't want other references to that field to be replaced as well (i.e. the second `c` in the expression).
                //
                //  class C
                //  {
                //      C c;
                //      void Test()
                //      {
                //          var x = [|c|].c;
                //      }
                //  }

                if (SemanticEquivalence.AreEquivalent(
                    originalSemanticModel, currentSemanticModel, expressionInOriginal, nodeInCurrent))
                {
                    var originalOperation = originalSemanticModel.GetOperation(expressionInOriginal, cancellationToken);
                    if (IsInstanceMemberReference(originalOperation))
                    {
                        var currentOperation = currentSemanticModel.GetOperation(nodeInCurrent, cancellationToken);
                        return IsInstanceMemberReference(currentOperation);
                    }

                    // If the original expression is within a static local function, further checks are unnecessary since our scope has already been narrowed down to within the local function.
                    // If the original expression is not within a static local function, we need to further check whether the expression we're comparing against is within a static local
                    // function. If so, the expression is not a valid match since we cannot refer to instance variables from within static local functions.
                    if (!IsExpressionInStaticLocalFunction(expressionInOriginal))
                    {
                        return !IsExpressionInStaticLocalFunction(nodeInCurrent);
                    }

                    return true;
                }
            }

            return false;
            static bool IsInstanceMemberReference(IOperation operation)
                => operation is IMemberReferenceOperation memberReferenceOperation &&
                    memberReferenceOperation.Instance?.Kind == OperationKind.InstanceReference;
        }

        protected TNode Rewrite<TNode>(
            SemanticDocument originalDocument,
            TExpressionSyntax expressionInOriginal,
            TExpressionSyntax variableName,
            SemanticDocument currentDocument,
            TNode withinNodeInCurrent,
            bool allOccurrences,
            CancellationToken cancellationToken)
            where TNode : SyntaxNode
        {
            var generator = SyntaxGenerator.GetGenerator(originalDocument.Document);
            var matches = FindMatches(originalDocument, expressionInOriginal, currentDocument, withinNodeInCurrent, allOccurrences, cancellationToken);

            // Parenthesize the variable, and go and replace anything we find with it.
            // NOTE: we do not want elastic trivia as we want to just replace the existing code 
            // as is, while preserving the trivia there.  We do not want to update it.
            var replacement = generator.AddParentheses(variableName, includeElasticTrivia: false)
                                         .WithAdditionalAnnotations(Formatter.Annotation);

            return RewriteCore(withinNodeInCurrent, replacement, matches);
        }

        protected abstract TNode RewriteCore<TNode>(
            TNode node,
            SyntaxNode replacementNode,
            ISet<TExpressionSyntax> matches)
            where TNode : SyntaxNode;

        protected static ITypeSymbol GetTypeSymbol(
            SemanticDocument document,
            TExpressionSyntax expression,
            CancellationToken cancellationToken,
            bool objectAsDefault = true)
        {
            var semanticModel = document.SemanticModel;
            var typeInfo = semanticModel.GetTypeInfo(expression, cancellationToken);

            if (typeInfo.Type?.SpecialType == SpecialType.System_String &&
                typeInfo.ConvertedType?.IsFormattableStringOrIFormattable() == true)
            {
                return typeInfo.ConvertedType;
            }

            if (typeInfo.Type != null)
            {
                return typeInfo.Type;
            }

            if (typeInfo.ConvertedType != null)
            {
                return typeInfo.ConvertedType;
            }

            if (objectAsDefault)
            {
                return semanticModel.Compilation.GetSpecialType(SpecialType.System_Object);
            }

            return null;
        }

        protected static IEnumerable<IParameterSymbol> GetAnonymousMethodParameters(
            SemanticDocument document, TExpressionSyntax expression, CancellationToken cancellationToken)
        {
            var semanticModel = document.SemanticModel;
            var semanticMap = semanticModel.GetSemanticMap(expression, cancellationToken);

            var anonymousMethodParameters = semanticMap.AllReferencedSymbols
                                                       .OfType<IParameterSymbol>()
                                                       .Where(p => p.ContainingSymbol.IsAnonymousFunction());
            return anonymousMethodParameters;
        }

        protected static async Task<(SemanticDocument newSemanticDocument, ISet<TExpressionSyntax> newMatches)> ComplexifyParentingStatementsAsync(
            SemanticDocument semanticDocument,
            ISet<TExpressionSyntax> matches,
            CancellationToken cancellationToken)
        {
            // First, track the matches so that we can get back to them later.
            var newRoot = semanticDocument.Root.TrackNodes(matches);
            var newDocument = semanticDocument.Document.WithSyntaxRoot(newRoot);
            var newSemanticDocument = await SemanticDocument.CreateAsync(newDocument, cancellationToken).ConfigureAwait(false);
            var newMatches = newSemanticDocument.Root.GetCurrentNodes(matches.AsEnumerable()).ToSet();

            // Next, expand the topmost parenting expression of each match, being careful
            // not to expand the matches themselves.
            var topMostExpressions = newMatches
                .Select(m => m.AncestorsAndSelf().OfType<TExpressionSyntax>().Last())
                .Distinct();

            newRoot = await newSemanticDocument.Root
                .ReplaceNodesAsync(
                    topMostExpressions,
                    computeReplacementAsync: async (oldNode, newNode, ct) =>
                    {
                        return await Simplifier
                            .ExpandAsync(
                                oldNode,
                                newSemanticDocument.Document,
                                expandInsideNode: node =>
                                {
                                    return !(node is TExpressionSyntax expression)
                                        || !newMatches.Contains(expression);
                                },
                                cancellationToken: ct)
                            .ConfigureAwait(false);
                    },
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            newDocument = newSemanticDocument.Document.WithSyntaxRoot(newRoot);
            newSemanticDocument = await SemanticDocument.CreateAsync(newDocument, cancellationToken).ConfigureAwait(false);
            newMatches = newSemanticDocument.Root.GetCurrentNodes(matches.AsEnumerable()).ToSet();

            return (newSemanticDocument, newMatches);
        }
    }
}
