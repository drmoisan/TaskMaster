# QA Gate — No Test Lost by the Split (P2-T5)

Timestamp: 2026-09-05T22-05

Command:

```powershell
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU"

$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
$vstest = & $vswhere -latest -products * -find Common7\IDE\Extensions\TestPlatform\vstest.console.exe |
    Select-Object -First 1
& $vstest `
    UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll `
    '/Settings:scripts\vscode\TaskMaster.cli.runsettings' `
    '/InIsolation' `
    '/Logger:trx' `
    '/ResultsDirectory:TestResults\782-p2' `
    '/Blame:CollectHangDump;TestTimeout=5min;HangDumpType=None' `
    '/TestCaseFilter:FullyQualifiedName~UtilitiesCS.Test.ProgressTracker_Tests'
```

The `/Blame:` switch is written in single quotes so PowerShell does not truncate it at the first
semicolon.

EXIT_CODE: 0

The build recorded `Build succeeded.`, `0 Warning(s)`, `0 Error(s)` and exited 0. The test run
exited 0.

Output Summary:

Console summary, verbatim:

```text
Test Run Successful.
Total tests: 24
     Passed: 24
```

The fully-qualified names and outcomes below were read from the TRX `UnitTestResult` elements,
joined to the `TestDefinitions/UnitTest` entries so each row carries the full
`className.methodName` form rather than the bare method name.

| # | Outcome | Fully-qualified name |
|---|---|---|
| 1 | Passed | `UtilitiesCS.Test.ProgressTracker_Tests.Constructor_WithParent_ShouldInheritJobName` |
| 2 | Passed | `UtilitiesCS.Test.ProgressTracker_Tests.Increment_ShouldAccumulateProgressValues` |
| 3 | Passed | `UtilitiesCS.Test.ProgressTracker_Tests.Increment_ShouldClampAt100` |
| 4 | Passed | `UtilitiesCS.Test.ProgressTracker_Tests.Increment_ShouldUpdateProgressAndForwardScaledValueAndJobName` |
| 5 | Passed | `UtilitiesCS.Test.ProgressTracker_Tests.Initialize_WithCurrentDispatcherAndScreen_InitializesViewerAndUpdatesUi` |
| 6 | Passed | `UtilitiesCS.Test.ProgressTracker_Tests.Report_At100Percent_SetsProgressToMaxAndForwardsToParent` |
| 7 | Passed | `UtilitiesCS.Test.ProgressTracker_Tests.Report_At100Percent_WhenRootTracker_ClosesProgressViewer` |
| 8 | Passed | `UtilitiesCS.Test.ProgressTracker_Tests.Report_DoubleOverload_ShouldClampAbove100` |
| 9 | Passed | `UtilitiesCS.Test.ProgressTracker_Tests.Report_DoubleOverload_ShouldThrowForNegative` |
| 10 | Passed | `UtilitiesCS.Test.ProgressTracker_Tests.Report_ShouldClampValuesAboveOneHundred` |
| 11 | Passed | `UtilitiesCS.Test.ProgressTracker_Tests.Report_ShouldThrowForNegativeValues` |
| 12 | Passed | `UtilitiesCS.Test.ProgressTracker_Tests.Report_ViaChild_ShiftsParentProgressByAllocatedRange` |
| 13 | Passed | `UtilitiesCS.Test.ProgressTracker_Tests.Report_WithDoubleAndJobName_ShouldClampAt100` |
| 14 | Passed | `UtilitiesCS.Test.ProgressTracker_Tests.Report_WithDoubleAndJobName_ShouldThrowForNegative` |
| 15 | Passed | `UtilitiesCS.Test.ProgressTracker_Tests.Report_WithJobName_RootReportsToStubPane` |
| 16 | Passed | `UtilitiesCS.Test.ProgressTracker_Tests.Report_WithTupleOverload_ShouldSetValueAndJobName` |
| 17 | Passed | `UtilitiesCS.Test.ProgressTracker_Tests.Report_WithValueAndJobName_UpdatesProgressAndForwardsMessage` |
| 18 | Passed | `UtilitiesCS.Test.ProgressTracker_Tests.ReportAsync_At100Percent_WhenRootTracker_ClosesProgressViewer` |
| 19 | Passed | `UtilitiesCS.Test.ProgressTracker_Tests.ReportAsync_WithNegativeValue_ThrowsArgumentOutOfRangeException` |
| 20 | Passed | `UtilitiesCS.Test.ProgressTracker_Tests.ReportAsync_WithValueOver100_ClampsTo100` |
| 21 | Passed | `UtilitiesCS.Test.ProgressTracker_Tests.SpawnChild_FromProgressedParent_MapsChildProgressIntoParentRange` |
| 22 | Passed | `UtilitiesCS.Test.ProgressTracker_Tests.SpawnChild_ShouldUseRemainingAllocationFromCurrentProgress` |
| 23 | Passed | `UtilitiesCS.Test.ProgressTracker_Tests.SpawnChild_WithAllocation_ShouldCreateChildWithSpecifiedAllocation` |
| 24 | Passed | `UtilitiesCS.Test.ProgressTracker_Tests.SpawnChild_WithDoubleAllocation_ShouldRoundAndCreateChild` |

Exactly 24 fully-qualified names were recorded. All 24 begin `UtilitiesCS.Test.ProgressTracker_Tests.`
and all 24 have outcome `Passed`.

## The partial class reassembled under the original names

The list contains one test from each part of the split, which is what proves the two files were
recompiled into a single `ProgressTracker_Tests` type rather than into two distinct types:

- `Initialize_WithCurrentDispatcherAndScreen_InitializesViewerAndUpdatesUi` — row 5 — now lives in
  `UtilitiesCS.Test/Threading/ProgressTracker_ReportAndViewerTests.cs`, the moved part.
- `Increment_ShouldUpdateProgressAndForwardScaledValueAndJobName` — row 4 — remains in
  `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs`, the retained part.

Both resolve under the original `UtilitiesCS.Test.ProgressTracker_Tests` class name, so no test
changed its fully-qualified name and no test was lost.

These are locally-filtered figures over one assembly, not CI figures.
