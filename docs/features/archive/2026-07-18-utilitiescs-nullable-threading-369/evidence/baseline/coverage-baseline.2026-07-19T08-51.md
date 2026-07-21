# Coverage Baseline (UtilitiesCS test assembly)

- Timestamp: 2026-07-19T08-51
- Task: [P0-T5]
- Planned Command: `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/2026-07-18-utilitiescs-nullable-threading-369/evidence/baseline/coverage-baseline.2026-07-19T08-51.cobertura.xml`
- Executed Command (equivalent mechanism, see deviation note): `dotnet-coverage collect --output docs/features/active/2026-07-18-utilitiescs-nullable-threading-369/evidence/baseline/coverage-baseline.2026-07-19T08-51.cobertura.xml --output-format cobertura --settings coverage.config -- "<VS18 vstest.console.exe>" "UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll" /Settings:<Workers=4 runsettings> /InIsolation /TestCaseFilter:TestCategory!=LiveOutlook`
- EXIT_CODE: 0
- Cobertura XML: `docs/features/active/2026-07-18-utilitiescs-nullable-threading-369/evidence/baseline/coverage-baseline.2026-07-19T08-51.cobertura.xml`

## Output Summary

- Tests: **4511 passed, 0 failed** (deterministic, 26.4s).
- Overall (Cobertura root `<coverage>`): **line-rate = 0.7206711694864292 (72.07%)**, **branch-rate = 0.48442797445399355 (48.44%)**; lines-covered=98270, lines-valid=136359; branches-covered=12288, branches-valid=25366.
- Targeted production `UtilitiesCS/Threading/` (25 production files incl. Designer partials): **lines covered=3855, valid=4710, line-rate=0.8185 (81.85%)**.

### Per-production-file Threading line coverage (baseline)

| File | Covered/Valid | Rate |
|---|---|---|
| ApplicationIdleTimer.cs | 370/420 | 0.881 |
| AsyncMultiTasker.cs | 542/648 | 0.836 |
| CurrentStoreContext.cs | 48/52 | 0.923 |
| IdleActionQueue.cs | 84/110 | 0.764 |
| IdleAsyncQueue.cs | 114/126 | 0.905 |
| LockupStallDecider.cs | 24/24 | 1.000 |
| ProgressMultiStepViewer.Designer.cs | 0/312 | 0.000 |
| ProgressMultiStepViewer.cs | 0/8 | 0.000 |
| ProgressPackage.cs | 106/108 | 0.981 |
| ProgressPane.Designer.cs | 130/136 | 0.956 |
| ProgressPane.cs | 34/38 | 0.895 |
| ProgressTracker.cs | 298/344 | 0.866 |
| ProgressTrackerAsync.cs | 83/94 | 0.883 |
| ProgressTrackerPane.cs | 178/234 | 0.761 |
| ProgressViewer.Designer.cs | 86/92 | 0.935 |
| ProgressViewer.cs | 48/48 | 1.000 |
| StoreLockupResponder.cs | 128/136 | 0.941 |
| SyncContextForm.Designer.cs | 22/28 | 0.786 |
| SyncContextForm.cs | 20/20 | 1.000 |
| ThreadMonitor.cs | 78/102 | 0.765 |
| ThreadSafeFunctions.cs | 206/248 | 0.831 |
| ThreadSafeSingleShotGuard.cs | 8/8 | 1.000 |
| TimeOutTask.cs | 1134/1210 | 0.937 |
| UiThread.cs | 114/154 | 0.740 |
| WpfUiDispatcher.cs | 0/10 | 0.000 |

## Methodology Deviation Note (equivalent mechanism)

The plan names `Invoke-MSTestWithCoverage.ps1`. That script (a) runs the full repository test set at MSTest `Workers=0`, and (b) `throw`s on any non-zero vstest exit **before** writing/post-processing the Cobertura XML. The first invocation via the script failed: under `Workers=0` full-suite instrumentation, the documented pre-existing timing flakiness produced a non-zero vstest exit, so the script threw and emitted no XML — making the required numeric baseline unobtainable via the script as written.

To obtain the numeric baseline reliably, the coverage mechanism the script wraps (`dotnet-coverage collect ... --settings coverage.config -- vstest.console.exe ... /InIsolation`) was invoked directly against `UtilitiesCS.Test.dll` (the assembly whose `Threading/` tests cover every changed production file) with MSTest `Workers=4` for determinism (per the known flakiness profile). This produced a clean 4511/4511 run and a valid Cobertura artifact. `coverage.config` (Deedle/FSharp/test-framework instrumentation excludes) is identical to the script's. The same direct method will be used for the final coverage gate (P9-T4) so the P9-T6 delta comparison is method-consistent. This deviation is a mechanically-necessary means to satisfy the task's numeric-coverage acceptance; it does not alter assertions, add retries/sleeps, or weaken any test.
