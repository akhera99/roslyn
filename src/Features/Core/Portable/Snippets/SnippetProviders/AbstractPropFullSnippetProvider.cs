// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.LanguageService;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.Snippets.SnippetProviders
{
    internal abstract class AbstractPropFullSnippetProvider<TPropertyDeclarationSyntax> : AbstractSnippetProvider<TPropertyDeclarationSyntax>
        where TPropertyDeclarationSyntax : SyntaxNode
    {
        protected abstract Task<(TPropertyDeclarationSyntax, SyntaxNode)> GeneratePropertyDeclarationAsync(Document document, int position, CancellationToken cancellationToken);
        protected sealed override async Task<ImmutableArray<TextChange>> GenerateSnippetTextChangesAsync(Document document, int position, CancellationToken cancellationToken)
        {
            var (propertyDeclaration, fieldSyntax) = await GeneratePropertyDeclarationAsync(document, position, cancellationToken).ConfigureAwait(false);
            var propertyTextChange = new TextChange(TextSpan.FromBounds(position, position), propertyDeclaration.NormalizeWhitespace().ToFullString());

            var emptyLineTextChange = new TextChange(new TextSpan(position, 0), Environment.NewLine + Environment.NewLine);

            var fieldTextChange = new TextChange(new TextSpan(position + emptyLineTextChange.NewText!.Length, 0), fieldSyntax.NormalizeWhitespace().ToFullString());
            return [propertyTextChange, emptyLineTextChange, fieldTextChange];
        }
    }
}
