# Stall Probe (P0-T9)

Timestamp: 2026-09-30T07-18
Task: P0-T9
Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~HelperClasses.ShellUtilities_Tests|FullyQualifiedName~HelperClasses.ShellUtilitiesStatic_Tests|FullyQualifiedName~HelperClasses.SysImageListHelperTests|FullyQualifiedName~EmailIntelligence.OSBrowser_Tests" "/ResultsDirectory:coverage\test-results\940\p0-t9" "/Logger:trx;LogFileName=p0-t9.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (resolved through vswhere)
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary: single invocation; 23 tests ran in about 4.7 seconds, 22 passed and 1 failed on an invalid Win32 icon handle; no hang (no Sequence document). The failure reproduces the pre-existing local shell-icon defect recorded by #931.
- RUNSETTINGS-HASH-NOW: 98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57 (equals RUNSETTINGS-HASH of P0-T4)
- TRX_PRESENT: True
- SEQUENCE_FILES: 0
- COUNTERS total=23 executed=23 passed=22 failed=1
- RESULT_COUNT: 23
- MESSAGE GetFileIcon_WithUseFileType_ShouldReturnIconsForDirectoryAndFileExtension :: Test method UtilitiesCS.Test.HelperClasses.ShellUtilities_Tests.GetFileIcon_WithUseFileType_ShouldReturnIconsForDirectoryAndFileExtension threw exception: System.ArgumentException: Win32 handle that was passed to Icon is not valid or is the wrong type.

## Derived selections

STALL-PROBE: REPRODUCES
UCS-FILTERARG: "/TestCaseFilter:FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests"
COVERAGE-ROUTE: DIRECT

The four excluded classes (HelperClasses.ShellUtilities_Tests, HelperClasses.ShellUtilitiesStatic_Tests, HelperClasses.SysImageListHelperTests, EmailIntelligence.OSBrowser_Tests) are a pre-existing local stall and failure reproduced on main and are executed by CI (plan fact 18).

Rule applied: `CLEAR` requires EXIT_CODE 0, failed 0 and SEQUENCE_FILES 0; the observed EXIT_CODE 1 with failed 1 yields `REPRODUCES`. The probe was invoked once and not re-run.
