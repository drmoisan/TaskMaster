# P2-T8 Build After Fix

Timestamp: 2026-09-29T09-18
Command: msbuild QuickFiler.Test\QuickFiler.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU (CMD-BUILD-QF, TASKID p2-t8-qf) and msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU (CMD-BUILD-UCS, TASKID p2-t8-ucs), resolved through vswhere, plus /nodeReuse:false; file logs under the git-ignored coverage\logs directory
EXIT_CODE: 0

Output Summary:
- EXIT_CODE is scoped to the QuickFiler.Test build.
- QuickFiler.Test build: MSBUILD_EXIT_CODE: 0; CSC_OUT_LINES: 2; PROD_CSC_OUT_LINES: 0 (no production file changed); ZERO_ERRORS_LINES: 1; DLL_ADVANCED: True; console: Build succeeded, 0 Warning(s), 0 Error(s).
- UCS-MSBUILD-EXIT: 0
- UtilitiesCS.Test build: MSBUILD_EXIT_CODE: 0; CSC_OUT_LINES: 2; PROD_CSC_OUT_LINES: 0 (no production file changed); ZERO_ERRORS_LINES: 1; DLL_ADVANCED: True; console: Build succeeded, 0 Warning(s), 0 Error(s).
- Acceptance: both exit codes 0; both CSC_OUT_LINES at least 1 and both DLL_ADVANCED True; both ZERO_ERRORS_LINES at least 1 - HOLD.
