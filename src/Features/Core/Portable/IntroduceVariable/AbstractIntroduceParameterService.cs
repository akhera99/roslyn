// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable disable

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Internal.Log;
using Microsoft.CodeAnalysis.LanguageServices;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;
using static Microsoft.CodeAnalysis.CodeActions.CodeAction;

namespace Microsoft.CodeAnalysis.IntroduceVariable
{
    internal abstract partial class AbstractIntroduceParameterProvider<TService, TExpressionSyntax> : IIntroduceParameterService
        where TService : AbstractIntroduceParameterProvider<TService, TExpressionSyntax>
        where TExpressionSyntax : SyntaxNode
    {

        public async Task<CodeAction> IntroduceParameterAsync(
             Document document,
             TextSpan textSpan,
             CancellationToken cancellationToken)
        {
            using (Logger.LogBlock(FunctionId.Refactoring_IntroduceVariable, cancellationToken))
            {
                var semanticDocument = await SemanticDocument.CreateAsync(document, cancellationToken).ConfigureAwait(false);

                var state = await State.GenerateAsync((TService)this, semanticDocument, textSpan, cancellationToken).ConfigureAwait(false);
                if (state != null)
                {
                    var (title, actions) = CreateActions(state, cancellationToken);
                    if (actions.Length > 0)
                    {
                        // We may end up creating a lot of viable code actions for the selected
                        // piece of code.  Create a top level code action so that we don't overwhelm
                        // the light bulb if there are a lot of other options in the list.  Set 
                        // the code action as 'inlinable' so that if the lightbulb is not cluttered
                        // then the nested items can just be lifted into it, giving the user fast
                        // access to them.
                        return new CodeActionWithNestedActions(title, actions, isInlinable: true);
                    }
                }

                return null;
            }
        }
    }
}
