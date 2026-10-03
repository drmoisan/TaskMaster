Timestamp: 2026-10-02T05-18
Command: git -C <worktree-root> add -- docs/features/active/2026-10-01-remaining-tracked-backup-files-and-hygiene-guard-rule-961; pwsh -NoProfile -Command 'Set-Location <worktree-root>; & ./scripts/hygiene/Test-RepositoryHygiene.ps1; exit $LASTEXITCODE'
EXIT_CODE: 0
Output Summary: The git add printed no error (line-ending warnings only). Guard output is exactly one line: `HYGIENE Findings=0`. The same command printed `HYGIENE Findings=3` and exited 1 in P1-T9.
