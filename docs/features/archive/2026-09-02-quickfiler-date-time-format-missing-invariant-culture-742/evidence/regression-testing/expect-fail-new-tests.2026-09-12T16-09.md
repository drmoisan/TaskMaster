# Expect-Fail Run of the New Regression Tests (issue #742, [P1-T4])

Timestamp: 2026-09-14T02-14

Command: `pwsh -NoProfile -Command '$vswherePath = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $vstestPath = (& $vswherePath -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe") | Select-Object -First 1; & $vstestPath "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /TestCaseFilter:"FullyQualifiedName~QuickFilerInvariantCultureIssue742Tests" /InIsolation /Logger:"console;verbosity=normal"; Write-Output "EXITCODE=$LASTEXITCODE"'`

EXIT_CODE: 1

ExpectedExitCode: 1

Output Summary: `Test Run Failed. Total tests: 5, Failed: 5, Total time: 1.4974 Seconds.` VSTest
version 18.10.0 (x64). All five new tests failed against the unfixed production tree, which is the
expected outcome for this `[expect-fail]` task.

Acceptance: `EXITCODE` is non-zero — satisfied.

## Per-test failure, each attributable to the defect under test

Every failure is a separator mismatch under the sentinel-separator culture
(`DateSeparator` = `#`, `TimeSeparator` = `~`), not a fixture or wiring error. The observed strings
below are quoted from the run's own failure messages.

1. `QuickFileMetricsWrite_UnderSentinelSeparatorCulture_RendersInvariantDataLineBeginning` —
   the captured `dataLineBeg` was `01#15#2024,09~30,` where `01/15/2024,09:30,` was expected.
   Source: the interpolated specifier in `QfcHomeController.Metrics.cs`.
2. `BuildQuickFileMetricLines_UnderSentinelSeparatorCulture_RendersInvariantDateAndTime` —
   the produced line was
   `01#15#2024,14~30,Subject text,SingleSorted,60,1.00,Recipient name,Sender name,Email,Destination folder,03#04#2024,14~30~45`,
   which contains no `/`. Both the leading date/time fields and the trailing sent-date fields are
   affected. Source: `EfcHomeController.Metrics.cs`.
3. `GetItemSummary_UnderSentinelSeparatorCulture_RendersInvariantDateAndTime` —
   the summary was `Subject: Subject text sent on 03#04#2024 at 14~30 by Sender name`, which
   contains no `/`. Source: `QfcItemController.ViewerSetup.cs`.
4. `QfcCollectionControllerRenderingSites_UnderSentinelSeparatorCulture_RenderInvariantDateAndTime` —
   the move-readiness notification was
   `Can't complete actions! Not all emails assigned to folder` / `1  03#04#2024  Subject text`,
   which contains no `/`. Source: `QfcCollectionController.cs`. The test fails at the first of its
   three assertions; the expansion-guard and move-diagnostics sites are asserted in the same test
   and are confirmed green in [P4-T4].
5. `EfcItemControllerSentDateAndSentTime_UnderSentinelSeparatorCulture_RenderInvariantSeparators` —
   `SentDate` was `03#04#2024`, which contains no `/`. Source: `EfcItemController.cs`.

## Notes

- The run used `/InIsolation` and was scoped by `/TestCaseFilter` to the new test class only, so it
  did not execute the `UtilitiesCS.Test` shell-icon classes known to stall vstest on this machine.
- The FluentAssertions licence banner printed on standard output is emitted by the assertion library
  on every run in this repository and is unrelated to this change.
- No raw `.trx` and no Cobertura XML was produced or committed by this task; the console logger was
  used and the summary transcribed above.
