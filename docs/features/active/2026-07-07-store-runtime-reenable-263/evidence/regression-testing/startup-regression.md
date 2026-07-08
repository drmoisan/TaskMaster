# Startup-Path Regression (P5-T4)

Timestamp: 2026-07-08T01-27

Command: vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook"

EXIT_CODE: 0

Output Summary:
- Total tests 4430, Passed 4430, Failed 0.
- Pre-existing startup-path suites all pass: AppEventsTests, AppOlObjects* (AppOlObjectsCoverageTests, StartupInboxAttributionProbeTests), HookReadinessCoordinatorTests, OutlookFolderNotificationSinkTests, and StoresWrapperTests (both UtilitiesCS.Test and TaskMaster.Test variants).
- The baseline non-instrumented run (P0-T13 cross-check) was 4411 passed / 1 failed (TryAddValuesAsync_UpdatesExistingValue, a ~22s flaky timing test); that test passed in this run. 18 new F3 tests were added (OutlookReadinessGateTests x4, StoresWrapperRehookTests x2, OutlookFolderNotificationSinkTests +3, AppEventsStoreRehookTests x3, StoreRehookCoordinatorTests x6). Net: 4412 -> 4430, all green.
- "Failed loading language 'eng'" lines are Tesseract OCR stderr noise, not test failures.

Tests updated only for the plan-mandated extraction (per P5-T4 "note any test updated only for a renamed extracted method"):
- TaskMaster.Test/OutlookObjects/Store/StoresWrapperTests.cs: three source-structure regression tests (RewireAfterDeserializeAsync_UsesStoreAdapterForWrappedStores, _IncrementsProcessedStoreCountForEachWrappedStore, _YieldsBetweenAdapterWrappedStores) asserted the pre-F3 inline loop structure of RewireOlObjectsAsync. The P3-T1 extraction moved the per-store Find/create/Restore branch into the shared AddOrRestoreStore primitive (required for AC1). The three tests were updated to assert the equivalent structure: the loop yields between stores then delegates to AddOrRestoreStore, and the Find/Restore branch is inspected on the extracted AddOrRestoreStore method. Behavioral intent (ordered iteration, cooperative yield after first store, restore-vs-create branch, one counter increment per store) is preserved. No behavioral test was weakened.

Result: 0 unexplained failures. PASS.
