// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Test.Utilities;
using Microsoft.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.Shell.TableManager;
using Roslyn.VisualStudio.IntegrationTests;
using WindowsInput.Native;
using Xunit;

namespace Roslyn.VisualStudio.NewIntegrationTests.CSharp
{
    [Trait(Traits.Feature, Traits.Features.CallHierarchy)]
    public class CSharpCallHierarchy : AbstractEditorTest
    {
        public CSharpCallHierarchy()
            : base(nameof(CSharpCallHierarchy))
        {
        }

        protected override string LanguageName => LanguageNames.CSharp;

        [IdeFact]
        public async Task ViewCallHierarchyForMethod()
        {
            await SetUpEditorAsync(@"
class HellWorld
{
}$$
", HangMitigatingCancellationToken);
            await TestServices.SolutionExplorer.AddFileAsync(ProjectName, "File2.cs", cancellationToken: HangMitigatingCancellationToken);
            await TestServices.SolutionExplorer.OpenFileAsync(ProjectName, "File2.cs", HangMitigatingCancellationToken);

            await SetUpEditorAsync(@"
Console.WriteLine(new HelloWorld().GetMessage());

class HelloWorld
{
    public string GetMessage() =>
        ""Hello, World!"";
}
", HangMitigatingCancellationToken);

            await TestServices.Input.SendAsync((VirtualKeyCode.F12, VirtualKeyCode.SHIFT), HangMitigatingCancellationToken);

            var results = await TestServices.Editor.ViewCallHierarchyAsync(HangMitigatingCancellationToken);

            Assert.Collection(
                results,
                new Action<ITableEntryHandle2>[]
                {
                    reference =>
                    {
                        Assert.Equal(expected: "class Program", actual: reference.TryGetValue(StandardTableKeyNames.Text, out string code) ? code : null);
                        Assert.Equal(expected: 1, actual: reference.TryGetValue(StandardTableKeyNames.Line, out int line) ? line : -1);
                        Assert.Equal(expected: 6, actual: reference.TryGetValue(StandardTableKeyNames.Column, out int column) ? column : -1);
                    },
                    reference =>
                    {
                        Assert.Equal(expected: "Program p = new Program();", actual: reference.TryGetValue(StandardTableKeyNames.Text, out string code) ? code : null);
                        Assert.Equal(expected: 5, actual: reference.TryGetValue(StandardTableKeyNames.Line, out int line) ? line : -1);
                        Assert.Equal(expected: 24, actual: reference.TryGetValue(StandardTableKeyNames.Column, out int column) ? column : -1);
                    }
                });


            // Assert we are in the right file now
            Assert.Equal($"Class1.cs", await TestServices.Shell.GetActiveDocumentFileNameAsync(HangMitigatingCancellationToken));
            Assert.Equal("Program", await TestServices.Editor.GetLineTextAfterCaretAsync(HangMitigatingCancellationToken));
        }
        /*
         * Console.WriteLine(new HelloWorld().GetMessage());

class HelloWorld
{
    public string GetMessage() =>
        "Hello, World!";
}
        */


    }
}
