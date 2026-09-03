# P0-T15 — Full-Assembly, Coverage-Enabled Baseline

Timestamp: 2026-09-03T11-28
Command: pwsh -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot QuickFiler.Test
-Configuration Debug -CoverageOutput 'docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/baseline/coverage-baseline.cobertura.xml'
(invoked via absolute path to the item worktree's script; MSYS_NO_PATHCONV=1 prefix used for the
same git-bash switch-mangling reason recorded in P0-T14)
EXIT_CODE: 1 (pwsh process; the script throws inside `Assert-CoberturaLineCoverageThreshold`
below the repository's 80% floor)

Deviation note: the delegation prompt flagged a known `.claude`-path-exclusion defect in
`Invoke-MSTestWithCoverage.ps1` (line ~301) that could cause a "No test assemblies found" failure
because this worktree lives under `.claude/worktrees/`. That specific failure mode did NOT occur:
the script's `Get-ChildItem` discovery found `Discovered 1 test assemblies.` and successfully ran
the full QuickFiler.Test assembly. The vstest.console.exe fallback described in the delegation
prompt was therefore not needed for this task.

Output Summary: vstest reported "Test Run Successful. Total tests: 1312, Passed: 1312." (0
failed) for the full `QuickFiler.Test` assembly. `Assert-CoberturaLineCoverageThreshold` then
threw: "Cobertura line coverage 23.8225% is below the required 80% threshold." — this is the
post-processed (Koverage-filtered) line-rate computed in memory; the exception is thrown before
`Set-Content` (line 343 of the script) writes the post-processed file back, so the artifact on
disk at the `-CoverageOutput` path is the RAW, un-post-processed `dotnet-coverage collect` output
(root `<coverage line-rate="0.19850795088566828" ...>`, i.e. 19.85% including vendored/
third-party assemblies not yet filtered out). Per Plan-Level Decision / P0-T15's own acceptance
text, the thrown percentage (23.8225%) is recorded verbatim as the baseline figure; this is a
pre-existing, repository-wide condition (the repo-wide coverage floor is not met when measured
across the whole loaded-module set), not a regression introduced by this change, and this plan's
three changed lines are already covered by existing passing tests per spec.md Test Strategy.

BaselineLineRate (thrown, post-processed): 23.8225%
