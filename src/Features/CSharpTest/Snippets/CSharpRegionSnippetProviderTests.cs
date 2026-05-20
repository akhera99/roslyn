// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Test.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.CSharp.UnitTests.Snippets;

[Trait(Traits.Feature, Traits.Features.Snippets)]
public sealed class CSharpRegionSnippetProviderTests : AbstractCSharpSnippetProviderTests
{
    protected override string SnippetIdentifier => "#region";

    [Fact]
    public Task InsertRegionSnippetInMethodTest()
        => VerifySnippetAsync("""
            class Program
            {
                public void Method()
                {
                    #$$
                }
            }
            """, """
            class Program
            {
                public void Method()
                {
                    #region {|0:MyRegion|}
            $$
                    #endregion
                }
            }
            """);

    [Fact]
    public Task InsertRegionSnippetInGlobalContextTest()
        => VerifySnippetAsync("""
            #$$
            """, """
            #region {|0:MyRegion|}
            $$
            #endregion
            """);

    [Fact]
    public Task InsertRegionSnippetInClassBodyTest()
        => VerifySnippetAsync("""
            class Program
            {
                #$$
            }
            """, """
            class Program
            {
                #region {|0:MyRegion|}
            $$
                #endregion
            }
            """);

    [Fact]
    public Task InsertRegionSnippetInNamespaceTest()
        => VerifySnippetAsync("""
            namespace Namespace
            {
                #$$
            }
            """, """
            namespace Namespace
            {
                #region {|0:MyRegion|}
            $$
                #endregion
            }
            """);

    [Fact]
    public Task InsertRegionSnippetInFileScopedNamespaceTest()
        => VerifySnippetAsync("""
            namespace Namespace;
            #$$
            """, """
            namespace Namespace;
            #region {|0:MyRegion|}
            $$
            #endregion
            """);

    [Fact]
    public Task NoRegionSnippetWithoutHashTest()
        => VerifySnippetIsAbsentAsync("""
            class Program
            {
                public void Method()
                {
                    $$
                }
            }
            """);

    [Fact]
    public Task InsertRegionSnippetInConstructorTest()
        => VerifySnippetAsync("""
            class Program
            {
                public Program()
                {
                    #$$
                }
            }
            """, """
            class Program
            {
                public Program()
                {
                    #region {|0:MyRegion|}
            $$
                    #endregion
                }
            }
            """);
}
