# Stall probe: UtilitiesCS.Test shell-icon classes (P0-T15)

Timestamp: 2026-10-02T00-53
Command: CMD-VSTEST with ASSEMBLY-UCS (UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll), FILTER-STALL, TASKID p0-t15 and empty NAMES, executed as one pwsh -NoProfile -Command payload: PREFIX, TOOLS, then the CMD-VSTEST body, run in the background and polled. Deviations from the body, recorded for audit: the per-result `RESULT` loop was omitted (P0-T15's acceptance reads only COUNTERS and MESSAGE lines), CLOCK, CLOCK-END and PROBE-END echo lines were added, and PROBE-END is also echoed before the exit 3 branch. The probe ran once and is not re-run.
Canonical command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FILTER-STALL" "/ResultsDirectory:coverage\test-results\950\p0-t15" "/Logger:trx;LogFileName=p0-t15.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"
EXIT_CODE: 1
ExpectedExitCode: 1 (presentational: the observed value, recorded per the P0-T15 acceptance; the probe outcome is not gated)

Output Summary:
WORKTREE-LEAF: agent-a7805823735145ca4
VSTEST_EXIT_CODE: 1
TRX_PRESENT: True
SEQUENCE_FILES: 0
COUNTERS total=23 executed=23 passed=22 failed=1
RESULT_COUNT: 23
MESSAGE GetFileIcon_WithUseFileType_ShouldReturnIconsForDirectoryAndFileExtension :: Test method UtilitiesCS.Test.HelperClasses.ShellUtilitiesStatic_Tests.GetFileIcon_WithUseFileType_ShouldReturnIconsForDirectoryAndFileExtension threw exception: System.ArgumentException: Win32 handle that was passed to Icon is not valid or is the wrong type.

The run completed within the same minute (no hang, no Sequence file); it failed one test rather than stalling. The plan's rule is CLEAR only when EXIT_CODE is 0, failed=0 and SEQUENCE_FILES is 0, so the result is REPRODUCES.

STALL-PROBE: REPRODUCES
COVERAGE-ROUTE: DIRECT
