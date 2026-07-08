# QA Gate — Final Tests with Coverage (P5-T4)

Timestamp: 2026-06-28T20-25
Command: vstest.console.exe QuickFiler.Test/bin/Debug/QuickFiler.Test.dll /Settings:TaskMaster.runsettings /InIsolation /EnableCodeCoverage
(Coverage extracted via dotnet-coverage merge <.coverage> --output cobertura)
EXIT_CODE: 0

Output Summary:
- Test Run Successful. Total tests: 186, Passed: 186, Failed: 0 (181 baseline + 5 new seam tests).
- New tests' pass count: 5/5 (WriteMetricsAsync_UsesInjectedClock_ForDateAndTimeStamps, QuickFileMetrics_WRITE_UsesInjectedClock_ForDataLine, NonBlockingProducer_DelaySeam_HonorsInjectedTwentyMillisecondDelay, ToggleOfflineMode_WhenOnline_AwaitsInjectedFiveMillisecondDelay, WaitForQueue_WhenWorkerBusyAndQueueShort_AwaitsInjectedTwoHundredMsDelay).

Post-change numeric coverage (this QuickFiler.Test-only run):
- Single-run overall cobertura line-rate: 0.1172 (11.72%); baseline 0.1145. NOTE: single-assembly run, not a true repo-wide figure (denominator includes incidentally-loaded modules).
- QuickFiler assembly (package) line-rate: 0.31666 (31.67%); baseline 0.30952.
- QfcHomeController.cs class: 0.9018 (90.18%); unchanged.
- QfcHomeController.Metrics.cs class: 0.6944 (69.44%); baseline 0.5493 (+14.5 points from new timestamp tests).
- QfcDatamodel: absent from coverage (class-level [ExcludeFromCodeCoverage]); its delay-site tests are correctness-only.

Per-changed-line hit counts (production sites):
- QfcHomeController.Metrics.cs line 17 (seam property): 2 — COVERED
- QfcHomeController.Metrics.cs line 27 (QuickFileMetrics_WRITE now): 2 — COVERED
- QfcHomeController.Metrics.cs line 107 (WriteMetricsAsync now): 2 — COVERED
- QfcHomeController.Metrics.cs line 108 (curDateText MM/dd/yyyy): 2 — COVERED
- QfcHomeController.Metrics.cs line 110 (curTimeText hh:mm): 2 — COVERED
- QfcHomeController.Metrics.cs line 122 (OlEndTime = now): 2 — COVERED
- QfcHomeController.Metrics.cs line 222 (NonBlockingProducer 20 ms delay): 0 — NOT COVERED (defensive/unreachable branch; dossier in regression-testing)
- QfcHomeController.cs line 54 (LaunchAsync TimeProvider assignment): 0 — NOT COVERED (COM-bound lifecycle; dossier in regression-testing)
- QfcHomeController.cs line 77 (LaunchAsync catch timestamp): 0 — NOT COVERED (COM-bound lifecycle; dossier in regression-testing)
