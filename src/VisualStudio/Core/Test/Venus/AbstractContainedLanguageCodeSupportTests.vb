﻿' Licensed to the .NET Foundation under one or more agreements.
' The .NET Foundation licenses this file to you under the MIT license.
' See the LICENSE file in the project root for more information.

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Test.Utilities
Imports Microsoft.VisualStudio.LanguageServices.Implementation.Venus

Namespace Microsoft.VisualStudio.LanguageServices.UnitTests.Venus

    <[UseExportProvider]>
    Public MustInherit Class AbstractContainedLanguageCodeSupportTests

        Protected MustOverride ReadOnly Property Language As String
        Protected MustOverride ReadOnly Property DefaultCode As String

        Protected Sub AssertValidId(id As String)
            AssertValidId(id, Sub(value) Assert.True(value))
        End Sub

        Protected Sub AssertNotValidId(id As String)
            AssertValidId(id, Sub(value) Assert.False(value))
        End Sub

#Disable Warning CA1822 ' Mark members as static - False positive due to https://github.com/dotnet/roslyn/issues/50582
        Private Sub AssertValidId(id As String, assertion As Action(Of Boolean))
#Enable Warning CA1822
            Using workspace = EditorTestWorkspace.Create(
<Workspace>
    <Project Language=<%= Language %> AssemblyName="Assembly" CommonReferences="true">
        <Document>
            <%= DefaultCode %>
        </Document>
    </Project>
</Workspace>)
                Dim document = workspace.CurrentSolution.Projects.Single().Documents.Single()
                assertion(ContainedLanguageCodeSupport.IsValidId(document, id))
            End Using

        End Sub

        Protected Function GetWorkspace(code As String) As EditorTestWorkspace
            Return EditorTestWorkspace.Create(
<Workspace>
    <Project Language=<%= Language %> AssemblyName="Assembly" CommonReferences="true">
        <Document FilePath="file">
            <%= code.Replace(vbCrLf, vbLf) %>
        </Document>
    </Project>
</Workspace>, composition:=VisualStudioTestCompositions.LanguageServices)
        End Function

        Protected Function GetDocument(workspace As EditorTestWorkspace) As Document
            Return workspace.CurrentSolution.Projects.Single().Documents.Single()
        End Function
    End Class
End Namespace
