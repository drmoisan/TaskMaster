# Baseline: Shell-Icon Stall Probe (P0-T12)

Timestamp: 2026-10-01T17-39
Task: P0-T12
Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~HelperClasses.ShellUtilities_Tests|FullyQualifiedName~HelperClasses.ShellUtilitiesStatic_Tests|FullyQualifiedName~HelperClasses.SysImageListHelperTests|FullyQualifiedName~EmailIntelligence.OSBrowser_Tests" "/ResultsDirectory:coverage\test-results\947\p0-t12" "/Logger:trx;LogFileName=p0-t12.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"
EXIT_CODE: 1
ExpectedExitCode: 1

Output Summary:
- VSTEST_EXIT_CODE: 1
- TRX_PRESENT: True
- SEQUENCE_FILES: 0
- COUNTERS total=23 executed=23 passed=22 failed=1
- RESULT_COUNT: 23
- FAILED GetFileIcon_WithUseFileType_ShouldReturnIconsForDirectoryAndFileExtension
- MESSAGE GetFileIcon_WithUseFileType_ShouldReturnIconsForDirectoryAndFileExtension :: Test method UtilitiesCS.Test.HelperClasses.ShellUtilitiesStatic_Tests.GetFileIcon_WithUseFileType_ShouldReturnIconsForDirectoryAndFileExtension threw exception: / System.ArgumentException: Win32 handle that was passed to Icon is not valid or is the wrong type.
- STALL-PROBE: REPRODUCES
- COVERAGE-ROUTE: DIRECT
- The four shell-icon test classes are a pre-existing local stall/failure on this workstation that CI executes (plan fact 9); this change does not touch them. The probe ran once and is not re-run.

Notes:
- The MESSAGE line carries no absolute path, so no REDACTED-PATH substitution was needed.
- The trx stays under the git-ignored coverage directory and is not copied into the feature folder.
