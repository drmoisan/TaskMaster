# P4-T4 Type-check (nullable) rebuild gate

Timestamp: 2026-10-01T21-20
ITERATION: 1
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true (resolved through vswhere; plus /nodeReuse:false; plus a normal-verbosity file logger coverage\logs\p4-t4.msbuild.log, git-ignored; no Nullable property override; Rebuild target; console stream discarded with Out-Null as at P0-T8; run as a background invocation)
EXIT_CODE: 0
Output Summary:
MSBUILD_EXIT_CODE: 0
SKIP_CORECOMPILE_LINES: 0
UCS_TEST_CSC_OUT_LINES: 2
UCS_CSC_OUT_LINES: 2
ZERO_ERRORS_LINES: 1
WARNINGS: 0
ERRORS: 0
WRITESET_DIAGNOSTIC_LINES: 0
NULLABLE-BASELINE-WRITESET-DIAGNOSTICS (P0-T8): 0
Acceptance: EXIT_CODE 0; SKIP_CORECOMPILE_LINES 0; both _CSC_OUT_LINES values 2 (at least 1); ERRORS 0; WRITESET_DIAGNOSTIC_LINES 0, not above the P0-T8 baseline of 0. All five hold.
