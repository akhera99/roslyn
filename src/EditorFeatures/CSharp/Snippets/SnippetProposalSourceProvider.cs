// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Editor.Shared.Utilities;
using Microsoft.CodeAnalysis.Host.Mef;
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
        private readonly SnippetProposalSourceProvider _factory;
        private static readonly IReadOnlyList<Snippet> _snippets = new Snippet[]
            {
                new Snippet("ctor", " \u2192 public Data($$)\r\n                {\r\n                }\r\n"),
                new Snippet("for", " \u2192 for (int $i$ = 0; ($i$ < $length$); ++$i$)\r\n                {\r\n                }\r\n"),
                new Snippet("foreach", " \u2192 foreach (var $item$ in $collection$)\r\n                {\r\n                }\r\n"),
                new Snippet("forr", " \u2192 for (int $i$ = $length$-1; ($i$ >= 0); --$i$)\r\n                {\r\n                }\r\n"),
                new Snippet("prop", " \u2192 public $int$ $MyProperty$ { get; set; }\r\n"),
            };

        internal static SnippetProposalSource GetOrCreate(ITextView view, SnippetProposalSourceProvider factory)
        {
            return view.Properties.GetOrCreateSingletonProperty(typeof(SnippetProposalSource), () => new SnippetProposalSource(view, factory));
        }

        private SnippetProposalSource(ITextView view, SnippetProposalSourceProvider factory)
        {
            _factory = factory;
        }

        public override Task<ProposalCollectionBase?> RequestProposalsAsync(VirtualSnapshotPoint caret, CompletionState? completionState, ProposalScenario scenario, char triggeringCharacter, CancellationToken cancel)
        {
            /*if (scenario == ProposalScenario.CaretMove)
            {
                if (!caret.IsInVirtualSpace)
                {
                    var line = caret.Position.GetContainingLine();
                    var proposals = new List<ProposalBase>(1);
                    foreach (var s in _snippets)
                    {
                        if (IsMatch(caret.Position, s.Name, line.Start.Position))
                        {
                            var proposal = Proposal.TryCreateProposal(description: $"Insert {s.Name} snippet",
                                                                      new[] { new ProposedEdit(new SnapshotSpan(caret.Position, 0), s.InsertionText, s.Fields) },
                                                                      caret,
                                                                      completionState,
                                                                      ProposalFlags.SingleTabToAccept,
                                                                      commitAction: () => false);
                            if (proposal != null)
                                proposals.Add(proposal);
                            break;
                        }
                    }

                    return Task.FromResult<ProposalCollectionBase?>(new ProposalCollection(nameof(SnippetProposalSourceProvider), proposals));
                }
            }*/
            if ((completionState != null) && completionState.IsSnippet)
            {
                var proposals = new List<ProposalBase>(1);
                foreach (var s in _snippets)
                {
                    if (completionState.SelectedItem == s.Name)
                    {
                        var proposal = Proposal.TryCreateProposal(description: $"Insert {s.Name} snippet",
                                                                  new[] { new ProposedEdit(new SnapshotSpan(caret.Position, 0), s.InsertionText, s.Fields) },
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

                return Task.FromResult<ProposalCollectionBase?>(new ProposalCollection(nameof(SnippetProposalSourceProvider), proposals));
            }

            return Task.FromResult<ProposalCollectionBase?>(null);
        }

        /*private static bool IsMatch(SnapshotPoint caret, string snippet, int lineStart)
        {
            if (caret.Position - lineStart > snippet.Length)
            {
                for (var i = 0; (i < snippet.Length); ++i)
                {
                    if (caret.Snapshot[caret.Position - snippet.Length + i] != snippet[i])
                        return false;
                }

                return true;
            }

            return false;
        }*/

        private class Snippet
        {
            public readonly string Name;
            public readonly string InsertionText;
            public readonly IReadOnlyList<Field> Fields;

            public Snippet(string name, string insertionText)
            {
                this.Name = name;

                if (insertionText.Any(c => c == '$'))
                {
                    var ids = new List<string>();
                    var fields = new List<Field>();

                    var builder = new StringBuilder(insertionText.Length);
                    var start = -1;

                    for (var i = 0; (i < insertionText.Length); ++i)
                    {
                        if (insertionText[i] == '$')
                        {
                            if (start == -1)
                            {
                                start = builder.Length;
                            }
                            else
                            {
                                var span = Span.FromBounds(start, builder.Length);
                                var id = insertionText.Substring(i - span.Length, span.Length);
                                var index = ids.IndexOf(id);
                                if ((index < 0) || (id.Length == 0))
                                {
                                    index = ids.Count;
                                    ids.Add(id);
                                }

                                fields.Add(new Field(span, index));
                                start = -1;
                            }
                        }
                        else
                        {
                            builder.Append(insertionText[i]);
                        }
                    }

                    this.InsertionText = builder.ToString();
                    this.Fields = fields;
                }
                else
                {
                    this.InsertionText = insertionText;
                    this.Fields = Array.Empty<Field>();
                }
            }
        }
    }
}
