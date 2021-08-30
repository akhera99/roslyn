// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.ExtractMethod;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.ExtractMethod
{
    internal class CSharpMethodExtractorV2 : MethodExtractorV2
    {

        public CSharpMethodExtractorV2(CSharpSelectionResult result, bool localFunction)
            : base(result, localFunction)
        {
        }

        protected override async Task<InsertionPoint> GetInsertionPointAsync(SemanticDocument document, CancellationToken cancellationToken)
        {
            var originalSpanStart = OriginalSelectionResult.OriginalSpan.Start;
            Contract.ThrowIfFalse(originalSpanStart >= 0);

            var root = await document.Document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var basePosition = root.FindToken(originalSpanStart);

            if (LocalFunction)
            {
                // If we are extracting a local function and are within a local function, then we want the new function to be created within the
                // existing local function instead of the overarching method.
                var localFunctionNode = basePosition.GetAncestor<LocalFunctionStatementSyntax>(node => (node.Body != null && node.Body.Span.Contains(OriginalSelectionResult.OriginalSpan)) ||
                                                                                                       (node.ExpressionBody != null && node.ExpressionBody.Span.Contains(OriginalSelectionResult.OriginalSpan)));
                if (localFunctionNode is object)
                {
                    return await InsertionPoint.CreateAsync(document, localFunctionNode, cancellationToken).ConfigureAwait(false);
                }
            }

            var memberNode = basePosition.GetAncestor<MemberDeclarationSyntax>();
            Contract.ThrowIfNull(memberNode);
            Contract.ThrowIfTrue(memberNode.Kind() == SyntaxKind.NamespaceDeclaration);

            if (LocalFunction && memberNode is BasePropertyDeclarationSyntax propertyDeclaration)
            {
                var accessorNode = basePosition.GetAncestor<AccessorDeclarationSyntax>();
                if (accessorNode is object)
                {
                    return await InsertionPoint.CreateAsync(document, accessorNode, cancellationToken).ConfigureAwait(false);
                }
            }

            if (memberNode is GlobalStatementSyntax globalStatement)
            {
                // check whether we are extracting whole global statement out
                if (OriginalSelectionResult.FinalSpan.Contains(memberNode.Span))
                {
                    return await InsertionPoint.CreateAsync(document, globalStatement.Parent, cancellationToken).ConfigureAwait(false);
                }

                // check whether the global statement is a statement container
                if (!globalStatement.Statement.IsStatementContainerNode() && !root.SyntaxTree.IsScript())
                {
                    // The extracted function will be a new global statement
                    return await InsertionPoint.CreateAsync(document, globalStatement.Parent, cancellationToken).ConfigureAwait(false);
                }

                return await InsertionPoint.CreateAsync(document, globalStatement.Statement, cancellationToken).ConfigureAwait(false);
            }

            return await InsertionPoint.CreateAsync(document, memberNode, cancellationToken).ConfigureAwait(false);
        }
    }
}
