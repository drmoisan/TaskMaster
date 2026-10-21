Timestamp: 2026-10-02T05-14
Command: pwsh -NoProfile -Command 'Set-Location <worktree-root>; & ./scripts/hygiene/Test-RepositoryHygiene.ps1; exit $LASTEXITCODE'
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary: Exactly four lines: `HYGIENE backup-file TaskMaster.sln.bak`, `HYGIENE backup-file TaskTree/TaskTree.vbproj.bak`, `HYGIENE backup-file TaskVisualization/TaskVisualization.vbproj.bak`, `HYGIENE Findings=3`. The P0-T12 run of the same command printed `HYGIENE Findings=0`; the difference is the new rule.
