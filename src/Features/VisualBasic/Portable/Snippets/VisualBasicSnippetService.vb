' Licensed to the .NET Foundation under one or more agreements.
' The .NET Foundation licenses this file to you under the MIT license.
' See the LICENSE file in the project root for more information.

Imports System.Composition
Imports Microsoft.CodeAnalysis.Host.Mef
Imports Microsoft.CodeAnalysis.Snippets
Imports Microsoft.CodeAnalysis.Snippets.SnippetProviders

Namespace Microsoft.CodeAnalysis.VisualBasic.Snippets
    <ExportSnippetProvider(NameOf(ISnippetService), LanguageNames.VisualBasic), [Shared]>
    Friend Class VisualBasicSnippetService
        Inherits AbstractSnippetService

        <ImportingConstructor>
        <Obsolete(MefConstruction.ImportingConstructorMessage, True)>
        Public Sub New(<ImportMany()> lazySnippetProviders As IEnumerable(Of Lazy(Of ISnippetProvider, LanguageMetadata)))
            MyBase.New(lazySnippetProviders)
        End Sub
    End Class
End Namespace
