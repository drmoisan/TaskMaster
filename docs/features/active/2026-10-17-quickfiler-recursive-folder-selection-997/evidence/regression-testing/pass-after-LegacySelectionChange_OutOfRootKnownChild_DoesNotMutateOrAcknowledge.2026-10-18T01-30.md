Timestamp: 2026-10-18T01-30
Command: pwsh -NoProfile -Command '. ./scripts/vscode/Invoke-MSTestWithCoverage.ps1; Invoke-MSTestWithCoverageMain -SearchRoot . -Configuration Debug -CoverageOutput coverage/issue-997-final-20261018012531.cobertura.xml -ResultsDirectory SCRATCH/issue-997-final-results-20261018012531 -ScriptRoot ./scripts/vscode'
EXIT_CODE: 0
ExpectedExitCode: 0
Output Summary:   Passed LegacySelectionChange_OutOfRootKnownChild_DoesNotMutateOrAcknowledge [12 ms]
Final full run: 7,504 passed, 0 failed. Source: csharp-tests-coverage.2026-10-18T01-27.md; matching fail-before record retained.

