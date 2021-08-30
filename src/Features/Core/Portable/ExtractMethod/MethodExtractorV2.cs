// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.ExtractMethod
{
    internal abstract class MethodExtractorV2
    {
        protected readonly SelectionResult OriginalSelectionResult;
        protected readonly bool LocalFunction;

        public MethodExtractorV2(SelectionResult selectionResult, bool localFunction)
        {
            Contract.ThrowIfNull(selectionResult);
            OriginalSelectionResult = selectionResult;
            LocalFunction = localFunction;
        }
        protected abstract Task<InsertionPoint> GetInsertionPointAsync(SemanticDocument document, CancellationToken cancellationToken);

        public async Task<ExtractMethodResult> ExtractMethodAsync(CancellationToken cancellationToken)
        {
            var semanticDocument = OriginalSelectionResult.SemanticDocument;
            var insertionPoint = await GetInsertionPointAsync(semanticDocument, cancellationToken).ConfigureAwait(false);

        }
    }
}
