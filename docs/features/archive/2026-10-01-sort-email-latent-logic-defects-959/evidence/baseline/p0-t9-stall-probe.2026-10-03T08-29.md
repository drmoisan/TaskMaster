# P0-T9 Shell-Icon Stall Probe

Timestamp: 2026-10-03T08-29
Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~HelperClasses.ShellUtilities_Tests|FullyQualifiedName~HelperClasses.ShellUtilitiesStatic_Tests|FullyQualifiedName~HelperClasses.SysImageListHelperTests|FullyQualifiedName~EmailIntelligence.OSBrowser_Tests" "/ResultsDirectory:coverage\test-results\959\p0-t9" "/Logger:trx;LogFileName=p0-t9.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (resolved through vswhere; CMD-VSTEST, run once)
EXIT_CODE: 1 (the printed VSTEST_EXIT_CODE)
ExpectedExitCode: 1
Output Summary: the probe ran once in about 2 seconds without a hang (no sequence file); 23 results, 22 passed, 1 failed (a shell-icon test raised ArgumentException for an invalid Win32 icon handle), so the shell-icon classes are excluded from the coverage runs on this workstation.

- RUNSETTINGS-HASH-NOW: 98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57 (equals RUNSETTINGS-HASH of P0-T4)
- SANDBOX-959-EXISTS-BEFORE: False
- SANDBOX-956-EXISTS-BEFORE: False
- SANDBOX-945-EXISTS-BEFORE: False
- SANDBOX-959-EXISTS-AFTER: False
- SANDBOX-956-EXISTS-AFTER: False
- SANDBOX-945-EXISTS-AFTER: False
- TRX_PRESENT: True
- SEQUENCE_FILES: 0
- COUNTERS total=23 executed=23 passed=22 failed=1
- MESSAGE GetFileIcon_WithUseFileType_ShouldReturnIconsForDirectoryAndFileExtension :: Test method UtilitiesCS.Test.HelperClasses.ShellUtilities_Tests.GetFileIcon_WithUseFileType_ShouldReturnIconsForDirectoryAndFileExtension threw exception: System.ArgumentException: Win32 handle that was passed to Icon is not valid or is the wrong type.

STALL-PROBE: REPRODUCES
EXCLUSION: &FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests
COVERAGE-ROUTE: DIRECT

Acceptance check: RUNSETTINGS-HASH-NOW equals RUNSETTINGS-HASH; every SANDBOX value False; the three single-value lines present with the derived values (EXIT_CODE non-zero and failed=1, so REPRODUCES with the Command Reference exclusion text); the probe ran once. All four hold.
