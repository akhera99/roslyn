﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Editor.Copilot;
using Microsoft.CodeAnalysis.Internal.Log;
using Microsoft.CodeAnalysis.Shared.TestHooks;
using Microsoft.VisualStudio.Language.Proposals;
using Microsoft.VisualStudio.Language.Suggestions;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Threading;

namespace Microsoft.CodeAnalysis.DocumentationComments
{
    internal sealed class DocumentationCommentSuggestion(CopilotGenerateDocumentationCommentProvider providerInstance,
        SuggestionManagerBase suggestionManager, VisualStudio.Threading.IAsyncDisposable? intelliCodeLineCompletionsDisposable) : SuggestionBase
    {
        public SuggestionManagerBase SuggestionManager { get; } = suggestionManager;

        public VisualStudio.Threading.IAsyncDisposable? IntelliCodeLineCompletionsDisposable { get; set; } = intelliCodeLineCompletionsDisposable;

        public override TipStyle TipStyle => TipStyle.AlwaysShowTip | CopilotConstants.ShowThinkingStateTipStyle;

        public override EditDisplayStyle EditStyle => EditDisplayStyle.GrayText;

        public override bool HasMultipleSuggestions => false;

        public override event PropertyChangedEventHandler PropertyChanged { add { } remove { } }

        private SuggestionSessionBase? _suggestionSession;

        public override async Task OnAcceptedAsync(SuggestionSessionBase session, ProposalBase originalProposal, ProposalBase currentProposal, ReasonForAccept reason, CancellationToken cancel)
        {
            var threadingContext = providerInstance.ThreadingContext;

            await threadingContext.JoinableTaskFactory.SwitchToMainThreadAsync(cancel);
            await DisposeAsync().ConfigureAwait(false);
            Logger.Log(FunctionId.Copilot_Generate_Documentation_Accepted, logLevel: LogLevel.Information);
        }

        public override Task OnChangeProposalAsync(SuggestionSessionBase session, ProposalBase originalProposal, ProposalBase currentProposal, bool forward, CancellationToken cancel)
        {
            return Task.CompletedTask;
        }

        public override async Task OnDismissedAsync(SuggestionSessionBase session, ProposalBase? originalProposal, ProposalBase? currentProposal, ReasonForDismiss reason, CancellationToken cancel)
        {
            var threadingContext = providerInstance.ThreadingContext;
            await threadingContext.JoinableTaskFactory.SwitchToMainThreadAsync(cancel);
            await ClearSuggestionAsync(reason, cancel).ConfigureAwait(false);
            Logger.Log(FunctionId.Copilot_Generate_Documentation_Dismissed, logLevel: LogLevel.Information);
        }

        public override Task OnProposalUpdatedAsync(SuggestionSessionBase session, ProposalBase? originalProposal, ProposalBase? currentProposal, ReasonForUpdate reason, VirtualSnapshotPoint caret, CompletionState? completionState, CancellationToken cancel)
        {
            if (reason.HasFlag(ReasonForUpdate.Diverged))
            {
                Logger.Log(FunctionId.Copilot_Generate_Documentation_Diverged, logLevel: LogLevel.Information);
                return session.DismissAsync(ReasonForDismiss.DismissedAfterBufferChange, cancel);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Starts the Suggestion Session. The TryDisplaySuggestion call doesn't display any grey text, but starts the session such that we have the
        /// exclusive right to display grey text later.
        /// </summary>
        /// <returns>If true, user will see the thinking state as long as the Suggestion Session is active and replace with grey text if a call to DisplayProposal succeeds.
        /// If unable to retrieve the session, the caller should bail out.
        /// </returns>
        public async Task<bool> StartSuggestionSessionAsync(CancellationToken cancellationToken)
        {
            _suggestionSession = await RunWithEnqueueActionAsync(
                "StartWork",
                async () => await SuggestionManager.TryDisplaySuggestionAsync(this, cancellationToken).ConfigureAwait(false),
            cancellationToken).ConfigureAwait(false);

            if (_suggestionSession is null)
            {
                await DisposeAsync().ConfigureAwait(false);
                return false;
            }

            return true;
        }

        public async Task TryDisplayDocumentationSuggestionAsync(ProposalBase proposal, CancellationToken cancellationToken)
        {
            try
            {
                await RunWithEnqueueActionAsync<bool>(
                    "DisplayProposal",
                    async () =>
                    {
                        await _suggestionSession!.DisplayProposalAsync(proposal, cancellationToken).ConfigureAwait(false);
                        return true;
                    },
                    cancellationToken).ConfigureAwait(false);

                Logger.Log(FunctionId.Copilot_Generate_Documentation_Displayed, logLevel: LogLevel.Information);
            }
            catch (OperationCanceledException)
            {
                Logger.Log(FunctionId.Copilot_Generate_Documentation_Canceled, logLevel: LogLevel.Information);
            }
        }

        /// <summary>
        /// Dismisses the session if the proposal we generated was invalid.
        /// </summary>
        public async Task DismissSuggestionSessionAsync(CancellationToken cancellationToken)
        {
            await DisposeAsync().ConfigureAwait(false);
            await RunWithEnqueueActionAsync<bool>(
                "DismissSuggestionSession",
                async () =>
                {
                    await _suggestionSession!.DismissAsync(ReasonForDismiss.DismissedDueToInvalidProposal, cancellationToken).ConfigureAwait(false);
                    return true;
                },
                cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// In general, calls to a SuggestionManager or SuggestionSession need to be wrapped in an EnqueueAction.
        /// This is the pattern recommended by VS Platform to avoid races.
        /// Pattern from platform shown here:
        /// https://devdiv.visualstudio.com/DevDiv/_git/IntelliCode-VS?path=/src/VSIX/IntelliCode.VSIX/SuggestionService/AmbientAI/SuggestionProviderForAmbientAI.cs
        /// </summary>
        private async Task<T> RunWithEnqueueActionAsync<T>(string description, Func<Task<T>> action, CancellationToken cancellationToken)
        {
            Assumes.NotNull(SuggestionManager);

            var taskCompletionSource = new TaskCompletionSource<T>();

            await providerInstance.ThreadingContext.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            SuggestionManager.EnqueueAction(description, async () =>
            {
                var task = action();
                try
                {
                    var result = await action().ConfigureAwait(false);
                    taskCompletionSource.TrySetResult(result);
                }
                catch (OperationCanceledException operationCanceledException)
                {
                    taskCompletionSource.TrySetCanceled(operationCanceledException.CancellationToken);
                }
                catch (Exception exception)
                {
                    taskCompletionSource.TrySetException(exception);
                }
            });

            return await taskCompletionSource.Task.WithCancellation(cancellationToken).ConfigureAwait(false);
        }

        private async Task ClearSuggestionAsync(ReasonForDismiss reason, CancellationToken cancellationToken)
        {
            if (_suggestionSession != null)
            {
                await _suggestionSession.DismissAsync(reason, cancellationToken).ConfigureAwait(false);
            }

            _suggestionSession = null;
            await DisposeAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// The IntelliCodeLineCompletionDisposable needs to be disposed any time we exit the SuggestionSession so that
        /// line completions can be shown again.
        /// </summary>
        private async Task DisposeAsync()
        {
            if (IntelliCodeLineCompletionsDisposable != null)
            {
                await IntelliCodeLineCompletionsDisposable.DisposeAsync().ConfigureAwait(false);
                IntelliCodeLineCompletionsDisposable = null;
            }
        }
    }
}
