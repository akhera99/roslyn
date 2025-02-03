// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Extensions.ContextQuery;
using Microsoft.CodeAnalysis.CSharp.Structure;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.Utilities;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Microsoft.CodeAnalysis.Shared.Extensions.ContextQuery;
using Microsoft.CodeAnalysis.Snippets;
using Microsoft.CodeAnalysis.Snippets.SnippetProviders;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Snippets;

[ExportSnippetProvider(nameof(ISnippetProvider), LanguageNames.CSharp), Shared]
[method: ImportingConstructor]
[method: Obsolete(MefConstruction.ImportingConstructorMessage, error: true)]
internal sealed class CSharpRegionSnippetProvider() : AbstractRegionSnippetProvider<RegionDirectiveTriviaSyntax>
{
    private static readonly ISet<SyntaxKind> s_validModifiers = new HashSet<SyntaxKind>(SyntaxFacts.EqualityComparer)
    {
    };

    protected override bool IsValidSnippetLocationCore(SnippetContext context, CancellationToken cancellationToken)
    {
        var syntaxContext = (CSharpSyntaxContext)context.SyntaxContext;

        return
            syntaxContext.IsMemberDeclarationContext(
                validModifiers: s_validModifiers,
                validTypeDeclarations: SyntaxKindSet.ClassStructRecordTypeDeclarations,
                cancellationToken: cancellationToken);
    }

    protected override async Task<TextChange> GenerateSnippetTextChangeAsync(Document document, int position, CancellationToken cancellationToken)
    {
        var semanticModel = await document.ReuseExistingSpeculativeModelAsync(position, cancellationToken).ConfigureAwait(false);
        var syntaxContext = (CSharpSyntaxContext)document.GetRequiredLanguageService<ISyntaxContextService>().CreateContext(document, semanticModel, position, cancellationToken);

        var containingType = syntaxContext.ContainingTypeDeclaration;
        Contract.ThrowIfNull(containingType);

        var containingTypeSymbol = semanticModel.GetDeclaredSymbol(containingType, cancellationToken);
        Contract.ThrowIfNull(containingTypeSymbol);

        var generator = SyntaxGenerator.GetGenerator(document);
        var regionDirectiveTriviaSyntax = SyntaxFactory.RegionDirectiveTrivia(true);

        return new TextChange(TextSpan.FromBounds(position, position), regionDirectiveTriviaSyntax.NormalizeWhitespace().ToFullString());
    }

    protected override int GetTargetCaretPosition(RegionDirectiveTriviaSyntax regionDirectiveTriviaSyntax, SourceText sourceText)
        => CSharpSnippetHelpers.GetTargetCaretPositionInBlock(
            regionDirectiveTriviaSyntax,
            static d => d.!,
            sourceText);

    protected override Task<Document> AddIndentationToDocumentAsync(Document document, RegionDirectiveTriviaSyntax constructorDeclaration, CancellationToken cancellationToken)
        => CSharpSnippetHelpers.AddBlockIndentationToDocumentAsync(
            document,
            constructorDeclaration,
            static d => d.Body!,
            cancellationToken);
}
