# MSBuild Nullable Baseline (P0-T15)

Timestamp: 2026-09-30T13-23
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true (CMD-REBUILD, TASKID p0-t15; no Nullable property override; MSBuild resolved through vswhere; plus /nodeReuse:false and a normal-verbosity file logger at coverage\logs\p0-t15.msbuild.log, git-ignored)
EXIT_CODE: 0
Output Summary: MSBUILD_EXIT_CODE: 0; ERRORS: 0; WARNINGS: 0; SKIP_CORECOMPILE_LINES: 0; CSC_OUT_TASKMASTER: 2; CSC_OUT_TASKMASTER_TEST: 2; WRITESET_DIAGNOSTIC_LINES: 0; TEST_DLL_EXISTS: True; UCS_TEST_DLL_EXISTS: True. No NULLABLE BASELINE NOT CLEAN.

## Observed

- MSBUILD_EXIT_CODE: 0
- ERRORS: 0
- WARNINGS: 0
- NULLABLE-BASELINE-WARNINGS: 0
- SKIP_CORECOMPILE_LINES: 0
- CSC_OUT_TASKMASTER: 2
- CSC_OUT_TASKMASTER_TEST: 2
- WRITESET_DIAGNOSTIC_LINES: 0
- TEST_DLL_EXISTS: True
- UCS_TEST_DLL_EXISTS: True
- Wall clock: 13-23-29 to 13-23-46 UTC (17 seconds). Supplementary check on the same log because of the short duration: 36 lines naming csc.exe, one "Build succeeded" line, and the TaskMaster.Test.dll write time 13:23:41 UTC falls inside the run, so the compiler ran.
