# Test Policy Audit, Refreshed after the Coverage Remediation (Issue #1007, P18-T4)

Timestamp: 2026-10-25T23-12
Command: per-file scan by a helper under SCRATCH: MSTest, FluentAssertions and Moq usings; `[TestMethod]`/`[DataTestMethod]` count; `// Arrange`, `// Act`, `// Assert` markers; ordinal counts of `Path.GetTemp`, `File.Write`, `File.Create`, `Thread.Sleep`, `Task.Delay`, `DoNotParallelize` and `new WebView2BreadcrumbHost`; the `<Compile Include>` entry for each new file; `#1007` references
EXIT_CODE: 0
Output Summary: 24 rows (11 TEST-CREATE C# files, 13 edited test files); every row passes. This supersedes test-policy-audit.2026-10-25T22-47.md. Since then, the P19 coverage remediation added 6 tests to `BreadcrumbSelectionSessionTreeIssue1007Tests.cs`, 3 to `FolderBreadcrumbBridgeRouterIssue1007Tests.cs` and 2 to `BreadcrumbBridgeRouterIssue1007Tests.DoubleClick.cs`, and 2 guard assertions to `BreadcrumbSelectorMessagesTests.cs`. Each new test has a summary citing #1007 (AC, decision or R reference), Arrange/Act/Assert markers, and no banned token. The seven banned tokens occur 0 times in all 24 files.

| File | MSTest | FluentAssertions | Moq | Test methods | AAA markers | Banned hits | Compile Include | #1007 refs | Result |
|---|---|---|---|---|---|---|---|---|---|
| UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbVisibleOrderTests.cs | yes | yes | n/a (no mocks) | 13 | 33 | 0 | yes | 1 | PASS |
| UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbSelectionSessionTreeIssue1007Tests.cs | yes | yes | n/a (no mocks) | 19 | 49 | 0 | yes | 1 | PASS |
| UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterIssue1007Tests.cs | yes | yes | yes | 17 | 51 | 0 | yes | 1 | PASS |
| UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderIssue1007Tests.cs | yes | yes | yes | 3 | 9 | 0 | yes | 1 | PASS |
| UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbEmptyExpanderIssue1007Tests.cs | yes | yes | n/a (no mocks) | 5 | 15 | 0 | yes | 1 | PASS |
| UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbBridgeJsKeysIssue1007Tests.cs | yes | yes | n/a (no mocks) | 6 | 6 (Assert-only string contracts) | 0 | yes | 1 | PASS |
| QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue1007Tests.cs | yes | yes | yes | 15 | 45 | 0 | yes | 1 | PASS |
| QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue1007Tests.Navigation.cs | yes | yes | n/a (partial; mocks in the main part) | 15 | 43 | 0 | yes | 1 | PASS |
| QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue1007Tests.DoubleClick.cs | yes | yes | yes | 7 | 21 | 0 | yes | 1 | PASS |
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
