' Licensed to the .NET Foundation under one or more agreements.
' The .NET Foundation licenses this file to you under the MIT license.
' See the LICENSE file in the project root for more information.

Namespace Microsoft.CodeAnalysis.Editor.VisualBasic.UnitTests.Completion.CompletionProviders.Snippets
    Public Class VisualBasicIfSnippetCompletionProviderTests
        Inherits AbstractVisualBasicSnippetCompletionProviderTests

        Protected Overrides ReadOnly Property ItemToCommit As String
            Get
                Return FeaturesResources.Insert_an_if_statement
            End Get
        End Property

        <WpfFact, Trait(Traits.Feature, Traits.Features.Completion)>
        Public Async Function InsertIfSnippetInMethodTest() As Task
            Dim markupBeforeCommit =
$"Public Class TestClass
    Public Sub TestMethod()
        $$
    End Sub
End Class"

            Dim markupAfterCommit =
$"Public Class TestClass
    Public Sub TestMethod()
        If True Then$$
        End If
    End Sub
End Class"

            Await VerifyCustomCommitProviderAsync(markupBeforeCommit, ItemToCommit, markupAfterCommit)
        End Function

        <WpfFact, Trait(Traits.Feature, Traits.Features.Completion)>
        Public Async Function NoIfSnippetGlobalTest() As Task
            Dim markupBeforeCommit =
$"$$
Public Class TestClass
    Public Sub TestMethod()
    End Sub
End Class"

            Await VerifyItemIsAbsentAsync(markupBeforeCommit, ItemToCommit)
        End Function

        <WpfFact, Trait(Traits.Feature, Traits.Features.Completion)>
        Public Async Function NoIfSnippetInBlockNamespaceTest() As Task
            Dim markupBeforeCommit =
$"Namespace TestNamespace
    $$
    Public Class TestClass
        Public Sub TestMethod()
        End Sub
    End Class
End Namespace"

            Await VerifyItemIsAbsentAsync(markupBeforeCommit, ItemToCommit)
        End Function

        <WpfFact, Trait(Traits.Feature, Traits.Features.Completion)>
        Public Async Function InsertIfSnippetInConstructorTest() As Task
            Dim markupBeforeCommit =
$"Public Class TestClass
    Public Sub New()
        $$
    End Sub
End Class"

            Dim markupAfterCommit =
$"Public Class TestClass
    Public Sub New()
        If True Then$$
        End If
    End Sub
End Class"

            Await VerifyCustomCommitProviderAsync(markupBeforeCommit, ItemToCommit, markupAfterCommit)
        End Function

        <WpfFact, Trait(Traits.Feature, Traits.Features.Completion)>
        Public Async Function InsertIfSnippetInLambdaTest() As Task
            Dim markupBeforeCommit =
$"Public Class TestClass
    Public Sub TestMethod()
            Dim x = Sub()
                        $$
                    End Sub
    End Sub
End Class"

            Dim markupAfterCommit =
$"Public Class TestClass
    Public Sub TestMethod()
            Dim x = Sub()
                        If True Then$$
                        End If
                    End Sub
    End Sub
End Class"

            Await VerifyCustomCommitProviderAsync(markupBeforeCommit, ItemToCommit, markupAfterCommit)
        End Function

        <WpfFact, Trait(Traits.Feature, Traits.Features.Completion)>
        Public Async Function NoIfSnippetInStringTest() As Task
            Dim markupBeforeCommit =
$"Public Class TestClass
    Public Sub TestMethod()
            Dim x = ""$$""
    End Sub
End Class"

            Await VerifyItemIsAbsentAsync(markupBeforeCommit, ItemToCommit)
        End Function

        <WpfFact, Trait(Traits.Feature, Traits.Features.Completion)>
        Public Async Function NoIfSnippetInObjectInitializerTest() As Task
            Dim markupBeforeCommit =
$"Public Class TestClass
    Public Sub TestMethod()
            Dim x = New Object($$)
    End Sub
End Class"

            Await VerifyItemIsAbsentAsync(markupBeforeCommit, ItemToCommit)
        End Function

        <WpfFact, Trait(Traits.Feature, Traits.Features.Completion)>
        Public Async Function NoIfSnippetInParameterListTest() As Task
            Dim markupBeforeCommit =
$"Public Class TestClass
    Public Sub TestMethod($$)
    End Sub
End Class"

            Await VerifyItemIsAbsentAsync(markupBeforeCommit, ItemToCommit)
        End Function

        <WpfFact, Trait(Traits.Feature, Traits.Features.Completion)>
        Public Async Function NoIfSnippetInVariableDeclarationTest() As Task
            Dim markupBeforeCommit =
$"Public Class TestClass
    Public Sub TestMethod()
            Dim x = $$
    End Sub
End Class"

            Await VerifyItemIsAbsentAsync(markupBeforeCommit, ItemToCommit)
        End Function
    End Class
End Namespace
