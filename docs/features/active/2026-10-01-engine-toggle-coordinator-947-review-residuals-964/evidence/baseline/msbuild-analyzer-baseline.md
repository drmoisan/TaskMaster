# Baseline: analyzer rebuild (P0-T11)

Timestamp: 2026-10-03T07-37
Task: P0-T11
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
EXIT_CODE: 0

Output Summary:
- MSBUILD_EXIT_CODE: 0
- ERRORS: 0
- ANALYZER-BASELINE-WARNINGS: 0
- CSC_OUT_TASKMASTER: 2
- CSC_OUT_TASKMASTER_TEST: 2
- WRITESET_DIAGNOSTIC_LINES: 0
- TEST_DLL_EXISTS: True
- Verdict: PASS (no ANALYZER BASELINE NOT CLEAN). MSBuild resolved through vswhere; solution passed by absolute path; file log at coverage\logs\p0-t11.msbuild.log (git-ignored, not copied).
