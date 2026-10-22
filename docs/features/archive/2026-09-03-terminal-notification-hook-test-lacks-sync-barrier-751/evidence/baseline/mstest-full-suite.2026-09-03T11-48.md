# P0-T14 — Full-Suite MSTest Baseline (Issue #751)

Timestamp: 2026-09-03T14-25

## Command

The command was executed from the script file `coverage\logs\P0-T14.ps1`, launched detached per the plan's
Long-running commands convention. The vstest command line written into that script file is:

```
& $vstest $asm /EnableCodeCoverage /InIsolation "/Logger:trx;LogFileName=P0-T14.trx" "/TestCaseFilter:TestCategory!=LiveOutlook" "/ResultsDirectory:coverage\trx\P0-T14"
```

The assembly discovery statement, which precedes the step-1 try block in the same script file, is:

```powershell
$asm = Get-ChildItem -Path . -Recurse -Filter '*.Test.dll' | Where-Object { $_.FullName -match '\\bin\\Debug\\' -and $_.FullName -notmatch '\\obj\\' -and $_.FullName -notmatch '\\ref\\' -and $_.FullName -notmatch '\\\.claude\\' } | Select-Object -ExpandProperty FullName
```

This matches `.github/workflows/_mstest-coverage.yml:86-92` with the `\\\.claude\\` exclusion that
`scripts/vscode/Invoke-MSTestWithCoverage.ps1:301` applies for local runs.

## Launch and polling record

| Field | Value |
|---|---|
| LaunchedPid | 81480 |
| LaunchedStart | 2026-09-03T14:24:03.7557580-04:00 |
| ChildPids | none captured (process exited before the first poll) |
| Attempts preceding the recorded result | 1 (no relaunch was required) |
| Polls performed | 1 |
| Poll 1 timestamp | 2026-09-03T14:24:55.3914358-04:00 |
| Poll 1 result | `Alive: False (no process returned)` — completion signal |

The single poll was preceded by its bounded `Wait-Process -Id 81480 -Timeout 120 -ErrorAction SilentlyContinue`,
which returned early when the process exited. Because the first poll already reported the process not alive,
no descendant set was observable, and `ChildPids:` records that fact per step 2 of the convention. No
process required termination under step 5.

## Result

| Field | Value |
|---|---|
| EXIT_CODE | **0** |
| Source of EXIT_CODE | `coverage\logs\P0-T14.exit.txt`, read per step 4 of the convention |
| Completion witness | `coverage\trx\P0-T14\P0-T14.trx` — present and parsed |
| Fault file present | No |
| Assembly count | **9** |

The exit code was read only from the durable on-disk exit file, never from `$proc.ExitCode`. The recorded
value is neither `9001` nor `9002`, so the run produced a genuine test result.

### Assemblies discovered (9)

`QuickFiler.Test.dll`, `SVGControl.Test.dll`, `Tags.Test.dll`, `TaskMaster.Test.dll`, `TaskTree.Test.dll`,
`TaskVisualization.Test.dll`, `ToDoModel.Test.dll`, `UtilitiesCS.Test.dll`, `VBFunctions.Test.dll` — each
resolved from its own `bin\Debug` directory. The raw `*.Test.dll` recursive match returned 18 files; the
`\bin\Debug\` inclusion and the `\obj\` exclusion reduced that to the 9 above.

## Counters (read from the TRX)

| Counter | Value |
|---|---|
| Total | **6984** |
| Executed | 6984 |
| Passed | **6984** |
| Failed | **0** |
| NotExecuted (Skipped) | **0** |
| ResultSummary outcome | Completed |

Run tail as reported by vstest:

```
Test Run Successful.
Total tests: 6984
     Passed: 6984
 Total time: 49.6908 Seconds
```

## BASELINE_FAILURE_SET

```
none
```

The full-suite baseline run recorded **zero** failed tests across all nine test assemblies. The
`TaskMaster.Test.dll` subset of `BASELINE_FAILURE_SET` is therefore also **empty**.

Consequences for the later baseline-relative gates, stated here so P3-T1, P3-T2 and P4-T5 can read them
directly:

- P3-T1's subset condition ("the set of failed fully qualified names in this run is a subset of the
  `TaskMaster.Test.dll` members of `BASELINE_FAILURE_SET`") reduces to requiring an **empty** failed set,
  because the only subset of the empty set is the empty set.
- P3-T2's per-run subset condition reduces the same way, for each of its five runs.
- P4-T5's subset condition against the whole `BASELINE_FAILURE_SET` likewise reduces to requiring an empty
  failed set.

This is a consequence of the measured baseline, not a substituted absolute-zero demand: the plan states the
gates relative to the recorded set, and the recorded set happens to be empty.

## Target test in this baseline run

| Test | Outcome | Duration | Assembly |
|---|---|---|---|
| `TerminalNotificationHookFailure_DoesNotReplaceDispatchFault` | Passed | 00:00:00.0019154 | `taskmaster.test.dll` |

The target test passed in this pre-change baseline run. That is consistent with the defect being
intermittent rather than deterministic, as research §2.4 establishes; it is not evidence that the defect is
absent. The dedicated pre-change repeat-run series is P0-T15.

## Coverage attachment produced by this run

`Get-ChildItem -Path 'coverage\trx\P0-T14' -Recurse -Filter '*.coverage'` returned **2** files. Both carry
an identical byte length of 21356385: vstest retains the in-run copy under its `In\<machine>\` subtree in
addition to the published attachment. The file paths are deliberately not transcribed, because the vstest
results directory names embed the account name. The consequence for P0-T17 is recorded in that task's
artifact.

## Acceptance

| Required | Observed | Result |
|---|---|---|
| The artifact exists and its `BASELINE_FAILURE_SET` section is present and explicit — either an enumerated list of fully qualified names each tagged with its assembly, or the single word `none` | The section is present and reads `none` | PASS |

This task did not require a zero-failure suite. The suite happened to be zero-failure, and that measured
fact is what the later gates are stated against.
