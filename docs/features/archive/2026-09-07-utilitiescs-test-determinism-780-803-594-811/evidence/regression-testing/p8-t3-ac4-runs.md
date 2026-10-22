# P8-T3 — AC4 runs 5 and 6

Timestamp: 2026-09-08T10-31
Task: [P8-T3]
Command: <vstest> <nine assemblies> /EnableCodeCoverage /InIsolation "/Logger:trx;LogFileName=p8-t3-run<k>.trx" /ResultsDirectory:coverage/trx/p8-t3 /Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~ShellUtilities&FullyQualifiedName!~SysImageListHelper&FullyQualifiedName!~OSBrowser"
EXIT_CODE: 0
EXIT_CODE (run 5): 0

AC4_COMMAND_SHAPE: CI-VERBATIM. No `/Settings:` argument.

## Tree state

| Observation | Value |
|---|---|
| `git rev-parse HEAD` | `03b7bd57cacfc902a8b9f4e917ace627ffcac464` |
| Equals `SOURCE-HEAD` from P7-T12 | `True` |
| `git status --porcelain -- "*.cs" "*.csproj"` entry count | 0 |

## Counters

| Run | total | executed | passed | failed | error | aborted | timeout | notExecuted | seconds |
|---|---|---|---|---|---|---|---|---|---|
| 5 | 7162 | 7162 | 7162 | 0 | 0 | 0 | 0 | 0 | 51.3 |
| 6 | 7162 | 7162 | 7162 | 0 | 0 | 0 | 0 | 0 | 70.7 |

Both `total` values equal the P7-T5 `total` of 7162. Both exit codes are 0. No result carried an
outcome other than `Passed`.

## `.coverage` cleanup

4 files found, all deleted, 0 remain.

## Acceptance evaluation

- Both TRX files exist. PASS
- Both runs report `failed`=0, `error`=0, `aborted`=0, `timeout`=0, `executed`=`total`. PASS
- Both `total` values equal the P7-T5 `total`. PASS
- Both exit codes are 0. PASS

## Output Summary

AC4 runs 5 and 6 of 10: 7162 tests each, all passed, both exit 0, 51.3 s and 70.7 s.
