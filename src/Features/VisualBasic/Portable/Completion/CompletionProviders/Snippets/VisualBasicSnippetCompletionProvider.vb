' Licensed to the .NET Foundation under one or more agreements.
' The .NET Foundation licenses this file to you under the MIT license.
' See the LICENSE file in the project root for more information.

Imports System.Composition
Imports Microsoft.CodeAnalysis.Completion
Imports Microsoft.CodeAnalysis.Completion.Providers.Snippets
Imports Microsoft.CodeAnalysis.Host.Mef
Imports Microsoft.CodeAnalysis.Snippets
Imports Microsoft.CodeAnalysis.VisualBasic.Completion.Providers

Namespace Microsoft.CodeAnalysis.VisualBasic.Completion.CompletionProviders.Snippets
    <ExportCompletionProvider(NameOf(VisualBasicSnippetCompletionProvider), LanguageNames.VisualBasic)>
    <ExtensionOrder(After:=NameOf(ExtensionMethodImportCompletionProvider))>
    <System.Composition.Shared>
    Friend Class VisualBasicSnippetCompletionProvider
        Inherits AbstractSnippetCompletionProvider

        <ImportingConstructor>
        <Obsolete(MefConstruction.ImportingConstructorMessage, True)>
        Public Sub New(roslynLSPSnippetExpander As IRoslynLSPSnippetExpander)
            MyBase.New(roslynLSPSnippetExpander)
        End Sub
    End Class
End Namespace
