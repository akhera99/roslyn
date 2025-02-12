// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Test.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.CSharp.UnitTests.Snippets
{
    [Trait(Traits.Feature, Traits.Features.Snippets)]
    public sealed class CSharpPropFullSnippetProviderTests : AbstractCSharpSnippetProviderTests
    {
        protected override string SnippetIdentifier => "propfull";

        [Fact]
        public async Task InsertPropFullSnippetInClassTest()
        {
            await VerifySnippetAsync("""
            class Program
            {
                $$
            }
            """, """
            class Program
            {
                public int {|0:MyProperty|}
                {
            	    get { return myVar; }
            	    set { myVar = value; }
                }$$
                private int myVar;
            }
            """);
        }
    }
}
