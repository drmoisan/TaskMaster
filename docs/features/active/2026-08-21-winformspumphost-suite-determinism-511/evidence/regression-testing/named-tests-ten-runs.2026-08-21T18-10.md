# P4-T4 — The Two Named Tests Across the Ten Runs

Timestamp: 2026-08-22T14-35

Command:
```powershell
Get-ChildItem docs/features/active/winformspumphost-suite-determinism-511/evidence/regression-testing/p4-t2/*.trx |
  Sort-Object Name |
  ForEach-Object {
      [xml]$x = Get-Content -Raw $_.FullName
      # map TestDefinitions/UnitTest id -> TestMethod name, then read Results/UnitTestResult outcome
  }
```

Each cell was read from that run's own TRX by resolving the result's `testId` against the run's
`TestDefinitions`, so a cell reports the outcome recorded for that exact test in that exact file.

EXIT_CODE: 0

Output Summary:

Column abbreviations: **Bool** = `InitializeBool_ThroughThePumpHost_CompletesAndInitializesState`;
**NineArg** = `InitializeNineArgOverload_ThroughThePumpHost_SavesParametersAndDelegates`.

| # | TRX | Bool | NineArg |
| --- | --- | --- | --- |
| 1 | `2026-08-22_11_53_56_net481.trx` | passed | passed |
| 2 | `2026-08-22_12_12_50_net481.trx` | passed | passed |
| 3 | `2026-08-22_12_30_40_net481.trx` | passed | passed |
| 4 | `2026-08-22_12_37_28_net481.trx` | passed | passed |
| 5 | `2026-08-22_12_53_39_net481.trx` | passed | passed |
| 6 | `2026-08-22_13_10_45_net481.trx` | passed | passed |
| 7 | `2026-08-22_13_17_44_net481.trx` | passed | passed |
| 8 | `2026-08-22_13_32_03_net481.trx` | passed | passed |
| 9 | `2026-08-22_13_39_20_net481.trx` | passed | passed |
| 10 | `2026-08-22_14_03_59_net481.trx` | passed | passed |

Acceptance: the table has exactly ten rows and every cell reads passed.

Neither test was recorded as `NotExecuted` in any of the ten files; each run's TRX carries
`notExecuted="0"`, so no cell above is a skipped test reported as absent.

Run 5 is the run that recorded one suite-wide failure. That failure is
`UtilitiesCS.Test.Extensions.DfDeedle_COM_Tests.GetEmailDataInViewAsync_SeparatesTableSnapshotFromDataFrameTransform`,
in a different assembly; both tests tracked by this task passed in run 5. The failure is recorded
against P4-T2, whose acceptance condition it violates.
