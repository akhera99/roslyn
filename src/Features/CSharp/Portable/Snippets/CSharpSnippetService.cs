// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Composition;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Snippets;
using Microsoft.CodeAnalysis.Snippets.SnippetProviders;

namespace Microsoft.CodeAnalysis.CSharp.Snippets
{
    [ExportLanguageService(typeof(ISnippetService), LanguageNames.CSharp), Shared]
    internal class CSharpSnippetService : AbstractSnippetService
    {
        [ImportingConstructor]
        [Obsolete(MefConstruction.ImportingConstructorMessage, error: true)]
        public CSharpSnippetService([ImportMany] IEnumerable<Lazy<ISnippetProvider, LanguageMetadata>> snippetProviders)
            : base(snippetProviders)
        {
        }
    }
}
