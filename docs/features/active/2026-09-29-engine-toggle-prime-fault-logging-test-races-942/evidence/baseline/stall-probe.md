# Baseline: shell-icon stall probe (issue 942)

Timestamp: 2026-09-30T07-30
Task: P0-T13
Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~HelperClasses.ShellUtilities_Tests|FullyQualifiedName~HelperClasses.ShellUtilitiesStatic_Tests|FullyQualifiedName~HelperClasses.SysImageListHelperTests|FullyQualifiedName~EmailIntelligence.OSBrowser_Tests" "/ResultsDirectory:coverage\test-results\942\p0-t13" "/Logger:trx;LogFileName=p0-t13.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"
EXIT_CODE: 1
ExpectedExitCode: 1

Output Summary:
- The probe was invoked once and not re-run.
- VSTEST_EXIT_CODE: 1
- TRX_PRESENT: True
- SEQUENCE_FILES: 0
- COUNTERS total=23 executed=23 passed=22 failed=1
- RESULT_COUNT: 23
- FAILED GetFileIcon_WithUseFileType_ShouldReturnIconsForDirectoryAndFileExtension
- MESSAGE GetFileIcon_WithUseFileType_ShouldReturnIconsForDirectoryAndFileExtension :: Test method UtilitiesCS.Test.HelperClasses.ShellUtilities_Tests.GetFileIcon_WithUseFileType_ShouldReturnIconsForDirectoryAndFileExtension threw exception: System.ArgumentException: Win32 handle that was passed to Icon is not valid or is the wrong type.
- ExpectedExitCode is presentational; nothing is gated on it.

STALL-PROBE: REPRODUCES
COVERAGE-ROUTE: DIRECT

The run did not stall (no Sequence document), but it did not satisfy the CLEAR condition (EXIT_CODE 0, failed 0, SEQUENCE_FILES 0) because one shell-icon test failed on this workstation. The four excluded classes are a pre-existing local defect on this workstation (fact 13 of the plan) and are executed by CI; the DIRECT coverage route excludes them from the local coverage run.
