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
using Microsoft.CodeAnalysis.CSharp.Extensions;
using Microsoft.CodeAnalysis.CSharp.Extensions.ContextQuery;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.Utilities;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.LanguageService;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Microsoft.CodeAnalysis.Shared.Utilities;
using Microsoft.CodeAnalysis.Snippets;
using Microsoft.CodeAnalysis.Snippets.SnippetProviders;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.CSharp.Snippets;

using static CSharpSyntaxTokens;

[ExportSnippetProvider(nameof(ISnippetProvider), LanguageNames.CSharp), Shared]
[method: ImportingConstructor]
[method: Obsolete(MefConstruction.ImportingConstructorMessage, error: true)]
internal sealed class CSharpPropFullSnippetProvider() : AbstractPropFullSnippetProvider<PropertyDeclarationSyntax>
{
    public override string Identifier => CSharpSnippetIdentifiers.PropFull;

    public override string Description => CSharpFeaturesResources.property_and_backing_field;

    protected override async Task<(PropertyDeclarationSyntax, SyntaxNode)> GeneratePropertyDeclarationAsync(Document document, int position, CancellationToken cancellationToken)
    {
        var compilation = await document.Project.GetRequiredCompilationAsync(cancellationToken).ConfigureAwait(false);
        var semanticModel = await document.GetRequiredSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        var generator = SyntaxGenerator.GetGenerator(document);
        var fieldIdentifierName = NameGenerator.GenerateUniqueName("myVar",
            n => semanticModel.LookupSymbols(position, name: n).IsEmpty);
        var fieldDeclaration = generator.FieldDeclaration(fieldIdentifierName, generator.TypeExpression(SpecialType.System_Int32), Accessibility.Private);

        var identifierName = NameGenerator.GenerateUniqueName("MyProperty",
            n => semanticModel.LookupSymbols(position, name: n).IsEmpty);
        var syntaxContext = CSharpSyntaxContext.CreateContext(document, semanticModel, position, cancellationToken);
        var accessors = new AccessorDeclarationSyntax?[]
        {
            (AccessorDeclarationSyntax)generator.GetAccessorDeclaration(statements: [generator.ReturnStatement(generator.IdentifierName(fieldIdentifierName))]),
            (AccessorDeclarationSyntax)generator.SetAccessorDeclaration(statements: [generator.AssignmentStatement(generator.IdentifierName(fieldIdentifierName), generator.IdentifierName("value"))]),
        };

        SyntaxTokenList modifiers = default;

        // If there are no preceding accessibility modifiers create default `public` one
        if (!syntaxContext.PrecedingModifiers.Any(SyntaxFacts.IsAccessibilityModifier))
        {
            modifiers = SyntaxTokenList.Create(PublicKeyword);
        }

        return (SyntaxFactory.PropertyDeclaration(
            attributeLists: default,
            modifiers: modifiers,
            type: compilation.GetSpecialType(SpecialType.System_Int32).GenerateTypeSyntax(allowVar: false),
            explicitInterfaceSpecifier: null,
            identifier: identifierName.ToIdentifierToken(),
            accessorList: SyntaxFactory.AccessorList([.. (IEnumerable<AccessorDeclarationSyntax>)accessors.Where(a => a is not null)])), fieldDeclaration);
    }

    protected override ImmutableArray<SnippetPlaceholder> GetPlaceHolderLocationsList(PropertyDeclarationSyntax node, ISyntaxFacts syntaxFacts, CancellationToken cancellationToken)
    {
        var identifier = node.Identifier;

        return
        [
            new SnippetPlaceholder(identifier.ValueText, identifier.SpanStart),
        ];
    }

    protected override int GetTargetCaretPosition(PropertyDeclarationSyntax caretTarget, SourceText sourceText)
    {
        return caretTarget.AccessorList!.CloseBraceToken.Span.End;
    }

    protected override bool IsValidSnippetLocationCore(SnippetContext context, CancellationToken cancellationToken)
    {
        return context.SyntaxContext.SyntaxTree.IsMemberDeclarationContext(context.Position, (CSharpSyntaxContext)context.SyntaxContext,
            SyntaxKindSet.AllMemberModifiers, SyntaxKindSet.ClassInterfaceStructRecordTypeDeclarations, canBePartial: true, cancellationToken);
    }
}
