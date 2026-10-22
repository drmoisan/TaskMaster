# Group C Pass-After: QFC Nested Affordance and Indent (#1002)

Timestamp: 2026-10-22T10-14
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true; vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~FolderBreadcrumbNestingIssue1002Tests|FullyQualifiedName~FolderBreadcrumbAssetContractTests|FullyQualifiedName~FolderBreadcrumbRecursiveAssetIssue997Tests" /ResultsDirectory:SCRATCH/results-issue-1002-p7t2 /Logger:"trx;LogFileName=issue-1002-p7t2.trx"; node --test --test-isolation=none --test-concurrency=1 --test-reporter=spec tests/quickfiler-breadcrumb/issue-1002-nesting.test.mjs
EXIT_CODE: 0
Output Summary:
- Build: `0 Error(s)`, `0 Warning(s)`.
- C# (derived from the trx): total 22, executed 22, passed 22, failed 0 across FolderBreadcrumbNestingIssue1002Tests, FolderBreadcrumbAssetContractTests, and FolderBreadcrumbRecursiveAssetIssue997Tests.
- Node: tests 2, pass 2, fail 0.
- Change confined to `makeSubfolders` in `QuickFiler/Resources/FolderBreadcrumb.js`: trailing `depth` parameter (zero when absent), `data-depth` and per-depth `padding-left` set on each `.sub`, name appended before the toggle block, recursive call `makeSubfolders(row, child.children, selectedChildOwnsActive, depth + 1)`. `render`, `makeRow`, the message listener, `FolderBreadcrumb.html`, and `Resources.resx` are unchanged; no function added.

| Outcome | Test | FB |
|---|---|---|
| Passed | MakeSubfolders_AppendsNameBeforeToggle | FB |
| Passed | MakeSubfolders_RecursiveCallPassesDepthPlusOne | FB |
| Passed | MakeSubfolders_SetsDataDepthAndPerDepthPadding | FB |
| Passed | MakeSubfolders_PinnedRecursiveCallPrefixPreserved |  |
| Passed (node) | nested toggle follows the name at every depth | FB |
| Passed (node) | nested rows carry data-depth and per-depth padding | FB |
