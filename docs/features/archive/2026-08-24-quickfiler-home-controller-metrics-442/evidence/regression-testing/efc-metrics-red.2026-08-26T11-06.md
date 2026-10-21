# Phase 1 — EFC Metrics Regression Tests, Red State (#451)

Timestamp: 2026-08-26T11-06
Task: [P1-T10] `[expect-fail]`
Command: `pwsh -NoProfile -Command '$vsw = "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe"; $mb = & $vsw -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; & $mb "TaskMaster.sln" /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"; $vt = & $vsw -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; & $vt "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/TestCaseFilter:FullyQualifiedName~EfcHomeControllerMetricsTests" /Logger:trx "/ResultsDirectory:TestResults\p1-t10"; Write-Host "EXIT_CODE=$LASTEXITCODE"'`
EXIT_CODE: 1
ExpectedExitCode: 1

TRX file: `TestResults\p1-t10\<account>_<HOST>_2026-08-26_11_06_53_net481.trx` (account and host
components redacted; vstest names TRX files `<account>_<HOST>_<timestamp>.trx` by default).

This is a build-for-test step and deliberately uses `/t:Build`. It is not the analyzer gate; the
analyzer gate uses `/t:Rebuild` and runs in Phase 6 as [P6-T3].

## Output Summary

```
Test Run Failed.
Total tests: 13
     Passed: 5
     Failed: 8
 Total time: 2.1017 Seconds
```

Eight tests fail, matching exactly the eight the task requires: those from [P1-T1], [P1-T2],
[P1-T3], [P1-T4], [P1-T6], [P1-T7], [P1-T8], and [P1-T9].
`BuildQuickFileMetricLines_WithNinetySeconds_RendersUntruncatedDuration` from [P1-T5] passes, as
the plan states it must.

### Failing tests with verbatim failure messages

**1. `BuildQuickFileMetricLines_WithMovedMailItems_FormatsMetricLine` ([P1-T1], root cause RC-7)**

> Expected result to be equal to {"07/04/2026,01:05,Quarterly Update,SingleSorted,120,2.00,Recipient,Sender,Email,Archive/Target,06/30/2026,09:45:10"}, but {"07/04/2026,01:05,Quarterly Update,SingleSorted,120,2.00,RecipientSender,Email,Archive/Target,06/30/2026,09:45:10"} differs at index 0.

**2. `BuildQuickFileMetricLines_RendersTwelveCommaSeparatedFields` ([P1-T2], root cause RC-7)**

> Expected result[0].Split(',') to contain 12 item(s) because the session-metrics row carries exactly twelve fields, but found 11: {"07/04/2026", "01:05", "Quarterly Update", "SingleSorted", "120", "2.00", "RecipientSender", "Email", "Archive/Target", "06/30/2026", "09:45:10"}.

**3. `BuildQuickFileMetricLines_WithEmbeddedCommas_StillRendersTwelveFields` ([P1-T3], root cause RC-7)**

> Expected result[0].Split(',') to contain 12 item(s) because commas embedded in free-text fields must be sanitized, not add fields, but found 14: {"07/04/2026", "01:05", "Quarterly Update", "SingleSorted", "120", "2.00", "Doe", " JaneRoe", " Richard", "Email", "Archive", " Target", "06/30/2026", "09:45:10"}.

**4. `BuildQuickFileMetricLines_WithMultipleMovedItems_PinsRealDivisionRounding` ([P1-T4], root cause RC-3)**

> Expected result to contain only items matching line.Contains(",3,0.04,") because 8 seconds over 3 items is 2.6667, which rounds to 3 seconds and 0.04 minutes, but {"07/04/2026,01:05,Quarterly Update,SingleSorted,2,0.03,RecipientSender,Email,Archive/Target,06/30/2026,09:45:10", ...} do(es) not match.

**5. `BuildQuickFileMetricLines_UnderGermanCulture_RendersInvariantDecimalSeparator` ([P1-T6], root cause RC-9)**

> Expected result[0] "07.04.2026,01:05,Quarterly Update,SingleSorted,120,2,00,RecipientSender,Email,Archive/Target,06.30.2026,09:45:10" to contain ",2.00," because numeric fields must use the invariant decimal separator.

**6. `StopWatch_AfterControllerConstruction_IsRunning` ([P1-T7], root cause RC-5)**

> Expected controller.StopWatch.IsRunning to be True because a stopwatch that is never started measures nothing, but found False.

**7. `QuickFileMetricsWriteFilenameOnly_WithAbsentPrerequisites_DoesNotThrow` ([P1-T8], root cause RC-8)**

> Did not expect any exception because absent prerequisites must produce a silent no-op, not an exception, but found System.NotImplementedException: The method or operation is not implemented.
> at QuickFiler.EfcHomeController.QuickFileMetrics_WRITE(String filename) in QuickFiler\Controllers\EfcHomeController.Metrics.cs:line 28

**8. `QuickFileMetricsWriteFilenameOnly_WithPrerequisites_DelegatesToThreeArgumentOverload` ([P1-T9], root cause RC-8)**

> Test method QuickFiler.Controllers.Tests.EfcHomeControllerMetricsTests.QuickFileMetricsWriteFilenameOnly_WithPrerequisites_DelegatesToThreeArgumentOverload threw exception:
> System.NotImplementedException: The method or operation is not implemented.
> at QuickFiler.EfcHomeController.QuickFileMetrics_WRITE(String filename) in QuickFiler\Controllers\EfcHomeController.Metrics.cs:line 28

### Passing tests in this run

- `BuildQuickFileMetricLines_WithNinetySeconds_RendersUntruncatedDuration` ([P1-T5], the
  deliberate pin that must pass on the pre-fix source)
- `BuildQuickFileMetricLines_WithNullOrEmptyMovedItems_ReturnsNoLines` (pre-existing)
- `QuickFileMetricsWrite_WithMyDocumentsFolder_InvokesInjectedWriter` (pre-existing)
- `QuickFileMetricsWrite_WithoutMyDocumentsFolder_DoesNotInvokeWriter` (pre-existing)
- `QuickFileMetricsWrite_WithNoMovedItems_DoesNotInvokeWriter` (pre-existing)

### Note on the [P1-T7] construction site

`StopWatch_AfterControllerConstruction_IsRunning` reaches the **constructor** site at
`QuickFiler/Controllers/EfcHomeController.cs:76`, which is the plan's preferred target, not the
`:225` fallback. Its failure message reports `IsRunning` as `False` rather than a null-reference
or a null-stopwatch failure, which proves the mail-bearing branch of the constructor was entered
and the stopwatch was allocated. The headless fixture that made this reachable replaces every
collaborator the branch creates, so no window handle is created and no Outlook process is
required. The disposition is recorded in full by [P7-T2].
