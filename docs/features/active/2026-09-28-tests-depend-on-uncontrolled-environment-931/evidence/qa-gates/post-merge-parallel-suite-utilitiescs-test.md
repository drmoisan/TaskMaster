# Post-Merge Step 4b: Parallel-Suite Run, UtilitiesCS.Test Full Assembly

Timestamp: 2026-09-29T19-55
HEAD: 55a50e9226173d39d3c169b0dc90400b430af59b
Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll "/Settings:TaskMaster.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests" "/ResultsDirectory:coverage\test-results\931\postmerge-ucs" "/Logger:trx;LogFileName=postmerge-ucs.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (resolved through vswhere; the plan's CMD-VSTEST block, as used by P4-T6). Runsettings file: TaskMaster.runsettings (repository root; Workers 0, Scope ClassLevel). Isolation switch: /InIsolation. Filter: only the UCS-FILTERARG shell-icon exclusion recorded by P0-T9. Console transcript written to the git-ignored coverage\logs\postmerge-ucs.vstest.log.
EXIT_CODE: 0

Output Summary:
- RUNSETTINGS-HASH-NOW: 199408CA53CE4E12AE1A894FC66A0926124F3AC0D6447BD93B0C121338297FFA (equals the P0-T4 RUNSETTINGS-HASH)
- VSTEST_EXIT_CODE: 0
- TRX_PRESENT: True
- SEQUENCE_FILES: 0
- COLLECTOR_LINES: 0
- Outcome: Completed
- COUNTERS total=4924 executed=4924 passed=4924 failed=0 error=0 timeout=0 aborted=0 notExecuted=0 inconclusive=0
- RESULT_COUNT: 4924 (P4-T6 recorded 4922; the merge changed two UtilitiesCS.Test files, ILGlobals_Tests.cs and UiThreadApartmentMeasurement_Tests.cs, which is the likely source of the two additional tests; not attributed per test)
- FileInfoWrapper_Tests (class UtilitiesCS.Test.HelperClasses.FileInfoWrapper_Tests) contains eight test methods in this run's TRX, and all eight are executed and Passed:
  - RESULT ToString_ShouldDelegateToWrappedFileInfo = Passed
  - RESULT Constructor_WhenFileInfoIsNull_ThrowsArgumentNullException = Passed
  - RESULT OpenRead_ShouldReturnReadableStreamForWrappedFile = Passed
  - RESULT ExplicitDirectoryCast_ShouldReturnWrappedContainingDirectory = Passed
  - RESULT AccessControlAndLifecycleMethods_ShouldDelegateToWrappedIFileInfo = Passed
  - RESULT StreamAndCopyMethods_ShouldDelegateToWrappedIFileInfo = Passed
  - RESULT PropertyDelegates_ShouldMirrorMockedIFileInfo = Passed
  - RESULT Properties_ShouldMirrorWrappedFileInfo = Passed
- Non-passed results: none

Acceptance: zero failed tests; every FileInfoWrapper_Tests name is executed and Passed. All hold. Note: the re-verification request referred to four FileInfoWrapper_Tests names; the class contains eight, matching the eight NAMES-FIW lines in P4-T6, and all eight are listed above.

The TRX document stays under the git-ignored coverage\test-results\931\ tree; this file is the committed projection.
