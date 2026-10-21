# [P0-T12] Baseline full-suite test run (no coverage)

Timestamp: 2026-09-08T00-40

Command: `& $vstest` followed by the nine-assembly list and the full-suite switch set with `<task-id>` `p0t12`:

```
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll SVGControl.Test\bin\Debug\SVGControl.Test.dll Tags.Test\bin\Debug\Tags.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll TaskTree.Test\bin\Debug\TaskTree.Test.dll TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll ToDoModel.Test\bin\Debug\ToDoModel.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll VBFunctions.Test\bin\Debug\VBFunctions.Test.dll '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx;LogFileName=p0t12.trx' '/ResultsDirectory:TestResults\809-p0t12' '/Blame:CollectHangDump;TestTimeout=5min;HangDumpType=None' '/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests'
```

`$vstest` was resolved by the vswhere form the standing conventions state and resolved to the Visual Studio 18 Community `vstest.console.exe`. `TestResults\809-p0t12` was created with `New-Item -ItemType Directory -Force` before the run.

EXIT_CODE: 0

BASELINE_TOTAL_TESTS: 7120

Output Summary:

```
Test Run Successful.
Total tests: 7120
     Passed: 7120
```

The console printed no `Failed:` line and no `Skipped:` line, which is the success-case output shape for this runner.

TRX selected: `p0t12.trx`, `LastWriteTimeUtc` `2026-09-08T04:19:46.3498332Z`, selected as the most recently modified `.trx` under `TestResults\809-p0t12` by `Get-ChildItem -Path <that directory> -Filter *.trx | Sort-Object LastWriteTimeUtc | Select-Object -Last 1`.

TRX `ResultSummary/Counters` attributes: `total` 7120, `executed` 7120, `passed` 7120, `failed` 0.

The `failed` attribute is 0.

SKIPPED_DERIVED: 0

`SKIPPED_DERIVED` is the TRX `total` attribute minus the TRX `executed` attribute. The TRX `notExecuted` attribute is not used, because the TRX logger writes it as `0` regardless of what the run did.
