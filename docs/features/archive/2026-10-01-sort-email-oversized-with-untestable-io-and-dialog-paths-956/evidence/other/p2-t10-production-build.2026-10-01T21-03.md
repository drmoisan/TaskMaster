# P2-T10 Production project rebuild with warnings as errors

Timestamp: 2026-10-01T21-03
Command: msbuild UtilitiesCS\UtilitiesCS.csproj /t:Rebuild /m /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true (resolved through vswhere; plus /nodeReuse:false; plus a normal-verbosity file logger coverage\logs\p2-t10.msbuild.log, git-ignored)
EXIT_CODE: 0
Output Summary:
MSBUILD_EXIT_CODE: 0
PROD_CSC_OUT_LINES: 2
ZERO_ERRORS_LINES: 1
ZERO_WARNINGS_LINES: 1
CS8632_LINES: 0
CS0111_LINES: 0
CS0102_LINES: 0
Deviation (recorded): the MSBuild console stream was piped to Out-Null to keep the tool output short; the file logger, the targets that ran and the exit code read from $LASTEXITCODE are unaffected, and every field above is read from the file log as the CMD-BUILD-PROD payload specifies.
Note: the test project is not built in this phase; it cannot compile until Phase 3 adds the five-argument overload and the session type (the P1-T6 expected failure).
Acceptance: MSBUILD_EXIT_CODE 0; PROD_CSC_OUT_LINES at least 1; ZERO_ERRORS_LINES and ZERO_WARNINGS_LINES each at least 1; CS8632_LINES, CS0111_LINES and CS0102_LINES 0 (all hold).
