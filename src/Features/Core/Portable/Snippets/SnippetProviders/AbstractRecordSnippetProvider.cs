// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeStyle;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.LanguageService;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Microsoft.CodeAnalysis.Shared.Utilities;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.Snippets
{
    internal abstract class AbstractRecordSnippetProvider : AbstractSnippetProvider
    {
        protected abstract Task<SyntaxNode> CreateRecordDeclarationSyntaxAsync(Document document, int position, CancellationToken cancellationToken);
        protected abstract void GetClassDeclarationIdentifier(SyntaxNode node, out SyntaxToken identifier);

        public override string SnippetIdentifier => "record";

        public override string SnippetDescription => FeaturesResources.record;

        protected override async Task<ImmutableArray<TextChange>> GenerateSnippetTextChangesAsync(Document document, int position, CancellationToken cancellationToken)
        {
            var recordDeclaration = await CreateRecordDeclarationSyntaxAsync(document, position, cancellationToken).ConfigureAwait(false);
            var snippetTextChange = new TextChange(TextSpan.FromBounds(position, position), recordDeclaration.NormalizeWhitespace().ToFullString());
            return ImmutableArray.Create(snippetTextChange);
        }

        protected override Func<SyntaxNode?, bool> GetSnippetContainerFunction(ISyntaxFacts syntaxFacts)
        {
            return syntaxFacts.IsClassDeclaration;
        }

        protected override ImmutableArray<SnippetPlaceholder> GetPlaceHolderLocationsList(SyntaxNode node, ISyntaxFacts syntaxFacts, CancellationToken cancellationToken)
        {
            using var _ = ArrayBuilder<SnippetPlaceholder>.GetInstance(out var arrayBuilder);
            GetClassDeclarationIdentifier(node, out var identifier);
            arrayBuilder.Add(new SnippetPlaceholder(identifier: identifier.ValueText, placeholderPositions: ImmutableArray.Create(identifier.SpanStart)));

            return arrayBuilder.ToImmutableArray();
        }
    }
}
