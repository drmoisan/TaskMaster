# Phase 0 — Ribbon Fixture Test Baseline (P0-T8)

Timestamp: 2026-09-03T01-23
Task: [P0-T8]
Command:

```
& $vstest TaskMaster.Test\bin\Debug\TaskMaster.Test.dll `
  /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation `
  "/TestCaseFilter:FullyQualifiedName~TaskMaster.Test.Ribbon" `
  "/Logger:trx;LogFileName=p0-t8.trx" `
  /ResultsDirectory:docs\features\active\2026-09-02-ribbon-engine-toggle-defects-735\evidence\baseline\p0-t8
```

EXIT_CODE: 0

vstest.console.exe resolved through vswhere to VSTest version 18.9.0 (x64). `/InIsolation` is
mandatory for the Moq-based assemblies.

## Results directory contents

Exactly one file, and no others:

```
docs\features\active\2026-09-02-ribbon-engine-toggle-defects-735\evidence\baseline\p0-t8\p0-t8.trx
```

## Counts read from the TRX `ResultSummary/Counters` element

| Counter | Value |
|---|---|
| total | 107 |
| executed | 107 |
| passed | 107 |
| failed | 0 |
| error | 0 |
| notExecuted (skipped) | 0 |
| inconclusive | 0 |

Console summary agreed: `Total tests: 107  Passed: 107  Total time: 1.6532 Seconds`, `Test Run
Successful.`

## Comparison basis established

This population of **107** is the baseline that P1-T8 and P3-T12 are compared against. P1-T8's
acceptance requires total equal to 107 + 2 = 109 with failed 0, after the two new
`RibbonExplorerXmlTests` methods are added.

The pre-existing set-equality test
`RibbonExplorerXml_GetEnabledIsDeclaredOnlyOnEngineBackedControls` is among the 107 passing tests.
It is the test that will prove, in P1-T8, that deleting the `BtnMigrateIDs` button did not disturb
the engine-command control set. The pre-existing ordering test
`ExecuteToggleAsync_PerformsToggleThenRefreshThenCacheThenInvalidate_InOrder` and the pre-existing
faulted-prime test `GetPressed_WhenPrimeFaults_LogsErrorAndStillReturnsFalse`, both required by
P3-T12 to remain passing and unmodified, are also among the 107.

Output Summary: Baseline ribbon fixture run succeeded with EXIT_CODE 0. TRX counters report total
107, passed 107, failed 0, skipped 0. The results directory holds exactly one TRX file.
