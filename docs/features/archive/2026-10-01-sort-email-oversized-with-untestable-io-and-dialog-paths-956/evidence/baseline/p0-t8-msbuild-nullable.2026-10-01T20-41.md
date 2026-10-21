# P0-T8 Baseline nullable rebuild

Timestamp: 2026-10-01T20-41
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true (resolved through vswhere; plus /nodeReuse:false; plus a normal-verbosity file logger coverage\logs\p0-t8.msbuild.log, git-ignored; no Nullable property override; Rebuild target; console stream discarded with Out-Null; run as a background invocation)
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
NULLABLE-BASELINE-WRITESET-DIAGNOSTICS: 0
UCS-TEST-DLL-EXISTS: True
Acceptance: EXIT_CODE 0; SKIP_CORECOMPILE_LINES 0; both CSC_OUT counts at least 1; ERRORS 0; UCS-TEST-DLL-EXISTS True (all hold).
