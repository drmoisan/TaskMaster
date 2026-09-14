# Phase 6 — POST-CHANGE coverage measurement and AC4 comparison, PARALLEL regime (P6-T6)

Task: [P6-T6]
Timestamp: 2026-09-13T03-49

## Command 1 — coverage runner (post-change)

Timestamp: 2026-09-13T03-49
Command: `pwsh -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot QuickFiler.Test -Configuration Debug -CoverageOutput coverage\743-postchange.cobertura.xml` Run from the item worktree root via Set-Location inside one pwsh invocation, with all output streams redirected to the ignored path `coverage\p6-t6-runner.log`. Run while holding the shared machine build lock for item 743 (acquired 03:49:37, released 03:50:13 immediately after the command returned). Outlook was closed; no induced load. Same command shape as P0-T9, changing only the output path.
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary:
- `Discovered 1 test assemblies.` (QuickFiler.Test\bin\Debug\QuickFiler.Test.dll)
- `Test Run Failed.` / `Total tests: 1400` / `Passed: 1397` / `Failed: 3` / `Total time: 14.0103 Seconds`
- Non-zero exit is case (2) of the P0-T9 task text: the runner threw at its line 236 with the quoted output line `MSTest with coverage failed with exit code 1`, BEFORE the post-processing at line 342, so `Post-processing coverage XML for Koverage compatibility...` was NOT printed and the document on disk carried absolute filenames. The case-(2) manual post-processing fallback was therefore applied (Command 3 below) before the extraction.
- Failed test names transcribed from the runner output (all three in class `QfcInitEmailQueueZeroBatchTests`, a class outside this item's Write Set that no task of this plan edits):
  - `InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing`
  - `InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker`
  - `InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop`
  - Message (identical for all three): `System.TypeInitializationException: The type initializer for 'Deedle.Reflection' threw an exception. ---> System.TypeInitializationException: The type initializer for '<StartupCode$Deedle>.$FrameUtils' threw an exception. ---> System.IO.FileNotFoundException: Could not load file or assembly 'netstandard, Version=2.1.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51' or one of its dependencies. The system cannot find the file specified.`
  - Classification: the same known-intermittent Deedle assembly-binding failure under class-level parallelism that the P4-T3 artifact recorded (run B) and that passed on its re-run there; the same three tests passed in the P6-T5 SERIAL run of this pass a minute earlier (`failed=0` over 1400 tests) and in the P0-T9 PARALLEL run. None of the seven Write Set files references Deedle. It is environmental and out of scope; no file was edited in response, and it is reported to the caller. It does not affect the two measured controller partials, which are exercised by other classes.
- REGIME: PARALLEL (the runner appends /Settings: resolved to the CLI runsettings file under the scripts directory, which declares Workers 0 and Scope ClassLevel).

## Command 3 (case-(2) fallback) — manual Koverage post-processing

Timestamp: 2026-09-13T03-50
Command: `pwsh -Command '. .\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1; $raw = Get-Content coverage\743-postchange.cobertura.xml -Raw -Encoding UTF8; $processed = ConvertTo-KoverageCoberturaXml -XmlContent $raw -RepoRoot (Get-Location).Path; Set-Content -Path coverage\743-postchange.cobertura.xml -Value $processed -Encoding UTF8 -NoNewline; Write-Output "POSTPROCESSED-MANUALLY"'` Run from the item worktree root via Set-Location inside one pwsh invocation (inner quoting inverted to single quotes; semantics identical). The function is declared at line 406 of the helpers file, which dot-sources its four sibling helper files at its lines 2-5.
EXIT_CODE: 0
Output Summary: `POSTPROCESSED-MANUALLY` was printed and the document was rewritten with workspace-relative backslash filenames, which the extraction below then matched (a `classNodes=1` match for each of the two filenames is only possible after post-processing). A diagnostic `$LASTEXITCODE` read that the executor appended after the plan's command body threw under the helper file's StrictMode because no native executable had run inside that invocation; it is not part of the plan's command and does not affect the written document.

## Command 2 — per-file Cobertura extraction (Command Reference)

Timestamp: 2026-09-13T03-51
Command: the per-file extraction span from the plan's Command Reference, verbatim, with `coverage\743-postchange.cobertura.xml` as the input path, plus one leading `Write-Output` of the root `line-rate`, `lines-valid` and `lines-covered` attributes. Run from the item worktree root via Set-Location inside one pwsh invocation (inner quoting inverted to single quotes; semantics identical).
EXIT_CODE: 0
Output Summary:
- Root attributes: `line-rate=0.241516`, `lines-valid=61855`, `lines-covered=14939`.
- Per-file figures printed by the extraction:
```
QuickFiler\Controllers\QfcItemController.ViewerSetup.cs classNodes=1 linesValid=213 linesCovered=193 rate=0.906103
QuickFiler\Controllers\QfcItemController.Initialization.cs classNodes=1 linesValid=262 linesCovered=249 rate=0.950382
```
- REGIME: PARALLEL (the runner appends /Settings: resolved to the CLI runsettings file under the scripts directory, which declares Workers 0 and Scope ClassLevel).

## AC4 items

### (i) Pre-change and post-change figures side by side, both measured fresh in this session with this same command

| File | Run | classNodes | linesValid | linesCovered | rate |
|---|---|---|---|---|---|
| QuickFiler/Controllers/QfcItemController.ViewerSetup.cs | pre-change (P0-T9, 2026-09-13T02-26) | 1 | 210 | 190 | 0.904762 |
| QuickFiler/Controllers/QfcItemController.ViewerSetup.cs | post-change (P6-T6, 2026-09-13T03-49) | 1 | 213 | 193 | 0.906103 |
| QuickFiler/Controllers/QfcItemController.Initialization.cs | pre-change (P0-T9, 2026-09-13T02-26) | 1 | 262 | 249 | 0.950382 |
| QuickFiler/Controllers/QfcItemController.Initialization.cs | post-change (P6-T6, 2026-09-13T03-49) | 1 | 262 | 249 | 0.950382 |

Both measurements were taken in this session (2026-09-13, same worktree, same machine) with the same command `pwsh -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot QuickFiler.Test -Configuration Debug -CoverageOutput <path>` and the same per-file extraction. Neither of the two disagreeing committed cross-session figures (0.950382 and 0.904762 in the 2026-09-08 report; 1.0 and 0.863014 in the 2026-09-06 report) was used as the baseline; the P0-T9 measurement of this session is the sole baseline.

### (ii) Denominator

The denominator for each file is the set of lines the merged Cobertura document reports for that filename in the P0-T9 pre-change run of this session: 210 distinct line numbers for `QuickFiler\Controllers\QfcItemController.ViewerSetup.cs` and 262 for `QuickFiler\Controllers\QfcItemController.Initialization.cs` (grouped on line number, maximum hit per line, per the Command Reference arithmetic). The web-view initialization member `InitializeWebViewAsync` is excluded by attribute at line 47 of `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` (`[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]`) and therefore contributes nothing to either denominator, so an edit confined to it cannot move the figure. This item made no edit to that member.

### (iii) Post-change rate for each file

- `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`: 0.906103 (pre-change 0.904762; post >= pre).
- `QuickFiler/Controllers/QfcItemController.Initialization.cs`: 0.950382 (pre-change 0.950382; post >= pre, equal).

### (iv) linesValid delta and accounting

- `QuickFiler/Controllers/QfcItemController.Initialization.cs`: delta 0 (262 to 262). The file is measured but not edited by this item; the anchored diff against `refs/plan/issue-743-base` does not list it.
- `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`: delta +3 (210 to 213), with `linesCovered` also +3 (190 to 193), so the uncovered-line count is unchanged at 20 and every added measurable line is covered. Accounting, from a line-level comparison of the two documents against the anchored diff: in the pre-change document the `AssignControlsAsync` marshal region contributed four measured lines, 371-374 (the three-line `await _itemViewer.UiDispatcher.InvokeAsync(() => AssignControls(...));` statement plus the member's closing brace), all hit; in the post-change document the same region contributes seven measured lines, 377-381, 384 and 385 (`var dispatcher = _uiDispatcher;`, `if (dispatcher is null)`, its opening brace, `AssignControls(itemInfo, viewerPosition);`, `return;`, `await dispatcher.InvokeAsync(() => AssignControls(itemInfo, viewerPosition));`, and the member's closing brace), all hit. Net +3 measurable lines, introduced by the P3-T2 null-tolerant seam conversion. The P3-T3 comment lines (4 lines inserted before the widened member) and the two P3-T1 identifier substitutions add no measurable line; the comment shifts every later line by +4 without changing the count. The null branch is exercised by the existing `AssignControlsAsync_DispatchesAssignThroughViewerDispatcher` test and the seam path by `AssignControlsAsync_WithSyncDispatcherDouble_AssignsThroughTheInjectedSeam`, which is why all seven post-change lines report hits. No logic was extracted out of the excluded method, so the excluded-method mechanism spec AC4 (iv) warns about did not operate here.

### (v) Pass outcome of each test named in the spec section 7 disposition table

Drawn from the P6-T5 artifact `evidence/qa-gates/final-serial-test-run.2026-09-12T19-30.md` (SERIAL regime, `total=1400 passed=1400 failed=0 timeout=0`), every test named in the section 7 disposition table exists in the post-change tree and passed:

- 7.1 seam tests, class `QfcItemController_SeamMarshallingTests` (5): all Passed.
- 7.1 deterministic mechanism regression test `TransactionGate_WhileThisTestHoldsATransaction_HasExactlyOneUnreleasedAcquisition`: Passed.
- 7.1 the six existing gate tests in `QfcItemController_UiThreadDispatcherFixtureTests`: all Passed.
- 7.2 the pump-hosted initialization tests in the Part3 file (class `QfcItemController_InitializationTests`, including the five `ThroughThePumpHost` tests and the two `BuildPumpHarness` tests): all Passed.
- 7.2 the two pump-hosted seam-factory tests `CreateSequentialAsync_WithInjectedSeams_ReturnsAnInitializedController` and `CreateAsync_WithFaultingWebViewSeam_FaultsWithThatExceptionAfterInitializing`: Passed.
- 7.2 the one pump-hosted test in the ViewerSetup test file `ResolveControlGroupsAsync_ThroughThePumpHost_PopulatesTipsAndControlGroups`: Passed.
- 7.2 the eight breadcrumb-host tests in `WebView2BreadcrumbHostTests`: all Passed.
- 7.2 the reflection contract tests in `ItemViewerBreadcrumbDropDownContractTests` (14, including `IItemViewer_StillDeclaresUiDispatcher` and `IItemViewer_StillDeclaresUiSyncContext`): all Passed.

The per-test rows with durations are in the P6-T5 artifact.

## Concrete-viewer coverage

No acceptance condition in this plan is phrased over concrete-viewer coverage: the type carries a type-level exclusion at line 20 of QuickFiler/Viewers/ItemViewer.cs and emits no Cobertura element at all.

## Acceptance

Post-change rate >= pre-change rate for both files (0.906103 >= 0.904762; 0.950382 >= 0.950382); all five AC4 items are present above; the required sentence on concrete-viewer coverage is recorded. The raw Cobertura documents remain under the ignored `coverage` directory and are discarded in P6-T18 (D1).
