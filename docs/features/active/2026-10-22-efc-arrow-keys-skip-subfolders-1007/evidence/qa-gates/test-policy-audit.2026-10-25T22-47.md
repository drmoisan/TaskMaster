# Test Policy Audit (Issue #1007, P18-T4)

Timestamp: 2026-10-25T22-47
Command: per-file scan by a helper under SCRATCH: MSTest, FluentAssertions and Moq usings; `[TestMethod]`/`[DataTestMethod]` count; `// Arrange`, `// Act`, `// Assert` markers; ordinal counts of `Path.GetTemp`, `File.Write`, `File.Create`, `Thread.Sleep`, `Task.Delay`, `DoNotParallelize` and `new WebView2BreadcrumbHost`; the `<Compile Include>` entry for each new file; `#1007` references
EXIT_CODE: 0
Output Summary: 24 rows (11 TEST-CREATE C# files, 13 edited test files); every row passes. Every file uses MSTest and FluentAssertions, and every file that builds a mock uses Moq. The seven banned tokens occur 0 times in all 24 files. Each new file has its `<Compile Include>`. Each rewritten test's summary cites #1007 (spot-checked per Disposition Register row during P8, P10, P12, P14 and P16). The node test file is outside this C# audit; it creates no files and uses no timers.

Audit fixes made in this task, both to test text only:
- `QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorIssue1007Tests.cs` had no Arrange/Act/Assert markers in its 18 tests: the earlier G5 tests and the G6 and G7 tests added in this run separated the phases with blank lines only. The two blank separators of each test became `// Act` and `// Assert`, and each body gained `// Arrange`. The file is now 477 physical lines and has 54 markers.
- `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbSelectorMessagesTests.cs` `SelectorKeyMessage_RoundTripsOnlySupportedKeys`, whose data rows P10-T4 extended, gained a summary citing #1007 AC8 and AC11.
The final toolchain pass (P19-T1 to P19-T4) runs after these fixes.

| File | MSTest | FluentAssertions | Moq | Test methods | AAA markers | Banned hits | Compile Include | #1007 refs | Result |
|---|---|---|---|---|---|---|---|---|---|
| UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbVisibleOrderTests.cs | yes | yes | n/a (no mocks) | 13 | 33 | 0 | yes | 1 | PASS |
| UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbSelectionSessionTreeIssue1007Tests.cs | yes | yes | n/a (no mocks) | 13 | 33 | 0 | yes | 1 | PASS |
| UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterIssue1007Tests.cs | yes | yes | yes | 14 | 42 | 0 | yes | 1 | PASS |
| UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderIssue1007Tests.cs | yes | yes | yes | 3 | 9 | 0 | yes | 1 | PASS |
| UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbEmptyExpanderIssue1007Tests.cs | yes | yes | n/a (no mocks) | 5 | 15 | 0 | yes | 1 | PASS |
| UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbBridgeJsKeysIssue1007Tests.cs | yes | yes | n/a (no mocks) | 6 | 6 (Assert-only string contracts) | 0 | yes | 1 | PASS |
| QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue1007Tests.cs | yes | yes | yes | 15 | 45 | 0 | yes | 1 | PASS |
| QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue1007Tests.Navigation.cs | yes | yes | n/a (partial; mocks in the main part) | 15 | 43 | 0 | yes | 1 | PASS |
| QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue1007Tests.DoubleClick.cs | yes | yes | yes | 5 | 15 | 0 | yes | 1 | PASS |
| QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorIssue1007Tests.cs | yes | yes | yes | 18 | 54 | 0 | yes | 1 | PASS |
| QuickFiler.Test/Viewers/FolderBreadcrumbKeysIssue1007Tests.cs | yes | yes | n/a (no mocks) | 8 | 13 | 0 | yes | 1 | PASS |
| QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.cs | yes | yes | yes | 11 | 35 | 0 | n/a | 6 | PASS |
| QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.Selection.cs | yes | yes | yes | 13 | 39 | 0 | n/a | 3 | PASS |
| QuickFiler.Test/Controllers/BreadcrumbBridgeRouterQueueTests.Part2.cs | yes | yes | yes | 12 | 36 | 0 | n/a | 1 | PASS |
| UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbRowStateTests.cs | yes | yes | n/a (no mocks) | 21 | 50 | 0 | n/a | 3 | PASS |
| UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelTests.cs | yes | yes | n/a (no mocks) | 23 | 63 | 0 | n/a | 2 | PASS |
| UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs | yes | yes | n/a (no mocks) | 14 | 36 | 0 | n/a | 5 | PASS |
| UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbSelectionSessionTests.cs | yes | yes | n/a (no mocks) | 9 | 25 | 0 | n/a | 1 | PASS |
| UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs | yes | yes | yes | 14 | 43 | 0 | n/a | 4 | PASS |
| UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterInFlightTests.cs | yes | yes | yes | 12 | 33 | 0 | n/a | 1 | PASS |
| UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbSelectorMessagesTests.cs | yes | yes | n/a (no mocks) | 17 | 40 | 0 | n/a | 6 | PASS |
| QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorTests.cs | yes | yes | yes | 16 | 44 | 0 | n/a | 4 | PASS |
| QuickFiler.Test/Viewers/BreadcrumbSelectorCoordinatorTests.cs | yes | yes | yes | 14 | 40 | 0 | n/a | 1 | PASS |
| QuickFiler.Test/Viewers/FolderBreadcrumbAssetContractTests.cs | yes | yes | yes | 15 | 18 | 0 | n/a | 3 | PASS |
