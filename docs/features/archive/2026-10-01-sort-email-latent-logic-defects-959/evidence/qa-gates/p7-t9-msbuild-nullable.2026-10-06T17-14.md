# P7-T9 Nullable Rebuild (Phase 7 Toolchain Pass)

Timestamp: 2026-10-06T17-14
ITERATION: 1
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true (CMD-REBUILD, nullable GATEARGS, TASKID p7-t9; no Nullable property override; resolved through vswhere; plus /nodeReuse:false; plus a normal-verbosity file logger coverage\logs\p7-t9.msbuild.log)
EXIT_CODE: 0 (the printed MSBUILD_EXIT_CODE)
Output Summary: Build succeeded, 0 Warning(s), 0 Error(s), elapsed 00:00:20.80; CoreCompile ran for all four projects in scope; no write-set diagnostic lines (baseline 0). This is the first nullable rebuild after the P6-T13 documentation-comment edit and covers the P7-T2 and P7-T3 edits.

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
- NULLABLE-BASELINE-WRITESET-DIAGNOSTICS (P0-T8): 0

Execution note: the console output of this invocation was captured by the tool harness to a session-local file and the labels above were read from it; the payload ran unchanged. The full build output is in the file logger.

## Acceptance (P7-T9, all five required)

1. EXIT_CODE: 0: met.
2. SKIP_CORECOMPILE_LINES: 0: met.
3. The four _CSC_OUT_LINES values each at least 1 (2, 2, 2, 2): met.
4. ERRORS: 0: met.
5. WRITESET_DIAGNOSTIC_LINES 0 at most NULLABLE-BASELINE-WRITESET-DIAGNOSTICS 0: met.
