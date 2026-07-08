# Test + Coverage Baseline (Remediation Cycle, Issue #240)

- Timestamp: 2026-07-06T12-15
- Command: `vstest.console.exe UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll /EnableCodeCoverage /InIsolation`
- EXIT_CODE: 1
- Output Summary: Total tests: 4170. Passed: 4169. Failed: 1. Total time: 42.21s. The single failure, `PrintTree_WritesIndentedTreeToConsole`, is a pre-existing, unrelated failure outside the `StoreWrapperController_Tests` class and outside the scope of this remediation cycle (Finding 1 only). All 39 `[TestMethod]`s in `StoreWrapperController_Tests.cs` passed, confirmed individually via log grep (`RunFolderSelectionDialog_*`, `PairwiseEquals_*`, `ButtonOk_Click_*`, `Launch_When*`, `EvaluateLaunchReadiness_*`, etc.).

Coverage extraction command: `dotnet-coverage merge <run>.coverage -f xml -o TestResults/remediation-baseline-coverage.xml`

Converted `.coverage` module report for `UtilitiesCS.dll` (the production assembly containing `StoreWrapperController`): line_coverage = **85.88%** (lines_covered=36896, lines_partially_covered=984, lines_not_covered=5084), block_coverage = 86.69%. This is the testable-denominator repository line-coverage baseline for this remediation cycle, matching the prior feature-cycle baseline recorded in `evidence/baseline/test-coverage-baseline.md` (85.87%) within measurement noise from incidental line additions unrelated to this cycle.
