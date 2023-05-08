// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.LanguageService;
using Microsoft.CodeAnalysis.Shared.Extensions;

using Microsoft.CodeAnalysis.Shared.Extensions.ContextQuery;
using Microsoft.CodeAnalysis.Snippets;
using Microsoft.CodeAnalysis.Snippets.SnippetProviders;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.CSharp.Snippets
{
    [ExportSnippetProvider(nameof(ISnippetProvider), LanguageNames.CSharp), Shared]
    internal sealed class CSharpForLoopSnippetProvider : AbstractForLoopSnippetProvider
    {
        [ImportingConstructor]
        [Obsolete(MefConstruction.ImportingConstructorMessage, error: true)]
        public CSharpForLoopSnippetProvider()
        {
        }

        protected override async Task<SyntaxNode> GenerateStatement(SyntaxGenerator generator, SyntaxContext syntaxContext, SyntaxNode? inlineExpression)
        {
            var document = syntaxContext.Document;
            var compilation = await document.Project.GetRequiredCompilationAsync(cancellationToken).ConfigureAwait(false);
            var generator = SyntaxGenerator.GetGenerator(document);
            var semanticModel = await document.GetRequiredSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            var iteratorName = NameGenerator.GenerateUniqueName(
                new List<string> { "i", "j", "k", "a", "b", "c" },
                n => semanticModel.LookupSymbols(position, name: n).IsEmpty);
            var indexVariable = generator.Identifier(iteratorName);

            // Creating the variable declaration based on if the user has
            // 'var for built in types' set in their editorconfig.
            var variableDeclarationSyntax =
                 SyntaxFactory.VariableDeclaration(compilation.GetSpecialType(SpecialType.System_Int32).GenerateTypeSyntax(allowVar: true),
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.VariableDeclarator(
                            indexVariable,
                            argumentList: null,
                            SyntaxFactory.EqualsValueClause((ExpressionSyntax)generator.LiteralExpression(0)))));

            var forLoopSyntax = SyntaxFactory.ForStatement(
                variableDeclarationSyntax,
                SyntaxFactory.SeparatedList<ExpressionSyntax>(),
                (ExpressionSyntax)generator.LessThanExpression(
                    generator.IdentifierName(indexVariable),
                    // Using a temporary identifier name for now, could later be changed
                    // to look for an iterable item in the scope of the insertion.
                    generator.IdentifierName("length")),
                SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                    SyntaxFactory.PostfixUnaryExpression(
                        SyntaxKind.PostIncrementExpression, SyntaxFactory.IdentifierName(indexVariable))),
                SyntaxFactory.Block());

            return forLoopSyntax;
        }

        protected override ImmutableArray<SnippetPlaceholder> GetPlaceHolderLocationsList(SyntaxNode node, ISyntaxFacts syntaxFacts, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        protected override int GetTargetCaretPosition(ISyntaxFactsService syntaxFacts, SyntaxNode caretTarget, SourceText sourceText)
        {
            throw new NotImplementedException();
        }
    }
}
