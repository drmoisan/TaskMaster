# Phase 1 — Fail-Before Regression Evidence (P1-T3) [expect-fail]

Timestamp: 2026-07-20T22-05

Command: `vstest.console.exe QuickFiler.Test/bin/Debug/QuickFiler.Test.dll /Tests:SelectRow_WhileSuggestionsUpgradeInFlight_DoesNotThrowAndAppliesSelection`
(QuickFiler.Test rebuilt at Configuration=Debug/Platform=AnyCPU before the run; production fix NOT yet applied.)

EXIT_CODE: 1

Output Summary:
- Total tests: 1. Failed: 1. (Expected pre-fix failure.)
- Failure is the exact defect signature from issue #398:
  `Did not expect System.ArgumentOutOfRangeException, but found System.ArgumentOutOfRangeException: Row selection requires -1 or an index in [0, 0].`
  `Actual value was 1.`
- Mechanism: `BreadcrumbBridgeCoordinator.SetSuggestions` starts the fire-and-forget `UpgradeSuggestionsAsync` -> `FolderBreadcrumbBridgeRouter.SetSuggestionsAsync`, which calls `_model.Clear()` before its first `await`, then re-adds rows on continuations. The TaskCompletionSource-gated fake `IFolderHierarchyProvider` parks the rebuild after adding exactly one row (first path resolves from a completed task, second path's leaf-key resolve is gated), so the host `SelectRow(1)` reaches `BreadcrumbStateModel.SelectRow(1)` against a transient 1-row model and throws.
- This artifact is the pre-fix half of AC-1's fail-before / pass-after pair; the pass-after run is recorded in pass-after.2026-07-20T21-41.md.
