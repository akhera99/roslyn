' Licensed to the .NET Foundation under one or more agreements.
' The .NET Foundation licenses this file to you under the MIT license.
' See the LICENSE file in the project root for more information.

Imports Microsoft.CodeAnalysis.VisualBasic.Completion.CompletionProviders.Snippets

Namespace Microsoft.CodeAnalysis.Editor.VisualBasic.UnitTests.Completion.CompletionProviders.Snippets
    Public Class VisualBasicConsoleSnippetCompletionProviderTests
        Inherits AbstractVisualBasicSnippetCompletionProviderTests

        Protected Overrides ReadOnly Property ItemToCommit As String
            Get
                Return FeaturesResources.Write_to_the_console
            End Get
        End Property

        <WpfFact, Trait(Traits.Feature, Traits.Features.Completion)>
        Public Async Function InsertConsoleSnippetInMethodTest() As Task
            Dim markupBeforeCommit =
$"Public Class TestClass
    Public Sub TestMethod()
        $$
    End Sub
End Class"

            Dim markupAfterCommit =
$"Imports System

Public Class TestClass
    Public Sub TestMethod()
        Global.System.Console.WriteLine($$)
    End Sub
End Class"

            Await VerifyCustomCommitProviderAsync(markupBeforeCommit, ItemToCommit, markupAfterCommit)
        End Function

        <WpfFact, Trait(Traits.Feature, Traits.Features.Completion)>
        Public Async Function InsertAsyncConsoleSnippetTest() As Task
            Dim markupBeforeCommit =
$"Public Class TestClass
    Public Async Sub TestMethod()
        $$
    End Sub
End Class"

            Dim markupAfterCommit =
$"Imports System

Public Class TestClass
    Public Async Sub TestMethod()
        Await Global.System.Console.Out.WriteLineAsync($$)
    End Sub
End Class"

            Await VerifyCustomCommitProviderAsync(markupBeforeCommit, ItemToCommit, markupAfterCommit)
        End Function

        <WpfFact, Trait(Traits.Feature, Traits.Features.Completion)>
        Public Async Function NoConsoleSnippetGlobalTest() As Task
            Dim markupBeforeCommit =
$"$$
Public Class TestClass
    Public Sub TestMethod()
    End Sub
End Class"

            Await VerifyItemIsAbsentAsync(markupBeforeCommit, ItemToCommit)
        End Function

        <WpfFact, Trait(Traits.Feature, Traits.Features.Completion)>
        Public Async Function NoConsoleSnippetInBlockNamespaceTest() As Task
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
        Public Async Function InsertConsoleSnippetInConstructorTest() As Task
            Dim markupBeforeCommit =
$"Public Class TestClass
    Public Sub New()
        $$
    End Sub
End Class"

            Dim markupAfterCommit =
$"Imports System

Public Class TestClass
    Public Sub New()
        Global.System.Console.WriteLine($$)
    End Sub
End Class"

            Await VerifyCustomCommitProviderAsync(markupBeforeCommit, ItemToCommit, markupAfterCommit)
        End Function

        <WpfFact, Trait(Traits.Feature, Traits.Features.Completion)>
        Public Async Function InsertConsoleSnippetInLambdaTest() As Task
            Dim markupBeforeCommit =
$"Public Class TestClass
    Public Sub TestMethod()
            Dim x = Sub()
                        $$
                    End Sub
    End Sub
End Class"

            Dim markupAfterCommit =
$"Imports System

Public Class TestClass
    Public Sub TestMethod()
            Dim x = Sub()
                        Global.System.Console.WriteLine($$)
                    End Sub
    End Sub
End Class"

            Await VerifyCustomCommitProviderAsync(markupBeforeCommit, ItemToCommit, markupAfterCommit)
        End Function

        <WpfFact, Trait(Traits.Feature, Traits.Features.Completion)>
        Public Async Function NoConsoleSnippetInStringTest() As Task
            Dim markupBeforeCommit =
$"Public Class TestClass
    Public Sub TestMethod()
            Dim x = ""$$""
    End Sub
End Class"

            Await VerifyItemIsAbsentAsync(markupBeforeCommit, ItemToCommit)
        End Function

        <WpfFact, Trait(Traits.Feature, Traits.Features.Completion)>
        Public Async Function NoConsoleSnippetInObjectInitializerTest() As Task
            Dim markupBeforeCommit =
$"Public Class TestClass
    Public Sub TestMethod()
            Dim x = New Object($$)
    End Sub
End Class"

            Await VerifyItemIsAbsentAsync(markupBeforeCommit, ItemToCommit)
        End Function

        <WpfFact, Trait(Traits.Feature, Traits.Features.Completion)>
        Public Async Function NoConsoleSnippetInParameterListTest() As Task
            Dim markupBeforeCommit =
$"Public Class TestClass
    Public Sub TestMethod($$)
    End Sub
End Class"

            Await VerifyItemIsAbsentAsync(markupBeforeCommit, ItemToCommit)
        End Function

        <WpfFact, Trait(Traits.Feature, Traits.Features.Completion)>
        Public Async Function NoConsoleSnippetInVariableDeclarationTest() As Task
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
