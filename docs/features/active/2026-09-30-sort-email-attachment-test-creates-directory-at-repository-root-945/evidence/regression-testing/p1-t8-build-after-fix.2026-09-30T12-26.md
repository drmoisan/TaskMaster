# P1-T8 build of the fixed state

Timestamp: 2026-09-30T12-26
Command: msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU (resolved through vswhere, plus /nodeReuse:false and a normal-verbosity file logger)
EXIT_CODE: 0

Output Summary:
MSBUILD_EXIT_CODE: 0
CSC_OUT_LINES: 2
PROD_CSC_OUT_LINES: 2
ZERO_ERRORS_LINES: 1
NAMED_ERROR_LINES: 0
CS1501_LINES: 0
DLL_ADVANCED: True
The same build that P1-T3 observed red (exit 1, four CS1501 lines) is green.
