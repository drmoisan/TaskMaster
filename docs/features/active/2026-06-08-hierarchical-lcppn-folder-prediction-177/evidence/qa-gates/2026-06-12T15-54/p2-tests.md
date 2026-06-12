# Phase 2 QA Gate — Step 4 Tests + Coverage (#177 Cycle 1)

- Timestamp: 2026-06-12T17-06 (UTC)
- Task: [P2-T4] step 4 of 4
- Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /InIsolation`
- EXIT_CODE: 0 (full suite excluding the pre-existing flaky test; see note)
- Output Summary:
  - Targeted F2 run (`FolderHierarchyTree_Tests` + `LcppnFolderPredictor_Tests`): 46/46 passed.
  - Full assembly excluding only the pre-existing flaky `IdleAsyncQueue_Tests.AddEntry_UseUiThreadTrue_DequeuesEntryAndSuppressesDispatcherException`: 3903/3903 passed (EXIT 0).
  - The flaky test passes deterministically in isolation (1/1).
  - **Post-change strict per-type line coverage (>= 90% gate):**
    - `FolderHierarchyTree`: **strict 100.00%** / inclusive 100.00% (covered=81, partial=0, not-covered=0, total=81) — was 86.42%.
    - `LcppnFolderPredictor`: **strict 97.71%** / inclusive 100.00% (covered=171, partial=4, not-covered=15->0, total=175) — was 89.14%.
  - Both target types exceed the >= 90% strict-line-coverage gate. Coverage source: full-suite .coverage (`cov-p2/6fa9f77f-.../...12_25_56.coverage`) merged to `p2-coverage.xml`; every F2 test passed in that run.

## Pre-existing flaky test (out of scope, not masked)

- `IdleAsyncQueue_Tests.AddEntry_UseUiThreadTrue_DequeuesEntryAndSuppressesDispatcherException`
  fails intermittently under full-suite class-level parallelization (24 workers): it asserts the queued
  action does NOT run when the WPF UI Dispatcher is unavailable (callCount==0), but `IdleAsyncQueue`
  uses static state shared across parallel test classes, so a concurrent test that makes a Dispatcher
  available causes the action to run once (callCount==1). The test passes deterministically in
  isolation (verified) and passed in the Phase 1 full run; adding the two F2 test classes shifted the
  parallel scheduling enough to surface this latent static-state isolation defect more reliably.
  It is in the Threading subsystem, unrelated to the F1/F2 changes, and outside cycle-1 scope. Per
  policy, no timing hack, retry, or assertion weakening was applied; the defect is recorded as a
  pre-existing flake/test-isolation issue and is excluded only from the deterministic coverage gate
  run (its own subject code is not an F2 target type). Recommended follow-up: fix `IdleAsyncQueue_Tests`
  static-state isolation (reset/serialize the static queue) outside this cycle.
