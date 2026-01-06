---
name: analyzers-codefixes
description: Create diagnostic analyzers and code fix providers in Roslyn. Use this skill when implementing new analyzers, code fixes, or IDE features that detect code issues and provide automated fixes. This includes creating DiagnosticAnalyzer classes, CodeFixProvider implementations, and their associated tests.
license: MIT
---

# Diagnostic Analyzers and Code Fixes

This skill provides essential patterns and practices for creating diagnostic analyzers and code fix providers in the Roslyn codebase.

## Overview

Roslyn's analyzer and code fix infrastructure enables both built-in IDE features and extensible third-party analyzers. The codebase uses consistent patterns for:
- Reporting diagnostics with configurable severity
- Providing code fixes with "Fix All" support
- Integrating with EditorConfig options
- Supporting both IDE and build-time enforcement

---

## Diagnostic Analyzer Patterns

### Basic Analyzer Structure

```csharp
[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
internal sealed class MyDiagnosticAnalyzer : AbstractBuiltInCodeStyleDiagnosticAnalyzer
{
    public MyDiagnosticAnalyzer()
        : base(
            diagnosticId: IDEDiagnosticIds.MyDiagnosticId,
            enforceOnBuild: EnforceOnBuild.WhenExplicitlyEnabled,
            option: CodeStyleOptions2.MyOption,
            title: new LocalizableResourceString(nameof(AnalyzersResources.My_title), AnalyzersResources.ResourceManager, typeof(AnalyzersResources)),
            messageFormat: new LocalizableResourceString(nameof(AnalyzersResources.My_message), AnalyzersResources.ResourceManager, typeof(AnalyzersResources)))
    {
    }

    public override DiagnosticAnalyzerCategory GetAnalyzerCategory()
        => DiagnosticAnalyzerCategory.SemanticDocumentAnalysis;

    protected override void InitializeWorker(AnalysisContext context)
    {
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.SomeNode);
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        // Analysis logic here
        var node = context.Node;
        var semanticModel = context.SemanticModel;
        
        // Report diagnostic if issue found
        context.ReportDiagnostic(Diagnostic.Create(
            Descriptor,
            node.GetLocation()));
    }
}
```

### Key Base Classes

**`AbstractBuiltInCodeStyleDiagnosticAnalyzer`** - For IDE analyzers with EditorConfig options:
- Automatically handles option-based severity configuration
- Supports `EnforceOnBuild` settings
- Use for style/preference analyzers

**`AbstractCodeQualityDiagnosticAnalyzer`** - For code quality issues:
- Not configurable via EditorConfig by default
- Typically warning or error severity
- Use for correctness/quality issues

**`DiagnosticAnalyzer`** (base) - For custom analyzers:
- Maximum flexibility
- Manually implement all registration
- Use when base classes don't fit

### Registration Methods

Choose the appropriate registration based on what you're analyzing:

```csharp
protected override void InitializeWorker(AnalysisContext context)
{
    // Syntax analysis - fastest, no semantic info
    context.RegisterSyntaxNodeAction(AnalyzeNode, 
        SyntaxKind.ClassDeclaration,
        SyntaxKind.StructDeclaration);

    // Operation analysis - semantic, language-agnostic
    context.RegisterOperationAction(AnalyzeOperation,
        OperationKind.Invocation,
        OperationKind.PropertyReference);

    // Symbol analysis - for declarations
    context.RegisterSymbolAction(AnalyzeSymbol,
        SymbolKind.Method,
        SymbolKind.Property);

    // Compilation analysis - for whole-compilation rules
    context.RegisterCompilationStartAction(compilationContext =>
    {
        // Set up state, register more actions
        compilationContext.RegisterOperationAction(/*...*/);
    });

    // Semantic model analysis - for document-level rules
    context.RegisterSemanticModelAction(AnalyzeSemanticModel);
}
```

### Diagnostic Descriptor Creation

```csharp
private static readonly DiagnosticDescriptor s_descriptor = new(
    id: IDEDiagnosticIds.UsePatternMatching,
    title: new LocalizableResourceString(nameof(AnalyzersResources.Use_pattern_matching), AnalyzersResources.ResourceManager, typeof(AnalyzersResources)),
    messageFormat: new LocalizableResourceString(nameof(AnalyzersResources.Use_pattern_matching), AnalyzersResources.ResourceManager, typeof(AnalyzersResources)),
    category: DiagnosticCategory.Style,
    defaultSeverity: DiagnosticSeverity.Info,
    isEnabledByDefault: true,
    customTags: DiagnosticCustomTags.Unnecessary);  // For faded/grayed-out code
```

