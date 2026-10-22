# QA Gate — Phase 1 Scoped Test Run (P1-T9)

Timestamp: 2026-09-05T22-01

This run is the first execution of P1-T9 to reach its acceptance condition. The first execution
attempt was blocked: the C03 latch re-arm then present in `UiThread.Init()` caused
`UtilitiesCS.Test.Extensions.DictionaryExtensions_Tests.TryAddValuesAsync_UpdatesExistingValue` to
fail reproducibly with `TaskCanceledException`. SD18 withdraws that re-arm and P1-T3 reverts it, so
the condition is now reachable. This run is taken against the reverted tree and the re-run builds
recorded in `evidence/qa-gates/p1-t8-phase1-builds.md`.

Command:

```powershell
$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
$vstest = & $vswhere -latest -products * -find Common7\IDE\Extensions\TestPlatform\vstest.console.exe |
    Select-Object -First 1
& $vstest `
    UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll `
    QuickFiler.Test\bin\Debug\QuickFiler.Test.dll `
    TaskMaster.Test\bin\Debug\TaskMaster.Test.dll `
    '/Settings:scripts\vscode\TaskMaster.cli.runsettings' `
    '/InIsolation' `
    '/Logger:trx' `
    '/ResultsDirectory:TestResults\782-p1' `
    '/Blame:CollectHangDump;TestTimeout=5min;HangDumpType=None' `
    '/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests'
```

The `/Blame:` switch is written in single quotes so PowerShell does not truncate it at the first
semicolon. `/InIsolation` is mandatory: without it the app.config binding redirects are not loaded
and roughly 1700 tests fail with empty messages and sub-millisecond durations, which resembles a
regression but is an invocation defect.

EXIT_CODE: 0

Output Summary:

Console summary, verbatim:

```text
Test Run Successful.
Total tests: 6519
     Passed: 6519
```

`vstest.console.exe` omits the `Failed:` and `Skipped:` lines when both are zero. The TRX
`ResultSummary/Counters` element was read directly to record those two values as explicit numerals:

| Field | Value |
|---|---|
| Total tests | 6519 |
| Passed | 6519 |
| Failed | 0 |
| Skipped (TRX `notExecuted`) | 0 |
| TRX outcome | Completed |

**These are locally-filtered figures over three assemblies** — `UtilitiesCS.Test`,
`QuickFiler.Test`, and `TaskMaster.Test` — with the four shell-icon classes and the `LiveOutlook`
category excluded. They are not CI figures and they are not the nine-assembly figure; the
nine-assembly baseline is 6997, recorded in `evidence/baseline/p0-t6-vstest.md`.

## The named acceptance test

`Dispatcher_WhenBackingFieldIsNull_ThrowsInvalidOperationExceptionNamingInitialize` appears in the
TRX exactly once with outcome **`Passed`**. That proves the P1-T5 assertion change
(`WithMessage("*UiThread.Init()*")`) matches the P1-T2 message change, which routes the throw
through the shared `UiThread.DispatcherNotInitializedMessage` constant.

## The previously failing test

`UtilitiesCS.Test.Extensions.DictionaryExtensions_Tests.TryAddValuesAsync_UpdatesExistingValue` did
**not** fail on this run. The TRX records zero failures across all 6519 executed tests, so no test
failed and no re-run was required.

That is the outcome the SD18 bisect predicted. The executor's bisect established that
`UtilitiesCS.Test` plus `TaskMaster.Test` returned 5179/5180 with the single line
`_loaded = new ThreadSafeSingleShotGuard();` present in the `catch`, and 5180/5180 with that one
line removed and nothing else changed. Those figures were measured at the superseded base
`b95a5252` and are recorded verbatim as measured rather than restated against the re-anchored
baseline. The reverted tree passing here is consistent with that bisect and confirms the failure was
delivery-attributable rather than the issue #780 flake.
