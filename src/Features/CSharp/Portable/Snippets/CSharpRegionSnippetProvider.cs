// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Extensions.ContextQuery;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.LanguageService;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Microsoft.CodeAnalysis.Snippets;
using Microsoft.CodeAnalysis.Snippets.SnippetProviders;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.CSharp.Snippets;

[ExportSnippetProvider(nameof(ISnippetProvider), LanguageNames.CSharp), Shared]
[method: ImportingConstructor]
[method: Obsolete(MefConstruction.ImportingConstructorMessage, error: true)]
internal sealed class CSharpRegionSnippetProvider() : AbstractSingleChangeSnippetProvider<RegionDirectiveTriviaSyntax>
{
    private const string RegionNamePlaceholder = "MyRegion";

    public override string Identifier => CSharpSnippetIdentifiers.Region;

    public override string Description => CSharpFeaturesResources.region_directive;

    public override ImmutableArray<string> AdditionalFilterTexts => ["region"];

    protected override bool IsValidSnippetLocationCore(SnippetContext context, CancellationToken cancellationToken)
    {
        var syntaxContext = (CSharpSyntaxContext)context.SyntaxContext;
        return syntaxContext.IsPreProcessorKeywordContext;
    }

    protected override async Task<TextChange> GenerateSnippetTextChangeAsync(
        Document document, int position, CancellationToken cancellationToken)
    {
        var text = await document.GetValueTextAsync(cancellationToken).ConfigureAwait(false);
        var syntaxFormattingOptions = await document.GetSyntaxFormattingOptionsAsync(cancellationToken).ConfigureAwait(false);
        var newLine = syntaxFormattingOptions.NewLine;

        var line = text.Lines.GetLineFromPosition(position);
        var lineText = text.ToString(line.Span);

        // Find the # on this line. In preprocessor keyword context, the # is already
        // in the document. Extend the replacement span to include it so we produce
        // a single well-formed #region / #endregion pair.
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

        return new TextChange(TextSpan.FromBounds(startPosition, position), snippetText);
    }

    protected override RegionDirectiveTriviaSyntax? FindAddedSnippetSyntaxNode(SyntaxNode root, int position)
    {
        // Preprocessor directives are structured trivia, so the base class's
        // FindNode won't locate them. Use FindToken with findInsideTrivia to
        // reach into the directive's token tree.
        var token = root.FindToken(position, findInsideTrivia: true);
        return token.Parent as RegionDirectiveTriviaSyntax
            ?? token.Parent?.FirstAncestorOrSelf<RegionDirectiveTriviaSyntax>();
    }

    protected override ValueTask<ImmutableArray<SnippetPlaceholder>> GetPlaceHolderLocationsListAsync(
        Document document, RegionDirectiveTriviaSyntax node, ISyntaxFacts syntaxFacts, CancellationToken cancellationToken)
    {
        // The region name text lives as PreprocessingMessageTrivia on the EndOfDirectiveToken.
        var endOfDirective = node.EndOfDirectiveToken;
        foreach (var trivia in endOfDirective.LeadingTrivia)
        {
            if (trivia.Kind() == SyntaxKind.PreprocessingMessageTrivia)
            {
                return new([new SnippetPlaceholder(trivia.ToString(), trivia.SpanStart)]);
            }
        }

        return new([]);
    }

    protected override int GetTargetCaretPosition(
        RegionDirectiveTriviaSyntax regionDirective, SourceText sourceText)
    {
        // Place the caret on the blank line between #region and #endregion.
        var regionLine = sourceText.Lines.GetLineFromPosition(regionDirective.SpanStart);
        var nextLineNumber = regionLine.LineNumber + 1;
        if (nextLineNumber < sourceText.Lines.Count)
            return sourceText.Lines[nextLineNumber].Span.End;

        return regionDirective.FullSpan.End;
    }
}
