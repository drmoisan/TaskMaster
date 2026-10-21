# QA Gate: MSBuild Nullable Type-Check Rebuild, Final (P2-T6)

Timestamp: 2026-10-01T18-05
Task: P2-T6
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true (resolved through vswhere, plus /nodeReuse:false and a normal-verbosity file logger under the git-ignored coverage directory; no Nullable property override)
EXIT_CODE: 0

Output Summary:
- MSBUILD_EXIT_CODE: 0
- ERRORS: 0
- WARNINGS: 0 (at most NULLABLE-BASELINE-WARNINGS: 0)
- SKIP_CORECOMPILE_LINES: 0
- CSC_OUT_TASKMASTER: 2
- CSC_OUT_TASKMASTER_TEST: 2
- WRITESET_DIAGNOSTIC_LINES: 0
- TEST_DLL_EXISTS: True
- UCS_TEST_DLL_EXISTS: True
- Result: P2-T6 acceptance holds.

Note: the msbuild log stays under the git-ignored coverage directory and is not copied into the feature folder.
