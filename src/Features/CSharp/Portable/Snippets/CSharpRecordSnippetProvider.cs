// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeStyle;
using Microsoft.CodeAnalysis.CSharp.CodeStyle;
using Microsoft.CodeAnalysis.CSharp.Extensions;
using Microsoft.CodeAnalysis.CSharp.Extensions.ContextQuery;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.Utilities;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Indentation;
using Microsoft.CodeAnalysis.LanguageService;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Microsoft.CodeAnalysis.Shared.Extensions.ContextQuery;
using Microsoft.CodeAnalysis.Shared.Utilities;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.Snippets;
using Microsoft.CodeAnalysis.Snippets.SnippetProviders;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Snippets
{
    [ExportSnippetProvider(nameof(ISnippetProvider), LanguageNames.CSharp), Shared]
    internal class CSharpRecordSnippetProvider : AbstractRecordSnippetProvider
    {
        [ImportingConstructor]
        [Obsolete(MefConstruction.ImportingConstructorMessage, error: true)]
        public CSharpRecordSnippetProvider()
        {
        }

        private static readonly ISet<SyntaxKind> s_validModifiers = new HashSet<SyntaxKind>(SyntaxFacts.EqualityComparer)
            {
                SyntaxKind.NewKeyword,
                SyntaxKind.PublicKeyword,
                SyntaxKind.ProtectedKeyword,
                SyntaxKind.InternalKeyword,
                SyntaxKind.PrivateKeyword,
                SyntaxKind.AbstractKeyword,
                SyntaxKind.SealedKeyword,
                SyntaxKind.StaticKeyword,
                SyntaxKind.UnsafeKeyword
            };

        protected override async Task<bool> IsValidSnippetLocationAsync(Document document, int position, CancellationToken cancellationToken)
        {
            var semanticModel = await document.ReuseExistingSpeculativeModelAsync(position, cancellationToken).ConfigureAwait(false);
            var syntaxContext = (CSharpSyntaxContext)document.GetRequiredLanguageService<ISyntaxContextService>().CreateContext(document, semanticModel, position, cancellationToken);

            return
                syntaxContext.IsGlobalStatementContext ||
                syntaxContext.IsTypeDeclarationContext(
                    validModifiers: s_validModifiers,
                    validTypeDeclarations: SyntaxKindSet.ClassInterfaceStructRecordTypeDeclarations,
                    canBePartial: true,
                    cancellationToken: cancellationToken);
        }

        protected override int GetTargetCaretPosition(ISyntaxFactsService syntaxFacts, SyntaxNode caretTarget, SourceText sourceText)
        {
            var recordDeclaration = (RecordDeclarationSyntax)caretTarget;
            var triviaSpan = recordDeclaration.CloseBraceToken.LeadingTrivia.Span;
            var line = sourceText.Lines.GetLineFromPosition(triviaSpan.Start);
            // Getting the location at the end of the line before the newline.
            return line.Span.End;
        }

        private static string GetIndentation(Document document, SyntaxNode node, SyntaxFormattingOptions syntaxFormattingOptions, CancellationToken cancellationToken)
        {
            var parsedDocument = ParsedDocument.CreateSynchronously(document, cancellationToken);
            var recordDeclarationSyntax = (RecordDeclarationSyntax)node;
            var openBraceLine = parsedDocument.Text.Lines.GetLineFromPosition(recordDeclarationSyntax.OpenBraceToken.SpanStart).LineNumber;

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
            var indentationString = GetIndentation(document, snippet, syntaxFormattingOptions, cancellationToken);

            var originalRecordDeclaration = (RecordDeclarationSyntax)snippet;
            var newRecordDeclaration = originalRecordDeclaration.WithCloseBraceToken(
                originalRecordDeclaration.CloseBraceToken.WithPrependedLeadingTrivia(SyntaxFactory.SyntaxTrivia(SyntaxKind.WhitespaceTrivia, indentationString)));

            var newRoot = root.ReplaceNode(originalRecordDeclaration, newRecordDeclaration.WithAdditionalAnnotations(_cursorAnnotation, _findSnippetAnnotation));
            return document.WithSyntaxRoot(newRoot);
        }

        protected override void GetClassDeclarationIdentifier(SyntaxNode node, out SyntaxToken identifier)
        {
            var classDeclarationSyntax = (RecordDeclarationSyntax)node;
            identifier = classDeclarationSyntax.Identifier;
        }

        protected override async Task<SyntaxNode> CreateRecordDeclarationSyntaxAsync(Document document, int position, CancellationToken cancellationToken)
        {
            var generator = SyntaxGenerator.GetGenerator(document);
            var semanticModel = await document.GetRequiredSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            var options = await document.GetAnalyzerConfigOptionsAsync(cancellationToken).ConfigureAwait(false);
            var accessibilityModifiersRequired = options.GetEditorConfigOptionValue(CodeStyleOptions2.AccessibilityModifiersRequired, AccessibilityModifiersRequired.Never);
            var useAccessibility = accessibilityModifiersRequired is AccessibilityModifiersRequired.ForNonInterfaceMembers or AccessibilityModifiersRequired.Always;

            var name = NameGenerator.GenerateUniqueName("MyRecord", name => semanticModel.LookupSymbols(position, name: name).IsEmpty);
            var recordDeclaration = useAccessibility is true
                ? SyntaxFactory.RecordDeclaration(name, accessibility: Accessibility.Public)
                : generator.ClassDeclaration(name);
        }
    }
}
