# Stall Probe (P0-T15)

Timestamp: 2026-10-01T23-24
Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~HelperClasses.ShellUtilities_Tests|FullyQualifiedName~HelperClasses.ShellUtilitiesStatic_Tests|FullyQualifiedName~HelperClasses.SysImageListHelperTests|FullyQualifiedName~EmailIntelligence.OSBrowser_Tests" "/ResultsDirectory:coverage\test-results\948\p0-t15" "/Logger:trx;LogFileName=p0-t15.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" (resolved through vswhere)
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary: VSTEST_EXIT_CODE 1; TRX_PRESENT True; SEQUENCE_FILES 0; COUNTERS total=23 executed=23 passed=22 failed=1 (the filter selected tests); one shell-icon test failed with a Win32 icon-handle ArgumentException; STALL-PROBE: REPRODUCES; COVERAGE-ROUTE: DIRECT.

Pre-run process check: FOREIGN_CANDIDATES: 0 (Get-CimInstance Win32_Process over vstest, testhost and dotnet-coverage names); STRAY_TEST_PROCESSES: 0.

```
VSTEST_EXIT_CODE: 1
TRX_PRESENT: True
SEQUENCE_FILES: 0
COUNTERS total=23 executed=23 passed=22 failed=1
RESULT_COUNT: 23
FAILED GetFileIcon_WithUseFileType_ShouldReturnIconsForDirectoryAndFileExtension
MESSAGE GetFileIcon_WithUseFileType_ShouldReturnIconsForDirectoryAndFileExtension :: Test method UtilitiesCS.Test.HelperClasses.ShellUtilities_Tests.GetFileIcon_WithUseFileType_ShouldReturnIconsForDirectoryAndFileExtension threw exception:
System.ArgumentException: Win32 handle that was passed to Icon is not valid or is the wrong type.
```

STALL-PROBE: REPRODUCES

COVERAGE-ROUTE: DIRECT

The four excluded classes (UtilitiesCS.Test.HelperClasses.ShellUtilities_Tests, UtilitiesCS.Test.HelperClasses.ShellUtilitiesStatic_Tests, UtilitiesCS.Test.HelperClasses.SysImageListHelperTests and UtilitiesCS.Test.EmailIntelligence.OSBrowser_Tests) are a pre-existing local shell-icon failure or stall on this machine; CI executes them unfiltered (fact 14, D-7). The probe was invoked once and is not re-run.
