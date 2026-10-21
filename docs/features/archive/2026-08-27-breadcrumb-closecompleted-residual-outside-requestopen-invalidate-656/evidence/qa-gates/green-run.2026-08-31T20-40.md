# QA Gate — Pass-After Run (Issue #656)

Timestamp: 2026-09-01T14-47
Task: [P3-T2]

Command:
```
$vswhere = 'C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe'
$vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
New-Item -ItemType Directory -Force -Path 'TestResults\p3-t2' | Out-Null
& $vstest 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName~CloseCore_AfterSuccessfulCloseAndHostReopen_ReachesHostCloseAgain' '/Logger:trx' '/ResultsDirectory:TestResults\p3-t2'
```

This command is identical to the P1-T3 red-run command apart from the results directory.

EXIT_CODE: 0

## TRX counter values

- `total` = 1
- `passed` = 1
- `failed` = 0

Read from `TestRun/ResultSummary/Counters` of the TRX written to
`<repo-root>/TestResults/p3-t2/<user>_<host>_2026-09-01_14_46_44_net481.trx`.

Per-test outcome from the TRX `UnitTestResult` node:
`Passed :: CloseCore_AfterSuccessfulCloseAndHostReopen_ReachesHostCloseAgain`

Console summary: `Test Run Successful.` / `Total tests: 1` / `Passed: 1`.

The only change between the red run and this run is the Phase 2 edit to
`QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs`: the hoisted `bool hostOpen = _host.IsOpen;`
and the narrowed guard `if (_closeCompleted && !hostOpen)`. The test text was not altered between
the two runs, and no other file was touched, so the transition from failing to passing is
attributable to the production fix alone.

Output Summary: The new regression test passes after the production edit. Exit code 0, 1 total, 1
passed, 0 failed. Together with the P1-T3 red run this establishes the fail-before / pass-after pair.
