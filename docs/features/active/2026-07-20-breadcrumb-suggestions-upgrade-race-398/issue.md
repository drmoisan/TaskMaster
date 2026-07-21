# breadcrumb-suggestions-upgrade-race (Issue #398)

- Date captured: 2026-07-20
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/breadcrumb-suggestions-upgrade-race/ (Issue #398)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #398
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/398
- Last Updated: 2026-07-21
- Work Mode: minor-audit

## Summary

`BreadcrumbBridgeCoordinator.SetSuggestions` starts a fire-and-forget rebuild (`UpgradeSuggestionsAsync` -> `FolderBreadcrumbBridgeRouter.SetSuggestionsAsync`) that calls `_model.Clear()` synchronously and re-adds rows one at a time on thread-pool continuations. The subsequent host call `SetFolderSelectedIndex(1)` in `QfcItemController.AssignFolderComboBox` races the rebuild: when the model transiently holds fewer than two rows, `BreadcrumbStateModel.SelectRow(1)` throws `ArgumentOutOfRangeException` even though `FolderArray.Length > 1`, aborting the QuickFiler load.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Runtime: .NET Framework 4.8 VSTO add-in (TaskMaster / QuickFiler)
- Command/flags used: Outlook ribbon action "QuickFiler High Confidence" (`RibbonViewer.QuickFilerHighConfidence_Click`)
- Data source or fixture: Live mailbox item whose folder predictor returns two or more folder suggestions

## Steps to Reproduce

1. Launch QuickFiler in high-confidence mode from the TaskMaster ribbon.
2. Load an email whose `FolderPredictor` produces a `FolderArray` with two or more entries and no predetermined folder match, where the injected `IFolderHierarchyProvider` resolves ancestor chains asynchronously (not synchronously from cache).
3. `QfcItemController.AssignFolderComboBox` calls `SetFolderSuggestions(FolderRowArray)`; the coordinator's `UpgradeSuggestionsAsync` clears the breadcrumb model and begins re-adding rows on thread-pool continuations.
4. Before the rebuild completes, `AssignFolderComboBox` calls `_itemViewer.SetFolderSelectedIndex(1)` (the multi-suggestion fallback), which reaches `BreadcrumbStateModel.SelectRow(1)` while the model transiently holds only one row.

## Expected Behavior

The breadcrumb model's row population is stable (or the selection is sequenced after population), so the index-1 fallback selection succeeds whenever `FolderArray.Length > 1`, and QuickFiler loading completes without error.

## Actual Behavior

`System.ArgumentOutOfRangeException` is thrown from `BreadcrumbStateModel.SelectRow` and propagates up through the QuickFiler load sequence as an unhandled exception:

```
Message=Row selection requires -1 or an index in [0, 0].
Parameter name: index
Actual value was 1.
  at UtilitiesCS.OutlookObjects.Folder.BreadcrumbStateModel.SelectRow(Int32 index) in BreadcrumbStateModel.cs:line 237
  at QuickFiler.Viewers.BreadcrumbBridgeCoordinator.SelectRow(Int32 index) in BreadcrumbBridgeCoordinator.cs:line 127
  at QuickFiler.Controllers.QfcItemController.AssignFolderComboBox() in QfcItemController.FolderHandling.cs:line 202
```

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: stack trace above (source: user-supplied crash report, 2026-07-20, post-#392-fix build).

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

High: the exception aborts the QuickFiler high-confidence load path intermittently whenever the hierarchy provider resolves asynchronously; the #392 index clamp does not protect this path because the mismatch is between `FolderArray.Length` and the transient breadcrumb row count.

## Suspected Cause / Notes

Root cause confirmed by code inspection (session 2026-07-20):

- `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs:78-93` — `SetSuggestions` populates N plain rows synchronously (`_router.SetItems`), then assigns `SuggestionsUpgrade = UpgradeSuggestionsAsync(rows)` without awaiting it.
- `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs:43-83` — `SetSuggestionsAsync` calls `_model.Clear()` synchronously before its first `await` (`ResolveLeafKeyAsync` with `ConfigureAwait(false)`), so control returns to the caller with the model emptied; rows are re-added one at a time on thread-pool continuations.
- `QuickFiler/Controllers/QfcItemController.FolderHandling.cs:182-205` — `AssignFolderComboBox` then calls `FolderContains(...)` and `SetFolderSelectedIndex(FolderArray.Length == 1 ? 0 : 1)` on the UI thread against whatever transient row count the rebuild has reached. A transient count of exactly 1 yields the reported `[0, 0]` message with actual value 1.
- Secondary concern: `BreadcrumbStateModel` (plain `List`-backed, `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs`) is mutated from thread-pool continuations while the UI thread reads and mutates it, with no synchronization.
- The doc comment on `SetSuggestions` claims the selection contract "holds without awaiting the provider"; the synchronous `Clear()` inside the unawaited upgrade violates that guarantee.
- `UpgradeSuggestionsAsync` captures the selected index before the rebuild to restore it afterward, but host selection happens after `SetSuggestions` returns, so the ordering assumption is backwards for this call site.
- Distinct from issue #392 (deterministic single-suggestion index clamp, fixed by PR #393); this defect requires `FolderArray.Length > 1` plus an asynchronous provider.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: preferred fix — eliminate the mid-rebuild empty window in `FolderBreadcrumbBridgeRouter.SetSuggestionsAsync` by building the upgraded rows into a local list and swapping them into the model atomically at the end (no up-front `Clear()`), preserving the readback contract (`FolderContains` / `GetSelectedFolder` / `SelectRow`) at every instant during the upgrade. Add MSTest regression tests with a deliberately-delayed fake `IFolderHierarchyProvider` proving: (a) `SelectRow(1)` succeeds mid-upgrade with N>=2 suggestions; (b) row count never drops below the pre-upgrade count during the upgrade; (c) the selected index survives the swap.
- [ ] Integration scenario to retest: QuickFiler high-confidence load with multi-suggestion items against a live (asynchronous) hierarchy provider.
- [x] Manual verification notes: re-run the ribbon "QuickFiler High Confidence" action repeatedly against multi-suggestion items after the fix; the `ArgumentOutOfRangeException` must not recur.

## Acceptance Criteria

- [x] AC-1: A deterministic MSTest regression test reproduces the defect: with two or more suggestion rows and an in-flight `UpgradeSuggestionsAsync` rebuild (fake `IFolderHierarchyProvider` gated by `TaskCompletionSource`, no timing sleeps), `SelectRow(1)` throws `ArgumentOutOfRangeException` before the fix and succeeds after. No temporary files, external dependencies, or wall-clock waits are used.
- [x] AC-2: `FolderBreadcrumbBridgeRouter.SetSuggestionsAsync` no longer exposes a transient cleared or partially-populated model: upgraded rows are built into a local collection and swapped into `BreadcrumbStateModel` atomically at the end, so the observable row count never drops below the pre-upgrade count while the upgrade is in flight.
- [x] AC-3: The breadcrumb readback contract (`FolderContains`, `GetSelectedFolder`, `GetFolderItems`, `SelectRow`) returns pre-upgrade-consistent results at every point during an in-flight upgrade, and the host-selected index survives the swap (the `UpgradeSuggestionsAsync` re-selection preserves a selection made after `SetSuggestions` returned).
- [x] AC-4: Completed-upgrade behavior is unchanged: suggestion rows carry ancestor chains and probabilities, unresolvable scored rows fall back to plain rows, non-scored rows remain plain verbatim rows, and all existing `FolderBreadcrumbBridgeRouter` / `BreadcrumbBridgeCoordinator` / `QfcItemController` tests continue to pass.
- [x] AC-5: The full C# toolchain passes in order (CSharpier format, .NET analyzers build, nullable build, MSTest via vstest.console.exe) with zero regressions relative to the Phase 0 baseline, and new/changed code meets the >= 90% coverage target.
  - Coverage sub-clause confirmed (remediation 2026-07-20T22-30): the canonical HEAD JaCoCo artifact was regenerated at `artifacts/csharp/coverage.xml` (first-party denominator UtilitiesCS + QuickFiler). Verified via the gate hook functions `Get-JacocoRepoCoverage` / `Get-JacocoBranchCoverage`: line 86.54% (>= 85%), branch 80.85% (>= 75%). Full suite 5061/5061 passing; CSharpier/analyzer/nullable gates green. Test-only remediation (R1 partial-class splits), so production coverage is unchanged and the prior fix's new-code coverage (100%) is unaffected.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [x] Move to active fix folder / branch
