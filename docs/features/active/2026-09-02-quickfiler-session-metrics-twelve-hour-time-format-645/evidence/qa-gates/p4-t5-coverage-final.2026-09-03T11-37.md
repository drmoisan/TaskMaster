# P4-T5 — Full-Assembly, Coverage-Enabled Final Run

Timestamp: 2026-09-03T11-37
Command: pwsh -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot QuickFiler.Test
-Configuration Debug -CoverageOutput 'docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/qa-gates/coverage-final.cobertura.xml'
(invoked via absolute path to the item worktree's script; MSYS_NO_PATHCONV=1 prefix used for
the same git-bash switch-mangling reason recorded in P0-T14/P0-T15)
EXIT_CODE: 1 (pwsh process; the script throws inside `Assert-CoberturaLineCoverageThreshold`
below the repository's 80% floor, matching the P0-T15 baseline behavior)

Output Summary: vstest reported "Test Run Successful. Total tests: 1312, Passed: 1312." (0
failed) for the full `QuickFiler.Test` assembly, satisfying the spec.md AC requiring the full
assembly to be green under a coverage-enabled run. `Assert-CoberturaLineCoverageThreshold` then
threw: "Cobertura line coverage 23.8225% is below the required 80% threshold." — identical to
the P0-T15 baseline figure, confirming no coverage regression. Per P4-T5's own acceptance text,
this coverage-threshold exception is treated as task completion rather than triggering the
Phase 4 restart rule; it is the same pre-existing, repository-wide condition documented in
P0-T15, which this plan's three-line format-string change neither introduces nor can remediate.

FinalLineRate (thrown, post-processed): 23.8225%
