# Phase 1 — Pre-Fix Behaviour, Full Nine-Assembly Suite (P1-T4, `[expect-fail]`)

Timestamp: 2026-08-22T10-24

Command (executed ten consecutive times):

```
"C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" ^
  QuickFiler.Test\bin\Debug\QuickFiler.Test.dll ^
  SVGControl.Test\bin\Debug\SVGControl.Test.dll ^
  Tags.Test\bin\Debug\Tags.Test.dll ^
  TaskMaster.Test\bin\Debug\TaskMaster.Test.dll ^
  TaskTree.Test\bin\Debug\TaskTree.Test.dll ^
  TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll ^
  ToDoModel.Test\bin\Debug\ToDoModel.Test.dll ^
  UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll ^
  VBFunctions.Test\bin\Debug\VBFunctions.Test.dll ^
  /EnableCodeCoverage /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook" ^
  /Logger:trx ^
  /ResultsDirectory:docs/features/active/winformspumphost-suite-determinism-511/evidence/regression-testing/p1-t4
```

Run from the worktree root
`<repo-root>\.claude\worktrees\agent-ad37a256a0fb60243` through
`pwsh -NoProfile`. All nine assembly paths come from the plan's canonical assembly list.

EXIT_CODE: 0

ExpectedExitCode: 1

Output Summary:

## The expectation was not met, and that is the measurement

This task is tagged `[expect-fail]` and the plan declares `ExpectedExitCode: 1`. **The observed exit
code was 0 on all ten runs.** All three tracked tests passed on every one of the ten full-suite runs.
The divergence between the declared expectation and the observed result is recorded verbatim.

Per the plan's explicit Phase 1 instruction, these green pre-fix runs are treated as **data about the
race window, not as evidence the defect is absent**, and the remedy is neither narrowed, widened, nor
abandoned on this basis.

## Per-run table (ten rows, no empty cell)

`IsHandleCreated` is derived from the probe outcome: the probe asserts
`harness.Viewer.IsHandleCreated` is `true` and that `harness.Viewer.InvokeRequired` evaluated on the
pump thread is `false`. A passing probe therefore establishes `IsHandleCreated: true` for that run.

| Run | Scope | Exit | `InitializeBool_...CompletesAndInitializesState` | `InitializeNineArgOverload_...SavesParametersAndDelegates` | `BuildPumpHarness_ForcesTheViewerWindowHandleOnThePumpThread` | `IsHandleCreated` | Total | Passed | Failed |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | full suite | 0 | Passed | Passed | Passed | true | 6438 | 6438 | 0 |
| 2 | full suite | 0 | Passed | Passed | Passed | true | 6438 | 6438 | 0 |
| 3 | full suite | 0 | Passed | Passed | Passed | true | 6438 | 6438 | 0 |
| 4 | full suite | 0 | Passed | Passed | Passed | true | 6438 | 6438 | 0 |
| 5 | full suite | 0 | Passed | Passed | Passed | true | 6438 | 6438 | 0 |
| 6 | full suite | 0 | Passed | Passed | Passed | true | 6438 | 6438 | 0 |
| 7 | full suite | 0 | Passed | Passed | Passed | true | 6438 | 6438 | 0 |
| 8 | full suite | 0 | Passed | Passed | Passed | true | 6438 | 6438 | 0 |
| 9 | full suite | 0 | Passed | Passed | Passed | true | 6438 | 6438 | 0 |
| 10 | full suite | 0 | Passed | Passed | Passed | true | 6438 | 6438 | 0 |

Observed failure rate in this scope: **0 of 10 runs** for each of the three tracked tests.

The total of **6438** is the P0-T15 baseline total of 6437 plus the one probe added in P1-T1,
confirming both that all nine assemblies loaded on every run and that the probe executed on every
run.

Each row's outcomes were read from that run's own TRX by matching the `testName` attribute on
`UnitTestResult` elements, with the run's TRX identified by diffing the results directory before and
after the run. The machine-readable row set is at `coverage\p1-t4-rows.json`.

## `/InIsolation` confirmation

`/InIsolation` was supplied on all ten runs. The phantom-failure signature the plan warns about
(roughly 1,695 failures with empty messages and sub-millisecond durations, surfacing as a Moq
`TypeInitializationException` via `System.Threading.Tasks.Extensions`) did not appear on any run.

## Acceptance conditions

1. **The subdirectory holds exactly ten TRX files and no others** — met. Inventory of
   `.../evidence/regression-testing/p1-t4/`:

   | Item | Count |
   | --- | --- |
   | `*.trx` files | **10** |
   | non-TRX files at top level | **0** |
   | subdirectories | 20 |

   "No others" is satisfied in the sense the plan's Toolchain section defines it — the condition is a
   TRX count scoped to this task's own subdirectory, and there are exactly ten TRX files and no
   eleventh. The 20 subdirectories are the attachment folders `vstest.console.exe` creates
   automatically under `/ResultsDirectory` when `/EnableCodeCoverage` is passed: one binary
   `.coverage` folder and one per-test attachment folder per run. The plan mandates
   `/EnableCodeCoverage` with this exact `/ResultsDirectory`, so they are a required by-product of the
   specified command rather than stray output.

2. **The table has exactly ten rows with no empty cell** — met.

## Size hazard flagged for the phase that commits evidence

Reported, not acted on, because remediation is outside Phase 0 and Phase 1 scope:

| Directory | Size |
| --- | --- |
| `evidence/regression-testing/p1-t4/` | **479 MB** |
| `evidence/baseline/p0-t15/` | 48 MB |
| `evidence/regression-testing/p1-t3/` | 284 KB |
| `evidence/` total | **528 MB** |

Nearly all of it is binary `.coverage` attachments, which carry no information this plan reads:
numeric coverage comes from the Cobertura XML produced by
`scripts/vscode/Invoke-MSTestWithCoverage.ps1`, not from these files. Committing 528 MB of binary
attachments into `docs/features/active/` would be a substantial and permanent repository-size cost.
The phase that commits evidence and asserts a clean tree (P6-T18) should decide whether to retain
only the ten TRX files and prune the attachment subdirectories. This artifact records the condition so
that decision is made deliberately rather than by accident.
