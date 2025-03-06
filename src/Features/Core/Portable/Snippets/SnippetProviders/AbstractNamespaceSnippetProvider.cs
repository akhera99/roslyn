// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.Snippets.SnippetProviders;

internal abstract class AbstractNamespaceSnippetProvider<TNamespaceDeclarationSyntax> : AbstractSnippetProvider<TNamespaceDeclarationSyntax>
    where TNamespaceDeclarationSyntax : SyntaxNode
{
    protected abstract Task<TNamespaceDeclarationSyntax> GenerateNamespaceDeclarationAsync(Document document, int position, CancellationToken cancellationToken);

    protected override async Task<ImmutableArray<TextChange>> GenerateSnippetTextChangesAsync(Document document, int position, CancellationToken cancellationToken)
    {
        var namespaceDeclarationSyntax = await GenerateNamespaceDeclarationAsync(document, position, cancellationToken).ConfigureAwait(false);

        return [new TextChange(TextSpan.FromBounds(position, position), namespaceDeclarationSyntax.NormalizeWhitespace().ToFullString())];
    }
}
