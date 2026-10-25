# AC14 Pass-After (Issue #1007, P3-T4)

Timestamp: 2026-10-23T13-54
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true; then vstest.console.exe UtilitiesCS.Test.dll QuickFiler.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~Breadcrumb|FullyQualifiedName~OutlookFolderHierarchyProvider" (results under SCRATCH)
EXIT_CODE: 0
Output Summary: build `0 Error(s)`, `0 Warning(s)`. BREADCRUMB-FILTER run: total 957, executed 957, passed 957, failed 0 (BASELINE-FAILURES is empty). Every G1 test passed.

Note: the first pass-after run reported 6 failures in `BreadcrumbSelectorMessagesTests` (`SelectorKeyMessage_RoundTripsOnlySupportedKeys`, `ViewMessage_RoundTripsModeOpenAndStableIdentities`), which construct the messages through `Activator.CreateInstance` with the original argument lists; reflection does not supply optional parameters. The Phase 1 optional parameters on `BreadcrumbSelectorKeyMessage` and `BreadcrumbSelectorViewMessage` were replaced by chained constructor overloads (no test change), and the run above is the re-run.

| Outcome | Class | Test |
|---|---|---|
| Passed | OutlookFolderHierarchyProviderIssue1007Tests | MapNode_UnresolvableChildKey_HasChildrenFalse |
| Passed | OutlookFolderHierarchyProviderIssue1007Tests | GetImmediateSubfolders_ChildWithOnlyUnresolvableKeys_HasChildrenFalse |
| Passed | OutlookFolderHierarchyProviderIssue1007Tests | GetAncestorChain_ResolvableChildKey_HasChildrenTrue |
| Passed | BreadcrumbEmptyExpanderIssue1007Tests | EfcRenderer_ZeroChildrenFetch_RendersNoAffordance |
| Passed | BreadcrumbEmptyExpanderIssue1007Tests | EfcRenderer_ZeroChildrenDescendantFetch_RendersNoNodeAffordance |
| Passed | BreadcrumbEmptyExpanderIssue1007Tests | QfcProjection_ZeroChildrenFetch_EmitsNoPlusOrMinus |
| Passed | BreadcrumbEmptyExpanderIssue1007Tests | QfcProjection_ZeroChildrenDescendantFetch_ReportsNoChildren |
| Passed | BreadcrumbEmptyExpanderIssue1007Tests | EfcRenderer_ChildrenFetch_KeepsMinusAffordance |
