' Licensed to the .NET Foundation under one or more agreements.
' The .NET Foundation licenses this file to you under the MIT license.
' See the LICENSE file in the project root for more information.

Imports System.Composition
Imports Microsoft.CodeAnalysis.Host.Mef
Imports Microsoft.CodeAnalysis.Snippets
Imports Microsoft.CodeAnalysis.Snippets.SnippetProviders

Namespace Microsoft.CodeAnalysis.Snippets

    <ExportSnippetProvider(NameOf(ISnippetProvider), LanguageNames.VisualBasic), [Shared]>
    Friend Class VisualBasicConsoleSnippetProvider
        Inherits AbstractConsoleSnippetProvider

        <ImportingConstructor>
        <Obsolete(MefConstruction.ImportingConstructorMessage, True)>
        Public Sub New()
        End Sub

        Protected Overrides Function GetAsyncSupportingDeclaration(token As SyntaxToken) As SyntaxNode
            Return Nothing
        End Function
    End Class
End Namespace
