# Parallel-Suite Run (P4-T6): UtilitiesCS.Test, full assembly, after the fix

Timestamp: 2026-09-29T09-37
Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll "/Settings:TaskMaster.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests" "/ResultsDirectory:coverage\test-results\931\p4-t6" "/Logger:trx;LogFileName=p4-t6.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (resolved through vswhere; CMD-VSTEST with ASSEMBLY-UCS, UCS-FILTERARG from P0-T9, NAMES-FIW, TASKID p4-t6). Runsettings file: TaskMaster.runsettings (repository root; Workers 0, Scope ClassLevel). Isolation switch: /InIsolation. Filter: the UCS-FILTERARG recorded by P0-T9 (STALL-PROBE: REPRODUCES), shown verbatim above. Excluded classes: HelperClasses.ShellUtilities_Tests, HelperClasses.ShellUtilitiesStatic_Tests, HelperClasses.SysImageListHelperTests, EmailIntelligence.OSBrowser_Tests. The stall is pre-existing on main and these classes are covered by CI. Console transcript teed to the git-ignored coverage\logs\p4-t6.vstest.log.
EXIT_CODE: 0
ITERATION: 1

Output Summary:
- RUNSETTINGS-HASH-NOW: 199408CA53CE4E12AE1A894FC66A0926124F3AC0D6447BD93B0C121338297FFA (equals RUNSETTINGS-HASH from P0-T4)
- VSTEST_EXIT_CODE: 0
- TRX_PRESENT: True
- COUNTERS total=4922 executed=4922 passed=4922 failed=0
- RESULT_COUNT: 4922
- SEQUENCE_FILES: 0
- COLLECTOR_LINES: 0
- RESULT ToString_ShouldDelegateToWrappedFileInfo = Passed
- RESULT Constructor_WhenFileInfoIsNull_ThrowsArgumentNullException = Passed
- RESULT OpenRead_ShouldReturnReadableStreamForWrappedFile = Passed
- RESULT ExplicitDirectoryCast_ShouldReturnWrappedContainingDirectory = Passed
- RESULT AccessControlAndLifecycleMethods_ShouldDelegateToWrappedIFileInfo = Passed
- RESULT StreamAndCopyMethods_ShouldDelegateToWrappedIFileInfo = Passed
- RESULT PropertyDelegates_ShouldMirrorMockedIFileInfo = Passed
- RESULT Properties_ShouldMirrorWrappedFileInfo = Passed
- MESSAGE lines: none (DictionaryExtensions_Tests.TryAddValuesAsync_UpdatesExistingValue passed in this run; no test was re-run)
- NEWLY-FAILING: NONE (BASELINE-FAILED-UCS: NONE; this run's failed set is empty)

Acceptance: EXIT_CODE 0; failed 0 and executed at least 1; SEQUENCE_FILES 0; the eight NAMES-FIW RESULT lines present and each Passed; RUNSETTINGS-HASH-NOW equals RUNSETTINGS-HASH; NEWLY-FAILING NONE. All six hold.

The TRX document stays under the git-ignored coverage\test-results\931\ tree; this file is the committed projection.
