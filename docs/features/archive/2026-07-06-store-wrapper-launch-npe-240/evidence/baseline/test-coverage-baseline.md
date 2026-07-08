# Pre-Change Test + Coverage Baseline (Issue #240)

Timestamp: 2026-07-06T07-20

Command: `vstest.console.exe UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll /EnableCodeCoverage /InIsolation`
(`/InIsolation` added because Moq test assemblies in this repo require it to avoid an STTE 4.2.0.1 Setup FileNotFound failure; dash-free forward-slash dll path required because git-bash mangles backslash-separated relative paths.)

EXIT_CODE: 0

Coverage extraction command: `dotnet-coverage merge <run>.coverage -f xml -o TestResults/baseline-coverage.xml`

Output Summary: Test Run Successful. Total tests: 4163, Passed: 4163, Failed: 0. Total time 43.12s. No `Launch()`-calling tests exist yet in `StoreWrapperController_Tests.cs` at this baseline. Converted `.coverage` module report for `UtilitiesCS.dll` (the production assembly containing `StoreWrapperController`): line_coverage = **85.87%** (lines_covered=36873, lines_partially_covered=984, lines_not_covered=5085), block_coverage = 86.68%. This is the testable-denominator repository line-coverage percentage referenced by AC5/P3-T4/P3-T5 for this issue's scope (UtilitiesCS project).
