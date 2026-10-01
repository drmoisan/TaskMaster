# P1-T9 scoped run of SortEmail_Tests after the fix

Timestamp: 2026-09-30T12-26
Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~EmailIntelligence.SortEmail_Tests" "/ResultsDirectory:coverage\test-results\945\p1-t9" "/Logger:trx;LogFileName=p1-t9.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (resolved through vswhere)
EXIT_CODE: 0

Output Summary:
RUNSETTINGS-HASH-NOW: 98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57
SANDBOX-EXISTS-BEFORE: False
SANDBOX-EXISTS-AFTER: False
COUNTERS total=15 executed=15 passed=15 failed=0
RESULT_COUNT: 15
SEQUENCE_FILES: 0
RESULT TrySaveAttachmentAsync_WhenSaveSucceeds_ReturnsTrueAndCallsSaveAsFile = Passed
RESULT TrySaveAttachmentAsync_WhenDirectoryCreationThrowsIOException_PropagatesAndDoesNotSave = Passed
MESSAGE lines: none (no failed test)
