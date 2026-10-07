# P3-T7 Test project build (green)

Timestamp: 2026-10-01T21-10
Command: msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU (resolved through vswhere; plus /nodeReuse:false; plus a normal-verbosity file logger coverage\logs\p3-t7.msbuild.log, git-ignored)
EXIT_CODE: 0
Output Summary:
MSBUILD_EXIT_CODE: 0
CSC_OUT_LINES: 2
ZERO_ERRORS_LINES: 1
ERROR_LINES: 0
ERROR_LINES_NEW_TEST_FILES: 0
ERROR_LINES_OTHER_FILES: 0
MISSING_SESSION_TYPE_LINES: 0
CS1501_LINES: 0
ERROR_CODES: (empty)
DLL_ADVANCED: True
Deviation (recorded): the MSBuild console stream was piped to Out-Null to keep the tool output short; the file logger, the targets and $LASTEXITCODE are unaffected, and every field is read from the file log as CMD-BUILD-TEST specifies.
Note: the P1-T6 fail-before build of the same project was red (missing five-argument overload and session type); with the Phase 3 seam and session type in place it compiles.
Acceptance: MSBUILD_EXIT_CODE 0; ERROR_LINES 0; CSC_OUT_LINES and ZERO_ERRORS_LINES each at least 1; DLL_ADVANCED True (all hold).
