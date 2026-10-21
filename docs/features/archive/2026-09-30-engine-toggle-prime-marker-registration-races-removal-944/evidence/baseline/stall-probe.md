# Stall Probe (P0-T16)

Timestamp: 2026-09-30T13-24
Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~HelperClasses.ShellUtilities_Tests|FullyQualifiedName~HelperClasses.ShellUtilitiesStatic_Tests|FullyQualifiedName~HelperClasses.SysImageListHelperTests|FullyQualifiedName~EmailIntelligence.OSBrowser_Tests" "/ResultsDirectory:coverage\test-results\944\p0-t16" "/Logger:trx;LogFileName=p0-t16.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (CMD-VSTEST, ASSEMBLY-UCS, FILTER-STALL, NAMES-NONE; vstest resolved through vswhere)
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary: VSTEST_EXIT_CODE: 1 after about 3 seconds (13-24-21 to 13-24-24 UTC). TRX_PRESENT: True; SEQUENCE_FILES: 0 (no hang). COUNTERS total=23 executed=23 passed=22 failed=1. One failure in ShellUtilitiesStatic_Tests (invalid Win32 icon handle). STALL-PROBE: REPRODUCES (the CLEAR condition requires exit 0 and failed 0). COVERAGE-ROUTE: DIRECT.

## Observed

- TRX_PRESENT: True
- SEQUENCE_FILES: 0
- COUNTERS total=23 executed=23 passed=22 failed=1
- RESULT_COUNT: 23
- FAILED GetFileIcon_WithUseFileType_ShouldReturnIconsForDirectoryAndFileExtension
- MESSAGE GetFileIcon_WithUseFileType_ShouldReturnIconsForDirectoryAndFileExtension :: Test method UtilitiesCS.Test.HelperClasses.ShellUtilitiesStatic_Tests.GetFileIcon_WithUseFileType_ShouldReturnIconsForDirectoryAndFileExtension threw exception: System.ArgumentException: Win32 handle that was passed to Icon is not valid or is the wrong type.

STALL-PROBE: REPRODUCES

COVERAGE-ROUTE: DIRECT

The four excluded classes (HelperClasses.ShellUtilities_Tests, HelperClasses.ShellUtilitiesStatic_Tests, HelperClasses.SysImageListHelperTests, EmailIntelligence.OSBrowser_Tests) are a pre-existing local shell-icon problem on this workstation and are executed by CI (plan fact 11). On this run the probe did not hang (SEQUENCE_FILES: 0); it failed one shell-icon test, which the plan's rule classifies as REPRODUCES because the CLEAR condition requires EXIT_CODE 0, failed 0 and SEQUENCE_FILES 0. The probe was invoked once and is not re-run.
