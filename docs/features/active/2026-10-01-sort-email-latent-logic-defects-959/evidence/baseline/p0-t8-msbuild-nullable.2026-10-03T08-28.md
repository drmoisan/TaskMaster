# P0-T8 Baseline Nullable Rebuild

Timestamp: 2026-10-03T08-28
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true (resolved through vswhere; plus /nodeReuse:false; plus a normal-verbosity file logger coverage\logs\p0-t8.msbuild.log; no Nullable property override; the console stream was discarded and every figure below is read from the file logger)
EXIT_CODE: 0 (the printed MSBUILD_EXIT_CODE)
Output Summary: solution rebuild with warnings as errors succeeded; CoreCompile ran for all four projects in scope; 0 errors; no write-set diagnostic lines; both test assemblies exist.

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
- NULLABLE-BASELINE-WRITESET-DIAGNOSTICS: 0
- UCT-DLL-EXISTS: True
- QFT-DLL-EXISTS: True

Acceptance check: EXIT_CODE 0; SKIP_CORECOMPILE_LINES 0; the four CSC_OUT_LINES values each at least 1; ERRORS 0; UCT-DLL-EXISTS True and QFT-DLL-EXISTS True. All five hold.
