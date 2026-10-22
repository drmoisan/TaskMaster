# Exception-Type Observation (P1-T7)

Timestamp: 2026-09-30T07-34
Task: P1-T7
ITERATION: 1
Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~HelperClasses.PhysicalFileSystemAdapters_Tests|FullyQualifiedName~HelperClasses.DirectoryInfoWrapper_Tests" "/ResultsDirectory:coverage\test-results\940\p1-t7" "/Logger:trx;LogFileName=p1-t7.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (resolved through vswhere; CMD-VSTEST with FILTER-BOTH and NAMES-BOTH)
EXIT_CODE: 0
Output Summary: branch (a); all fifteen tests of the two rewritten classes passed on the first run, so every predicted exception type was observed as written and no assertion was pinned.

- RUNSETTINGS-HASH-NOW: 98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57 (equals RUNSETTINGS-HASH)
- TRX_PRESENT: True
- SEQUENCE_FILES: 0
- COUNTERS total=15 executed=15 passed=15 failed=0
- RESULT_COUNT: 15
- RESULT EnumerateFileSystemInfos_ShouldWrapDirectoriesAndFiles = Passed
- RESULT PhysicalDirectoryInfoAdapter_AccessorsAndNoOpCreation_MirrorOwnedDirectory = Passed
- RESULT PhysicalDirectoryInfoAdapter_Enumeration_WrapsOwnedDirectoryEntries = Passed
- RESULT PropertyDelegates_ShouldMirrorMockedIDirectoryInfo = Passed
- RESULT GetDirectoriesAndGetFiles_ShouldReturnWrappedEntries = Passed
- RESULT Constructor_WhenDirectoryInfoIsNull_ThrowsArgumentNullException = Passed
- RESULT Properties_ShouldMirrorWrappedDirectoryInfo = Passed
- RESULT EnumerationAndArrayMethods_ShouldDelegateToWrappedIDirectoryInfo = Passed
- RESULT PhysicalFileInfoAdapter_PropertiesStreamsAndAccessors_MirrorFileInfo = Passed
- RESULT PhysicalDirectoryInfoAdapter_MissingDirectoryAndUnsupportedInfo_BranchesBehaveAsExpected = Passed
- RESULT PhysicalFileInfoAdapter_SettersOnMissingFile_ThrowWithoutCreatingFiles = Passed
- RESULT ToString_ShouldDelegateToWrappedDirectoryInfo = Passed
- RESULT PhysicalFileInfoAdapter_MissingFileBranches_ThrowOrNoOpWithoutCreatingFiles = Passed
- RESULT LifecycleAndAccessControlMethods_ShouldDelegateToWrappedIDirectoryInfo = Passed
- RESULT PhysicalDirectoryInfoAdapter_SettersOnMissingDirectory_ThrowWithoutCreatingEntries = Passed
- MESSAGE lines: none (no failed test)
- EXCEPTION-TYPES: AS PREDICTED
- PINNED-COUNT: 0
