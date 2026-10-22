# P7-T5 — Toolchain step 4 (final nine-assembly coverage run)

Timestamp: 2026-09-08T10-16
Task: [P7-T5]
Command: dotnet-coverage collect --output coverage/p7-final.cobertura.xml --output-format cobertura --settings coverage.config -- <vstest> <nine assemblies> /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/Logger:trx;LogFileName=p7-t5.trx" /ResultsDirectory:coverage/trx/p7-t5 /Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~ShellUtilities&FullyQualifiedName!~SysImageListHelper&FullyQualifiedName!~OSBrowser"
EXIT_CODE: 0
Toolchain pass: 3

RERUN_COUNT: 0

Each execution of this task passed on its first run. No re-run-until-green was performed and none
is permitted: any failure here, including a failure of one of the two retained sentinels, would
have been a defect requiring a fix and a restart of the loop at P7-T1.

This task ran twice, once per toolchain-loop pass. The pass-2 run recorded
`total=7160 passed=7160 failed=0`, exit 0, 76 s, `line-rate=0.8603698145941662`. P7-T6 then failed
its clause 2 on two uncovered added lines, P7-T7 added two tests to close them, and the loop
restarted at P7-T1. The figures below are the final pass-3 run, which is the one this artifact
records.

## TRX counters (final pass)

```
total=7162 executed=7162 passed=7162 failed=0 error=0 timeout=0 aborted=0 notExecuted=0
```

`failed` + `error` + `aborted` + `timeout` = 0. No result carried an outcome other than `Passed`.

## Test-count delta

| Observation | Value |
|---|---|
| P0-T10 baseline `total` | 7153 |
| P7-T5 final `total` | 7162 |
| Delta | +9 |

The plan's stated expectation is +7 for the seven C4 tests, adjusted upward by the count of any
test P7-T7 adds. P7-T7 added two, so the adjusted expectation is +9 and the observed delta is
exactly +9. The seven C4 tests:

1. `DictionaryExtensions_Tests.TryAddValuesAsync_PreCancelledToken_ThrowsTaskCanceledAndLeavesValueUnchanged`
2. `DfDeedleEtlTimeoutTests.GetEmailDataInViewAsync_EtlDeadlineExpires_ThrowsInvalidOperationNamingFolder`
3. `DfDeedleEtlTimeoutTests.GetEmailDataInViewAsync_ClockNeverAdvances_ReturnsOneRowFrame`
4. `OlTableExtensionsEtlClockTests.EtlAsync_DeadlineExpires_ReturnsNullDataAndCancelsTokenSource`
5. `OlTableExtensionsEtlClockTests.EtlAsync_ClockNeverAdvances_ReturnsTransformedRows`
6. `StackGeek_Tests.Run_WritesScenarioToSuppliedWriter`
7. `PrettyPrint_Tests.PrettyPrint_NullWriter_WritesToConsoleWithoutThrowing`

plus the two tests P7-T7 added:

8. `DfDeedleEtlTimeoutTests.GetEmailDataInView_NoEtlArgument_UsesProductionDefaultDelegate`
9. `OlTableExtensionsEtlClockTests.EtlAsync_NoBinaryOrObjectFields_UsesGetArrayBranchOnControlledClock`

The delta being exactly 9 also confirms that the three renamed tests were renamed and not
duplicated, and that no test was lost when `Main_RunsSampleScenarioWithoutThrowing` was rewritten.

## The nine C4 names

| Test | Outcome |
|---|---|
| `TryAddValuesAsync_PreCancelledToken_ThrowsTaskCanceledAndLeavesValueUnchanged` | `Passed` |
| `GetEmailDataInViewAsync_EtlDeadlineExpires_ThrowsInvalidOperationNamingFolder` | `Passed` |
| `GetEmailDataInViewAsync_ClockNeverAdvances_ReturnsOneRowFrame` | `Passed` |
| `EtlAsync_DeadlineExpires_ReturnsNullDataAndCancelsTokenSource` | `Passed` |
| `EtlAsync_ClockNeverAdvances_ReturnsTransformedRows` | `Passed` |
| `Run_WritesScenarioToSuppliedWriter` | `Passed` |
| `PrettyPrint_NullWriter_WritesToConsoleWithoutThrowing` | `Passed` |
| `TryAddValuesAsync_UpdatesExistingValue` (retained #780 sentinel) | `Passed` |
| `GetEmailDataInViewAsync_SeparatesTableSnapshotFromDataFrameTransform` (retained #803 sentinel) | `Passed` |

None returned `ABSENT`. The two P7-T7 gap-closure tests,
`GetEmailDataInView_NoEtlArgument_UsesProductionDefaultDelegate` and
`EtlAsync_NoBinaryOrObjectFields_UsesGetArrayBranchOnControlledClock`, also both report `Passed`.

## Root Cobertura figures

Read from the root element of `coverage/p7-final.cobertura.xml`.

| Attribute | Baseline (P0-T10) | Final (P7-T5 pass 3) |
|---|---|---|
| `line-rate` | `0.8601092896174863` | `0.8604239666249645` |
| `lines-covered` | `172353` | `172626` |
| `lines-valid` | `200385` | `200629` |
| `branch-rate` | `0.6630576006929406` | `0.6639784946236559` |
| `branches-covered` | `21434` | `21489` |
| `branches-valid` | `32326` | `32364` |

Repository-wide line coverage rose from 86.011 to 86.042 percent. The full delta analysis,
including the per-file and per-changed-line gates, is in `p7-t6-coverage-delta.md`.

## Wall clock

RUN_SECONDS: 58 for the final pass (the pass-2 run took 76 s; the baseline run took 64 s). The
variation is run-to-run scheduling noise on a shared workstation, not a property of the change.

## Acceptance evaluation

- `EXIT_CODE: 0`. PASS
- Counters recorded. PASS
- `failed` + `error` + `aborted` + `timeout` = 0, with `RERUN_COUNT: 0`. PASS
- `total` exceeds the P0-T10 `total` by exactly 9, the adjusted expectation stated above (7 C4
  tests plus the 2 tests P7-T7 added). PASS
- All six root Cobertura figures recorded as numbers. PASS
- `Read-TrxOutcome` is `Passed` for all nine C4 names. PASS

## Output Summary

Final full-suite coverage run: 7162 tests, 7162 passed, 0 failed, exit 0, 58 s, no re-runs.
Repository-wide line coverage 0.860424 (172626 of 200629). All nine tracked test names pass,
including both intermittent-failure sentinels this item repairs, as do the two gap-closure tests.
