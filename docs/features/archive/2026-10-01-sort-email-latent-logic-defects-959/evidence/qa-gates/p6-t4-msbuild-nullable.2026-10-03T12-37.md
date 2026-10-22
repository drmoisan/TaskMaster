# P6-T4 Final Nullable Rebuild

Timestamp: 2026-10-03T12-37
ITERATION: 1
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true (resolved through vswhere; plus /nodeReuse:false; plus a normal-verbosity file logger coverage\logs\p6-t4.msbuild.log; no Nullable property override)
EXIT_CODE: 0 (the printed MSBUILD_EXIT_CODE)
Output Summary: Build succeeded, 0 Warning(s), 0 Error(s), elapsed 00:00:16.10; CoreCompile ran for all four projects in scope; no write-set diagnostic lines (baseline 0).

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

Execution note: the first invocation of this task added the console switch /v:minimal, which is not in the payload. It printed the same figures (exit 0, 0 errors, 0 write-set lines). The payload was then rerun exactly as written, and the figures above come from that verbatim run.

## Acceptance (P6-T4, all five required)

1. EXIT_CODE: 0: met.
2. SKIP_CORECOMPILE_LINES: 0: met.
3. The four _CSC_OUT_LINES values each at least 1 (2, 2, 2, 2): met.
4. ERRORS: 0: met.
5. WRITESET_DIAGNOSTIC_LINES 0 at most NULLABLE-BASELINE-WRITESET-DIAGNOSTICS 0: met.
