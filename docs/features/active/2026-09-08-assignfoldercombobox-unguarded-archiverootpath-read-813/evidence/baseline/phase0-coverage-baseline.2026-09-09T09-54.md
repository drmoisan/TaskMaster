Timestamp: 2026-09-09T10-05
Command: pwsh -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs/features/active/2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read-813/evidence/baseline/coverage-baseline.cobertura.xml
EXIT_CODE: 0
Output Summary: Test Run Successful. Total tests: 7195. Passed: 7195. Failed: 0. Total time:
37.1161 seconds (9 test assemblies discovered and executed). Repo-wide Cobertura coverage from
produced XML: line-rate = 0.855936 (85.5936%), branch-rate = 0.797998 (79.7998%),
lines-covered = 55950, lines-valid = 65367.

Note: this measured baseline (85.5936% line / 79.7998% branch) differs from the figure cited in the
plan-wide "Coverage floor resolution" note (86.0424% line / 66.3978% branch, taken from a prior
sibling audit's same-methodology measurement). This artifact records the actual measurement taken
at P0-T8 execution time on this branch/worktree, which is the baseline P5-T6 compares against for
this plan's no-regression check. Both baselines exceed the 85% line floor and the 75% branch floor.
