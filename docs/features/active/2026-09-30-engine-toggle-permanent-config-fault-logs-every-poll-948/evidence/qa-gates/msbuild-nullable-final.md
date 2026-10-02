# MSBuild Nullable Final (P3-T6)

Timestamp: 2026-10-02T00-22
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true (resolved through vswhere, plus /nodeReuse:false and a normal-verbosity file logger at the git-ignored coverage\logs\p3-t6.msbuild.log; no Nullable property override)
EXIT_CODE: 0
Output Summary: MSBUILD_EXIT_CODE 0; ERRORS 0; WARNINGS 0 (at most NULLABLE-BASELINE-WARNINGS 0); SKIP_CORECOMPILE_LINES 0; CSC_OUT_TASKMASTER 2; CSC_OUT_TASKMASTER_TEST 2; WRITESET_DIAGNOSTIC_LINES 0; TEST_DLL_EXISTS True.

Pass: 1

```
MSBUILD_EXIT_CODE: 0
ERRORS: 0
WARNINGS: 0
SKIP_CORECOMPILE_LINES: 0
CSC_OUT_TASKMASTER: 2
CSC_OUT_TASKMASTER_TEST: 2
WRITESET_DIAGNOSTIC_LINES: 0
TEST_DLL_EXISTS: True
UCS_TEST_DLL_EXISTS: True
```
