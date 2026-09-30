# Baseline Scoped Test Run of the Two Classes (P0-T10)

Timestamp: 2026-09-30T07-18
Task: P0-T10
Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~HelperClasses.PhysicalFileSystemAdapters_Tests|FullyQualifiedName~HelperClasses.DirectoryInfoWrapper_Tests" "/ResultsDirectory:coverage\test-results\940\p0-t10" "/Logger:trx;LogFileName=p0-t10.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (resolved through vswhere; runsettings scripts\vscode\TaskMaster.cli.runsettings; isolation switch /InIsolation)
EXIT_CODE: 0
Output Summary: pre-edit population of 12 tests ran and all passed; no hang; the TRX stays under the git-ignored coverage tree.
- RUNSETTINGS-HASH-NOW: 98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57 (equals RUNSETTINGS-HASH)
- TRX_PRESENT: True
- COUNTERS total=12 executed=12 passed=12 failed=0
- RESULT_COUNT: 12
- SEQUENCE_FILES: 0
- RESULT ToString_ShouldDelegateToWrappedDirectoryInfo = Passed
- RESULT PhysicalDirectoryInfoAdapter_PropertiesEnumerationAndAccessors_MirrorDirectoryInfo = Passed
- RESULT EnumerateFileSystemInfos_ShouldWrapDirectoriesAndFiles = Passed
- RESULT GetDirectoriesAndGetFiles_ShouldReturnWrappedEntries = Passed
- RESULT Properties_ShouldMirrorWrappedDirectoryInfo = Passed
- RESULT Constructor_WhenDirectoryInfoIsNull_ThrowsArgumentNullException = Passed
- RESULT EnumerationAndArrayMethods_ShouldDelegateToWrappedIDirectoryInfo = Passed
- RESULT PhysicalDirectoryInfoAdapter_MissingDirectoryAndUnsupportedInfo_BranchesBehaveAsExpected = Passed
- RESULT PropertyDelegates_ShouldMirrorMockedIDirectoryInfo = Passed
- RESULT PhysicalFileInfoAdapter_MissingFileBranches_ThrowOrNoOpWithoutCreatingFiles = Passed
- RESULT PhysicalFileInfoAdapter_PropertiesStreamsAndAccessors_MirrorFileInfo = Passed
- RESULT LifecycleAndAccessControlMethods_ShouldDelegateToWrappedIDirectoryInfo = Passed
- MESSAGE lines: none (no failed test)
- BASELINE-FAILED-TWO-CLASSES: NONE
