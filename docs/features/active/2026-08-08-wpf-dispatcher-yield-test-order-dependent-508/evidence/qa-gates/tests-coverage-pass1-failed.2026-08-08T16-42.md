# Toolchain Step 4 (test with coverage) — PASS 1, FAILED

Timestamp: 2026-08-08T16-42

Task: [P2-T5] — final QC loop, pass 1 (FAILED; loop restarted at P2-T1)

Command: `pwsh -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput "docs/features/active/2026-08-08-wpf-dispatcher-yield-test-order-dependent-508/evidence/qa-gates/coverage-postchange.cobertura.xml"`

EXIT_CODE: 1

```
Discovered 9 test assemblies.
Total tests: 6295
     Passed: 6293
     Failed: 2
Test Run Failed.
```

This artifact records a failed pass honestly rather than discarding it. The loop restarted at
P2-T1; the passing evidence is in the pass-2 artifacts.

## Discovery assertion — PASS

```
DISCOVERED_COUNT=9
OUTSIDE_WORKSPACE_ROOT_COUNT=0
NESTED_WORKTREE_SEGMENT_COUNT=0
```

All 9 assemblies under the workspace-root prefix; no stale sibling-worktree build.

## Test count reconciliation

| Run | Total |
|---|---|
| Baseline (P0-T10) | 6293 |
| Post-change (this run) | 6295 |

Delta +2, exactly the two tests added by P1-T10 and P1-T11
(`YieldAsync_ThreadAffinitizedDispatcherPresent_YieldsWithoutFallback`,
`YieldAsync_ThreadDispatcherAbsent_FallsBackToProcessGlobalDispatcher`). No test was deleted,
skipped, or ignored.

## The two failures are out of scope

Both failures are in `QuickFiler.Test`, in `QfcItemController_InitializationTests`:

```
Failed InitializeBool_ThroughThePumpHost_CompletesAndInitializesState [237 ms]
Failed InitializeNineArgOverload_ThroughThePumpHost_SavesParametersAndDelegates [117 ms]

System.InvalidOperationException: Invoke or BeginInvoke cannot be called on a control
until the window handle has been created.
   at System.Windows.Forms.Control.MarshaledInvoke(...)
   at QuickFiler.Controllers.QfcItemController.InvokeBeginInvoke(Boolean async, Action action)
        in QuickFiler\Controllers\QfcItemController.FocusAndTheme.cs:line 256
   at QuickFiler.Controllers.QfcItemController.ToggleTips(Boolean async, ToggleState desiredState)
        in QuickFiler\Controllers\QfcItemController.FocusAndTheme.cs:line 204
   at QuickFiler.Controllers.QfcItemController.Initialize(Boolean async)
        in QuickFiler\Controllers\QfcItemController.Initialization.cs:line 185
```

Assessment:

- This is a WinForms **window-handle-creation race** in a `WinFormsPumpHost` harness
  (`QuickFiler.Test/TestSupport/WinFormsPumpHost.cs:95`). It has no relationship to
  `WpfDispatcherYield`, to WPF `Dispatcher`, or to either changed file.
- Neither failing test, nor any code in its stack, is in the scoped diff. The entire `.cs` diff is
  the two in-scope files (P1-T15).
- The failure is pre-existing flakiness of exactly the kind this issue documents.
  `<FEATURE>/issue.md:50-54` records two consecutive baseline runs at merge-base `003c5715` with
  `Failed: 2` and `Failed: 1` respectively — the suite is not reliably green at baseline, which is
  the stated motivation for issue #508 in the first place.

## All four in-scope tests PASSED in this run

```
Passed YieldAsync_CanceledToken_ThrowsBeforeDispatcherYield [1 ms]
Passed YieldAsync_ThreadAffinitizedDispatcherPresent_YieldsWithoutFallback [35 ms]
Passed YieldAsync_ThreadDispatcherAbsent_FallsBackToProcessGlobalDispatcher [13 ms]
Passed YieldAsync_WithoutDispatcher_RemainsStrict [1 ms]
```

The defect under repair did not recur.

## Loop action taken

`.claude/rules/general-code-change.md` and the execution directive require restarting the loop at
step 1 on **any** failure. The step failed (EXIT_CODE 1), so the loop restarts at P2-T1 regardless
of the failures being out of scope. The gate is not weakened, no test is quarantined, and no
`[Ignore]` or filter was added to route around the failures.

Output Summary: FAILED, EXIT_CODE 1. Full suite Total 6295 / Passed 6293 / Failed 2. The +2 total
versus the 6293 baseline is exactly the two tests added by P1-T10 and P1-T11. Both failures are
out-of-scope pre-existing `QuickFiler.Test` WinForms handle-creation flakiness
(`QfcItemController_InitializationTests`, "Invoke or BeginInvoke cannot be called on a control until
the window handle has been created"), unrelated to either changed file; the issue itself records
`Failed: 2` and `Failed: 1` at baseline. All four `WpfDispatcherYieldTests` passed. Per the loop
rule the toolchain restarts at P2-T1; this pass is recorded as failed and is not counted toward
P2-T6.
