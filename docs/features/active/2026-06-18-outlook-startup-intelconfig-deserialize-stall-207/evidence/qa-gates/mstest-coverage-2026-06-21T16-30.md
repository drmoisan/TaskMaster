# Final QC — MSTest with Coverage, LiveOutlook excluded (P6-T4)

Timestamp: 2026-06-22T15-25

Command:
```
vstest.console.exe TaskMaster.Test/bin/Debug/TaskMaster.Test.dll /InIsolation /EnableCodeCoverage /Settings:TaskMaster.runsettings /TestCaseFilter:"TestCategory!=LiveOutlook"
dotnet-coverage merge <.coverage> -f cobertura -o evidence/qa-gates/trx/postchange-2026-06-21T16-30.cobertura.xml
```
(vstest from VS18 Community TestPlatform; `/InIsolation` required for the Moq test assembly; `TaskMaster.runsettings` supplies the Code Coverage DataCollector module excludes for Moq/FluentAssertions/MSTest/FSharp/Deedle/Castle.)

EXIT_CODE: 0

Output Summary (numeric):
- Total tests: 117. Passed: 117. Failed: 0. Total time: 3.63 s. The run completed with NO hang under the pump-less MSTest host (the pump-independent `NonBlockingDelay` is what allows the `AppEvents` tests to terminate).
- `LoadAsync_WhenEventsHooked_EmitsStartupHookLifecycleLogs` (the formerly-failing test updated in P2-T7 to the deferred-hookup contract): PASSED [52 ms].
- LiveOutlook category EXCLUDED via `/TestCaseFilter:"TestCategory!=LiveOutlook"`: zero LiveOutlook tests executed (the single `LiveOutlookHookupIntegrationTests` method carries `[TestCategory("LiveOutlook")]` and is filtered out).
- Test count delta vs Phase-0 baseline: baseline 111 → post-change 117 (+6: the two `HookReadinessCoordinator`/`NonBlockingDelay` test classes added by Phase 1; the `RemindersProbeScheduleTests` removed by Phase 4; the `AppEventsTests` lifecycle test updated, not added).
- Aggregate repo-wide line coverage (all instrumented modules): 13.04% (lines-covered=8513 / lines-valid=65308), line-rate=0.13035. Baseline aggregate was 12.89% (8383 / 65052). Coverage increased; no repo-wide regression.
- New pure seam coverage (production types, NOT COM/VSTO-exempt):
  - `HookReadinessCoordinator`: 100.00% (covered=44 / total=44 lines).
  - `NonBlockingDelay`: 100.00% (covered=34 / total=34 lines).
  Both exceed the ≥ 90% new-code obligation.
- `OutlookReadinessGate`: 20.00% (covered=8 / total=40). COM-bound (`Application.Session.DefaultStore.GetDefaultFolder` probe), documented COM/VSTO coverage-exempt by inspection (cite P2-T5 exemption dossier `evidence/regression-testing/hook-readiness-com-exemption-2026-06-21T16-30.md`); excluded from the testable denominator.

Cobertura artifact: docs/features/active/2026-06-18-outlook-startup-intelconfig-deserialize-stall-207/evidence/qa-gates/trx/postchange-2026-06-21T16-30.cobertura.xml
