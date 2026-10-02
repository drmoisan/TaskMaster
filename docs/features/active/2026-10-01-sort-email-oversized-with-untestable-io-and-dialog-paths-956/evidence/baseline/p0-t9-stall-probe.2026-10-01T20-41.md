# P0-T9 Stall probe

Timestamp: 2026-10-01T20-41
Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~HelperClasses.ShellUtilities_Tests|FullyQualifiedName~HelperClasses.ShellUtilitiesStatic_Tests|FullyQualifiedName~HelperClasses.SysImageListHelperTests|FullyQualifiedName~EmailIntelligence.OSBrowser_Tests" "/ResultsDirectory:coverage\test-results\956\p0-t9" "/Logger:trx;LogFileName=p0-t9.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (resolved through vswhere; run once, as a background invocation; console stream teed to coverage\logs\p0-t9.vstest.log)
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary:
RUNSETTINGS-HASH: 98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57 (P0-T4)
RUNSETTINGS-HASH-NOW: 98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57
SANDBOX-956-EXISTS-BEFORE: False
SANDBOX-945-EXISTS-BEFORE: False
VSTEST_EXIT_CODE: 1
SANDBOX-956-EXISTS-AFTER: False
SANDBOX-945-EXISTS-AFTER: False
TRX_PRESENT: True
SEQUENCE_FILES: 0
COUNTERS total=23 executed=23 passed=22 failed=1
MESSAGE GetFileIcon_WithUseFileType_ShouldReturnIconsForDirectoryAndFileExtension :: Test method UtilitiesCS.Test.HelperClasses.ShellUtilities_Tests.GetFileIcon_WithUseFileType_ShouldReturnIconsForDirectoryAndFileExtension threw exception: System.ArgumentException: Win32 handle that was passed to Icon is not valid or is the wrong type.
STALL-PROBE: REPRODUCES
EXCLUSION: &FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests
COVERAGE-ROUTE: DIRECT
Derivation: the rule yields CLEAR only when EXIT_CODE is 0, failed is 0 and SEQUENCE_FILES is 0; EXIT_CODE is 1 and failed is 1, so the probe REPRODUCES a local defect in the four classes (a failure rather than a hang) and the Command Reference exclusion text applies verbatim.
Acceptance: RUNSETTINGS-HASH-NOW equals RUNSETTINGS-HASH; every SANDBOX value is False; the three single-value lines are present with the derived values; the probe ran once (all hold). This task gates nothing on the exit code.
