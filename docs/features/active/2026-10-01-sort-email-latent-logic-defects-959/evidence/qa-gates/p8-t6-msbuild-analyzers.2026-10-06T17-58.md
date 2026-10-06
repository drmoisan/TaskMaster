# P8-T6 Analyzer Rebuild on the Merged Tree (Phase 8 Toolchain Pass)

Timestamp: 2026-10-06T17-58
ITERATION: 1
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true (CMD-REBUILD, analyzer GATEARGS, TASKID p8-t6; resolved through vswhere; plus /nodeReuse:false; plus a normal-verbosity file logger coverage\logs\p8-t6.msbuild.log)
EXIT_CODE: 0 (the printed MSBUILD_EXIT_CODE)
Output Summary: Build succeeded, 0 Warning(s), 0 Error(s), Time Elapsed 00:00:19.32 on the merged tree (merge commit 9163994569e24c5c539a285724f9c8f9f6fd8a0e); CoreCompile ran for all four projects in scope; no write-set diagnostic lines (baseline 0).

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

Execution note: the msbuild console stream was piped to Out-Null inside the payload to limit tool output; the file logger, which every label above reads, is unaffected, and no target, property or diagnostic setting changed.

## Acceptance (P8-T6, all five required)

1. EXIT_CODE: 0: met.
2. SKIP_CORECOMPILE_LINES: 0: met.
3. The four _CSC_OUT_LINES values each at least 1 (2, 2, 2, 2): met.
4. ERRORS: 0: met.
5. WRITESET_DIAGNOSTIC_LINES 0 at most ANALYZE-BASELINE-WRITESET-DIAGNOSTICS 0: met.
