// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Editor.Shared.Utilities;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Microsoft.CodeAnalysis.Snippets;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Language.Proposals;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.CodeAnalysis.Editor.CSharp.Snippets
{
    [Export(typeof(ProposalSourceProviderBase))]
    [Name(nameof(SnippetProposalSourceProvider))]
    [ContentType(ContentTypeNames.CSharpContentType)]
    [Obsolete]
    internal class SnippetProposalSourceProvider : ProposalSourceProviderBase
    {
        private readonly IThreadingContext _threadingContext;

        [ImportingConstructor]
        [Obsolete(MefConstruction.ImportingConstructorMessage, error: true)]
        public SnippetProposalSourceProvider(IThreadingContext threadingContext)
        {
            _threadingContext = threadingContext;
        }

        public override async Task<ProposalSourceBase?> GetProposalSourceAsync(ITextView view, CancellationToken cancellationToken)
        {
            await _threadingContext.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            return SnippetProposalSource.GetOrCreate(view, this);
        }
    }

    [Obsolete]
    internal class SnippetProposalSource : ProposalSourceBase
    {
        private readonly ITextView _textView;
        private readonly SnippetProposalSourceProvider _factory;

        private static readonly HashSet<string> snippetsWithProposals = new()
        {
            "class", "foreach", "if", "ctor", "interface", "prop", "struct", "while"
        };

        internal static SnippetProposalSource GetOrCreate(ITextView view, SnippetProposalSourceProvider factory)
        {
            return view.Properties.GetOrCreateSingletonProperty(typeof(SnippetProposalSource), () => new SnippetProposalSource(view, factory));
        }

        private SnippetProposalSource(ITextView view, SnippetProposalSourceProvider factory)
        {
            _textView = view;
            _factory = factory;
        }

        public override async Task<ProposalCollectionBase?> RequestProposalsAsync(VirtualSnapshotPoint caret, CompletionState? completionState, ProposalScenario scenario, char triggeringCharacter, CancellationToken cancel)
        {
            if ((completionState != null) && completionState.IsSnippet)
            {
                var proposals = new List<ProposalBase>(1);
                foreach (var s in snippetsWithProposals)
                {
                    if (completionState.SelectedItem == s)
                    {
                        var changes = await GetSelectedSnippetAsync(s, caret.Position, cancel).ConfigureAwait(false);
                        if (changes is null)
                        {
                            break;
                        }

                        var snippet = new Snippet(s, changes.Value.change, changes.Value.snippet);
                        var proposal = Proposal.TryCreateProposal(description: $"Insert {s} snippet",
                                                                  new[] { new ProposedEdit(new SnapshotSpan(caret.Position, 0), snippet.InsertionText, snippet.Fields) },
                                                                  caret,
                                                                  completionState,
                                                                  ProposalFlags.SingleTabToAccept,
                                                                  commitAction: () => false);
                        if (proposal != null)
                        {
                            proposals.Add(proposal);
                        }

                        break;
                    }
                }

                return new ProposalCollection(nameof(SnippetProposalSourceProvider), proposals);
            }

            return null;
        }

        private async Task<(TextChange change, SnippetChange snippet)?> GetSelectedSnippetAsync(string selectedSnippet, int position, CancellationToken cancellationToken)
        {
            var openDocument = _textView.TextBuffer.AsTextContainer()?.GetOpenDocumentInCurrentContext();
            if (openDocument is null)
            {
                return null;
            }

            var snippetService = openDocument.GetRequiredLanguageService<ISnippetService>();
            var snippetProvider = snippetService.GetSnippetProvider(selectedSnippet);
            var (document, invokePosition) = await GetDocumentWithoutInvokingTextAsync(openDocument, position, cancellationToken).ConfigureAwait(false);
            var snippet = await snippetProvider.GetSnippetAsync(document, invokePosition, cancellationToken).ConfigureAwait(false);
            var strippedText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

            var allChangesText = strippedText.WithChanges(snippet.TextChanges);

            // This retrieves ALL text changes from the original document which includes the TextChanges from the snippet
            // as well as the clean up.
            var allChangesDocument = document.WithText(allChangesText);
            var allTextChanges = await allChangesDocument.GetTextChangesAsync(document, cancellationToken).ConfigureAwait(false);

            var change = Completion.Utilities.Collapse(allChangesText, allTextChanges.AsImmutable());
            return (change, snippet);
        }

        private static async Task<(Document, int)> GetDocumentWithoutInvokingTextAsync(Document document, int position, CancellationToken cancellationToken)
        {
            var originalText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

            // Uses the existing CompletionService logic to find the TextSpan we want to use for the document sans invoking text
            var completionService = document.GetRequiredLanguageService<CompletionService>();
            var span = completionService.GetDefaultCompletionListSpan(originalText, position);

            var textChange = new TextChange(span, string.Empty);
            originalText = originalText.WithChanges(textChange);
            var newDocument = document.WithText(originalText);
            return (newDocument, span.Start);
        }

        private class Snippet
        {
            public readonly string Name;
            public readonly string InsertionText;
            public readonly IReadOnlyList<Field> Fields;

            public Snippet(string name, TextChange textChange, SnippetChange snippet)
            {
                this.Name = name;
                var textChangeStart = textChange.Span.Start;
                var insertionText = textChange.NewText;

                var fields = new List<Field>();
                var dictionary = new Dictionary<int, (string name, int id)>();
                var placeholders = snippet.Placeholders;

                PopulateMapOfSpanStartsToLSPStringItem(dictionary, placeholders, textChangeStart);

                for (var i = 0; i < insertionText.Length; i++)
                {
                    if (dictionary.TryGetValue(i, out var placeholderInfo))
                    {
                        fields.Add(new Field(Span.FromBounds(i, i + placeholderInfo.name.Length), placeholderInfo.id));
                    }
                }

                this.Fields = fields;
                this.InsertionText = insertionText;
            }

            private static void PopulateMapOfSpanStartsToLSPStringItem(Dictionary<int, (string name, int id)> dictionary, ImmutableArray<SnippetPlaceholder> placeholders, int textChangeStart)
            {
                for (var i = 0; i < placeholders.Length; i++)
                {
                    var placeholder = placeholders[i];
                    foreach (var position in placeholder.PlaceHolderPositions)
                    {
                        dictionary.Add(position - textChangeStart, (placeholder.Identifier, i));
                    }
                }
            }
        }
    }
}