**Common Custom Tags:**
- `DiagnosticCustomTags.Unnecessary` - Grays out unnecessary code
- `DiagnosticCustomTags.Microsoft` - Microsoft-authored analyzer
- `WellKnownDiagnosticTags.Telemetry` - Disable telemetry for this diagnostic

### Performance Guidelines

**DO:**
- Use `RegisterSyntaxNodeAction` with specific `SyntaxKind` filters
- Limit semantic analysis to necessary nodes only
- Use `SymbolEqualityComparer.Default` for symbol comparisons
- Cache expensive computations in compilation start actions

**DON'T:**
- Register for every node type unnecessarily
- Call `GetSymbolInfo()` or `GetTypeInfo()` in hot loops
- Use LINQ in analysis code (performance-critical path)
- Allocate collections without using object pools

```csharp
// GOOD: Specific node types
context.RegisterSyntaxNodeAction(AnalyzeNode, 
    SyntaxKind.IfStatement, 
    SyntaxKind.SwitchStatement);

// BAD: Too broad, will fire on every token
context.RegisterSyntaxTreeAction(AnalyzeTree);

// GOOD: Use pooled collections
using var _ = ArrayBuilder<IOperation>.GetInstance(out var operations);
foreach (var op in rootOperation.DescendantsAndSelf())
{
    operations.Add(op);
}

// BAD: LINQ allocation in hot path
var operations = rootOperation.DescendantsAndSelf().ToList();
```

### Diagnostic ID Conventions

Follow the existing ID patterns in `IDEDiagnosticIds.cs`:
- **IDE0001-IDE0999**: IDE analyzers
- **IDE1000+**: Newer IDE features
- Use sequential numbering within feature areas
- Document in `IDEDiagnosticIds.cs` with descriptive constant names

---

## Code Fix Provider Patterns

### Basic Code Fix Structure

```csharp
[ExportCodeFixProvider(LanguageNames.CSharp, Name = PredefinedCodeFixProviderNames.UsePatternMatching), Shared]
[method: ImportingConstructor]
[method: Obsolete(MefConstruction.ImportingConstructorMessage, error: true)]
internal sealed class UsePatternMatchingCodeFixProvider() : SyntaxEditorBasedCodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds 
        => [IDEDiagnosticIds.InlineAsTypeCheckId];

    public override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var diagnostic = context.Diagnostics.First();
        context.RegisterCodeFix(
            CodeAction.Create(
                CSharpAnalyzersResources.Use_pattern_matching,
                cancellationToken => FixAsync(context.Document, diagnostic, cancellationToken),
                nameof(CSharpAnalyzersResources.Use_pattern_matching)),
            diagnostic);
        
        return Task.CompletedTask;
    }

    protected override Task FixAllAsync(
        Document document,
        ImmutableArray<Diagnostic> diagnostics,
        SyntaxEditor editor,
        CancellationToken cancellationToken)
    {
        foreach (var diagnostic in diagnostics)
        {
            var node = diagnostic.Location.FindNode(getInnermostNodeForTie: true, cancellationToken);
            
            // Create replacement using editor
            var replacement = CreateReplacement(node);
            editor.ReplaceNode(node, replacement);
        }
        
        return Task.CompletedTask;
    }
}
```

### Key Base Classes

**`SyntaxEditorBasedCodeFixProvider`** - Recommended for most fixes:
- Provides automatic "Fix All" support
- Uses `SyntaxEditor` for reliable transformations
- Handles document-level fixes efficiently
- Most common pattern in Roslyn

**`CodeFixProvider`** (base) - For complex scenarios:
- Full control over fix behavior
- Multiple document changes
- Project/solution-level changes
- Use when `SyntaxEditor` isn't sufficient

### SyntaxEditor Best Practices

The `SyntaxEditor` provides reliable syntax tree transformations:

```csharp
protected override Task FixAllAsync(
    Document document,
    ImmutableArray<Diagnostic> diagnostics,
    SyntaxEditor editor,
    CancellationToken cancellationToken)
{
    foreach (var diagnostic in diagnostics)
    {
        // Get the problematic node
        var node = diagnostic.Location.FindNode(
            getInnermostNodeForTie: true, 
            cancellationToken);
        
        // Option 1: Replace entire node
        var newNode = node.WithLeadingTrivia(/*...*/)
                         .WithAdditionalAnnotations(Formatter.Annotation);
        editor.ReplaceNode(node, newNode);
        
        // Option 2: Remove node
        editor.RemoveNode(node);
        
        // Option 3: Insert before/after
        editor.InsertBefore(node, newNode);
        editor.InsertAfter(node, newNode);
        
        // Track nodes across edits
        var trackedNode = editor.Generator.TrackNode(node);
    }
    
    return Task.CompletedTask;
}
```

### Multiple Fixes Pattern

When offering multiple fix options:

