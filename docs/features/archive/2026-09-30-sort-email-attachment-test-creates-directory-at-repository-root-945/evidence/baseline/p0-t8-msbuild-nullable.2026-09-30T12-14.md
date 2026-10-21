# P0-T8 baseline nullable and type-check rebuild

Timestamp: 2026-09-30T12-14
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true (resolved through vswhere, plus /nodeReuse:false and a normal-verbosity file logger; no Nullable property override, no incremental Build target)
EXIT_CODE: 0
NULLABLE-BASELINE-EXIT: 0

Output Summary:
MSBUILD_EXIT_CODE: 0
SKIP_CORECOMPILE_LINES: 0
UCS_TEST_CSC_OUT_LINES: 2
UCS_CSC_OUT_LINES: 2
ZERO_ERRORS_LINES: 1
WARNINGS: 0
NULLABLE-BASELINE-WARNINGS: 0
ERRORS: 0
WRITESET_DIAGNOSTIC_LINES: 0
NULLABLE-BASELINE-WRITESET-DIAGNOSTICS: 0
UCS-TEST-DLL-EXISTS: True
