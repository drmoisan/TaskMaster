# P0-T8 Baseline Nullable / Type-Check Rebuild

Timestamp: 2026-09-29T08-59
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true (resolved through vswhere, plus /nodeReuse:false; CMD-REBUILD with TASKID p0-t8; no Nullable property override, no incremental Build target; file log coverage\logs\p0-t8.msbuild.log; console output discarded, the file log is the observed source)
EXIT_CODE: 0

Output Summary:
- NULLABLE-BASELINE-EXIT: 0
- SKIP_CORECOMPILE_LINES: 0
- QF_TEST_CSC_OUT_LINES: 2
- UCS_TEST_CSC_OUT_LINES: 2
- ZERO_ERRORS_LINES: 1
- WARNINGS: 0 (NULLABLE-BASELINE-WARNINGS: 0)
- ERRORS: 0
- WRITESET_DIAGNOSTIC_LINES: 0
- QF-TEST-DLL-EXISTS: True
- UCS-TEST-DLL-EXISTS: True
- The analyzer packages provisioned in P0-T7 (git-ignored) were present for this run; no provisioning occurred in this task.

Acceptance: all six conditions hold.
