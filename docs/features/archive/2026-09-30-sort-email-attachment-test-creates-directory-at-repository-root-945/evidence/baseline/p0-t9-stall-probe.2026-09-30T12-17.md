# P0-T9 stall probe

Timestamp: 2026-09-30T12-17
Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~HelperClasses.ShellUtilities_Tests|FullyQualifiedName~HelperClasses.ShellUtilitiesStatic_Tests|FullyQualifiedName~HelperClasses.SysImageListHelperTests|FullyQualifiedName~EmailIntelligence.OSBrowser_Tests" "/ResultsDirectory:coverage\test-results\945\p0-t9" "/Logger:trx;LogFileName=p0-t9.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (resolved through vswhere)
EXIT_CODE: 1
ExpectedExitCode: 1

Output Summary:
RUNSETTINGS-HASH-NOW: 98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57
SANDBOX-EXISTS-BEFORE: False
SANDBOX-EXISTS-AFTER: False
TRX_PRESENT: True
SEQUENCE_FILES: 0
COUNTERS total=23 executed=23 passed=22 failed=1
RESULT_COUNT: 23
MESSAGE GetFileIcon_WithUseFileType_ShouldReturnIconsForDirectoryAndFileExtension :: Test method UtilitiesCS.Test.HelperClasses.ShellUtilitiesStatic_Tests.GetFileIcon_WithUseFileType_ShouldReturnIconsForDirectoryAndFileExtension threw exception: System.ArgumentException: Win32 handle that was passed to Icon is not valid or is the wrong type.
Observation: the run completed in about 2 seconds with no hang and no Sequence file; the exit code is 1 because one shell-icon test failed.

STALL-PROBE: REPRODUCES
EXCLUSION: &FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests
COVERAGE-ROUTE: DIRECT

The RUNNER route cannot discover assemblies from a `.claude/worktrees` worktree (issue #752, plan fact 5), so the route is DIRECT. The four excluded classes are a pre-existing local failure or stall on this workstation reproduced on main and are executed by CI.