```csharp
public override Task RegisterCodeFixesAsync(CodeFixContext context)
{
    var diagnostic = context.Diagnostics[0];
    
    // Option 1: Simple fix
    context.RegisterCodeFix(
        CodeAction.Create(
            "Use pattern matching",
            ct => FixWithPatternAsync(context.Document, diagnostic, ct),
            equivalenceKey: "UsePattern"),
        diagnostic);
    
    // Option 2: Alternative fix
    context.RegisterCodeFix(
        CodeAction.Create(
            "Use 'is' check",
            ct => FixWithIsCheckAsync(context.Document, diagnostic, ct),
            equivalenceKey: "UseIsCheck"),
        diagnostic);
    
    return Task.CompletedTask;
}
```

### Fix All Support

The `SyntaxEditorBasedCodeFixProvider` automatically provides Fix All, but you can customize:

```csharp
public override FixAllProvider? GetFixAllProvider()
{
    // Default: Fix All in Document/Project/Solution
    return base.GetFixAllProvider();
    
    // Custom: Disable Fix All
    // return null;
    
    // Custom: Specific scopes only
    // return FixAllProvider.Create(...);
}

protected virtual bool IncludeDiagnosticDuringFixAll(
    Diagnostic diagnostic,
    Document document,
    string? equivalenceKey,
    CancellationToken cancellationToken)
{
    // Filter diagnostics during Fix All
    // Default: include all diagnostics matching equivalenceKey
    return true;
}
```

### Code Action Priority

Control the ordering of fixes in the lightbulb menu:

```csharp
context.RegisterCodeFix(
    CodeAction.Create(
        title,
        createChangedDocument,
        equivalenceKey,
        priority: CodeActionPriority.High),  // Appears first
    diagnostic);
```

**Priority levels:**
- `CodeActionPriority.High` - Most important fixes (e.g., correct critical errors)
- `CodeActionPriority.Default` - Normal fixes
- `CodeActionPriority.Low` - Less common alternatives

### Working with Annotations

Use annotations to track nodes and apply formatting:

```csharp
// Mark for formatting
var newNode = node.WithAdditionalAnnotations(Formatter.Annotation);

// Mark for simplification
var newNode = node.WithAdditionalAnnotations(Simplifier.Annotation);

// Custom tracking
var trackingAnnotation = new SyntaxAnnotation();
var trackedNode = node.WithAdditionalAnnotations(trackingAnnotation);

// Later, find it
var root = await document.GetSyntaxRootAsync(cancellationToken);
var found = root.GetAnnotatedNodes(trackingAnnotation).Single();
```

---

## Testing Patterns

### Analyzer Tests

```csharp
[Fact]
public async Task TestAnalyzer_FindsIssue()
{
    var source = """
        class C
        {
            void M()
            {
                object x = "test";
                if (x is string)  // Should suggest pattern matching
                {
                    var s = (string)x;
                }
            }
        }
        """;

    var expected = Diagnostic(IDEDiagnosticIds.UsePatternMatching)
        .WithSpan(7, 17, 7, 27);

    await VerifyCS.VerifyCodeFixAsync(source, expected, fixedSource);
}
```

### Code Fix Tests

```csharp
[Fact]
public async Task TestCodeFix_ProducesExpectedResult()
{
    var source = """
        class C
        {
            void M(object x)
            {
                if ([|x is string|])
                {
                    var s = (string)x;
                }
            }
        }
        """;

    var fixedSource = """
        class C
        {
            void M(object x)
            {
                if (x is string s)
                {
                }
            }
        }
        """;

    await VerifyCS.VerifyCodeFixAsync(source, fixedSource);
}
```

### Key Testing Utilities

- `VerifyCS.VerifyAnalyzerAsync()` - Verify C# analyzer
- `VerifyVB.VerifyAnalyzerAsync()` - Verify VB analyzer
- `VerifyCS.VerifyCodeFixAsync()` - Verify C# code fix
- `[|...|]` - Mark diagnostic location in test source
- Use raw string literals (`"""..."""`) for test code

### Testing Options

```csharp
[Fact]
public async Task TestWithOptions()
{
    var source = "...";
    
    await new VerifyCS.Test
    {
        TestCode = source,
        FixedCode = fixedSource,
        Options =
        {
            { CodeStyleOptions2.PreferPatternMatching, true, NotificationOption2.Suggestion }
        }
    }.RunAsync();
}
```

---

## Common Patterns

### Checking for Null/Default

```csharp
// In analyzer
var typeInfo = context.SemanticModel.GetTypeInfo(expression, cancellationToken);
if (typeInfo.Type is null)
    return;

var symbolInfo = context.SemanticModel.GetSymbolInfo(expression, cancellationToken);
if (symbolInfo.Symbol is not IMethodSymbol method)
    return;
```

