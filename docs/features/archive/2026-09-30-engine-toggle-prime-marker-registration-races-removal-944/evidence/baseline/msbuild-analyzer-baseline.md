# MSBuild Analyzer Baseline (P0-T14)

Timestamp: 2026-09-30T13-22
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true (CMD-REBUILD, TASKID p0-t14; MSBuild resolved through vswhere; plus /nodeReuse:false and a normal-verbosity file logger at coverage\logs\p0-t14.msbuild.log, git-ignored)
EXIT_CODE: 0
Output Summary: MSBUILD_EXIT_CODE: 0; ERRORS: 0; WARNINGS: 0; SKIP_CORECOMPILE_LINES: 0; CSC_OUT_TASKMASTER: 2; CSC_OUT_TASKMASTER_TEST: 2; WRITESET_DIAGNOSTIC_LINES: 0; TEST_DLL_EXISTS: True; UCS_TEST_DLL_EXISTS: True. No ANALYZER BASELINE NOT CLEAN.

## Observed

- MSBUILD_EXIT_CODE: 0
- ERRORS: 0
- WARNINGS: 0
- ANALYZER-BASELINE-WARNINGS: 0
- SKIP_CORECOMPILE_LINES: 0
- CSC_OUT_TASKMASTER: 2
- CSC_OUT_TASKMASTER_TEST: 2
- WRITESET_DIAGNOSTIC_LINES: 0
- TEST_DLL_EXISTS: True
- UCS_TEST_DLL_EXISTS: True
- Run started and ended within the minute 2026-09-30T13-22 (payload START_UTC and END_UTC); run as a background process with completion detected by the PAYLOAD-COMPLETE line.
