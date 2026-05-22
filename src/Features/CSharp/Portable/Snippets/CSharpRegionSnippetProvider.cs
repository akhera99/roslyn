// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Extensions.ContextQuery;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.LanguageService;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Microsoft.CodeAnalysis.Shared.Extensions.ContextQuery;
using Microsoft.CodeAnalysis.Snippets;
using Microsoft.CodeAnalysis.Snippets.SnippetProviders;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.CSharp.Snippets;

[ExportSnippetProvider(nameof(ISnippetProvider), LanguageNames.CSharp), Shared]
[method: ImportingConstructor]
[method: Obsolete(MefConstruction.ImportingConstructorMessage, error: true)]
internal sealed class CSharpRegionSnippetProvider() : ISnippetProvider
{
    private const string RegionNamePlaceholder = "MyRegion";

    public string Identifier => CSharpSnippetIdentifiers.Region;

    public string Description => CSharpFeaturesResources.region_directive;

    public ImmutableArray<string> AdditionalFilterTexts => ["region"];

    public bool IsValidSnippetLocation(SnippetContext context, CancellationToken cancellationToken)
    {
        var syntaxFacts = context.Document.GetRequiredLanguageService<ISyntaxFactsService>();
        if (syntaxFacts.IsInNonUserCode(context.SyntaxContext.SyntaxTree, context.Position, cancellationToken))
            return false;

        var syntaxContext = (CSharpSyntaxContext)context.SyntaxContext;
        return syntaxContext.IsPreProcessorKeywordContext;
    }

    public async Task<SnippetChange> GetSnippetChangeAsync(
        Document document, int position, CancellationToken cancellationToken)
    {
        var text = await document.GetValueTextAsync(cancellationToken).ConfigureAwait(false);
        var syntaxFormattingOptions = await document.GetSyntaxFormattingOptionsAsync(cancellationToken).ConfigureAwait(false);
        var newLine = syntaxFormattingOptions.NewLine;

        var line = text.Lines.GetLineFromPosition(position);
        var lineText = text.ToString(line.Span);

        // Find the # on this line and replace from there.
        var hashIndexInLine = lineText.IndexOf('#');
        int startPosition;
        string indentation;

        if (hashIndexInLine >= 0)
        {
            startPosition = line.Start + hashIndexInLine;
            indentation = lineText[..hashIndexInLine];
        }
        else
        {
            startPosition = position;
            indentation = "";
        }

        var snippetText = $"#region {RegionNamePlaceholder}{newLine}{newLine}{indentation}#endregion";
        var change = new TextChange(TextSpan.FromBounds(startPosition, position), snippetText);

        // Apply to compute positions in the resulting text.
        var newText = text.WithChanges(change);

        // "MyRegion" starts right after "#region " in the inserted text.
        var regionNameStart = startPosition + "#region ".Length;

        // Caret goes on the blank line between #region and #endregion.
        var regionLineNumber = newText.Lines.GetLineFromPosition(startPosition).LineNumber;
        var caretPosition = newText.Lines[regionLineNumber + 1].Span.End;

        return new SnippetChange(
            textChanges: [change],
            placeholders: [new SnippetPlaceholder(RegionNamePlaceholder, regionNameStart)],
            finalCaretPosition: caretPosition);
    }
}
