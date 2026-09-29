# Test Run Baseline (P0-T10): QuickFiler.Test and UtilitiesCS.Test, parallel regime, before any edit

Timestamp: 2026-09-29T09-02
Command:
- QuickFiler.Test: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll "/Settings:TaskMaster.runsettings" /InIsolation "/ResultsDirectory:coverage\test-results\931\p0-t10-qf" "/Logger:trx;LogFileName=p0-t10-qf.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (resolved through vswhere; test-case filter: NONE)
- UtilitiesCS.Test: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll "/Settings:TaskMaster.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests" "/ResultsDirectory:coverage\test-results\931\p0-t10-ucs" "/Logger:trx;LogFileName=p0-t10-ucs.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (resolved through vswhere)
- Runsettings file: TaskMaster.runsettings (repository root; Workers 0, Scope ClassLevel). Isolation switch: /InIsolation.
- UtilitiesCS.Test filter source: UCS-FILTERARG from P0-T9 (STALL-PROBE: REPRODUCES). Excluded classes: HelperClasses.ShellUtilities_Tests, HelperClasses.ShellUtilitiesStatic_Tests, HelperClasses.SysImageListHelperTests, EmailIntelligence.OSBrowser_Tests (pre-existing local shell-icon stall reproduced on main; executed by CI).
EXIT_CODE: 0
UCS-VSTEST-EXIT: 0

Output Summary:

QuickFiler.Test (TASKID p0-t10-qf):
- RUNSETTINGS-HASH-NOW: 199408CA53CE4E12AE1A894FC66A0926124F3AC0D6447BD93B0C121338297FFA
- COUNTERS total=1468 executed=1468 passed=1468 failed=0
- RESULT_COUNT: 1468
- SEQUENCE_FILES: 0
- COLLECTOR_LINES: 0
- TRX_PRESENT: True
- RESULT ConfigureBreadcrumbDropDown_OwningThreadInsideDispatcherOperation_DoesNotThrow = Passed
- RESULT InitializeBreadcrumbPipeline_ConstructedInsideDispatcherOperation_SucceedsUnderDifferentAmbientContext = Passed
- RESULT InitializeBreadcrumbPipeline_OwningThreadDifferentPlainContext_DoesNotThrow = Passed
- RESULT InitializeBreadcrumbPipeline_NullOwningDispatcher_DoesNotThrow = Passed
- RESULT Dispatcher_OwnerOnlyWorker_ReportsWithoutRunningAction = Passed
- RESULT InitializeBreadcrumbPipeline_WorkerThread_ThrowsBoundaryDiagnostic = Passed
- RESULT ConfigureBreadcrumbDropDown_WorkerThread_ThrowsBoundaryDiagnostic = Passed
- RESULT InitializeBreadcrumbPipeline_OwningThreadNullAmbientContext_DoesNotThrow = Passed
- MESSAGE lines: none
- BASELINE-FAILED-QF: NONE

UtilitiesCS.Test (TASKID p0-t10-ucs):
- RUNSETTINGS-HASH-NOW: 199408CA53CE4E12AE1A894FC66A0926124F3AC0D6447BD93B0C121338297FFA
- COUNTERS total=4922 executed=4922 passed=4922 failed=0
- RESULT_COUNT: 4922
- SEQUENCE_FILES: 0
- COLLECTOR_LINES: 0
- TRX_PRESENT: True
- RESULT StreamAndCopyMethods_ShouldDelegateToWrappedIFileInfo = Passed
- RESULT ExplicitDirectoryCast_ShouldReturnWrappedContainingDirectory = Passed
- RESULT AccessControlAndLifecycleMethods_ShouldDelegateToWrappedIFileInfo = Passed
- RESULT ToString_ShouldDelegateToWrappedFileInfo = Passed
- RESULT PropertyDelegates_ShouldMirrorMockedIFileInfo = Passed
- RESULT OpenRead_ShouldReturnReadableStreamForWrappedFile = Passed
- RESULT Constructor_WhenFileInfoIsNull_ThrowsArgumentNullException = Passed
- RESULT Properties_ShouldMirrorWrappedFileInfo = Passed
- MESSAGE lines: none
- BASELINE-FAILED-UCS: NONE

The TRX documents stay under the git-ignored coverage\test-results\931\ tree; this file is the committed projection.
