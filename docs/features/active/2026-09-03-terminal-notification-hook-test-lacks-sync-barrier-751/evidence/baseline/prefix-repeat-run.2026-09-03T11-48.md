# P0-T15 — Pre-Change Repeat-Run Series (Issue #751)

Timestamp: 2026-09-03T14-28

Three consecutive runs of the identical CI-shaped command against the affected assembly, with **no**
intervening rebuild, edit, or configuration change. The three runs were executed back to back against the
same `TaskMaster.Test\bin\Debug\TaskMaster.Test.dll` binary produced by the P0-T13 rebuild.

## Command (identical for all three runs, apart from the run index)

Each run was executed from its own script file `coverage\logs\P0-T15-run<n>.ps1`, launched detached per the
plan's Long-running commands convention. The vstest command line written into each script file is:

```
& $vstest 'TaskMaster.Test\bin\Debug\TaskMaster.Test.dll' /EnableCodeCoverage /InIsolation "/Logger:trx;LogFileName=P0-T15-run<n>.trx" "/TestCaseFilter:TestCategory!=LiveOutlook" "/ResultsDirectory:coverage\trx\P0-T15-run<n>"
```

with `<n>` taking the values 1, 2, and 3.

## Per-run launch and completion record

| Field | Run 1 | Run 2 | Run 3 |
|---|---|---|---|
| LaunchedPid | 110128 | 126276 | 122828 |
| LaunchedStart | 2026-09-03T14:26:56.9769970-04:00 | 2026-09-03T14:27:15.5892575-04:00 | 2026-09-03T14:27:26.5199769-04:00 |
| ChildPids | none captured (process exited before the first poll) | none captured (process exited before the first poll) | none captured (process exited before the first poll) |
| Polls performed | 1 | 1 | 1 |
| Poll 1 timestamp | 2026-09-03T14:27:04.1715325-04:00 | 2026-09-03T14:27:22.7855558-04:00 | 2026-09-03T14:27:34.5526125-04:00 |
| Poll 1 result | not alive (completion signal) | not alive (completion signal) | not alive (completion signal) |
| Attempts behind the recorded run | 1 | 1 | 1 |
| Fault file present | No | No | No |
| Completion witness TRX | `coverage\trx\P0-T15-run1\P0-T15-run1.trx` | `coverage\trx\P0-T15-run2\P0-T15-run2.trx` | `coverage\trx\P0-T15-run3\P0-T15-run3.trx` |
| **EXIT_CODE** | **0** | **0** | **0** |

Every poll was preceded by its bounded `Wait-Process -Id <pid> -Timeout 120 -ErrorAction SilentlyContinue`.
Each wait returned early because the run completed well inside the 120-second bound. Each `EXIT_CODE:` was
read from `coverage\logs\P0-T15-run<n>.exit.txt` per step 4 of the convention, never from `$proc.ExitCode`;
none of the three is `9001` or `9002`, so all three are genuine test results. No run required a relaunch and
no run required termination under step 5.

## Per-run counters

| Counter | Run 1 | Run 2 | Run 3 |
|---|---|---|---|
| Total | 408 | 408 | 408 |
| Executed | 408 | 408 | 408 |
| Passed | 408 | 408 | 408 |
| Failed | **0** | **0** | **0** |
| NotExecuted (Skipped) | 0 | 0 | 0 |
| ResultSummary outcome | Completed | Completed | Completed |

## Per-run outcome of the target test

`TerminalNotificationHookFailure_DoesNotReplaceDispatchFault`, read from each run's TRX:

| Run | Outcome | Duration | Assembly |
|---|---|---|---|
| 1 | **Passed** | 00:00:00.0016355 | `taskmaster.test.dll` |
| 2 | **Passed** | 00:00:00.0018380 | `taskmaster.test.dll` |
| 3 | **Passed** | 00:00:00.0019062 | `taskmaster.test.dll` |

## Observed branch

**All-green branch.**

None of the three runs recorded `TerminalNotificationHookFailure_DoesNotReplaceDispatchFault` as `Failed`.
The `NATURAL_RED_OBSERVED` branch was therefore **not** taken, and this artifact carries no
`NATURAL_RED_OBSERVED` section.

This is the observation that the P1-T2 dossier cites for its claim that a natural red is not reliably
producible on the unmodified tree. It is consistent with research §2.4: interleaving (b) runs the hook
inline on the worker thread and passes unconditionally, and the race window in interleaving (a) is
sub-microsecond, so a red on any given run is possible but not reliable. Three green runs are evidence that
a red is not reliably producible; they are not evidence that the defect is absent, and this artifact makes
no such claim. The one recorded natural red for this defect is the PR #746 `mstest-coverage` CI failure
cited at `spec.md:53`.

## Compliance notes

- This task carried **no** pass/fail expectation on the named test. Its purpose was to record what the
  pre-change tree actually does, and the observation was allowed to come out either way.
- The series was **not** re-run to chase a particular outcome, and no fourth completed run was substituted
  for any of the three. All three recorded runs completed on their first attempt, so the relaunch provision
  of the Long-running commands convention was never invoked.
- No `Sanitization: applied` line is carried, because that requirement attaches to the
  `NATURAL_RED_OBSERVED` branch only and no verbatim path-bearing tool output is transcribed here.
