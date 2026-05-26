// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Internal.Log;
using Microsoft.CodeAnalysis.LanguageService;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Microsoft.CodeAnalysis.Snippets;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.Completion.Providers.Snippets;

internal abstract class AbstractSnippetCompletionProvider : CompletionProvider
{
    internal override bool IsSnippetProvider => true;

    public override async Task<CompletionChange> GetChangeAsync(Document document, CompletionItem item, char? commitKey = null, CancellationToken cancellationToken = default)
    {
        // This retrieves the document without the text used to invoke completion
        // as well as the new cursor position after that has been removed.
        var (strippedDocument, position) = await GetDocumentWithoutInvokingTextAsync(document, SnippetCompletionItem.GetInvocationPosition(item), cancellationToken).ConfigureAwait(false);

        // For preprocessor snippets the editor's format-on-type may have moved
        // the # away from where the user typed it. Restore the original line
        // indentation so the snippet is generated at the user's invocation point.
        var originalIndentation = SnippetCompletionItem.GetLineIndentation(item);
        if (originalIndentation is not null)
        {
            var currentText = await strippedDocument.GetValueTextAsync(cancellationToken).ConfigureAwait(false);
            var line = currentText.Lines.GetLineFromPosition(position);
            var currentLineText = currentText.ToString(line.Span);
            var hashIndex = currentLineText.IndexOf('#');
            if (hashIndex >= 0)
            {
                var currentIndentation = currentLineText[..hashIndex];
                if (currentIndentation != originalIndentation)
                {
                    // Replace the line's leading whitespace up to # with the original indentation.
                    var indentSpan = TextSpan.FromBounds(line.Start, line.Start + hashIndex);
                    currentText = currentText.WithChanges(new TextChange(indentSpan, originalIndentation));
                    strippedDocument = strippedDocument.WithText(currentText);
                    // Adjust position: it was right after #, which shifted by the indentation difference.
                    position = line.Start + originalIndentation.Length + (position - line.Start - hashIndex);
                }
            }
        }

        var service = strippedDocument.GetRequiredLanguageService<ISnippetService>();
        var snippetIdentifier = SnippetCompletionItem.GetSnippetIdentifier(item);
        var snippetProvider = service.GetSnippetProvider(snippetIdentifier);

        // Logging for telemetry.
        Logger.Log(FunctionId.Completion_SemanticSnippets, $"Name: {snippetIdentifier}", LogLevel.Information);

        // This retrieves the generated Snippet
        var snippetChange = await snippetProvider.GetSnippetChangeAsync(strippedDocument, position, cancellationToken).ConfigureAwait(false);
        var strippedText = await strippedDocument.GetValueTextAsync(cancellationToken).ConfigureAwait(false);

        // This introduces the text changes of the snippet into the document with the completion invoking text
        var allChangesText = strippedText.WithChanges(snippetChange.TextChanges);

        // This retrieves ALL text changes from the original document which includes the TextChanges from the snippet
        // as well as the clean up.
        var allChangesDocument = document.WithText(allChangesText);
        var allTextChanges = await allChangesDocument.GetTextChangesAsync(document, cancellationToken).ConfigureAwait(false);

        var change = Utilities.Collapse(allChangesText, allTextChanges.AsImmutable());

        // Converts the snippet to an LSP formatted snippet string.
        var lspSnippet = await RoslynLSPSnippetConverter.GenerateLSPSnippetAsync(allChangesDocument, snippetChange.FinalCaretPosition, snippetChange.Placeholders, change, item.Span.Start, cancellationToken).ConfigureAwait(false);

        // If the TextChanges retrieved starts after the trigger point of the CompletionItem,
        // then we need to move the bounds backwards and encapsulate the trigger point and adjust the changed text.
        if (change.Span.Start > item.Span.Start)
        {
            var textSpan = TextSpan.FromBounds(item.Span.Start, change.Span.End);
            var snippetText = allChangesText.GetSubText(textSpan).ToString();
            change = new TextChange(textSpan, snippetText);
        }

        var props = ImmutableDictionary<string, string>.Empty
            .Add(SnippetCompletionItem.LSPSnippetKey, lspSnippet);

        return CompletionChange.Create(change, allTextChanges.AsImmutable(), properties: props, snippetChange.FinalCaretPosition, includesCommitCharacter: true);
    }

