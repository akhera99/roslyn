// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Indentation;
using Microsoft.CodeAnalysis.LanguageService;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Microsoft.CodeAnalysis.Snippets;
using Microsoft.CodeAnalysis.Snippets.SnippetProviders;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.CSharp.Snippets
{
    [ExportSnippetProvider(nameof(ISnippetProvider), LanguageNames.CSharp), Shared]
    internal class CSharpTryCatchSnippetProvider : AbstractTryCatchSnippetProvider
    {
        [ImportingConstructor]
        [Obsolete(MefConstruction.ImportingConstructorMessage, error: true)]
        public CSharpTryCatchSnippetProvider()
        {
        }

        protected override ImmutableArray<SnippetPlaceholder> GetTargetCaretPosition(ISyntaxFactsService syntaxFacts, SyntaxNode caretTarget, SourceText sourceText)
        {
            using var _ = ArrayBuilder<SnippetPlaceholder>.GetInstance(out var arrayBuilder);
            var catchStatement = (TryStatementSyntax)caretTarget;
            var triviaSpan = catchStatement.Block.CloseBraceToken.LeadingTrivia.Span;
            var tryBodyLine = sourceText.Lines.GetLineFromPosition(triviaSpan.Start);
            arrayBuilder.Add(new SnippetPlaceholder(cursorIndex: 1, tabStopPosition: tryBodyLine.Span.End));
            var catchBlockTriviaSpan = catchStatement.Catches.First().Block.Statements.First().GetTrailingTrivia().First().Span;
            var catchBodyLine = sourceText.Lines.GetLineFromPosition(catchBlockTriviaSpan.Start);
            arrayBuilder.Add(new SnippetPlaceholder(cursorIndex: 0, tabStopPosition: catchBodyLine.Span.End));
            return arrayBuilder.ToImmutableArray();
        }

        protected override ImmutableArray<SnippetPlaceholder> GetPlaceHolderLocationsList(SyntaxNode node, ISyntaxFacts syntaxFacts, CancellationToken cancellationToken)
        {
            using var _ = ArrayBuilder<SnippetPlaceholder>.GetInstance(out var arrayBuilder);
            var tryStatementSyntax = (TryStatementSyntax)node;
            var catchStatementDeclaration = tryStatementSyntax.Catches.First().Declaration;
            var exceptionType = catchStatementDeclaration.Type;
            var exceptionIdentifier = catchStatementDeclaration.Identifier;

            arrayBuilder.Add(new SnippetPlaceholder(exceptionType.ToString(), cursorIndex: 2, ImmutableArray.Create(exceptionType.SpanStart)));
            arrayBuilder.Add(new SnippetPlaceholder(exceptionIdentifier.ToString(), cursorIndex: 3, ImmutableArray.Create(exceptionIdentifier.SpanStart)));

            return arrayBuilder.ToImmutableArray();
        }

        private static string GetIndentation(Document document, BlockSyntax block, SyntaxFormattingOptions syntaxFormattingOptions, CancellationToken cancellationToken)
        {
            var parsedDocument = ParsedDocument.CreateSynchronously(document, cancellationToken);
            var openBraceLine = parsedDocument.Text.Lines.GetLineFromPosition(block.SpanStart).LineNumber;

            var indentationOptions = new IndentationOptions(syntaxFormattingOptions);
            var newLine = indentationOptions.FormattingOptions.NewLine;

            var indentationService = parsedDocument.LanguageServices.GetRequiredService<IIndentationService>();
            var indentation = indentationService.GetIndentation(parsedDocument, openBraceLine + 1, indentationOptions, cancellationToken);

            // Adding the offset calculated with one tab so that it is indented once past the line containing the opening brace
            var newIndentation = new IndentationResult(indentation.BasePosition, indentation.Offset + syntaxFormattingOptions.TabSize);
            return newIndentation.GetIndentationString(parsedDocument.Text, syntaxFormattingOptions.UseTabs, syntaxFormattingOptions.TabSize) + newLine;
        }

        protected override async Task<Document> AddIndentationToDocumentAsync(Document document, int position, ISyntaxFacts syntaxFacts, CancellationToken cancellationToken)
        {
            var root = await document.GetRequiredSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var snippet = root.GetAnnotatedNodes(_findSnippetAnnotation).FirstOrDefault();

            var syntaxFormattingOptions = await document.GetSyntaxFormattingOptionsAsync(fallbackOptions: null, cancellationToken).ConfigureAwait(false);
            var tryStatementSyntax = (TryStatementSyntax)snippet;
            var indentationStringForTry = GetIndentation(document, tryStatementSyntax.Block, syntaxFormattingOptions, cancellationToken);

            var tryBlock = tryStatementSyntax.Block;
            tryBlock = tryBlock.WithCloseBraceToken(tryBlock.CloseBraceToken.WithPrependedLeadingTrivia(SyntaxFactory.SyntaxTrivia(SyntaxKind.WhitespaceTrivia, indentationStringForTry)));
            var newTryStatementSyntax = tryStatementSyntax.ReplaceNode(tryStatementSyntax.Block, tryBlock);

            var catchBlock = newTryStatementSyntax.Catches.First().Block;
            var indentationStringForCatch = GetIndentation(document, catchBlock, syntaxFormattingOptions, cancellationToken);
            catchBlock = catchBlock.WithOpenBraceToken(catchBlock.OpenBraceToken.WithAppendedTrailingTrivia(SyntaxFactory.SyntaxTrivia(SyntaxKind.WhitespaceTrivia, indentationStringForCatch)));
            newTryStatementSyntax = newTryStatementSyntax.ReplaceNode(newTryStatementSyntax.Catches.First().Block, catchBlock);

            var newRoot = root.ReplaceNode(tryStatementSyntax, newTryStatementSyntax);
            return document.WithSyntaxRoot(newRoot);
        }
    }
}
