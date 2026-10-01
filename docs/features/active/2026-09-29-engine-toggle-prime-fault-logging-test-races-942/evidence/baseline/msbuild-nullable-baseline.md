# Baseline: nullable rebuild (issue 942)

Timestamp: 2026-09-30T07-27
Task: P0-T10
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
EXIT_CODE: 0

Output Summary:
- Run as CMD-REBUILD (TASKID p0-t10): MSBuild resolved through vswhere, plus /nodeReuse:false and a normal-verbosity file logger under the ignored coverage directory. No Nullable property override.
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
