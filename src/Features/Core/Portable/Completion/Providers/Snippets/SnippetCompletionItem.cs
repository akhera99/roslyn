// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Immutable;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Completion.Providers.Snippets;

internal sealed class SnippetCompletionItem
{
    public static string LSPSnippetKey = "LSPSnippet";
    public static string SnippetIdentifierKey = "SnippetIdentifier";
    private static string LineIndentationKey = "LineIndentation";

    public static CompletionItem Create(
        string displayText,
        string displayTextSuffix,
        int position,
        string snippetIdentifier,
        Glyph glyph,
        ImmutableArray<SymbolDisplayPart> description,
        string inlineDescription,
        ImmutableArray<string> additionalFilterTexts,
        string? lineIndentation = null)
    {
        var properties = new List<KeyValuePair<string, string>>
        {
            KeyValuePair.Create("Position", position.ToString()),
            KeyValuePair.Create(SnippetIdentifierKey, snippetIdentifier)
        };

        if (lineIndentation is not null)
            properties.Add(KeyValuePair.Create(LineIndentationKey, lineIndentation));

        return CommonCompletionItem.Create(
            displayText: displayText,
            displayTextSuffix: displayTextSuffix,
            glyph: glyph,
            description: description,
            // Adding a space after the identifier string that way it will always be sorted after a keyword.
            sortText: snippetIdentifier + " ",
            filterText: snippetIdentifier,
            properties: [.. properties],
            isComplexTextEdit: true,
            inlineDescription: inlineDescription,
            rules: CompletionItemRules.Default)
            .WithAdditionalFilterTexts(additionalFilterTexts);
    }

    public static string GetSnippetIdentifier(CompletionItem item)
    {
        Contract.ThrowIfFalse(item.TryGetProperty(SnippetIdentifierKey, out var text));
        return text;
    }

    public static int GetInvocationPosition(CompletionItem item)
    {
        Contract.ThrowIfFalse(item.TryGetProperty("Position", out var text));
        Contract.ThrowIfFalse(int.TryParse(text, out var num));
        return num;
    }

    public static bool IsSnippet(CompletionItem item)
    {
        return item.TryGetProperty(SnippetIdentifierKey, out var _);
    }

    public static string? GetLineIndentation(CompletionItem item)
    {
        item.TryGetProperty(LineIndentationKey, out var indentation);
        return indentation;
    }
}
