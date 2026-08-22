# Phase 1 — Pre-Fix Behaviour, Class-Filtered Scope (P1-T3, `[expect-fail]`)

Timestamp: 2026-08-22T09-53

Command (executed ten consecutive times):

```
"C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" ^
  QuickFiler.Test\bin\Debug\QuickFiler.Test.dll ^
  /InIsolation ^
  "/TestCaseFilter:FullyQualifiedName~QfcItemController_InitializationTests" ^
  /Logger:trx ^
  /ResultsDirectory:docs/features/active/winformspumphost-suite-determinism-511/evidence/regression-testing/p1-t3
```

Run from the worktree root
`C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-ad37a256a0fb60243` through
`pwsh -NoProfile`. `vstest.console.exe` was resolved with `vswhere`. `/EnableCodeCoverage` is
deliberately absent: the task specifies it only for the full-suite run in P1-T4.

EXIT_CODE: 0

ExpectedExitCode: 1

Output Summary:

## The expectation was not met, and that is the measurement

This task is tagged `[expect-fail]` and the plan therefore declares `ExpectedExitCode: 1`. **The
observed exit code was 0 on all ten runs.** The two named tests, and the new probe, **passed on every
one of the ten class-filtered runs.** The declared expectation and the observed result diverge, and
the observed result is recorded verbatim rather than reconciled.

Per the plan's explicit Phase 1 instruction, a green pre-fix run is treated as **data about the race
window, not as evidence the defect is absent**, and the remedy is neither narrowed, widened, nor
abandoned on this basis.

## Per-run table (ten rows, no empty cell)

`IsHandleCreated` is derived from the probe outcome: the probe asserts
`harness.Viewer.IsHandleCreated` is `true` and that `harness.Viewer.InvokeRequired` evaluated on the
pump thread is `false`. A passing probe therefore establishes `IsHandleCreated: true` for that run.

| Run | Scope | Exit | `InitializeBool_...CompletesAndInitializesState` | `InitializeNineArgOverload_...SavesParametersAndDelegates` | `BuildPumpHarness_ForcesTheViewerWindowHandleOnThePumpThread` | `IsHandleCreated` | Total | Passed | Failed | TRX |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | class-filtered | 0 | Passed | Passed | Passed | true | 10 | 10 | 0 | `DanMoisan_MEGALODON4_2026-08-22_09_52_25_net481.trx` |
| 2 | class-filtered | 0 | Passed | Passed | Passed | true | 10 | 10 | 0 | `DanMoisan_MEGALODON4_2026-08-22_09_52_28_net481.trx` |
| 3 | class-filtered | 0 | Passed | Passed | Passed | true | 10 | 10 | 0 | `DanMoisan_MEGALODON4_2026-08-22_09_52_33_net481.trx` |
| 4 | class-filtered | 0 | Passed | Passed | Passed | true | 10 | 10 | 0 | `DanMoisan_MEGALODON4_2026-08-22_09_52_41_net481.trx` |
| 5 | class-filtered | 0 | Passed | Passed | Passed | true | 10 | 10 | 0 | `DanMoisan_MEGALODON4_2026-08-22_09_52_51_net481.trx` |
| 6 | class-filtered | 0 | Passed | Passed | Passed | true | 10 | 10 | 0 | `DanMoisan_MEGALODON4_2026-08-22_09_52_55_net481.trx` |
| 7 | class-filtered | 0 | Passed | Passed | Passed | true | 10 | 10 | 0 | `DanMoisan_MEGALODON4_2026-08-22_09_52_59_net481.trx` |
| 8 | class-filtered | 0 | Passed | Passed | Passed | true | 10 | 10 | 0 | `DanMoisan_MEGALODON4_2026-08-22_09_53_04_net481.trx` |
| 9 | class-filtered | 0 | Passed | Passed | Passed | true | 10 | 10 | 0 | `DanMoisan_MEGALODON4_2026-08-22_09_53_08_net481.trx` |
| 10 | class-filtered | 0 | Passed | Passed | Passed | true | 10 | 10 | 0 | `DanMoisan_MEGALODON4_2026-08-22_09_53_16_net481.trx` |

Observed failure rate in this scope: **0 of 10 runs** for each of the three tracked tests.

Each row's outcomes were read from that run's own TRX by matching the `testName` attribute on
`UnitTestResult` elements, and each run's TRX was identified by diffing the results directory before
and after the run, so no row is attributed to the wrong file. The machine-readable row set is at
`coverage\p1-t3-rows.json`.

## Scope contents

The filter `FullyQualifiedName~QfcItemController_InitializationTests` matched exactly 10 tests on
every run:

```
AsyncFlagConstructor_AssignsFieldsViaSaveParameters
BuildPumpHarness_ForcesTheViewerWindowHandleOnThePumpThread
InitializeAsync_ThroughThePumpHost_RunsToTheMockedWebViewSeamAndFaults
InitializeBool_ThroughThePumpHost_CompletesAndInitializesState
InitializeGraphicsAsync_ThroughThePumpHost_CompletesAndAppliesDarkTheme
InitializeNineArgOverload_ThroughThePumpHost_SavesParametersAndDelegates
InitializeSequentialAsync_ThroughThePumpHost_CompletesAndInitializesState
PredeterminedFolderConstructor_StoresPredeterminedFolder
PrimaryConstructor_AssignsFieldsAndSetsControllerBackReference
SaveParameters_AssignsAllFieldsAndResolvesCollaborators
```

## Acceptance conditions

1. **The subdirectory holds exactly ten TRX files and no others** — met. Directory inventory:
   10 files matching `*.trx`, **0** non-TRX files, and **0** subdirectories. (`/EnableCodeCoverage`
   was not passed in this scope, so no `.coverage` attachment folder was created.)
2. **The table has exactly ten rows with no empty cell** — met; 10 rows, every cell populated.
3. **A run in which the probe passes is recorded as `IsHandleCreated: true`** — met; all ten rows
   record `true`.

## What this measures, stated no more strongly than the evidence supports

In the **isolated class-filtered scope**, with only `QuickFiler.Test.dll` loaded and only these ten
tests selected, the harness viewer's window handle was present on all ten runs without any fix. The
probe's `InvokeRequired == false` assertion also held on all ten, so the handle was owned by the pump
thread.

This establishes that *something* in the current initialization path creates the handle under these
conditions. It does **not** establish what, and it does **not** generalize to the full-suite scope,
which P1-T4 measures separately — and the P0-T16 coverage-script invocation already recorded a run in
which both named tests failed with 60,000 ms timeouts. Attribution of the mechanism is deferred to
P1-T6, which is required to cite an observation rather than close the question by assertion.
