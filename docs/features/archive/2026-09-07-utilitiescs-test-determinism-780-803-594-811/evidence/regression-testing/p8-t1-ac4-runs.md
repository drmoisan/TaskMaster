# P8-T1 — AC4 runs 1 and 2

Timestamp: 2026-09-08T10-26
Task: [P8-T1]
Command: <vstest> <nine assemblies> /EnableCodeCoverage /InIsolation "/Logger:trx;LogFileName=p8-t1-run<k>.trx" /ResultsDirectory:coverage/trx/p8-t1 /Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~ShellUtilities&FullyQualifiedName!~SysImageListHelper&FullyQualifiedName!~OSBrowser"
EXIT_CODE: 0
EXIT_CODE (run 1): 0

AC4_COMMAND_SHAPE: CI-VERBATIM, read from `p0-t12-ci-shape-probe.md`. No `/Settings:` argument is
passed, so parallelism comes only from `[assembly: Parallelize(Workers = 0, Scope = ClassLevel)]`
at `UtilitiesCS.Test/Properties/AssemblyInfo.cs:18-21`, exactly as in CI.

## Tree state

| Observation | Value |
|---|---|
| `git rev-parse HEAD` | `03b7bd57cacfc902a8b9f4e917ace627ffcac464` |
| Equals `SOURCE-HEAD` from P7-T12 | `True` |
| `git status --porcelain -- "*.cs" "*.csproj"` entry count | 0 |

The ten runs are consecutive on one unchanged tree; no source file is edited between them.

## Counters

| Run | total | executed | passed | failed | error | aborted | timeout | notExecuted | seconds |
|---|---|---|---|---|---|---|---|---|---|
| 1 | 7162 | 7162 | 7162 | 0 | 0 | 0 | 0 | 0 | 70.9 |
| 2 | 7162 | 7162 | 7162 | 0 | 0 | 0 | 0 | 0 | 54.2 |

Both `total` values equal the P7-T5 `total` of 7162. Both exit codes are 0. No result in either run
carried an outcome other than `Passed`.

## Non-vacuity control (run 1)

All nine C4 names read `Passed` from `p8-t1-run1.trx`. An unregistered or undiscovered test would
read `ABSENT`, which would fail this clause; none did.

| Test | Outcome |
|---|---|
| `TryAddValuesAsync_PreCancelledToken_ThrowsTaskCanceledAndLeavesValueUnchanged` | `Passed` |
| `GetEmailDataInViewAsync_EtlDeadlineExpires_ThrowsInvalidOperationNamingFolder` | `Passed` |
| `GetEmailDataInViewAsync_ClockNeverAdvances_ReturnsOneRowFrame` | `Passed` |
| `EtlAsync_DeadlineExpires_ReturnsNullDataAndCancelsTokenSource` | `Passed` |
| `EtlAsync_ClockNeverAdvances_ReturnsTransformedRows` | `Passed` |
| `Run_WritesScenarioToSuppliedWriter` | `Passed` |
| `PrettyPrint_NullWriter_WritesToConsoleWithoutThrowing` | `Passed` |
| `TryAddValuesAsync_UpdatesExistingValue` | `Passed` |
| `GetEmailDataInViewAsync_SeparatesTableSnapshotFromDataFrameTransform` | `Passed` |

## `.coverage` cleanup

4 `*.coverage` files found under `coverage/trx/p8-t1` (two per run), all deleted after the counters
were read. 0 remain. They are large binaries and are never committed.

## Local worker count

`[Environment]::ProcessorCount` is 24, which is what `Workers = 0` resolves to on this workstation.

## Acceptance evaluation

- Both TRX files exist. PASS
- Both runs report `failed`=0, `error`=0, `aborted`=0, `timeout`=0, and `executed`=`total`. PASS
- Both `total` values equal the P7-T5 `total` (7162). PASS
- Both exit codes are 0. PASS
- `Read-TrxOutcome` on `p8-t1-run1.trx` is `Passed` for all nine C4 names. PASS

## Output Summary

AC4 runs 1 and 2 of 10: 7162 tests each, all passed, both exit 0, 70.9 s and 54.2 s. The
non-vacuity control confirms all nine tracked tests were discovered and executed.
