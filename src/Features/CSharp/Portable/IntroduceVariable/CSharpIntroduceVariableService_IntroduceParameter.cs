// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable disable

using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.AddParameter;
using Microsoft.CodeAnalysis.CSharp.Extensions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.LanguageServices;
using Microsoft.CodeAnalysis.Shared.Extensions;

namespace Microsoft.CodeAnalysis.CSharp.IntroduceVariable
{
    internal partial class CSharpIntroduceVariableService
    {
        protected override async Task<Document> IntroduceParameterAsync(
            SemanticDocument document,
            ExpressionSyntax expression,
            bool allOccurrences,
            CancellationToken cancellationToken)
        {
            var invocationDocument = document.Document;

            var test = expression.FirstAncestorOrSelf<MethodDeclarationSyntax>(node => node is MethodDeclarationSyntax, true);

            var semanticModel = document.SemanticModel;
            var symbolInfo = semanticModel.GetDeclaredSymbol(test, cancellationToken);
            var containingMethod = (IMethodSymbol)symbolInfo;
            //var containingMethod = (IMethodSymbol)semanticModel.GetSymbolInfo(test, cancellationToken).Symbol;
            var syntaxFacts = invocationDocument.GetLanguageService<ISyntaxFactsService>();
            var parameterType = document.SemanticModel.GetTypeInfo(expression, cancellationToken).Type ?? document.SemanticModel.Compilation.ObjectType;
            var refKind = syntaxFacts.GetRefKindOfArgument(expression);

            var semanticFacts = invocationDocument.GetLanguageService<ISemanticFactsService>();
            var parameterName = semanticFacts.GenerateNameForExpression(
                    document.SemanticModel, expression, capitalize: false, cancellationToken: cancellationToken);

            var solution = await AddParameterService.Instance.AddParameterAsync(
                invocationDocument,
                containingMethod,
                parameterType,
                refKind,
                parameterName,
                null,
                allOccurrences,
                cancellationToken).ConfigureAwait(false);

            return solution.GetDocument(invocationDocument.Id);
        }
    }
}
