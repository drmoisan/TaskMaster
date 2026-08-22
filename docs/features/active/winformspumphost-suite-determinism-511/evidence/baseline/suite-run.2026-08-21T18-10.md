# Baseline — Full Nine-Assembly Suite Run

Timestamp: 2026-08-22T09-28

Command:

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
  /ResultsDirectory:docs/features/active/winformspumphost-suite-determinism-511/evidence/baseline/p0-t15
```

Run from the worktree root
`C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-ad37a256a0fb60243` through
`pwsh -NoProfile`. `vstest.console.exe` was resolved with
`vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe'`. All
nine assembly paths came from the plan's canonical assembly list and all nine were confirmed present
on disk (built at 09:25 by the P0-T14 `/t:Rebuild`) before the run.

EXIT_CODE: 0

Output Summary:

| Measure | Value |
| --- | --- |
| Total | **6437** |
| Passed | **6437** |
| Failed | **0** |
| Skipped / not executed | **0** |
| Exit code | 0 |
| Verdict line | `Test Run Successful.` |

The TRX `<Counters>` element corroborates the console summary exactly:

```
<Counters total="6437" executed="6437" passed="6437" failed="0" error="0" timeout="0"
          aborted="0" inconclusive="0" passedButRunAborted="0" notRunnable="0"
          notExecuted="0" disconnected="0" warning="0" completed="0" inProgress="0" pending="0" />
```

## TRX path

```
docs/features/active/winformspumphost-suite-determinism-511/evidence/baseline/p0-t15/DanMoisan_MEGALODON4_2026-08-22_09_27_19_net481.trx
```

The file exists (9,150,580 bytes) and is the **only** TRX file in that subdirectory. Directory
listing:

```
d76a53ba-c575-4bfc-94cd-7d71737150a5/                          (holds the binary .coverage attachment)
DanMoisan_MEGALODON4_2026-08-22_09_27_19/                      (per-test attachment folder)
DanMoisan_MEGALODON4_2026-08-22_09_27_19_net481.trx            (the single TRX)
```

## Acceptance conditions

1. **Artifact exists with all four fields** — met.
2. **Named TRX exists under `.../evidence/baseline/p0-t15/` and is the only TRX there** — met; TRX
   count in that subdirectory is 1.
3. **Recorded total exceeds 1,000 tests** — met at **6,437**, confirming all nine assemblies loaded.

## `/InIsolation` confirmation

`/InIsolation` was supplied. The phantom-failure signature the plan warns about — roughly 1,695
failures with empty messages and sub-millisecond durations, surfacing as a Moq
`TypeInitializationException` via `System.Threading.Tasks.Extensions` — did **not** appear. Failed
count is 0, so no correction re-run was required and nothing was "fixed".

## Baseline state of the two named tests (data carried into Phase 1)

Both tests that #511 and #571 concern **passed** on this baseline run:

- `InitializeBool_ThroughThePumpHost_CompletesAndInitializesState` — `outcome="Passed"`,
  duration `00:00:00.0915828`
- `InitializeNineArgOverload_ThroughThePumpHost_SavesParametersAndDelegates` — `outcome="Passed"`

This is a single observation, not a failure-rate measurement. It is consistent with #571's report
that the tests pass on some runs, and it is exactly the condition Phase 1 exists to measure across
twenty runs. Per the plan's explicit instruction, a green pre-fix run is treated as **data about the
race window**, not as evidence the defect is absent.

## Pre-existing baseline failures

**None.** There is no pre-existing failing test in the nine-assembly suite at this baseline. Any
failure observed later in this execution is therefore either an `[expect-fail]` Phase 1 measurement
or a regression introduced by the change, and the two are distinguishable.

## Note on artifact size

`/EnableCodeCoverage` wrote a binary `.coverage` attachment of 20,525,261 bytes into the
`d76a53ba-c575-4bfc-94cd-7d71737150a5/` subfolder, making the `p0-t15` directory 48 MB in total. The
plan mandates this exact command shape with this exact `/ResultsDirectory`, so the attachment is
recorded here as produced. The binary `.coverage` file is not the source of numeric coverage in this
plan; P0-T16 produces numeric coverage separately via Cobertura XML.
