# Stall probe (issue #968, task P0-T16)

Timestamp: 2026-10-03T02-51
Command: pwsh -NoProfile -Command '<CMD-VSTEST payload>' with ASSEMBLY `UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll`, FILTER `FullyQualifiedName~HelperClasses.ShellUtilities_Tests|FullyQualifiedName~HelperClasses.ShellUtilitiesStatic_Tests|FullyQualifiedName~HelperClasses.SysImageListHelperTests|FullyQualifiedName~EmailIntelligence.OSBrowser_Tests` (FILTER-STALL), TASKID p0-t16 and an empty NAMES list; the Command Reference CMD-VSTEST macro executed verbatim with PREFIX and TOOLS expanded and WORKTREE substituted
Canonical command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FILTER-STALL" "/ResultsDirectory:coverage\test-results\968\p0-t16" "/Logger:trx;LogFileName=p0-t16.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary:
- WORKTREE-LEAF: agent-a291a7fbabf9d0229
- VSTEST_EXIT_CODE: 1 (ExpectedExitCode carries the observed value; presentational)
- TRX_PRESENT: True
- SEQUENCE_FILES: 0
- COUNTERS total=23 executed=23 passed=22 failed=1
- RESULT_COUNT: 23
- MESSAGE GetFileIcon_WithUseFileType_ShouldReturnIconsForDirectoryAndFileExtension :: Test method UtilitiesCS.Test.HelperClasses.ShellUtilities_Tests.GetFileIcon_WithUseFileType_ShouldReturnIconsForDirectoryAndFileExtension threw exception: System.ArgumentException: Win32 handle that was passed to Icon is not valid or is the wrong type.
- The run did not stall (no Sequence file, no Timeout or Aborted outcome); one test failed fast.

STALL-PROBE: REPRODUCES
COVERAGE-ROUTE: DIRECT

Rule applied mechanically: CLEAR requires EXIT_CODE 0, failed=0 and SEQUENCE_FILES 0; the observed exit is 1 with failed=1, so the value is REPRODUCES and the route is DIRECT (D-6). The probe ran once and is not re-run. Consequence recorded per D-6 and the Risks section: AC22 cannot be met as worded under DIRECT and its check-off will record `AC22: NOT MET (ENVIRONMENTAL: COVERAGE-ROUTE DIRECT)` for the orchestrator's decision.
