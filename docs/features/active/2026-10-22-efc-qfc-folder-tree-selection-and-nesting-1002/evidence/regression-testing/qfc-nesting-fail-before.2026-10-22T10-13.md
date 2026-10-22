# Group C Fail-Before: QFC Nested Affordance and Indent (#1002)

Timestamp: 2026-10-22T10-13
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true; vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~FolderBreadcrumbNestingIssue1002Tests" /ResultsDirectory:SCRATCH/results-issue-1002-p6t3 /Logger:"trx;LogFileName=issue-1002-p6t3.trx"; node --test --test-isolation=none --test-concurrency=1 --test-reporter=spec tests/quickfiler-breadcrumb/issue-1002-nesting.test.mjs
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary:
- Build: `0 Error(s)`, `0 Warning(s)` (the rebuild re-embeds the unchanged script resource).
- C# (derived from the trx): total 4, executed 4, passed 1, failed 3; vstest EXIT_CODE 1.
- Node: tests 2, pass 0, fail 2; node EXIT_CODE 1.
- Every Group C fail-before test is reported failing; `MakeSubfolders_PinnedRecursiveCallPrefixPreserved` is listed with its outcome (Passed).

| Outcome | Test | FB | Message (truncated) |
|---|---|---|---|
| Failed | MakeSubfolders_AppendsNameBeforeToggle | FB | Expected name to be less than 1694 because the +/- follows the folder name, but found 1748. |
| Failed | MakeSubfolders_RecursiveCallPassesDepthPlusOne | FB | Expected passesDepth to be True because each nested level is one step deeper than its parent, but found False. |
| Failed | MakeSubfolders_SetsDataDepthAndPerDepthPadding | FB | Expected the makeSubfolders body to contain "data-depth"; it did not. |
| Passed | MakeSubfolders_PinnedRecursiveCallPrefixPreserved |  |  |
| Failed (node) | nested toggle follows the name at every depth | FB | AssertionError: the toggle of folder-option-0-node-Alpha follows its name (actual false, expected true) |
| Failed (node) | nested rows carry data-depth and per-depth padding | FB | AssertionError: Expected values to be strictly equal (actual null, expected '0') |
