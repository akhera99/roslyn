' Licensed to the .NET Foundation under one or more agreements.
' The .NET Foundation licenses this file to you under the MIT license.
' See the LICENSE file in the project root for more information.

Imports System.Composition
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Host.Mef
Imports Microsoft.CodeAnalysis.Snippets
Imports Microsoft.CodeAnalysis.Snippets.SnippetProviders
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Namespace Microsoft.CodeAnalysis.VisualBasic.Snippets
    <ExportSnippetProvider(NameOf(ISnippetProvider), LanguageNames.VisualBasic), [Shared]>
    Friend Class VisualBasicIfSnippetProvider
        Inherits AbstractIfSnippetProvider

        <ImportingConstructor>
        <Obsolete(MefConstruction.ImportingConstructorMessage, True)>
        Public Sub New()
        End Sub

        Protected Overrides Sub GetIfStatementConditionAndCursorPosition(node As SyntaxNode, ByRef condition As SyntaxNode, ByRef cursorPosition As Integer)
            Dim ifStatement = DirectCast(node, IfStatementSyntax)
            condition = ifStatement.Condition
            cursorPosition = ifStatement.ThenKeyword.Span.End + 1
        End Sub
    End Class
End Namespace
