// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;
using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Test.Utilities;
using Microsoft.VisualStudio.Shell.TableControl;
using Roslyn.VisualStudio.IntegrationTests;
using WindowsInput.Native;
using Xunit;
using Microsoft.VisualStudio.Shell.TableManager;

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
        public async Task CallHierarchyToCtor()
        {
            await SetUpEditorAsync(@"
class Program
{
}$$
", HangMitigatingCancellationToken);
            await TestServices.SolutionExplorer.AddFileAsync(ProjectName, "File2.cs", cancellationToken: HangMitigatingCancellationToken);
            await TestServices.SolutionExplorer.OpenFileAsync(ProjectName, "File2.cs", HangMitigatingCancellationToken);

            await SetUpEditorAsync(@"
class SomeOtherClass
{
    void M()
    {
        Program p = new Progr$$am();
    }
}
", HangMitigatingCancellationToken);

            await TestServices.Input.SendAsync((VirtualKeyCode.CONTROL, VirtualKeyCode.VK_K), HangMitigatingCancellationToken);
            await TestServices.Input.SendAsync((VirtualKeyCode.CONTROL, VirtualKeyCode.VK_T), HangMitigatingCancellationToken);

            var results = await TestServices.CallHierarchyWindow.GetContentsAsync(HangMitigatingCancellationToken);

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
        }
    }
}
