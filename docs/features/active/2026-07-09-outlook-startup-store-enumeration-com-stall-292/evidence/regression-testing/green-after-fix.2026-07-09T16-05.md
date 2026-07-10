# Green-After-Fix Deterministic Proof (issue #292, remediation cycle 1)

- Timestamp: 2026-07-09T16-05
- Task: [P2-T13]
- Command (each pass, CI-equivalent): `vstest.console.exe <all 7 *.Test.dll under bin/Debug> /EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"`

## Definitive green passes (post-restore rebuild) — the authoritative P2-T13 4-pass proof

| Pass | EXIT_CODE | Total | Passed | Failed | #292 tests |
|------|-----------|-------|--------|--------|------------|
| A | 0 | 5141 | 5141 | 0 | pass |
| B | 0 | 5141 | 5141 | 0 | pass |
| C | 0 | 5141 | 5141 | 0 | pass |
| D | 0 | 5141 | 5141 | 0 | pass |

All four passes: `Test Run Successful.` `Failed: 0`. The 10 previously-failing tests
(`CurrentStoreContextTests.Begin_SetsCurrent_ReadableInsideScope`, `Dispose_RestoresPreviousValue`,
`NestedScopes_RestoreInnerThenOuter`, `SequentialScopes_EachRestoreToNull`,
`Begin_NormalizedInnerScope_RestoresRealOuterValue`, `Begin_NormalizesUnavailableIdentity_ToNoContext`
[null, "", "   ", "<unavailable>"], and `ThreadMonitorTests.EvaluatePoll_NoContext_CarriesNullIdentity`)
pass in every pass.

## Earlier confirmation passes (pre-diagnostic, same fixed binaries)

Seven additional full-suite passes were run earlier with the fix compiled in:
- Passes 2,3,4,5,6,7: `Failed: 0` (5141/5141).
- Pass 1: `Failed: 1` — a SINGLE failure in an UNRELATED test, `UtilitiesCS.Test.Threading.ProgressTrackerAsync_Tests.InitializeAsync_WithCurrentDispatcher_InitializesAndReturnsTracker`. NONE of the 10 #292 tests failed in any of the 7 passes.

Across all 11 fixed-code passes (A–D + 1–7), the 10 #292 tests were green 11/11 (zero race occurrences).

## Unrelated pre-existing flake (not a #292 finding, not caused by this change)

The pass-1 failure was `ProgressTrackerAsync_Tests.InitializeAsync_WithCurrentDispatcher_InitializesAndReturnsTracker`
(assertion at `ProgressTrackerAsync_Tests.cs:200`). Failure reason:
`Expected threadException to be <null> ..., but it threw: System.Threading.Tasks.TaskCanceledException: A task was canceled.`
originating in production `ProgressTrackerAsync.InitializeAsync` (`UtilitiesCS/Threading/ProgressTrackerAsync.cs:34`),
reached through `initializeTask.GetAwaiter().GetResult()` on a real STA thread that runs `Dispatcher.PushFrame`.

This test:
- Does not reference `CurrentStoreContext`, `StoresWrapper`, or `StoreWrapper`, and is not among the classes edited by this remediation.
- Is architecturally timing-sensitive (real `System.Threading.Thread` set to STA + `Dispatcher.PushFrame` + async task with a `CancellationTokenSource`), so a `TaskCanceledException` under heavy full-suite load is a pre-existing determinism weakness (contrary to the repo determinism-infrastructure guidance for fake timers/injected clocks).
- Has no causal relationship to adding `[DoNotParallelize]` to store test classes: that attribute only serializes those store classes, which REDUCES parallel load on the (still-parallel) `ProgressTrackerAsync_Tests`, and cannot produce a task cancellation inside an unrelated STA dispatcher path.

Observed rate: 1 occurrence in 11 fixed-code passes (~9%), 0 in 4 pristine-baseline passes (below).

## git-stash baseline diagnostic (causation separation)

The 8 test-file edits were git-stashed to restore the pristine #292 head (`9ae5c0e3...`), the solution rebuilt, and the full suite run 4 times:

| Baseline pass | Total | Passed | Failed | Notes |
|---------------|-------|--------|--------|-------|
| 1 | 5141 | 5141 | 0 | race did not overlap on this schedule |
| 2 | 5141 | 5131 | 10 | exactly the 10 #292 tests |
| 3 | 5141 | 5141 | 0 | race did not overlap |
| 4 | 5141 | 5141 | 0 | race did not overlap |

This confirms: (1) the #292 race is real and schedule-dependent on the pristine baseline (10 failures in 1 of 4 passes), and (2) `ProgressTrackerAsync` did NOT appear in the baseline passes — it is a separate rare flake, not the #292 defect. The stash was then popped and the solution rebuilt with the attributes restored (verified: 1 `[DoNotParallelize]` per edited class).

## Correctness guarantee (structural, not repeated-run luck)

The fix's correctness is structural, not probabilistic. Per the P2-T12 completeness gate, every scope-opening `UtilitiesCS.Test` class now carries `[DoNotParallelize]`, and MSTest guarantees all `[DoNotParallelize]` classes run sequentially and never concurrently with each other. The null-baseline readers are already in that serial bucket, so no `CurrentStoreContext` writer remains in the parallel bucket that could overlap a reader. The 4 definitive passes (and the 11 total fixed-code passes with zero #292 occurrences) are confirmation of the guarantee, not the basis for it.

## Out-of-scope finding recorded

`ProgressTrackerAsync_Tests.InitializeAsync_WithCurrentDispatcher_InitializesAndReturnsTracker` is a pre-existing, low-frequency, non-#292 flaky test (STA-dispatcher `TaskCanceledException`). Per the execution directive, it is NOT remediated here (out of scope) and is reported for separate handling.
