# P6-T3 Final Analyzer Rebuild

Timestamp: 2026-10-03T12-36
ITERATION: 1
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true (resolved through vswhere; plus /nodeReuse:false; plus a normal-verbosity file logger coverage\logs\p6-t3.msbuild.log)
EXIT_CODE: 0 (the printed MSBUILD_EXIT_CODE)
Output Summary: Build succeeded, 0 Warning(s), 0 Error(s), elapsed 00:00:15.90; CoreCompile ran for all four projects in scope; no write-set diagnostic lines (baseline 0).

- MSBUILD_EXIT_CODE: 0
- SKIP_CORECOMPILE_LINES: 0
- UCS_TEST_CSC_OUT_LINES: 2
- UCS_CSC_OUT_LINES: 2
- QF_CSC_OUT_LINES: 2
- QFT_CSC_OUT_LINES: 2
- ZERO_ERRORS_LINES: 1
- WARNINGS: 0
- ERRORS: 0
- WRITESET_DIAGNOSTIC_LINES: 0
- ANALYZE-BASELINE-WRITESET-DIAGNOSTICS (P0-T7): 0

Execution note: the console stream of this invocation was piped to a second pwsh that displayed only its last 25 lines. The payload ran unchanged and every label above was printed in full. The full build output is in the file logger.

## Acceptance (P6-T3, all five required)

1. EXIT_CODE: 0: met.
2. SKIP_CORECOMPILE_LINES: 0: met.
3. The four _CSC_OUT_LINES values each at least 1 (2, 2, 2, 2): met.
4. ERRORS: 0: met.
5. WRITESET_DIAGNOSTIC_LINES 0 at most ANALYZE-BASELINE-WRITESET-DIAGNOSTICS 0: met.
