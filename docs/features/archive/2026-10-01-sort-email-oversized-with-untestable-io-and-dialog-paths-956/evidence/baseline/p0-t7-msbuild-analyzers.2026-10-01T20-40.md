# P0-T7 Baseline analyzer rebuild

Timestamp: 2026-10-01T20-40
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true (resolved through vswhere; plus /nodeReuse:false; plus a normal-verbosity file logger coverage\logs\p0-t7.msbuild.log, git-ignored; the console stream was discarded with Out-Null, which does not change the targets run or the diagnostics reported; run as a background invocation)
EXIT_CODE: 0
Output Summary:
MSBUILD_EXIT_CODE: 0
SKIP_CORECOMPILE_LINES: 0
UCS_TEST_CSC_OUT_LINES: 2
UCS_CSC_OUT_LINES: 2
ZERO_ERRORS_LINES: 1
WARNINGS: 0
ANALYZE-BASELINE-WARNINGS: 0
ERRORS: 0
WRITESET_DIAGNOSTIC_LINES: 0
ANALYZE-BASELINE-WRITESET-DIAGNOSTICS: 0
Acceptance: EXIT_CODE 0; SKIP_CORECOMPILE_LINES 0; both CSC_OUT counts at least 1; ERRORS 0 (all hold).
