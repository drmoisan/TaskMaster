# MSBuild Nullable Baseline (P0-T14)

Timestamp: 2026-10-01T23-21
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true (resolved through vswhere, plus /nodeReuse:false and a normal-verbosity file logger at the git-ignored coverage\logs\p0-t14.msbuild.log; no Nullable property override)
EXIT_CODE: 0
Output Summary: MSBUILD_EXIT_CODE 0; ERRORS 0; WARNINGS 0; SKIP_CORECOMPILE_LINES 0; CSC_OUT_TASKMASTER 2; CSC_OUT_TASKMASTER_TEST 2; WRITESET_DIAGNOSTIC_LINES 0; both test DLLs exist.

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

NULLABLE-BASELINE-WARNINGS: 0
