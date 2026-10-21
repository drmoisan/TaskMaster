# P0-T9 Stall Probe (four UtilitiesCS.Test shell-icon classes)

Timestamp: 2026-09-29T09-00
Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll "/Settings:TaskMaster.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~HelperClasses.ShellUtilities_Tests|FullyQualifiedName~HelperClasses.ShellUtilitiesStatic_Tests|FullyQualifiedName~HelperClasses.SysImageListHelperTests|FullyQualifiedName~EmailIntelligence.OSBrowser_Tests" "/ResultsDirectory:coverage\test-results\931\p0-t9" "/Logger:trx;LogFileName=p0-t9.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (resolved through vswhere; CMD-VSTEST with ASSEMBLY-UCS, FILTER-STALL, NAMES-NONE, TASKID p0-t9; console transcript teed to the git-ignored coverage\logs\p0-t9.vstest.log)
EXIT_CODE: 1
ExpectedExitCode: 1

Output Summary:
- RUNSETTINGS-HASH-NOW: 199408CA53CE4E12AE1A894FC66A0926124F3AC0D6447BD93B0C121338297FFA (equals RUNSETTINGS-HASH from P0-T4)
- VSTEST_EXIT_CODE: 1
- TRX_PRESENT: True
- SEQUENCE_FILES: 0
- COLLECTOR_LINES: 0
- COUNTERS total=23 executed=23 passed=22 failed=1
- RESULT_COUNT: 23
- MESSAGE GetFileIcon_WithUseFileType_ShouldReturnIconsForDirectoryAndFileExtension :: Test method UtilitiesCS.Test.HelperClasses.ShellUtilities_Tests.GetFileIcon_WithUseFileType_ShouldReturnIconsForDirectoryAndFileExtension threw exception: System.ArgumentException: Win32 handle that was passed to Icon is not valid or is the wrong type.
- Invocations: one (the probe was not re-run).

STALL-PROBE: REPRODUCES
UCS-FILTERARG: "/TestCaseFilter:FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests"
COVERAGE-ROUTE: DIRECT

Derivation: the rule records CLEAR only when EXIT_CODE is 0, failed is 0 and SEQUENCE_FILES is 0; this run exited 1 with failed=1, so the value is REPRODUCES. On this run the four classes completed without a stall, and one shell-icon test failed on an invalid Win32 icon handle; either outcome is the environment-dependent shell-icon behaviour the rule excludes. The four excluded classes (HelperClasses.ShellUtilities_Tests, HelperClasses.ShellUtilitiesStatic_Tests, HelperClasses.SysImageListHelperTests, EmailIntelligence.OSBrowser_Tests) are a pre-existing local stall reproduced on main and are executed by CI (fact 16).
