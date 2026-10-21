# Nullable / Type-Check Baseline Rebuild (P0-T8)

Timestamp: 2026-09-30T07-17
Task: P0-T8
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true (resolved through vswhere, plus /nodeReuse:false; no Nullable property override, no incremental Build target; file logger coverage\logs\p0-t8.msbuild.log at normal verbosity, git-ignored; console stream discarded with Out-Null, every value read from the file log)
EXIT_CODE: 0
Output Summary: TreatWarningsAsErrors rebuild clean; CoreCompile ran for both projects; zero warnings and errors; the test assembly exists for P0-T9 and P0-T10.
- NULLABLE-BASELINE-EXIT: 0
- SKIP_CORECOMPILE_LINES: 0
- UCS_TEST_CSC_OUT_LINES: 2
- UCS_CSC_OUT_LINES: 2
- ZERO_ERRORS_LINES: 1
- WARNINGS: 0
- NULLABLE-BASELINE-WARNINGS: 0
- ERRORS: 0
- WRITESET_DIAGNOSTIC_LINES: 0
- UCS-TEST-DLL-EXISTS: True
