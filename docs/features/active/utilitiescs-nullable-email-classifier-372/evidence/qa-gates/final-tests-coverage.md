# Final QC — Full Test Suite with Coverage

Timestamp: 2026-07-19T06-20

Command: `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-email-classifier/evidence/qa-gates/final-coverage.cobertura.xml`

EXIT_CODE: 0

Output Summary:
- Total tests: 5702. Passed: 5702. Failed: 0. Test Run Successful.
- Post-change line coverage: 83.83% (line-rate 0.838258; lines-covered 86841 / lines-valid 103597).
- Post-change branch coverage: 76.36% (branch-rate 0.763641; branches-covered 19537 / branches-valid 25584).
- The golden/property/characterization scoring suites, the SpamBayes/Triage/predictor suites, the Flags suites, and the subclass test doubles (SubBayesianClassifier/SubClassifierGroup/SubCorpus) all pass unchanged (AC3).
- Baseline was 83.78% line / 76.33% branch (5702 passed); post-change is at or slightly above baseline — no regression.
