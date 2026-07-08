# Phase 1 QA Gate — Step 4 Tests + Coverage (#177 Cycle 1)

- Timestamp: 2026-06-12T16-48 (UTC)
- Task: [P1-T9] step 4 of 4
- Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /InIsolation`
- EXIT_CODE: 0
- Output Summary:
  - Full assembly: Test Run Successful. Total tests 3892, Passed 3892, Failed 0 (3890 prior + 2 new F1 regression tests).
  - Targeted FolderPredictorSeam_Tests run: 8/8 passed, including the two new F1 regression tests:
    - `GetFolderPredictorAsync_FlagOn_ReachableThroughFreshPerCallInstance` (PASS) — proves the held LCPPN predictor on `Globals.AF.FolderPredictor` is reachable by two independent fresh per-call instances.
    - `GetFolderPredictorAsync_FlagOff_FreshPerCallInstance_ReturnsFlat` (PASS) — proves flag-off fresh per-call returns the flat `Manager["Folder"]` group (AC13 preserved).
  - The four existing seam tests (AC13/AC14) still pass after routing `SetLcppnPredictor` through the shared holder via `SetupProperty`.

## Known-flaky pre-existing test (out of scope)

- `IdleAsyncQueue_Tests.AddEntry_UseUiThreadTrue_DequeuesEntryAndSuppressesDispatcherException`
  intermittently failed once during a parallel full run (asserts callCount==0 when the WPF UI
  Dispatcher is unavailable; under 24-worker class-level parallelization the action occasionally
  ran once). It passed in isolation and on the immediate green full re-run (3892/3892). This is a
  pre-existing UI-Dispatcher timing test in an unrelated subsystem (Threading), not touched by the
  F1 change and outside cycle 1 scope. Per policy, no timing hack or assertion weakening was applied;
  recorded as a pre-existing flake.
