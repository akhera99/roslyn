// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editor.Shared.Options;
using Microsoft.CodeAnalysis.InlineHints;
using Microsoft.CodeAnalysis.Test.Utilities;
using Microsoft.VisualStudio.IntegrationTest.Utilities;
using Microsoft.VisualStudio.IntegrationTest.Utilities.OutOfProcess;
using Roslyn.Test.Utilities;
using Xunit;

namespace Roslyn.VisualStudio.IntegrationTests.CSharp
{
    [Collection(nameof(SharedIntegrationHostFixture))]
    public class CSharpInlineHints : AbstractEditorTest
    {
        protected override string LanguageName => LanguageNames.CSharp;
        private InlineHints_OutOfProc InlineHints => VisualStudio.InlineHints;

        public CSharpInlineHints(VisualStudioInstanceFactory instanceFactory)
            : base(instanceFactory, nameof(CSharpInlineHints))
        {
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.InlineHints)]
        public void QuickInfo_Inline_Hint_Simple_Case()
        {
            SetUpEditor(@"
using System;
public class Program
{
    public int Method(int a, int b)
    {
        int result = a * b;
        return result;
    }

    public void CallingMethod()
    {
        _ = Method($$5, 10);
    }
}");
            VisualStudio.Workspace.SetFeatureOption(InlineHintsOptions.EnabledForParameters.Feature, InlineHintsOptions.EnabledForParameters.Name, LanguageName, "True");
            VisualStudio.Editor.InvokeQuickInfo();
            Assert.Equal("class Program\r\nHello!", VisualStudio.Editor.GetQuickInfo());
        }
    }
}
