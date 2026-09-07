# P4-T5 — Full-Suite Test Gate Under Coverage (Issue #751)

Timestamp: 2026-09-03T14-42

## Command

Executed from `coverage\logs\P4-T5.ps1`, launched detached per the Long-running commands convention. The
vstest command line written into that script file:

```
& $vstest $asm /EnableCodeCoverage /InIsolation "/Logger:trx;LogFileName=P4-T5.trx" "/TestCaseFilter:TestCategory!=LiveOutlook" "/ResultsDirectory:coverage\trx\P4-T5"
```

The same assembly discovery statement as P0-T14 precedes the step-1 try block:

```powershell
$asm = Get-ChildItem -Path . -Recurse -Filter '*.Test.dll' | Where-Object { $_.FullName -match '\\bin\\Debug\\' -and $_.FullName -notmatch '\\obj\\' -and $_.FullName -notmatch '\\ref\\' -and $_.FullName -notmatch '\\\.claude\\' } | Select-Object -ExpandProperty FullName
```

## Launch and completion record

| Field | Value |
|---|---|
| LaunchedPid | 110128 |
| LaunchedStart | 2026-09-03T14:41:25.9452273-04:00 |
| ChildPids | none captured (process exited before the first poll) |
| Attempts behind the recorded result | 1 |
| Polls performed | 1, at 2026-09-03T14:42:11.8832118-04:00 |
| Poll 1 result | not alive (completion signal) |
| Completion witness | `coverage\trx\P4-T5\P4-T5.trx` — present and parsed |
| Fault file present | No |
| **EXIT_CODE** | **0** |
| Assembly count | **9** |

The poll was preceded by its bounded `Wait-Process -Id 110128 -Timeout 120 -ErrorAction SilentlyContinue`.
`EXIT_CODE:` was read from `coverage\logs\P4-T5.exit.txt` per step 4 of the convention, never from
`$proc.ExitCode`, and is neither `9001` nor `9002`.

The recorded `LaunchedPid` value 110128 coincides with the pid recorded by P0-T14; the operating system
reused the pid after the earlier process exited. The two are distinguished by their `LaunchedStart` values
(P0-T14: 2026-09-03T14:24:03.7557580-04:00; this run: 2026-09-03T14:41:25.9452273-04:00), which is exactly
the start-time check step 3 of the convention prescribes for this situation.

## Counters

| Counter | Value |
|---|---|
| Total | **6984** |
| Executed | 6984 |
| Passed | **6984** |
| Failed | **0** |
| NotExecuted (Skipped) | **0** |
| ResultSummary outcome | Completed |

## Acceptance — the two sets, printed so the subset relation is checkable

**Set A — failed fully qualified names in this run:**

```
(empty)
```

**Set B — `BASELINE_FAILURE_SET` as recorded by P0-T14:**

```
(empty)
```

| Required | Observed | Result |
|---|---|---|
| The set of failed fully qualified names in this run is a subset of `BASELINE_FAILURE_SET` | empty subset of empty | PASS |
| Every test whose fully qualified name contains `AppOlObjectsFolderTreeService` and is not itself a member of `BASELINE_FAILURE_SET` is recorded with outcome `Passed` | 31 matching tests; 0 not passed | PASS |
| `TerminalNotificationHookFailure_DoesNotReplaceDispatchFault` is recorded with outcome `Passed` unconditionally | Passed, duration 00:00:00.0023735, assembly `taskmaster.test.dll` | PASS |

The second condition was evaluated mechanically over the TRX: 31 test results carry a fully qualified name
containing `AppOlObjectsFolderTreeService`, and 0 of them recorded an outcome other than `Passed`. Because
`BASELINE_FAILURE_SET` is empty, none of the 31 is exempted by the baseline clause.

## Bearing on P5-T9

This run recorded `EXIT_CODE: 0` with a **zero** failed-name set. The P5-T9 Outcome C condition requires the
P4-T6 artifact to record a **non-zero** exit code for the P4-T5 step arising from a non-empty failed-name
set. That condition is **not** met, so Outcome C does not apply.

## Coverage attachment produced by this run

`Get-ChildItem -Path 'coverage\trx\P4-T5' -Recurse -Filter '*.coverage'` returned **2** files, the same
structural situation P0-T14 produced: the published attachment plus vstest's in-run copy under its
`In\<machine>\` subtree. The paths are not transcribed, because the results directory names embed the
account name. The consequence for P4-T12 is recorded in that task's artifact.
