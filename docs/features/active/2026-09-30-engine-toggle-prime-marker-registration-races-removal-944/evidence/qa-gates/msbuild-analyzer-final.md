# MSBuild Analyzer Gate, Final (P3-T5)

Timestamp: 2026-09-30T13-47
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true (CMD-REBUILD, TASKID p3-t5; MSBuild resolved through vswhere; plus /nodeReuse:false and a normal-verbosity file logger at coverage\logs\p3-t5.msbuild.log, git-ignored)
EXIT_CODE: 0
Output Summary: MSBUILD_EXIT_CODE: 0; ERRORS: 0; WARNINGS: 0 (at most ANALYZER-BASELINE-WARNINGS 0); SKIP_CORECOMPILE_LINES: 0; CSC_OUT_TASKMASTER: 2; CSC_OUT_TASKMASTER_TEST: 2; WRITESET_DIAGNOSTIC_LINES: 0; TEST_DLL_EXISTS: True; UCS_TEST_DLL_EXISTS: True. Every P3-T5 clause holds. Pass number: 1.

## Observed

- MSBUILD_EXIT_CODE: 0
- ERRORS: 0
- WARNINGS: 0
- SKIP_CORECOMPILE_LINES: 0
- CSC_OUT_TASKMASTER: 2
- CSC_OUT_TASKMASTER_TEST: 2
- WRITESET_DIAGNOSTIC_LINES: 0
- TEST_DLL_EXISTS: True
- UCS_TEST_DLL_EXISTS: True
- Run window (payload START_UTC and END_UTC): 13-46-38 to 13-47-01 UTC, foreground.
