# Baseline — vstest over the nine assemblies (P0-T6, re-recorded under SD23)

SUPERSEDED BASELINE RE-RECORDED: SD23

RE-ANCHORED BASE: 736c2cf2

Timestamp: 2026-09-05T21-58

## Why the earlier figure is superseded

An external actor rebased the feature branch from `a007f72e` onto `origin/main` at `77c6d314`
during execution. Every prior commit received a new SHA. The base commit the superseded record was
taken at, `b95a5252`, is orphaned and is no longer an ancestor of HEAD, so the figure it carried
describes a tree that is no longer this branch's baseline.

The superseded figure was **6992**. The re-measured figure is **6997**. The rise of exactly five is
consistent with the 419-line `ItemViewerBreadcrumbThreadAffinityTests.cs` added to `QuickFiler.Test`
by the main advance. That file is not in this delivery's Write Set, and the main advance touches no
file that is.

## Measurement method and measuring party

This gate was measured by the **orchestrator, not the executor**, at the re-anchored base commit
`736c2cf2`, by the temporary-restore method: the orchestrator restored the six Write Set source
files Phase 1 has changed so far — `UtilitiesCS/Threading/UiThread.cs`,
`UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs`, `UtilitiesCS/Threading/ProgressTracker.cs`,
`UtilitiesCS/Threading/ProgressTrackerAsync.cs`, `TaskMaster/Ribbon/RibbonViewer.EngineCommands.cs`,
and `UtilitiesCS.Test/Threading/UiThread_Tests.cs` — to their `pre-782-base` content with
`git checkout pre-782-base -- <those six paths>`, ran the four gates, restored those files to HEAD
in a `finally` block, and left the worktree clean and at HEAD afterwards.

The executor did **not** re-run vstest for this task, and this artifact does not present the figures
as an executor run.

Command (the orchestrator's command):

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
semicolon. `/EnableCodeCoverage` is deliberately not passed (SD17):
`scripts/vscode/TaskMaster.cli.runsettings` carries no data collector and no coverage exclusions, so
the built-in collector would instrument Deedle and FSharp.Core, which is the failure mode
`coverage.config` exists to prevent, and `scripts/vscode/Invoke-MSTestWithCoverage.ps1` lines 22-24
state that omission is deliberate. Coverage for the baseline is recorded separately by P0-T7
through `dotnet-coverage` with the derived configuration.

EXIT_CODE: 0

That is the exit code the **orchestrator** observed, not an exit code the executor observed.

BASELINE_TOTAL_TESTS: 6997

Output Summary:

| Field | Value |
|---|---|
| Total tests | 6997 |
| Passed | 6997 |
| Failed | 0 |
| Skipped | 0 |

Recorded as the quoted summary values:

```text
Total tests: 6997
     Passed: 6997
     Failed: 0
    Skipped: 0
```

`vstest.console.exe` omits the `Failed:` and `Skipped:` lines when both are zero, so those two
values were read directly from the TRX `ResultSummary/Counters` element and are recorded above as
explicit numerals.

**These are locally-filtered figures, not CI figures.** The four shell-icon test classes
`HelperClasses.ShellUtilities_Tests`, `HelperClasses.ShellUtilitiesStatic_Tests`,
`HelperClasses.SysImageListHelperTests`, and `EmailIntelligence.OSBrowser_Tests` are excluded by the
`/TestCaseFilter` expression because they issue `SHGetFileInfo` with `SHGFI_ICON`, which stalls
process-wide on this workstation and hangs the test host. That stall reproduces against
`origin/main`, so it is environmental; CI covers those classes.

P4-T11 and P7-T5 derive their expected minimum from the `BASELINE_TOTAL_TESTS:` line above plus
three, which is **7000** for this re-recorded baseline of 6997, because this delivery adds three new
tests and removes none. The expected value is derived from that recorded line rather than from any
figure tabled in the plan, so a further baseline correction propagates without editing those tasks.

`DictionaryExtensions_Tests.TryAddValuesAsync_UpdatesExistingValue`, the known issue #780 flake, did
not fail on this run, so no re-run was required.
