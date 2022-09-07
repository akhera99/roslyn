﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.LanguageService;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Microsoft.CodeAnalysis.Shared.Extensions.ContextQuery;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.Snippets
{
    internal abstract class AbstractForLoopSnippetProvider : AbstractSnippetProvider
    {
        protected abstract Task<SyntaxNode> CreateForLoopStatementSyntaxAsync(Document document, int position, CancellationToken cancellationToken);

        public override string SnippetIdentifier => "for";
        public override string SnippetDescription => FeaturesResources.for_loop;

        protected override async Task<bool> IsValidSnippetLocationAsync(Document document, int position, CancellationToken cancellationToken)
        {
            var semanticModel = await document.ReuseExistingSpeculativeModelAsync(position, cancellationToken).ConfigureAwait(false);

            var syntaxContext = document.GetRequiredLanguageService<ISyntaxContextService>().CreateContext(document, semanticModel, position, cancellationToken);
            return syntaxContext.IsStatementContext || syntaxContext.IsGlobalStatementContext;
        }

        protected override async Task<ImmutableArray<TextChange>> GenerateSnippetTextChangesAsync(Document document, int position, CancellationToken cancellationToken)
        {
            var forLoopSyntax = await CreateForLoopStatementSyntaxAsync(document, position, cancellationToken).ConfigureAwait(false);
            var snippetTextChange = new TextChange(TextSpan.FromBounds(position, position), forLoopSyntax.NormalizeWhitespace().ToFullString());
            return ImmutableArray.Create(snippetTextChange);
        }

        protected override async Task<SyntaxNode> AnnotateNodesToReformatAsync(Document document,
            SyntaxAnnotation findSnippetAnnotation, SyntaxAnnotation cursorAnnotation, int position, CancellationToken cancellationToken)
        {
            var root = await document.GetRequiredSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var syntaxFacts = document.GetRequiredLanguageService<ISyntaxFactsService>();
            var snippetStatementNode = FindAddedSnippetSyntaxNode(root, position, GetSnippetContainerFunction(syntaxFacts));
            if (snippetStatementNode is null)
            {
                return root;
            }

            var reformatSnippetNode = snippetStatementNode.WithAdditionalAnnotations(findSnippetAnnotation, cursorAnnotation, Simplifier.Annotation, Formatter.Annotation);
            return root.ReplaceNode(snippetStatementNode, reformatSnippetNode);
        }

        protected override Func<SyntaxNode?, bool> GetSnippetContainerFunction(ISyntaxFacts syntaxFacts)
        {
            return syntaxFacts.IsForStatement;
        }
    }
}

