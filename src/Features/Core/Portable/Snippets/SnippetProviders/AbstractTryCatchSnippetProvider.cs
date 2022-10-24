﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.LanguageService;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Microsoft.CodeAnalysis.Shared.Extensions.ContextQuery;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.Snippets.SnippetProviders
{
    internal abstract class AbstractTryCatchSnippetProvider : AbstractSnippetProvider
    {
        public override string SnippetIdentifier => "try";

        public override string SnippetDescription => FeaturesResources.try_catch;

        protected override async Task<bool> IsValidSnippetLocationAsync(Document document, int position, CancellationToken cancellationToken)
        {
            var semanticModel = await document.ReuseExistingSpeculativeModelAsync(position, cancellationToken).ConfigureAwait(false);

            var syntaxContext = document.GetRequiredLanguageService<ISyntaxContextService>().CreateContext(document, semanticModel, position, cancellationToken);
            return syntaxContext.IsStatementContext || syntaxContext.IsGlobalStatementContext;
        }

        protected override Task<ImmutableArray<TextChange>> GenerateSnippetTextChangesAsync(Document document, int position, CancellationToken cancellationToken)
        {
            var generator = SyntaxGenerator.GetGenerator(document);
            var tryCatchStatement = generator.TryCatchStatement(tryStatements: Array.Empty<SyntaxNode>(),
                generator.CatchClause(generator.IdentifierName("Exception"), identifier: "e", new[] { generator.ThrowStatement() }));
            return Task.FromResult(ImmutableArray.Create(new TextChange(TextSpan.FromBounds(position, position),
                tryCatchStatement.NormalizeWhitespace().ToFullString())));
        }

        protected override Func<SyntaxNode?, bool> GetSnippetContainerFunction(ISyntaxFacts syntaxFacts)
        {
            return syntaxFacts.IsTryStatement;
        }
    }
}
