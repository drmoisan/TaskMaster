# Baseline — vstest over the nine assemblies (P0-T6)

Timestamp: 2026-09-05T19-24

Command:

```powershell
$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
$vstest = & $vswhere -latest -products * -find Common7\IDE\Extensions\TestPlatform\vstest.console.exe |
    Select-Object -First 1
& $vstest `
    QuickFiler.Test\bin\Debug\QuickFiler.Test.dll `
    SVGControl.Test\bin\Debug\SVGControl.Test.dll `
    Tags.Test\bin\Debug\Tags.Test.dll `
    TaskMaster.Test\bin\Debug\TaskMaster.Test.dll `
    TaskTree.Test\bin\Debug\TaskTree.Test.dll `
    TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll `
    ToDoModel.Test\bin\Debug\ToDoModel.Test.dll `
    UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll `
    VBFunctions.Test\bin\Debug\VBFunctions.Test.dll `
    '/Settings:scripts\vscode\TaskMaster.cli.runsettings' `
    '/InIsolation' `
    '/Logger:trx' `
    '/ResultsDirectory:TestResults\782-p0-baseline' `
    '/Blame:CollectHangDump;TestTimeout=5min;HangDumpType=None' `
    '/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests'
```

The `/Blame:` switch is written in single quotes so PowerShell does not truncate it at the first
semicolon. `/EnableCodeCoverage` is deliberately not passed:
`scripts/vscode/TaskMaster.cli.runsettings` carries no data collector and no coverage exclusions, so
the built-in collector would instrument Deedle and FSharp.Core, which is the failure mode
`coverage.config` exists to prevent, and `scripts/vscode/Invoke-MSTestWithCoverage.ps1` lines 22-24
state that omission is deliberate. Coverage for the baseline is collected separately by P0-T7
through `dotnet-coverage` with the derived configuration.

EXIT_CODE: 0

Output Summary:

Console summary, verbatim:

```text
Test Run Successful.
Total tests: 6992
     Passed: 6992
 Total time: 41.9310 Seconds
```

`vstest.console.exe` omits the `Failed:` and `Skipped:` lines when both are zero. The TRX
`ResultSummary/Counters` element was read directly to record those two values as explicit numerals:

| Field | Value |
|---|---|
| Total tests | 6992 |
| Passed | 6992 |
| Failed | 0 |
| Skipped (TRX `notExecuted`) | 0 |
| TRX outcome | Completed |

**These are locally-filtered figures, not CI figures.** The four shell-icon test classes
`HelperClasses.ShellUtilities_Tests`, `HelperClasses.ShellUtilitiesStatic_Tests`,
`HelperClasses.SysImageListHelperTests`, and `EmailIntelligence.OSBrowser_Tests` are excluded by the
`/TestCaseFilter` expression because they issue `SHGetFileInfo` with `SHGFI_ICON`, which stalls
process-wide on this workstation and hangs the test host. That stall reproduces against
`origin/main`, so it is environmental; CI covers those classes.

The observed figures match the tabled baseline of 6992 / 6992 / 0 exactly. No
`BASELINE_TOTAL_TESTS:` escape line is required, so P4-T11 and P7-T5 derive their expected minimum
from the tabled 6992 plus three, which is 6995.

`DictionaryExtensions_Tests.TryAddValuesAsync_UpdatesExistingValue`, the known issue #780 flake, did
not fail on this run, so no re-run was required.
