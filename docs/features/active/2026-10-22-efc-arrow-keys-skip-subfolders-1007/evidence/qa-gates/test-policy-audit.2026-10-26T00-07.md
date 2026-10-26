# Test Policy Audit (Issue #1007, remediation cycle 1, P5-T12)

Timestamp: 2026-10-26T00-07
Command: regex searches over the seven edited or new test files for the using directives and the banned tokens; `git status --porcelain --untracked-files=all` for new paths; a regex search of `<FEATURE>/evidence/` for drive-rooted paths and `Users` profile segments
EXIT_CODE: 0
Output Summary: PASS. All 7 rows and both evidence-form checks pass. The 7 files contain 0 banned tokens, and `<FEATURE>/evidence/` contains 0 drive-rooted paths and 0 `Users` profile segments.

Banned-token set searched: `Path.GetTemp`, `File.Write`, `File.Create`, `Thread.Sleep`, `Task.Delay`, `DoNotParallelize`, `ExcludeFromCodeCoverage`.

## Per-file rows

| File | MSTest + FluentAssertions usings | Moq where a mock is used | Arrange-Act-Assert in new/changed tests | XML summary citing #1007 on each new test | Banned tokens | Result |
|---|---|---|---|---|---|---|
| QuickFiler.Test/Viewers/FolderBreadcrumbKeysIssue1007Tests.cs | yes | no mock used | yes (`Stylesheet_StylesPendingDescendantLikeSelectedSubfolder`) | yes (#1007 AC4, B-1) | 0 | PASS |
| QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue1007Tests.DoubleClick.cs | yes | yes (`Moq`, `Times`) | yes (`LeafExpandToggle_OnRowCollapsedAwayFromActiveSegment_RestoresChain`) | yes (#1007 R3, N-1) | 0 | PASS |
| QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.cs | yes | yes | renames only (bodies unchanged) | n/a (no new test) | 0 | PASS |
| QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorIssue1007Tests.cs | yes | yes (strict provider mock in `Harness`) | modifier edit only | n/a (no new test) | 0 | PASS |
| QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorIssue1007Tests.Unhandled.cs | yes | the mock is built by the reused `Harness` | yes (`OpenUnhandledArrowLeft_OnExpandedPendingRow_RaisesUnhandledArrowFromRouterOutput`) | yes (#1007 R8, N-1) | 0 | PASS |
| UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs | yes | no mock used | yes (`RightArrow_OnRowCollapsedAwayFromActiveSegment_RestoresChain`); other changes are renames | yes (#1007 R3, N-1) | 0 | PASS |
| UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterIssue1007Tests.Unanchored.cs | yes | yes (`provider.Verify`, `Times.Never()`) | yes (`AffordanceToggle_OnRowCollapsedAwayFromActiveSegment_RestoresChainWithoutQuery`) | yes (#1007 R3, N-1) | 0 | PASS |

None of the new tests creates a file, sleeps, delays, reads the clock, or retries. No class carries `[DoNotParallelize]`. The isolation runs in P5-T9 and the full parallel run in P5-T4 passed.

## Evidence-form checks

| Check | Result |
|---|---|
| New paths ending in `.trx`, `.coverage`, or `.xml` (porcelain) | none. The raw trx and Cobertura copies stay under SCRATCH. `artifacts/csharp/coverage.xml` and `coverage/` are git-ignored and untracked. |
| Every new file under `<FEATURE>/evidence/` is Markdown | yes (every new evidence path ends in `.md`) |
| Drive-rooted path or `Users` profile segment under `<FEATURE>/evidence/` | 0 matches |
