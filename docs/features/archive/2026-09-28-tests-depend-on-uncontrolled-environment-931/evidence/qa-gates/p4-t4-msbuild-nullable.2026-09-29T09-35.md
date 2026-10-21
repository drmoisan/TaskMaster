# P4-T4 Nullable / Type-Check Rebuild Gate

Timestamp: 2026-09-29T09-35
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true (resolved through vswhere, plus /nodeReuse:false; CMD-REBUILD with TASKID p4-t4; no Nullable property override, no incremental Build target; file log coverage\logs\p4-t4.msbuild.log; console output discarded, the file log is the observed source)
EXIT_CODE: 0
ITERATION: 1

Output Summary:
- MSBUILD_EXIT_CODE: 0
- SKIP_CORECOMPILE_LINES: 0
- QF_TEST_CSC_OUT_LINES: 2
- UCS_TEST_CSC_OUT_LINES: 2
- ZERO_ERRORS_LINES: 1
- WARNINGS: 0
- ERRORS: 0
- WRITESET_DIAGNOSTIC_LINES: 0
- WARNINGS-DELTA: 0 (0 minus NULLABLE-BASELINE-WARNINGS 0; observation)
- QF-TEST-DLL-EXISTS: True
- UCS-TEST-DLL-EXISTS: True
- Wall clock of the rebuild: 16 seconds (the compiler echo counts above show every test project was compiled, not skipped).

Acceptance: EXIT_CODE 0; ERRORS 0; SKIP_CORECOMPILE_LINES 0; both compiler echoes at least 1; WRITESET_DIAGNOSTIC_LINES 0. All five hold.
