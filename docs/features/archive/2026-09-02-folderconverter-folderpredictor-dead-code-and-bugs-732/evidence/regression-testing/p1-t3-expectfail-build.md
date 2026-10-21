# P1-T3 [expect-fail]: Test Project Compile Check

Timestamp: 2026-09-03T11-30

Command (plan-literal, "Any CPU" with a space): msbuild UtilitiesCS.Test/UtilitiesCS.Test.csproj /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"
EXIT_CODE: 1

Output Summary (plan-literal attempt): failed with "The BaseOutputPath/OutputPath
property is not set for project 'UtilitiesCS.Test.csproj'." A standalone (non-solution)
project build does not recognize the solution-level `Any CPU` platform alias; the
project itself defines `AnyCPU` (no space).

Command (corrected): msbuild UtilitiesCS.Test/UtilitiesCS.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU
EXIT_CODE: 0

Output Summary (corrected, primary result for this task):
"Build succeeded. 0 Warning(s) 0 Error(s)." Time Elapsed 00:00:08.94. Confirms the new
`CreateFolder_WhenParentBranchPathIsEmpty_DoesNotThrowIndexOutOfRangeException` test
method added in P1-T1 compiles cleanly against the pre-fix production code.
