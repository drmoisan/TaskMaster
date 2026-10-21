# Baseline — Repository-Wide Coverage

Timestamp: 2026-09-09T16-39

Command: pwsh -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -CoverageOutput docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/baseline/coverage-baseline.cobertura.xml

EXIT_CODE: 0

LineRate: 0.856211
LinesCovered: 56033
LinesValid: 65443
BranchRate: 0.79807
BranchesCovered: 13481
BranchesValid: 16892
TestsPassed: 7212
TestsFailed: 0 (omitted category, transcribed per D10)
TestsSkipped: 0 (omitted category, transcribed per D10)

Output Summary: The run discovered every test assembly under bin\Debug\ from the repository root,
excluded .claude\ worktree builds and applied the runner's hardcoded
/TestCaseFilter:TestCategory!=LiveOutlook. vstest printed "Test Run Successful.", "Total tests:
7212", "Passed: 7212" and "Total time: 29.3362 Seconds". Per D10 a fully green run on this toolchain
emits no `Failed:` and no `Skipped:` line, so both counters are transcribed as 0; that transcription
is corroborated by the printed success header and by `Passed:` equalling `Total tests:`. The run was
green, so D9's re-run ladder was not entered and the processed Cobertura XML was produced. The
runner's own post-processing line reported "First-party coverage: lines 56033/65443 (85.62%),
branches 13481/16892 (79.81%)", which reconciles with the six root-element values above. The
processed Cobertura carries 9 package elements. D6's non-termination hazard did not occur: the run
completed in well under a minute and no shell-icon class stalled it.
