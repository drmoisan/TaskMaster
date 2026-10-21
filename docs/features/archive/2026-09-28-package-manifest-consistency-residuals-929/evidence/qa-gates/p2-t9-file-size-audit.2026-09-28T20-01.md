# P2-T9 — File size audit of the change footprint

Timestamp: 2026-09-30T11-08
Command: pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; foreach ($p in <the six non-Markdown Write Set files>) { "LINECOUNT " + $p + " " + @(Get-Content -LiteralPath $p).Count }'
EXIT_CODE: 0
Output Summary:
- QuickFiler.Test/QuickFiler.Test.csproj: 568 (the post-merge 570 minus the two deleted imports)
- SVGControl/app.config: 23
- .github/workflows/dependabot-repair.yml: 173
- scripts/dependencies/ConsistencyVerifier.psm1: 499 (equals VERIFIER-LINES 499; at most 500)
- tests/scripts/dependencies/ConsistencyVerifier.Tests.ps1: 337 (at most 500)
- tests/scripts/dependencies/RepositoryTreeConsistency.Tests.ps1: 152 (at most 500)
- Exactly 6 files listed. Markdown files (the workflows README, the 911 runbook, issue.md and the plan) are exempt from the 500-line cap and are deliberately not listed.
