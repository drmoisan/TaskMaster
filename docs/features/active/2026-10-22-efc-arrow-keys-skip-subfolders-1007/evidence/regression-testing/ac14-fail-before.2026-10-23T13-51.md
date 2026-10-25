# AC14 Fail-Before (Issue #1007, P2-T3)

Timestamp: 2026-10-23T13-51
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true; then vstest.console.exe UtilitiesCS.Test.dll QuickFiler.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~OutlookFolderHierarchyProviderIssue1007Tests|FullyQualifiedName~BreadcrumbEmptyExpanderIssue1007Tests" (results directory and trx under SCRATCH)
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary: build `0 Error(s)`, `0 Warning(s)`. Tests: total 8, executed 8, passed 2, failed 6. Every G1 non-guard test failed on its assertion; both guards passed. Summary derived from the trx; the trx is not committed.

| Outcome | Test | Failure message (from the trx) |
|---|---|---|
| Failed | MapNode_UnresolvableChildKey_HasChildrenFalse | Expected chain.Last().HasChildren to be False because Acme's only child key does not resolve in the snapshot, but found True. |
| Failed | GetImmediateSubfolders_ChildWithOnlyUnresolvableKeys_HasChildrenFalse | Expected children[0].HasChildren to be False because the subfolder fetch for Acme would return zero children, but found True. |
| Passed (guard) | GetAncestorChain_ResolvableChildKey_HasChildrenTrue | - |
| Failed | EfcRenderer_ZeroChildrenFetch_RendersNoAffordance | Did not expect html (row fragment) to contain `data-role="leaf"` because the fetch attached zero children. |
| Failed | EfcRenderer_ZeroChildrenDescendantFetch_RendersNoNodeAffordance | Did not expect html (row fragment) to contain `data-role="node"` because the node's fetch returned no child. |
| Failed | QfcProjection_ZeroChildrenFetch_EmitsNoPlusOrMinus | Expected row.Cells kinds {Segment, Arrow, Segment, Minus} to not contain BreadcrumbCellKind.Minus. |
| Failed | QfcProjection_ZeroChildrenDescendantFetch_ReportsNoChildren | Expected projected.HasChildren to be False because the node's fetch returned no child, but found True. |
| Passed (guard) | EfcRenderer_ChildrenFetch_KeepsMinusAffordance | - |
