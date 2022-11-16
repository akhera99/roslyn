// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.LanguageService;
using Microsoft.CodeAnalysis.Snippets.SnippetProviders;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.Snippets
{
    internal abstract class AbstractIfSnippetProvider : AbstractConditionalBlockSnippetProvider
    {
        public override string Identifier => "if";

        public override string Description => FeaturesResources.if_statement;

        public override ImmutableArray<string> AdditionalFilterTexts { get; } = ImmutableArray.Create("statement");

        protected override Func<SyntaxNode?, bool> GetSnippetContainerFunction(ISyntaxFacts syntaxFacts) => syntaxFacts.IsIfStatement;

        protected override TextChange GenerateSnippetTextChange(Document document, int position)
        {
            var generator = SyntaxGenerator.GetGenerator(document);
            var ifStatement = generator.IfStatement(generator.TrueLiteralExpression(), Array.Empty<SyntaxNode>());

            return new TextChange(TextSpan.FromBounds(position, position), ifStatement.ToFullString());
        }
<<<<<<< HEAD

        protected override ImmutableArray<SnippetPlaceholder> GetTargetCaretPosition(ISyntaxFactsService syntaxFacts, SyntaxNode caretTarget, SourceText sourceText)
        {
            GetIfStatementCursorPosition(sourceText, caretTarget, out var cursorPosition);

            // Place at the end of the node specified for cursor position.
            // Is the statement node in C# and the "Then" keyword
            return ImmutableArray.Create(new SnippetPlaceholder(cursorIndex: 0, tabStopPosition: cursorPosition));
        }

        protected override ImmutableArray<SnippetPlaceholder> GetPlaceHolderLocationsList(SyntaxNode node, ISyntaxFacts syntaxFacts, CancellationToken cancellationToken)
        {
            using var _ = ArrayBuilder<SnippetPlaceholder>.GetInstance(out var arrayBuilder);
            GetIfStatementCondition(node, out var condition);
            arrayBuilder.Add(new SnippetPlaceholder(identifier: condition.ToString(), cursorIndex: 1, placeholderPositions: ImmutableArray.Create(condition.SpanStart)));

            return arrayBuilder.ToImmutableArray();
        }
=======
>>>>>>> main
    }
}
