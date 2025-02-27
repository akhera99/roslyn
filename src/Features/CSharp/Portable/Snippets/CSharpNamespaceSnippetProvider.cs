// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.LanguageService;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Microsoft.CodeAnalysis.Shared.Utilities;
using Microsoft.CodeAnalysis.Snippets;
using Microsoft.CodeAnalysis.Snippets.SnippetProviders;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.CSharp.Snippets
{
    [ExportSnippetProvider(nameof(ISnippetProvider), LanguageNames.CSharp), Shared]
    [method: ImportingConstructor]
    [method: Obsolete(MefConstruction.ImportingConstructorMessage, error: true)]
    internal sealed class CSharpNamespaceSnippetProvider() : AbstractNamespaceSnippetProvider<BaseNamespaceDeclarationSyntax>
    {
        public override string Identifier => CSharpSnippetIdentifiers.Namespace;

        public override string Description => CSharpFeaturesResources.namespace_declaration;

        protected override async Task<ImmutableArray<TextChange>> GenerateSnippetTextChangesAsync(Document document, int position, CancellationToken cancellationToken)
        {
            var options = (CSharpAnalyzerOptionsProvider)await document.GetAnalyzerOptionsProviderAsync(cancellationToken).ConfigureAwait(false);
            var namespaceDeclarationPreference = options.NamespaceDeclarations.Value;
            var syntaxFacts = document.GetRequiredLanguageService<ISyntaxFactsService>();
            var namespaceDeclarationName = PathMetadataUtilities.TryBuildNamespaceFromFolders(document.Folders, syntaxFacts);

            if (namespaceDeclarationPreference == CodeAnalysis.CodeStyle.NamespaceDeclarationPreference.BlockScoped)
            {
                var generator = SyntaxGenerator.GetGenerator(document);
                return generator.NamespaceDeclaration(namespaceDeclarationName, )
            }
            else
            {

            }
        }

        protected override ImmutableArray<SnippetPlaceholder> GetPlaceHolderLocationsList(BaseNamespaceDeclarationSyntax node, ISyntaxFacts syntaxFacts, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        protected override int GetTargetCaretPosition(BaseNamespaceDeclarationSyntax caretTarget, SourceText sourceText)
        {
            throw new NotImplementedException();
        }

        protected override bool IsValidSnippetLocationCore(SnippetContext context, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