### Language-Agnostic Analysis (IOperation)

```csharp
context.RegisterOperationAction(context =>
{
    var invocation = (IInvocationOperation)context.Operation;
    
    // Works for both C# and VB
    if (invocation.TargetMethod.Name == "ToString")
    {
        context.ReportDiagnostic(/*...*/);
    }
}, OperationKind.Invocation);
```

### Detecting Unnecessary Code

```csharp
// Mark as unnecessary (grayed out)
var descriptor = new DiagnosticDescriptor(
    /* ... */,
    customTags: DiagnosticCustomTags.Unnecessary);

// In code fix, remove it
editor.RemoveNode(unnecessaryNode, SyntaxRemoveOptions.KeepNoTrivia);
```

### Working with Trivia

```csharp
// Preserve trivia when replacing
var newNode = replacement
    .WithLeadingTrivia(oldNode.GetLeadingTrivia())
    .WithTrailingTrivia(oldNode.GetTrailingTrivia());

// Add elastic markers for formatting
var newNode = node
    .WithLeadingTrivia(SyntaxFactory.ElasticMarker)
    .WithAdditionalAnnotations(Formatter.Annotation);
```

---

## Anti-Patterns to Avoid

❌ **Calling `GetSymbolInfo` unnecessarily**
```csharp
// BAD: Every node gets semantic analysis
context.RegisterSyntaxNodeAction(context =>
{
    var symbol = context.SemanticModel.GetSymbolInfo(context.Node).Symbol;
    // ...
}, SyntaxKind.IdentifierName);  // Fires millions of times!
```

✅ **Filter first, then analyze**
```csharp
// GOOD: Only analyze relevant nodes
context.RegisterSyntaxNodeAction(context =>
{
    if (!IsRelevantNode(context.Node))
        return;
    
    var symbol = context.SemanticModel.GetSymbolInfo(context.Node).Symbol;
    // ...
}, SyntaxKind.InvocationExpression);
```

❌ **LINQ in hot paths**
```csharp
// BAD: Allocations in analyzer
var methods = type.GetMembers().OfType<IMethodSymbol>().ToList();
```

✅ **Manual iteration with pooled collections**
```csharp
// GOOD: Use pooled collections
using var _ = ArrayBuilder<IMethodSymbol>.GetInstance(out var methods);
foreach (var member in type.GetMembers())
{
    if (member is IMethodSymbol method)
        methods.Add(method);
}
```

❌ **Direct string manipulation**
```csharp
// BAD: Don't build code as strings
var newCode = "var " + variableName + " = " + expression + ";";
```

✅ **Use SyntaxFactory or SyntaxGenerator**
```csharp
// GOOD: Type-safe syntax construction
var declaration = SyntaxFactory.LocalDeclarationStatement(
    SyntaxFactory.VariableDeclaration(
        SyntaxFactory.IdentifierName("var"),
        SyntaxFactory.SingletonSeparatedList(
            SyntaxFactory.VariableDeclarator(variableName)
                .WithInitializer(SyntaxFactory.EqualsValueClause(expression)))));
```

---

## Resources

- **Base classes**: `src/Analyzers/Core/Analyzers/AbstractBuiltInCodeStyleDiagnosticAnalyzer.cs`
- **Code fix bases**: `src/Workspaces/SharedUtilitiesAndExtensions/Workspace/Core/CodeFixes/`
- **Examples**: Browse `src/Analyzers/CSharp/Analyzers/` for real-world patterns
- **Diagnostic IDs**: `src/Analyzers/Core/Analyzers/IDEDiagnosticIds.cs`
- **Testing**: `src/EditorFeatures/Test*` and `src/Features/*Test*`

---

## Quick Reference

| Task | Pattern |
|------|---------|
| Simple style analyzer | Inherit from `AbstractBuiltInCodeStyleDiagnosticAnalyzer` |
| Quality/correctness analyzer | Inherit from `AbstractCodeQualityDiagnosticAnalyzer` |
| Code fix with Fix All | Inherit from `SyntaxEditorBasedCodeFixProvider` |
| Syntax-only analysis | `RegisterSyntaxNodeAction` |
| Language-agnostic | `RegisterOperationAction` |
| Symbol analysis | `RegisterSymbolAction` |
| Mark code as unnecessary | Use `DiagnosticCustomTags.Unnecessary` |
| Preserve formatting | Add `Formatter.Annotation` |
| Track nodes in edits | Use `SyntaxAnnotation` or `editor.TrackNode()` |
| Test analyzer | `VerifyCS.VerifyAnalyzerAsync()` |
| Test code fix | `VerifyCS.VerifyCodeFixAsync()` |
