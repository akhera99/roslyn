﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable disable

namespace Microsoft.CodeAnalysis.Test.Utilities
{
    public static class Traits
    {
        public const string Editor = nameof(Editor);
        public static class Editors
        {
            public const string KeyProcessors = nameof(KeyProcessors);
            public const string KeyProcessorProviders = nameof(KeyProcessorProviders);
            public const string Preview = nameof(Preview);
            public const string LanguageServerProtocol = nameof(LanguageServerProtocol);
        }

        public const string Feature = nameof(Feature);
        public static class Features
        {
            public const string AddAwait = "Refactoring.AddAwait";
            public const string AddMissingImports = "Refactoring.AddMissingImports";
            public const string AddMissingReference = nameof(AddMissingReference);
            public const string AddMissingTokens = nameof(AddMissingTokens);
            public const string Adornments = nameof(Adornments);
            public const string AsyncLazy = nameof(AsyncLazy);
            public const string AutomaticCompletion = nameof(AutomaticCompletion);
            public const string AutomaticEndConstructCorrection = nameof(AutomaticEndConstructCorrection);
            public const string BlockCommentEditing = nameof(BlockCommentEditing);
            public const string BraceHighlighting = nameof(BraceHighlighting);
            public const string BraceMatching = nameof(BraceMatching);
            public const string Build = nameof(Build);
            public const string CallHierarchy = nameof(CallHierarchy);
            public const string CaseCorrection = nameof(CaseCorrection);
            public const string ChangeSignature = nameof(ChangeSignature);
            public const string ClassView = nameof(ClassView);
            public const string Classification = nameof(Classification);
            public const string CodeActionsAddOrRemoveAccessibilityModifiers = "CodeActions.AddOrRemoveAccessibilityModifiers";
            public const string CodeActionsAddAnonymousTypeMemberName = "CodeActions.AddAnonymousTypeMemberName";
            public const string CodeActionsAddAwait = "CodeActions.AddAwait";
            public const string CodeActionsAddBraces = "CodeActions.AddBraces";
            public const string CodeActionsAddConstructorParametersFromMembers = "CodeActions.AddConstructorParametersFromMembers";
            public const string CodeActionsAddDebuggerDisplay = "CodeActions.AddDebuggerDisplay";
            public const string CodeActionsAddDocCommentNodes = "CodeActions.AddDocCommentParamNodes";
            public const string CodeActionsAddExplicitCast = "CodeActions.AddExplicitCast";
            public const string CodeActionsAddInheritdoc = "CodeActions.AddInheritdoc";
            public const string CodeActionsAddFileBanner = "CodeActions.AddFileBanner";
            public const string CodeActionsAddImport = "CodeActions.AddImport";
            public const string CodeActionsAddMissingReference = "CodeActions.AddMissingReference";
            public const string CodeActionsAddNew = "CodeActions.AddNew";
            public const string CodeActionsRemoveNewModifier = "CodeActions.RemoveNewModifier";
            public const string CodeActionsAddObsoleteAttribute = "CodeActions.AddObsoleteAttribute";
            public const string CodeActionsAddOverload = "CodeActions.AddOverloads";
            public const string CodeActionsAddParameter = "CodeActions.AddParameter";
            public const string CodeActionsAddParenthesesAroundConditionalExpressionInInterpolatedString = "CodeActions.AddParenthesesAroundConditionalExpressionInInterpolatedString";
            public const string CodeActionsAddRequiredParentheses = "CodeActions.AddRequiredParentheses";
            public const string CodeActionsAddShadows = "CodeActions.AddShadows";
            public const string CodeActionsMakeMemberStatic = "CodeActions.MakeMemberStatic";
            public const string CodeActionsAliasAmbiguousType = "CodeActions.AliasAmbiguousType";
            public const string CodeActionsAssignOutParameters = "CodeActions.AssignOutParameters";
            public const string CodeActionsChangeToAsync = "CodeActions.ChangeToAsync";
            public const string CodeActionsChangeToIEnumerable = "CodeActions.ChangeToIEnumerable";
            public const string CodeActionsChangeToYield = "CodeActions.ChangeToYield";
            public const string CodeActionsConfiguration = "CodeActions.Configuration";
            public const string CodeActionsConvertAnonymousTypeToClass = "CodeActions.ConvertAnonymousTypeToClass";
            public const string CodeActionsConvertAnonymousTypeToTuple = "CodeActions.ConvertAnonymousTypeToTuple";
            public const string CodeActionsConvertBetweenRegularAndVerbatimString = "CodeActions.ConvertBetweenRegularAndVerbatimString";
            public const string CodeActionsConvertForEachToFor = "CodeActions.ConvertForEachToFor";
            public const string CodeActionsConvertForEachToQuery = "CodeActions.ConvertForEachToQuery";
            public const string CodeActionsConvertForToForEach = "CodeActions.ConvertForToForEach";
            public const string CodeActionsConvertIfToSwitch = "CodeActions.ConvertIfToSwitch";
            public const string CodeActionsConvertLocalFunctionToMethod = "CodeActions.ConvertLocalFunctionToMethod";
            public const string CodeActionsConvertNumericLiteral = "CodeActions.ConvertNumericLiteral";
            public const string CodeActionsConvertQueryToForEach = "CodeActions.ConvertQueryToForEach";
            public const string CodeActionsConvertToRawString = "CodeActions.CodeActionsConvertToRawString";
            public const string CodeActionsConvertSwitchStatementToExpression = "CodeActions.ConvertSwitchStatementToExpression";
            public const string CodeActionsConvertToExtension = "CodeActions.ConvertToExtension";
            public const string CodeActionsConvertToInterpolatedString = "CodeActions.ConvertToInterpolatedString";
            public const string CodeActionsConvertToIterator = "CodeActions.ConvertToIterator";
            public const string CodeActionsConvertToRecord = "CodeActions.ConvertToRecord";
            public const string CodeActionsConvertTupleToStruct = "CodeActions.ConvertTupleToStruct";
            public const string CodeActionsCorrectExitContinue = "CodeActions.CorrectExitContinue";
            public const string CodeActionsCorrectFunctionReturnType = "CodeActions.CorrectFunctionReturnType";
            public const string CodeActionsCorrectNextControlVariable = "CodeActions.CorrectNextControlVariable";
            public const string CodeActionsDeclareAsNullable = "CodeActions.DeclareAsNullable";
            public const string CodeActionsDetectJsonString = "CodeActions.DetectJsonString";
            public const string CodeActionsExtractInterface = "CodeActions.ExtractInterface";
            public const string CodeActionsExtractLocalFunction = "CodeActions.ExtractLocalFunction";
            public const string CodeActionsExtractMethod = "CodeActions.ExtractMethod";
            public const string CodeActionsFixAllOccurrences = "CodeActions.FixAllOccurrences";
            public const string CodeActionsFixReturnType = "CodeActions.FixReturnType";
            public const string CodeActionsFullyQualify = "CodeActions.FullyQualify";
            public const string CodeActionsGenerateComparisonOperators = "CodeActions.GenerateComparisonOperators";
            public const string CodeActionsGenerateConstructor = "CodeActions.GenerateConstructor";
            public const string CodeActionsGenerateConstructorFromMembers = "CodeActions.GenerateConstructorFromMembers";
            public const string CodeActionsGenerateDefaultConstructors = "CodeActions.GenerateDefaultConstructors";
            public const string CodeActionsGenerateEndConstruct = "CodeActions.GenerateEndConstruct";
            public const string CodeActionsGenerateEnumMember = "CodeActions.GenerateEnumMember";
            public const string CodeActionsGenerateEqualsAndGetHashCode = "CodeActions.GenerateEqualsAndGetHashCodeFromMembers";
            public const string CodeActionsGenerateEvent = "CodeActions.GenerateEvent";
            public const string CodeActionsGenerateLocal = "CodeActions.GenerateLocal";
            public const string CodeActionsGenerateMethod = "CodeActions.GenerateMethod";
            public const string CodeActionsGenerateOverrides = "CodeActions.GenerateOverrides";
            public const string CodeActionsGenerateType = "CodeActions.GenerateType";
            public const string CodeActionsGenerateVariable = "CodeActions.GenerateVariable";
            public const string CodeActionsImplementAbstractClass = "CodeActions.ImplementAbstractClass";
            public const string CodeActionsImplementInterface = "CodeActions.ImplementInterface";
            public const string CodeActionsInitializeParameter = "CodeActions.InitializeParameter";
            public const string CodeActionsInlineMethod = "CodeActions.InlineMethod";
            public const string CodeActionsInlineDeclaration = "CodeActions.InlineDeclaration";
            public const string CodeActionsInlineTemporary = "CodeActions.InlineTemporary";
            public const string CodeActionsInlineTypeCheck = "CodeActions.InlineTypeCheck";
            public const string CodeActionsInsertBraces = "CodeActions.InsertBraces";
            public const string CodeActionsInsertMissingTokens = "CodeActions.InsertMissingTokens";
            public const string CodeActionsIntroduceLocalForExpression = "CodeActions.IntroduceLocalForExpression";
            public const string CodeActionsIntroduceParameter = "CodeActions.IntroduceParameter";
            public const string CodeActionsIntroduceUsingStatement = "CodeActions.IntroduceUsingStatement";
            public const string CodeActionsIntroduceVariable = "CodeActions.IntroduceVariable";
            public const string CodeActionsInvertConditional = "CodeActions.InvertConditional";
            public const string CodeActionsInvertIf = "CodeActions.InvertIf";
            public const string CodeActionsInvertLogical = "CodeActions.InvertLogical";
            public const string CodeActionsInvokeDelegateWithConditionalAccess = "CodeActions.InvokeDelegateWithConditionalAccess";
            public const string CodeActionsRemoveUnnecessaryLambdaExpression = "CodeActions.RemoveUnnecessaryLambdaExpression";
            public const string CodeActionsMakeTypePartial = "CodeActions.MakeTypePartial";
            public const string CodeActionsMakeFieldReadonly = "CodeActions.MakeFieldReadonly";
            public const string CodeActionsMakeLocalFunctionStatic = "CodeActions.MakeLocalFunctionStatic";
            public const string CodeActionsMakeAnonymousFunctionStatic = "CodeActions.MakeAnonymousFunctionStatic";
            public const string CodeActionsMakeMemberRequired = "CodeActions.MakeMemberRequired";
            public const string CodeActionsMakeMethodAsynchronous = "CodeActions.MakeMethodAsynchronous";
            public const string CodeActionsMakeMethodSynchronous = "CodeActions.MakeMethodSynchronous";
            public const string CodeActionsMakeRefStruct = "CodeActions.MakeRefStruct";
            public const string CodeActionsMakeStatementAsynchronous = "CodeActions.MakeStatementAsynchronous";
            public const string CodeActionsMakeStructFieldsWritable = "CodeActions.MakeStructFieldsWritable";
            public const string CodeActionsMakeStructReadOnly = "CodeActions.MakeStructReadOnly";
            public const string CodeActionsMakeStructMemberReadOnly = "CodeActions.MakeStructMemberReadOnly";
            public const string CodeActionsMakeTypeAbstract = "CodeActions.MakeTypeAbstract";
            public const string CodeActionsMergeConsecutiveIfStatements = "CodeActions.MergeConsecutiveIfStatements";
            public const string CodeActionsMergeNestedIfStatements = "CodeActions.MergeNestedIfStatements";
            public const string CodeActionsMoveDeclarationNearReference = "CodeActions.MoveDeclarationNearReference";
            public const string CodeActionsMoveStaticMembers = "CodeActions.MoveStaticMembers";
            public const string CodeActionsMoveToNamespace = nameof(CodeActionsMoveToNamespace);
            public const string CodeActionsMoveToTopOfFile = "CodeActions.MoveToTopOfFile";
            public const string CodeActionsMoveType = "CodeActions.MoveType";
            public const string CodeActionsNameTupleElement = "CodeActions.NameTupleElement";
            public const string CodeActionsOrderModifiers = "CodeActions.OrderModifiers";
            public const string CodeActionsOrganizeImports = "CodeActions.OrganizeImports";
            public const string CodeActionsPopulateSwitch = "CodeActions.PopulateSwitch";
            public const string CodeActionsPullMemberUp = "CodeActions.PullMemberUp";
            public const string CodeActionsQualifyMemberAccess = "CodeActions.QualifyMemberAccess";
            public const string CodeActionsRemoveAsyncModifier = "CodeActions.RemoveAsyncModifier";
            public const string CodeActionsRemoveByVal = "CodeActions.RemoveByVal";
            public const string CodeActionsRemoveDocCommentNode = "CodeActions.RemoveDocCommentNode";
            public const string CodeActionsRemoveInKeyword = "CodeActions.RemoveInKeyword";
            public const string CodeActionsRemoveUnnecessarySuppressions = "CodeActions.RemoveUnnecessarySuppressions";
            public const string CodeActionsRemoveUnnecessaryCast = "CodeActions.RemoveUnnecessaryCast";
            public const string CodeActionsRemoveUnnecessaryDiscardDesignation = "CodeActions.RemoveUnnecessaryDiscardDesignation";
            public const string CodeActionsRemoveUnnecessaryImports = "CodeActions.RemoveUnnecessaryImports";
            public const string CodeActionsRemoveUnnecessaryNullableDirective = "CodeActions.RemoveUnnecessaryNullableDirective";
            public const string CodeActionsRemoveUnnecessaryParentheses = "CodeActions.RemoveUnnecessaryParentheses";
            public const string CodeActionsRemoveUnreachableCode = "CodeActions.RemoveUnreachableCode";
            public const string CodeActionsRemoveUnusedLocalFunction = "CodeActions.RemoveUnusedLocalFunction";
            public const string CodeActionsRemoveUnusedMembers = "CodeActions.RemoveUnusedMembers";
            public const string CodeActionsRemoveUnusedParameters = "CodeActions.RemoveUnusedParameters";
            public const string CodeActionsRemoveUnusedValues = "CodeActions.RemoveUnusedValues";
            public const string CodeActionsRemoveUnusedVariable = "CodeActions.RemoveUnusedVariable";
            public const string CodeActionsReplaceDefaultLiteral = "CodeActions.ReplaceDefaultLiteral";
            public const string CodeActionsReplaceDocCommentTextWithTag = "CodeActions.ReplaceDocCommentTextWithTag";
            public const string CodeActionsReplaceMethodWithProperty = "CodeActions.ReplaceMethodWithProperty";
            public const string CodeActionsReplacePropertyWithMethods = "CodeActions.ReplacePropertyWithMethods";
            public const string CodeActionsResolveConflictMarker = "CodeActions.ResolveConflictMarker";
            public const string CodeActionsReverseForStatement = "CodeActions.ReverseForStatement";
            public const string CodeActionsSimplifyConditional = "CodeActions.SimplifyConditional";
            public const string CodeActionsSimplifyInterpolation = "CodeActions.SimplifyInterpolation";
            public const string CodeActionsSimplifyLinqExpression = "CodeActions.SimplifyLinqExpression";
            public const string CodeActionsSimplifyLinqTypeCheckAndCast = "CodeActions.SimplifyLinqTypeCheckAndCast";
            public const string CodeActionsSimplifyPropertyPattern = "CodeActions.SimplifyPropertyPattern";
            public const string CodeActionsSimplifyThisOrMe = "CodeActions.SimplifyThisOrMe";
            public const string CodeActionsSimplifyTypeNames = "CodeActions.SimplifyTypeNames";
            public const string CodeActionsSpellcheck = "CodeActions.Spellcheck";
            public const string CodeActionsSplitIntoConsecutiveIfStatements = "CodeActions.SplitIntoConsecutiveIfStatements";
            public const string CodeActionsSplitIntoNestedIfStatements = "CodeActions.SplitIntoNestedIfStatements";
            public const string CodeActionsSuppression = "CodeActions.Suppression";
            public const string CodeActionsSyncNamespace = "CodeActions.SyncNamespace";
            public const string CodeActionsUnsealClass = "CodeActions.UnsealClass";
            public const string CodeActionsUpdateLegacySuppressions = "CodeActions.UpdateLegacySuppressions";
            public const string CodeActionsUpdateProjectToAllowUnsafe = "CodeActions.UpdateProjectToAllowUnsafe";
            public const string CodeActionsUpgradeProject = "CodeActions.UpgradeProject";
            public const string CodeActionsUseAutoProperty = "CodeActions.UseAutoProperty";
            public const string CodeActionsUseCoalesceExpression = "CodeActions.UseCoalesceExpression";
            public const string CodeActionsUseCollectionExpression = "CodeActions.UseCollectionExpression";
            public const string CodeActionsUseCollectionInitializer = "CodeActions.UseCollectionInitializer";
            public const string CodeActionsUseCompoundAssignment = "CodeActions.UseCompoundAssignment";
            public const string CodeActionsUseConditionalExpression = "CodeActions.UseConditionalExpression";
            public const string CodeActionsUseDeconstruction = "CodeActions.UseDeconstruction";
            public const string CodeActionsUseDefaultLiteral = "CodeActions.UseDefaultLiteral";
            public const string CodeActionsUseExplicitArrayInExpressionTree = "CodeActions.UseExplicitArrayInExpressionTree";
            public const string CodeActionsUseExplicitTupleName = "CodeActions.UseExplicitTupleName";
            public const string CodeActionsUseExplicitType = "CodeActions.UseExplicitType";
            public const string CodeActionsUseExplicitTypeForConst = "CodeActions.UseExplicitTypeForConst";
            public const string CodeActionsUseExpressionBody = "CodeActions.UseExpressionBody";
            public const string CodeActionsUseFrameworkType = "CodeActions.UseFrameworkType";
            public const string CodeActionsUseImplicitObjectCreation = "CodeActions.UseImplicitObjectCreation";
            public const string CodeActionsUseImplicitType = "CodeActions.UseImplicitType";
            public const string CodeActionsUseIndexOperator = "CodeActions.UseIndexOperator";
            public const string CodeActionsUseInferredMemberName = "CodeActions.UseInferredMemberName";
            public const string CodeActionsUseInterpolatedVerbatimString = "CodeActions.UseInterpolatedVerbatimString";
            public const string CodeActionsUseIsNotExpression = "CodeActions.UseIsNotExpression";
            public const string CodeActionsUseIsNullCheck = "CodeActions.UseIsNullCheck";
            public const string CodeActionsUseLocalFunction = "CodeActions.UseLocalFunction";
            public const string CodeActionsUseNamedArguments = "CodeActions.UseNamedArguments";
            public const string CodeActionsUseNotPattern = "CodeActions.UseNotPattern";
            public const string CodeActionsUsePatternCombinators = "CodeActions.UsePatternCombinators";
            public const string CodeActionsUsePatternMatchingForAsAndMemberAccess = "CodeActions.UsePatternMatchingForAsAndMemberAccess";
            public const string CodeActionsUsePrimaryConstructor = "CodeActions.UsePrimaryConstructor";
            public const string CodeActionsUseRecursivePatterns = "CodeActions.UseRecursivePatterns";
            public const string CodeActionsUseNullPropagation = "CodeActions.UseNullPropagation";
            public const string CodeActionsUseObjectInitializer = "CodeActions.UseObjectInitializer";
            public const string CodeActionsUseRangeOperator = "CodeActions.UseRangeOperator";
            public const string CodeActionsUseSimpleUsingStatement = "CodeActions.UseSimpleUsingStatement";
            public const string CodeActionsUseSystemHashCode = "CodeActions.UseSystemHashCode";
            public const string CodeActionsUseSystemThreadingLock = "CodeActions.UseSystemThreadingLock";
            public const string CodeActionsUseThrowExpression = "CodeActions.UseThrowExpression";
            public const string CodeActionsUseTupleSwap = "CodeActions.UseTupleSwap";
            public const string CodeActionsUseUnboundGenericTypeInNameOf = "CodeActions.UseUnboundGenericTypeInNameOf";
            public const string CodeActionsUseUtf8StringLiteral = "CodeActions.CodeActionsUseUtf8StringLiteral";
            public const string CodeActionsWrapping = "CodeActions.Wrapping";
            public const string CodeCleanup = nameof(CodeCleanup);
            public const string CodeDefinitionWindow = nameof(CodeDefinitionWindow);
            public const string CodeGeneration = nameof(CodeGeneration);
            public const string CodeGenerationSortDeclarations = "CodeGeneration.SortDeclarations";
            public const string CodeLens = nameof(CodeLens);
            public const string CodeModel = nameof(CodeModel);
            public const string CodeModelEvents = "CodeModel.Events";
            public const string CodeModelMethodXml = "CodeModel.MethodXml";
            public const string CommentSelection = nameof(CommentSelection);
            public const string CompleteStatement = nameof(CompleteStatement);
            public const string Completion = nameof(Completion);
            public const string ConvertAutoPropertyToFullProperty = nameof(ConvertAutoPropertyToFullProperty);
            public const string ConvertCast = nameof(ConvertCast);
            public const string ConvertTypeOfToNameOf = nameof(ConvertTypeOfToNameOf);
            public const string CopilotImplementNotImplementedException = nameof(CopilotImplementNotImplementedException);
            public const string DebuggingBreakpoints = "Debugging.Breakpoints";
            public const string DebuggingDataTips = "Debugging.DataTips";
            public const string DebuggingEditAndContinue = "Debugging.EditAndContinue";
            public const string DebuggingIntelliSense = "Debugging.IntelliSense";
            public const string DebuggingLocationName = "Debugging.LocationName";
            public const string DebuggingNameResolver = "Debugging.NameResolver";
            public const string DebuggingProximityExpressions = "Debugging.ProximityExpressions";
            public const string DecompiledSource = nameof(DecompiledSource);
            public const string Diagnostics = nameof(Diagnostics);
            public const string DisposeAnalysis = nameof(DisposeAnalysis);
            public const string DocCommentFormatting = nameof(DocCommentFormatting);
            public const string DocumentationComments = nameof(DocumentationComments);
            public const string DocumentOutline = nameof(DocumentOutline);
            public const string EditorConfig = nameof(EditorConfig);
            public const string EditorConfigUI = nameof(EditorConfigUI);
            public const string EncapsulateField = nameof(EncapsulateField);
            public const string EndConstructGeneration = nameof(EndConstructGeneration);
            public const string ErrorList = nameof(ErrorList);
            public const string ErrorSquiggles = nameof(ErrorSquiggles);
            public const string EventHookup = nameof(EventHookup);
            public const string Expansion = nameof(Expansion);
            public const string ExtractInterface = "Refactoring.ExtractInterface";
            public const string ExtractMethod = "Refactoring.ExtractMethod";
            public const string F1Help = nameof(F1Help);
            public const string FindReferences = nameof(FindReferences);
            public const string FixIncorrectTokens = nameof(FixIncorrectTokens);
            public const string FixInterpolatedVerbatimString = nameof(FixInterpolatedVerbatimString);
            public const string Formatting = nameof(Formatting);
            public const string GoToAdjacentMember = nameof(GoToAdjacentMember);
            public const string GoToBase = nameof(GoToBase);
            public const string GoToDefinition = nameof(GoToDefinition);
            public const string GoToImplementation = nameof(GoToImplementation);
            public const string InheritanceMargin = nameof(InheritanceMargin);
            public const string InlineHints = nameof(InlineHints);
            public const string Interactive = nameof(Interactive);
            public const string InteractiveHost = nameof(InteractiveHost);
            public const string KeywordHighlighting = nameof(KeywordHighlighting);
            public const string KeywordRecommending = nameof(KeywordRecommending);
            public const string LineCommit = nameof(LineCommit);
            public const string LineSeparators = nameof(LineSeparators);
            public const string LinkedFileDiffMerging = nameof(LinkedFileDiffMerging);
            public const string MSBuildWorkspace = nameof(MSBuildWorkspace);
            public const string MetadataAsSource = nameof(MetadataAsSource);
            public const string MoveToNamespace = nameof(MoveToNamespace);
            public const string NamingStyle = nameof(NamingStyle);
            public const string NavigableSymbols = nameof(NavigableSymbols);
            public const string NavigateTo = nameof(NavigateTo);
            public const string NavigationBar = nameof(NavigationBar);
            public const string NetCore = nameof(NetCore);
            public const string NormalizeModifiersOrOperators = nameof(NormalizeModifiersOrOperators);
            public const string ObjectBrowser = nameof(ObjectBrowser);
            public const string OnTheFlyDocs = nameof(OnTheFlyDocs);
            public const string Options = nameof(Options);
            public const string Organizing = nameof(Organizing);
            public const string Outlining = nameof(Outlining);
            public const string Packaging = nameof(Packaging);
            public const string PasteTracking = nameof(PasteTracking);
            public const string Peek = nameof(Peek);
            public const string ProjectSystemShims = nameof(ProjectSystemShims);
            public const string SarifErrorLogging = nameof(SarifErrorLogging);
            public const string QuickInfo = nameof(QuickInfo);
            public const string RQName = nameof(RQName);
            public const string ReduceTokens = nameof(ReduceTokens);
            public const string ReferenceHighlighting = nameof(ReferenceHighlighting);
            public const string RemoteHost = nameof(RemoteHost);
            public const string RemoveUnnecessaryLineContinuation = nameof(RemoveUnnecessaryLineContinuation);
            public const string Rename = nameof(Rename);
            public const string RenameTracking = nameof(RenameTracking);
            public const string RoslynLSPSnippetConverter = nameof(RoslynLSPSnippetConverter);
            public const string SignatureHelp = nameof(SignatureHelp);
            public const string Simplification = nameof(Simplification);
            public const string SmartIndent = nameof(SmartIndent);
            public const string SmartTokenFormatting = nameof(SmartTokenFormatting);
            public const string Snippets = nameof(Snippets);
            public const string SolutionExplorer = nameof(SolutionExplorer);
            public const string SourceGenerators = nameof(SourceGenerators);
            public const string SplitComment = nameof(SplitComment);
            public const string SplitStringLiteral = nameof(SplitStringLiteral);
            public const string StringIndentation = nameof(StringIndentation);
            public const string SuggestionTags = nameof(SuggestionTags);
            public const string SyncNamespaces = nameof(SyncNamespaces);
            public const string Tagging = nameof(Tagging);
            public const string TargetTypedCompletion = nameof(TargetTypedCompletion);
            public const string TextStructureNavigator = nameof(TextStructureNavigator);
            public const string TaskList = nameof(TaskList);
            public const string ToggleBlockComment = nameof(ToggleBlockComment);
            public const string ToggleLineComment = nameof(ToggleLineComment);
            public const string TypeInferenceService = nameof(TypeInferenceService);
            public const string UnusedReferences = nameof(UnusedReferences);
            public const string ValidateFormatString = nameof(ValidateFormatString);
            public const string ValidateJsonString = nameof(ValidateJsonString);
            public const string ValidateRegexString = nameof(ValidateRegexString);
            public const string Venus = nameof(Venus);
            public const string VsLanguageBlock = nameof(VsLanguageBlock);
            public const string VsNavInfo = nameof(VsNavInfo);
            public const string VsSearch = nameof(VsSearch);
            public const string WinForms = nameof(WinForms);
            public const string Workspace = nameof(Workspace);
            public const string XmlTagCompletion = nameof(XmlTagCompletion);
        }

        public const string Environment = nameof(Environment);
        public static class Environments
        {
            public const string VSProductInstall = nameof(VSProductInstall);
        }
    }
}
