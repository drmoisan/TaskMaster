# Baseline: analyzer rebuild (issue 942)

Timestamp: 2026-09-30T07-27
Task: P0-T9
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
EXIT_CODE: 0

Output Summary:
- Run as CMD-REBUILD (TASKID p0-t9): MSBuild resolved through vswhere, plus /nodeReuse:false and a normal-verbosity file logger under the ignored coverage directory.
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
