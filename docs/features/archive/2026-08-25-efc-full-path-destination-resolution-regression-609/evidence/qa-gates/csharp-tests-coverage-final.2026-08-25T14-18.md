Timestamp: 2026-08-25T14:22:47-04:00
Command: pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs/features/active/2026-08-25-efc-full-path-destination-resolution-regression-609/evidence/qa-gates/issue-609-remediation-final.cobertura.xml
EXIT_CODE: 0
Output Summary: Final restarted coverage run passed 6,479 of 6,479 tests and produced the post-processed Cobertura report. Repository line coverage: 53,761 / 63,418 = 84.7853%.

QA Restart Evidence: The first final coverage run exposed the existing null-globals AddSuggestions compatibility test. The archive-root projection now preserves the input when globals are absent; CSharpier, analyzer, nullable, and coverage gates were restarted. The final run passed.
