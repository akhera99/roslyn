' Licensed to the .NET Foundation under one or more agreements.
' The .NET Foundation licenses this file to you under the MIT license.
' See the LICENSE file in the project root for more information.

Imports Microsoft.CodeAnalysis.Editor.VisualBasic.UnitTests.Completion.CompletionProviders
Imports Microsoft.CodeAnalysis.Snippets
Imports Microsoft.CodeAnalysis.Test.Utilities.Snippets
Imports Microsoft.CodeAnalysis.VisualBasic.Completion.CompletionProviders.Snippets

Public MustInherit Class AbstractVisualBasicSnippetCompletionProviderTests
    Inherits AbstractVisualBasicCompletionProviderTests

    Protected MustOverride ReadOnly Property ItemToCommit() As String

    Protected Overrides Function GetComposition() As TestComposition
        Return MyBase.GetComposition().AddExcludedPartTypes(GetType(IRoslynLSPSnippetExpander)).AddParts(GetType(TestRoslynLanguageServerSnippetExpander))
    End Function

    Friend Overrides Function GetCompletionProviderType() As Type
        Return GetType(VisualBasicSnippetCompletionProvider)
    End Function
End Class
