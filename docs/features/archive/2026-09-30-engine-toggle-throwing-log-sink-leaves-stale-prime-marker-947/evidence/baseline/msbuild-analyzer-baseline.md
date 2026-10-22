# Baseline: MSBuild Analyzer Rebuild (P0-T10)

Timestamp: 2026-10-01T17-37
Task: P0-T10
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true (resolved through vswhere, plus /nodeReuse:false and a normal-verbosity file logger under the git-ignored coverage directory)
EXIT_CODE: 0

Output Summary:
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
- Result: P0-T10 acceptance holds; no ANALYZER BASELINE NOT CLEAN.

Note: the msbuild log stays under the git-ignored coverage directory and is not copied into the feature folder.
