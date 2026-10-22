# P0-T7 Baseline Analyzer Rebuild

Timestamp: 2026-10-03T08-27
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true (resolved through vswhere; plus /nodeReuse:false; plus a normal-verbosity file logger coverage\logs\p0-t7.msbuild.log)
EXIT_CODE: 0 (the printed MSBUILD_EXIT_CODE)
Output Summary: Build succeeded, 0 Warning(s), 0 Error(s), elapsed 00:00:17.74; CoreCompile ran for all four projects in scope; no write-set diagnostic lines.

- MSBUILD_EXIT_CODE: 0
- SKIP_CORECOMPILE_LINES: 0
- UCS_TEST_CSC_OUT_LINES: 2
- UCS_CSC_OUT_LINES: 2
- QF_CSC_OUT_LINES: 2
- QFT_CSC_OUT_LINES: 2
- ZERO_ERRORS_LINES: 1
- WARNINGS: 0
- ANALYZE-BASELINE-WARNINGS: 0
- ERRORS: 0
- WRITESET_DIAGNOSTIC_LINES: 0
- ANALYZE-BASELINE-WRITESET-DIAGNOSTICS: 0

Acceptance check: EXIT_CODE 0; SKIP_CORECOMPILE_LINES 0; the four CSC_OUT_LINES values each at least 1; ERRORS 0. All four hold.
