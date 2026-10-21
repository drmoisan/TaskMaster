# P6-T16 Guard post-sweep run over the final tree (AC17, gate six)

Timestamp: 2026-09-29T22-28
Command: GATE6: pwsh -NoProfile -Command '& ./scripts/hygiene/Test-RepositoryHygiene.ps1 2>&1 | Tee-Object -FilePath coverage/logs/927-guard-run.log; exit $LASTEXITCODE' (run as a background process from the item worktree root, wrapped in a System.Diagnostics.Stopwatch that printed GUARD-SECONDS= and GUARD-EXIT=); then pwsh -NoProfile -Command '$l = Get-Content -LiteralPath "coverage/logs/927-guard-run.log"; "FINDING-LINES=" + @($l | Where-Object { $_ -like "HYGIENE * *" -and $_ -notlike "HYGIENE Findings=*" }).Count; $l | Where-Object { $_ -like "HYGIENE Findings=*" }; "ENUMERATED-FEATURE-FOLDER=" + @(git ls-files -- "docs/features/active/2026-09-28-evidence-and-identity-hygiene-sweep-927").Count'
EXIT_CODE: 0
Output Summary:
- GATE6 exit code 0 (GUARD-EXIT=0)
- HYGIENE Findings=0
- FINDING-LINES=0
- ENUMERATED-FEATURE-FOLDER=61
- GUARD-SECONDS=39 (post-cleanup guard runtime, measured by stopwatch around the guard invocation; below the new CI job's 600-second timeout; the pre-sweep run took about 1,190 seconds)
- Enumeration statement: the guard enumerates the tracked tree with git ls-files and reads content from the working tree, so the plan file and every artifact committed by P6-T15 (commit 58e2c94de) were inside the scan.
- The raw guard log stays under the ignored coverage/logs/ directory and is not committed.
