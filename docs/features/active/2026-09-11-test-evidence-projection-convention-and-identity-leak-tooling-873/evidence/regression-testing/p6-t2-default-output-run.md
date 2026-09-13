# P6-T2 — Default-Output End-to-End Run (ABORTED, acceptance NOT met)

Timestamp: 2026-09-13T06-46

Command: `pwsh -NoProfile -Command 'Set-Location -LiteralPath <worktree-root>; & <worktree-root>/scripts/vscode/Invoke-MSTestWithCoverage.ps1'`

EXIT_CODE: 1

ExpectedExitCode: 0

Output Summary: The run reached the inner test console and executed 7221 tests across the 9 discovered assemblies in 42.89 seconds. 7218 passed and 3 failed. The coverage entry point throws on any nonzero test-console exit code inside `Invoke-DotnetCoverageCollection`, which is upstream of the post-processing, the projection write and the summary write, so the run terminated with `MSTest with coverage failed with exit code 1` and produced neither the projection file nor the test-result summary file that this task's acceptance observes. This task's acceptance is NOT met and its checkbox remains unchecked. The three failures are a pre-existing defect outside this delivery's Write Set; per the plan's "Phase 6 flakiness attribution" rule the failing tests are recorded and reported here rather than modified.

VERDICT: BLOCKED — pre-existing defect outside the Write Set.

## Attribution of the Failure (plan "Phase 6 flakiness attribution" rule)

FAILING_TEST_COUNT: 3

FAILING_TEST_CLASS: `QuickFiler.Controllers.Tests.QfcInitEmailQueueZeroBatchTests`

FAILING_TEST_ASSEMBLY: `QuickFiler.Test` (discovered as `QuickFiler.Test/bin/Debug/QuickFiler.Test.dll`)

FAILING_TEST_NAMES:

- `InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing`
- `InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker`
- `InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop`

OBSERVED_SYMPTOM: each of the three throws `System.TypeInitializationException` for `Deedle.Reflection`, whose inner exception is a `System.TypeInitializationException` for `<StartupCode$Deedle>.$FrameUtils`, whose inner exception is `System.IO.FileNotFoundException: Could not load file or assembly 'netstandard, Version=2.1.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51' or one of its dependencies.` The throw site is the test class's own `CreateTwoRowEmailFrame` helper calling `Deedle.Reflection.convertRecordSequence`. The failure is an assembly-load failure at static-initialiser time, not an assertion failure: the elapsed times are 9 ms, under 1 ms and 1 ms.

RELATION_TO_THIS_DELIVERY: none. `QuickFiler.Test` is not in this delivery's Write Set, this delivery changes no C# source file, and the failing class exercises a Deedle data-frame path that no file this delivery touches participates in.

## Corroboration of the Documented Mechanism

The console banner for the first assembly reads `Test Parallelization enabled for <worktree-root>\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll (Workers: 24, Scope: ClassLevel)`. That class-level parallelisation is supplied by `scripts/vscode/TaskMaster.cli.runsettings`, which the entry point's argument builder appends as `/Settings:` on the inner vstest segment. The continuous-integration workflow `.github/workflows/_mstest-coverage.yml` passes no settings file, which is why the defect does not surface there. Neither that runsettings file nor the `/Settings:` switch was altered by this task: both sit outside this delivery's Write Set and the delegation prohibits editing them.

## Post-Run State of the Repository Coverage Directory

Listed repository-relative; no absolute host path is recorded. Sizes in bytes.

```
coverage/.gitkeep | 0
coverage/coverage.cobertura.xml | 18302528
coverage/test-results/mstest-coverage-run.trx | 10034397
coverage/test-results/Deploy_<account> <timestamp>_<pid>/ (MSTest deployment directory, empty subtree)
```

RAW_COVERAGE_DOCUMENT_PRESENT: true

PROJECTION_FILE_PRESENT: false

TEST_RESULT_SUMMARY_PRESENT: false

RECONCILIATION_INTEGERS: not derivable — no projection document was written.

The raw Cobertura document and the TRX are present because the collector and the test console wrote them before the entry point threw. They remain uncommitted: the whole `coverage` tree is already ignored by the repository ignore file, which is the arrangement this delivery relies on. No raw document was added to version control by this task.

The MSTest deployment directory carries the account and host tokens in its name. It is a test-console artifact, it sits beneath the ignored coverage tree, and it is not committed. It is recorded here as an observation only; the default-name scan that would have evaluated it is P6-T4, which did not run.

## Elapsed Time

RUN_START: 2026-09-13T06:46:36-04:00

RUN_END: 2026-09-13T06:47:21-04:00

ELAPSED_SECONDS: 45

Neither bound in the delegated abort protocol was approached: the run neither went 20 minutes without console output nor reached the 45-minute ceiling. It failed outright in 45 seconds.

## Build Lock

The shared build lock was acquired for item 873 at 2026-09-13T06:46:19-04:00, held across this single run only, and released at 2026-09-13T06:49:52-04:00 immediately after the run completed.

## Consequence for the Plan

[P6-T2] is left unchecked. [P6-T3], [P6-T4] and [P6-T5] are left unchecked because each depends on an artifact this run did not produce. AC23 is left unchecked. Phase 7 is not begun.