    public override async Task ProvideCompletionsAsync(CompletionContext context)
    {
        if (!context.CompletionOptions.ShouldShowNewSnippetExperience(context.Document))
        {
            return;
        }

        var document = context.Document;
        var cancellationToken = context.CancellationToken;
        var position = context.Position;
        var service = document.GetLanguageService<ISnippetService>();

        if (service == null)
        {
            return;
        }

        var syntaxContext = await context.GetSyntaxContextWithExistingSpeculativeModelAsync(document, cancellationToken).ConfigureAwait(false);
        var snippetContext = new SnippetContext(syntaxContext);
        var snippets = service.GetSnippets(snippetContext, cancellationToken);

        // For preprocessor directive snippets (e.g. #region), capture the
        // indentation of the line at trigger time. The editor's format-on-type
        // may move the # before the snippet is committed, but we want to
        // preserve the user's original invocation position.
        string? lineIndentation = null;
        if (syntaxContext.IsPreProcessorDirectiveContext)
        {
            var text = await document.GetValueTextAsync(cancellationToken).ConfigureAwait(false);
            var line = text.Lines.GetLineFromPosition(position);
            var lineText = text.ToString(line.Span);
            var hashIndex = lineText.IndexOf('#');
            if (hashIndex >= 0)
                lineIndentation = lineText[..hashIndex];
        }

        foreach (var snippetData in snippets)
        {
            var completionItem = SnippetCompletionItem.Create(
                displayText: snippetData.Identifier,
                displayTextSuffix: "",
                position: position,
                snippetIdentifier: snippetData.Identifier,
                glyph: Glyph.Snippet,
                description: (snippetData.Description + Environment.NewLine + string.Format(FeaturesResources.Code_snippet_for_0, snippetData.Description)).ToSymbolDisplayParts(),
                inlineDescription: snippetData.Description,
                additionalFilterTexts: snippetData.AdditionalFilterTexts,
                lineIndentation: lineIndentation);
            context.AddItem(completionItem);
        }
    }

    internal override async Task<CompletionDescription?> GetDescriptionAsync(Document document, CompletionItem item, CompletionOptions options, SymbolDescriptionOptions displayOptions, CancellationToken cancellationToken)
    {
        return await Task.FromResult(CommonCompletionItem.GetDescription(item)).ConfigureAwait(false);
    }

    /// Gets the document without whatever text was used to invoke the completion.
    /// Also gets the new position the cursor will be on.
    /// Returns the original document and position if completion was invoked using Ctrl-Space.
    /// 
    /// public void Method()
    /// {
    ///     $$               //invoked by typing Ctrl-Space
    /// }
    /// Example invoking when span is not empty:
    /// public void Method()
    /// {
    ///     Wr$$             //invoked by typing out the completion 
    /// }
    private static async Task<(Document, int)> GetDocumentWithoutInvokingTextAsync(Document document, int position, CancellationToken cancellationToken)
    {
        var originalText = await document.GetValueTextAsync(cancellationToken).ConfigureAwait(false);

        // Uses the existing CompletionService logic to find the TextSpan we want to use for the document sans invoking text
        var completionService = document.GetRequiredLanguageService<CompletionService>();
        var span = completionService.GetDefaultCompletionListSpan(originalText, position);

        var textChange = new TextChange(span, string.Empty);
        originalText = originalText.WithChanges(textChange);

        // The document might not be frozen, so make sure we freeze it here to avoid triggering source generator which
        // is not needed for snippet completion and will cause perf issue. Pass in 'forceFreeze: true' to ensure all
        // further transformations we make do not run generators either.
        var newDocument = document.WithText(originalText).WithFrozenPartialSemantics(forceFreeze: true, cancellationToken);
        return (newDocument, span.Start);
    }
}
