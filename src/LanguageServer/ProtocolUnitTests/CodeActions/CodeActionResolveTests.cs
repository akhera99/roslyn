﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Roslyn.LanguageServer.Protocol;
using Roslyn.Test.Utilities;
using Xunit;
using Xunit.Abstractions;
using LSP = Roslyn.LanguageServer.Protocol;

namespace Microsoft.CodeAnalysis.LanguageServer.UnitTests.CodeActions;

public sealed class CodeActionResolveTests : AbstractLanguageServerProtocolTests
{
    public CodeActionResolveTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
    }

    [Theory, CombinatorialData]
    public async Task TestCodeActionResolveHandlerAsync(bool mutatingLspWorkspace)
    {
        var initialMarkup =
            """
            class A
            {
                void M()
                {
                    {|caret:|}int i = 1;
                }
            }
            """;
        await using var testLspServer = await CreateTestLspServerAsync(initialMarkup, mutatingLspWorkspace);
        var titlePath = new string[] { CSharpAnalyzersResources.Use_implicit_type };
        var unresolvedCodeAction = CodeActionsTests.CreateCodeAction(
            title: CSharpAnalyzersResources.Use_implicit_type,
            kind: CodeActionKind.Refactor,
            children: [],
            data: CreateCodeActionResolveData(CSharpAnalyzersResources.Use_implicit_type, testLspServer.GetLocations("caret").Single(), titlePath),
            priority: VSInternalPriorityLevel.Low,
            groupName: "Roslyn1",
            applicableRange: new LSP.Range { Start = new Position { Line = 4, Character = 8 }, End = new Position { Line = 4, Character = 11 } },
            diagnostics: null);

        // Expected text after edit:
        //     class A
        //     {
        //         void M()
        //         {
        //             var i = 1;
        //         }
        //     }
        var expectedTextEdits = new SumType<TextEdit, AnnotatedTextEdit>[]
        {
            GenerateTextEdit("var", new LSP.Range { Start = new Position(4, 8), End = new Position(4, 11) })
        };

        var expectedResolvedAction = CodeActionsTests.CreateCodeAction(
            title: CSharpAnalyzersResources.Use_implicit_type,
            kind: CodeActionKind.Refactor,
            children: [],
            data: CreateCodeActionResolveData(CSharpAnalyzersResources.Use_implicit_type, testLspServer.GetLocations("caret").Single(), titlePath),
            priority: VSInternalPriorityLevel.Low,
            groupName: "Roslyn1",
            diagnostics: null,
            applicableRange: new LSP.Range { Start = new Position { Line = 4, Character = 8 }, End = new Position { Line = 4, Character = 11 } },
            edit: GenerateWorkspaceEdit(testLspServer.GetLocations("caret"), expectedTextEdits));

        var actualResolvedAction = await RunGetCodeActionResolveAsync(testLspServer, unresolvedCodeAction);
        AssertJsonEquals(expectedResolvedAction, actualResolvedAction);
    }

    [Theory, CombinatorialData]
    public async Task TestCodeActionResolveHandlerAsync_NestedAction(bool mutatingLspWorkspace)
    {
        var initialMarkup =
            """
            class A
            {
                void M()
                {
                    int {|caret:|}i = 1;
                }
            }
            """;
        await using var testLspServer = await CreateTestLspServerAsync(initialMarkup, mutatingLspWorkspace);
        var titlePath = new string[] { FeaturesResources.Introduce_constant, string.Format(FeaturesResources.Introduce_constant_for_0, "1") };
        var unresolvedCodeAction = CodeActionsTests.CreateCodeAction(
            title: string.Format(FeaturesResources.Introduce_constant_for_0, "1"),
            kind: CodeActionKind.Refactor,
            children: [],
            data: CreateCodeActionResolveData(
                FeaturesResources.Introduce_constant + "|" + string.Format(FeaturesResources.Introduce_constant_for_0, "1"),
                testLspServer.GetLocations("caret").Single(), titlePath),
            priority: VSInternalPriorityLevel.Normal,
            groupName: "Roslyn2",
            applicableRange: new LSP.Range { Start = new Position { Line = 4, Character = 8 }, End = new Position { Line = 4, Character = 11 } },
            diagnostics: null);

        // Expected text after edits:
        //     class A
        //     {
        //         private const int V = 1;
        //
        //         void M()
        //         {
        //             int i = V;
        //         }
        //     }
        var expectedTextEdits = new SumType<TextEdit, AnnotatedTextEdit>[]
        {
            GenerateTextEdit("""
                private const int V = 1;


                """, new LSP.Range { Start = new Position(2, 4), End = new Position(2, 4) }),
            GenerateTextEdit("V", new LSP.Range { Start = new Position(4, 16), End = new Position(4, 17) })
        };

        var expectedResolvedAction = CodeActionsTests.CreateCodeAction(
            title: string.Format(FeaturesResources.Introduce_constant_for_0, "1"),
            kind: CodeActionKind.Refactor,
            children: [],
            data: CreateCodeActionResolveData(
                FeaturesResources.Introduce_constant + "|" + string.Format(FeaturesResources.Introduce_constant_for_0, "1"),
                testLspServer.GetLocations("caret").Single(), titlePath),
            priority: VSInternalPriorityLevel.Normal,
            groupName: "Roslyn2",
            applicableRange: new LSP.Range { Start = new Position { Line = 4, Character = 8 }, End = new Position { Line = 4, Character = 11 } },
            diagnostics: null,
            edit: GenerateWorkspaceEdit(
                testLspServer.GetLocations("caret"), expectedTextEdits));

        var actualResolvedAction = await RunGetCodeActionResolveAsync(testLspServer, unresolvedCodeAction);
        AssertJsonEquals(expectedResolvedAction, actualResolvedAction);
    }

    [Theory, CombinatorialData]
    public async Task TestRename(bool mutatingLspWorkspace)
    {
        var markUp = """

            class {|caret:ABC|}
            {
            }
            """;

        await using var testLspServer = await CreateTestLspServerAsync(markUp, mutatingLspWorkspace, new InitializationOptions
        {
            ClientCapabilities = new ClientCapabilities()
            {
                Workspace = new WorkspaceClientCapabilities
                {
                    WorkspaceEdit = new WorkspaceEditSetting
                    {
                        ResourceOperations = [ResourceOperationKind.Rename]
                    }
                }
            }
        });

        var titlePath = new string[] { string.Format(FeaturesResources.Rename_file_to_0, "ABC.cs") };
        var unresolvedCodeAction = CodeActionsTests.CreateCodeAction(
            title: string.Format(FeaturesResources.Rename_file_to_0, "ABC.cs"),
            kind: CodeActionKind.Refactor,
            children: [],
            data: CreateCodeActionResolveData(
                string.Format(FeaturesResources.Rename_file_to_0, "ABC.cs"),
                testLspServer.GetLocations("caret").Single(), titlePath),
            priority: VSInternalPriorityLevel.Normal,
            groupName: "Roslyn2",
            applicableRange: new LSP.Range { Start = new Position { Line = 0, Character = 6 }, End = new Position { Line = 0, Character = 9 } },
            diagnostics: null);

        var testWorkspace = testLspServer.TestWorkspace;
        var documentBefore = testWorkspace.CurrentSolution.GetDocument(testWorkspace.Documents.Single().Id)!;
        var documentUriBefore = documentBefore.GetUriForRenamedDocument();

        var actualResolvedAction = await RunGetCodeActionResolveAsync(testLspServer, unresolvedCodeAction);

        var documentAfter = testWorkspace.CurrentSolution.GetDocument(testWorkspace.Documents.Single().Id)!;
        var documentUriAfter = documentBefore.WithName("ABC.cs").GetUriForRenamedDocument();

        var expectedCodeAction = CodeActionsTests.CreateCodeAction(
            title: string.Format(FeaturesResources.Rename_file_to_0, "ABC.cs"),
            kind: CodeActionKind.Refactor,
            children: [],
            data: CreateCodeActionResolveData(
                string.Format(FeaturesResources.Rename_file_to_0, "ABC.cs"),
                testLspServer.GetLocations("caret").Single(), titlePath),
            priority: VSInternalPriorityLevel.Normal,
            groupName: "Roslyn2",
            applicableRange: new LSP.Range { Start = new Position { Line = 0, Character = 6 }, End = new Position { Line = 0, Character = 9 } },
            diagnostics: null,
            edit: GenerateRenameFileEdit(new List<(DocumentUri, DocumentUri)> { (documentUriBefore, documentUriAfter) }));

        AssertJsonEquals(expectedCodeAction, actualResolvedAction);
    }

    [Theory, CombinatorialData]
    public async Task TestLinkedDocuments(bool mutatingLspWorkspace)
    {
        var originalMarkup = """
            class C
            {
                public static readonly int {|caret:_value|} = 10;
            }
            """;
        var xmlWorkspace = $"""
            <Workspace>
                <Project Language='C#' CommonReferences='true' AssemblyName='LinkedProj' Name='CSProj.1'>
                    <Document FilePath='C:\C.cs'>{originalMarkup}</Document>
                </Project>
                <Project Language='C#' CommonReferences='true' AssemblyName='LinkedProj' Name='CSProj.2'>
                    <Document IsLinkFile='true' LinkProjectName='CSProj.1' LinkFilePath='C:\C.cs'/>
                </Project>
            </Workspace>
            """;
        await using var testLspServer = await CreateXmlTestLspServerAsync(xmlWorkspace, mutatingLspWorkspace);
        var titlePath = new string[] { string.Format(FeaturesResources.Encapsulate_field_colon_0_and_use_property, "_value") };
        var unresolvedCodeAction = CodeActionsTests.CreateCodeAction(
            title: string.Format(FeaturesResources.Encapsulate_field_colon_0_and_use_property, "_value"),
            kind: CodeActionKind.Refactor,
            children: [],
            data: CreateCodeActionResolveData(
                string.Format(FeaturesResources.Encapsulate_field_colon_0_and_use_property, "_value"),
                testLspServer.GetLocations("caret").Single(), titlePath),
            priority: VSInternalPriorityLevel.Normal,
            groupName: "Roslyn2",
            applicableRange: new LSP.Range { Start = new Position { Line = 2, Character = 33 }, End = new Position { Line = 39, Character = 2 } },
            diagnostics: null);

        var actualResolvedAction = await RunGetCodeActionResolveAsync(testLspServer, unresolvedCodeAction);

        AssertEx.NotNull(actualResolvedAction.Edit);
        var textDocumentEdit = (LSP.TextDocumentEdit[])actualResolvedAction.Edit.DocumentChanges!.Value;
        Assert.Single(textDocumentEdit);
        var originalText = await testLspServer.GetDocumentTextAsync(textDocumentEdit[0].TextDocument.DocumentUri);
        var edits = textDocumentEdit[0].Edits.Select(e => (LSP.TextEdit)e.Value!).ToArray();
        var updatedText = ApplyTextEdits(edits, originalText);
        Assert.Equal("""
            class C
            {
                private static readonly int value = 10;

                public static int Value => value;
            }
            """, updatedText);

    }

    [Theory, CombinatorialData]
    public async Task TestMoveTypeToDifferentFile(bool mutatingLspWorkspace)
    {
        var markUp = """

            class {|caret:ABC|}
            {
            }
            class BCD 
            {
            }
            """;

        await using var testLspServer = await CreateTestLspServerAsync(markUp, mutatingLspWorkspace, new InitializationOptions
        {
            ClientCapabilities = new ClientCapabilities()
            {
                Workspace = new WorkspaceClientCapabilities
                {
                    WorkspaceEdit = new WorkspaceEditSetting
                    {
                        ResourceOperations = [ResourceOperationKind.Create]
                    }
                }
            }
        });

        var titlePath = new string[] { string.Format(FeaturesResources.Move_type_to_0, "ABC.cs") };
        var unresolvedCodeAction = CodeActionsTests.CreateCodeAction(
            title: string.Format(FeaturesResources.Move_type_to_0, "ABC.cs"),
            kind: CodeActionKind.Refactor,
            children: [],
            data: CreateCodeActionResolveData(
                string.Format(FeaturesResources.Move_type_to_0, "ABC.cs"),
                testLspServer.GetLocations("caret").Single(), titlePath),
            priority: VSInternalPriorityLevel.Normal,
            groupName: "Roslyn2",
            applicableRange: new LSP.Range { Start = new Position { Line = 0, Character = 6 }, End = new Position { Line = 0, Character = 9 } },
            diagnostics: null);

        var testWorkspace = testLspServer.TestWorkspace;
        var actualResolvedAction = await RunGetCodeActionResolveAsync(testLspServer, unresolvedCodeAction);

        var project = testWorkspace.CurrentSolution.Projects.Single();
        var newDocumentUri = ProtocolConversions.CreateAbsoluteDocumentUri(Path.Combine(Path.GetDirectoryName(project.FilePath)!, "ABC.cs"));
        var existingDocumentUri = testWorkspace.CurrentSolution.GetRequiredDocument(testWorkspace.Documents.Single().Id).GetURI();
        var workspaceEdit = new WorkspaceEdit()
        {
            DocumentChanges = new SumType<TextDocumentEdit, CreateFile, RenameFile, DeleteFile>[]
            {
                // Create file
                new CreateFile() { DocumentUri = newDocumentUri },
                // Add content to file
                new TextDocumentEdit()
                {
                    TextDocument = new OptionalVersionedTextDocumentIdentifier { DocumentUri = newDocumentUri },
                    Edits =
                    [
                        new TextEdit()
                        {
                            Range = new LSP.Range
                            {
                                Start = new Position()
                                {
                                    Line = 0,
                                    Character = 0,
                                },
                                End = new Position()
                                {
                                    Line = 0,
                                    Character = 0
                                }
                            },
                            NewText = """
                            class ABC
                            {
                            }

                            """
                        }
                    ]
                },
                // Remove the declaration from existing file
                new TextDocumentEdit()
                {
                    TextDocument = new OptionalVersionedTextDocumentIdentifier() { DocumentUri = existingDocumentUri },
                    Edits =
                    [
                        new TextEdit()
                        {
                            Range = new LSP.Range
                            {
                                Start = new Position()
                                {
                                    Line = 0,
                                    Character = 0,
                                },
                                End = new Position()
                                {
                                    Line = 4,
                                    Character = 0
                                }
                            },
                            NewText = ""
                        }
                    ]
                }
            }
        };

        var expectedCodeAction = CodeActionsTests.CreateCodeAction(
            title: string.Format(FeaturesResources.Move_type_to_0, "ABC.cs"),
            kind: CodeActionKind.Refactor,
            children: [],
            data: CreateCodeActionResolveData(
                string.Format(FeaturesResources.Move_type_to_0, "ABC.cs"),
                testLspServer.GetLocations("caret").Single(), titlePath),
            priority: VSInternalPriorityLevel.Normal,
            groupName: "Roslyn2",
            applicableRange: new LSP.Range { Start = new Position { Line = 0, Character = 6 }, End = new Position { Line = 0, Character = 9 } },
            diagnostics: null,
            edit: workspaceEdit);

        AssertJsonEquals(expectedCodeAction, actualResolvedAction);
    }

    [Theory, CombinatorialData]
    public async Task TestMoveTypeToDifferentFileInDirectory(bool mutatingLspWorkspace)
    {
        var markup =
            """
            class ABC
            {
            }
            class {|caret:BCD|} 
            {
            }
            """;

        await using var testLspServer = await CreateTestLspServerAsync(markup, mutatingLspWorkspace, new InitializationOptions
        {
            ClientCapabilities = new ClientCapabilities()
            {
                Workspace = new WorkspaceClientCapabilities
                {
                    WorkspaceEdit = new WorkspaceEditSetting
                    {
                        ResourceOperations = [ResourceOperationKind.Create]
                    }
                }
            },

            DocumentFileContainingFolders = [Path.Combine("dir1", "dir2", "dir3")],
        });

        var titlePath = new string[] { string.Format(FeaturesResources.Move_type_to_0, "BCD.cs") };
        var unresolvedCodeAction = CodeActionsTests.CreateCodeAction(
            title: string.Format(FeaturesResources.Move_type_to_0, "BCD.cs"),
            kind: CodeActionKind.Refactor,
            children: [],
            data: CreateCodeActionResolveData(
                string.Format(FeaturesResources.Move_type_to_0, "BCD.cs"),
                testLspServer.GetLocations("caret").Single(), titlePath),
            priority: VSInternalPriorityLevel.Normal,
            groupName: "Roslyn2",
            applicableRange: new LSP.Range { Start = new Position { Line = 3, Character = 6 }, End = new Position { Line = 3, Character = 9 } },
            diagnostics: null);

        var testWorkspace = testLspServer.TestWorkspace;
        var actualResolvedAction = await RunGetCodeActionResolveAsync(testLspServer, unresolvedCodeAction);

        var existingDocument = testWorkspace.CurrentSolution.GetRequiredDocument(testWorkspace.Documents.Single().Id);
        var existingDocumentUri = existingDocument.GetURI();

        Assert.Contains(Path.Combine("dir1", "dir2", "dir3"), existingDocument.FilePath);
        var newDocumentUri = ProtocolConversions.CreateAbsoluteDocumentUri(
            Path.Combine(Path.GetDirectoryName(existingDocument.FilePath)!, "BCD.cs"));
        var workspaceEdit = new WorkspaceEdit()
        {
            DocumentChanges = new SumType<TextDocumentEdit, CreateFile, RenameFile, DeleteFile>[]
            {
                // Create file
                new CreateFile() { DocumentUri = newDocumentUri },
                // Add content to file
                new TextDocumentEdit()
                {
                    TextDocument = new OptionalVersionedTextDocumentIdentifier { DocumentUri = newDocumentUri },
                    Edits =
                    [
                        new TextEdit()
                        {
                            Range = new LSP.Range
                            {
                                Start = new Position()
                                {
                                    Line = 0,
                                    Character = 0,
                                },
                                End = new Position()
                                {
                                    Line = 0,
                                    Character = 0
                                }
                            },
                            NewText = """
                            class BCD
                            {
                            }
                            """
                        }
                    ]
                },
                // Remove the declaration from existing file
                new TextDocumentEdit()
                {
                    TextDocument = new OptionalVersionedTextDocumentIdentifier() { DocumentUri = existingDocumentUri },
                    Edits =
                    [
                        new TextEdit()
                        {
                            Range = new LSP.Range
                            {
                                Start = new Position()
                                {
                                    Line = 3,
                                    Character = 0,
                                },
                                End = new Position()
                                {
                                    Line = 5,
                                    Character = 1
                                }
                            },
                            NewText = ""
                        }
                    ]
                }
            }
        };

        var expectedCodeAction = CodeActionsTests.CreateCodeAction(
            title: string.Format(FeaturesResources.Move_type_to_0, "BCD.cs"),
            kind: CodeActionKind.Refactor,
            children: [],
            data: CreateCodeActionResolveData(
                string.Format(FeaturesResources.Move_type_to_0, "BCD.cs"),
                testLspServer.GetLocations("caret").Single(), titlePath),
            priority: VSInternalPriorityLevel.Normal,
            groupName: "Roslyn2",
            applicableRange: new LSP.Range { Start = new Position { Line = 3, Character = 6 }, End = new Position { Line = 3, Character = 9 } },
            diagnostics: null,
            edit: workspaceEdit);

        AssertJsonEquals(expectedCodeAction, actualResolvedAction);
    }

    private static async Task<LSP.VSInternalCodeAction> RunGetCodeActionResolveAsync(
        TestLspServer testLspServer,
        VSInternalCodeAction unresolvedCodeAction)
    {
        var result = (VSInternalCodeAction?)await testLspServer.ExecuteRequestAsync<LSP.CodeAction, LSP.CodeAction>(
            LSP.Methods.CodeActionResolveName, unresolvedCodeAction, CancellationToken.None);
        return result!;
    }

    private static LSP.TextEdit GenerateTextEdit(string newText, LSP.Range range)
        => new LSP.TextEdit
        {
            NewText = newText,
            Range = range
        };

    private static WorkspaceEdit GenerateWorkspaceEdit(
        IList<LSP.Location> locations,
        SumType<TextEdit, AnnotatedTextEdit>[] edits)
        => new LSP.WorkspaceEdit
        {
            DocumentChanges = new TextDocumentEdit[]
            {
                new TextDocumentEdit
                {
                    TextDocument = new OptionalVersionedTextDocumentIdentifier
                    {
                        DocumentUri = locations.Single().DocumentUri
                    },
                    Edits = edits,
                }
            }
        };

    private static WorkspaceEdit GenerateRenameFileEdit(IList<(DocumentUri oldUri, DocumentUri newUri)> renameLocations)
        => new()
        {
            DocumentChanges = renameLocations.Select(
                locations => new SumType<TextDocumentEdit, CreateFile, RenameFile, DeleteFile>(new RenameFile() { OldDocumentUri = locations.oldUri, NewDocumentUri = locations.newUri })).ToArray()
        };
}
