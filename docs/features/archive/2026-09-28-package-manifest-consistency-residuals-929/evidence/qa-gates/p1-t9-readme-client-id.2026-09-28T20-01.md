# P1-T9 — Workflows README secret row names the Client ID

Timestamp: 2026-09-30T10-14
Command: Edit .github/workflows/README.md line 116; pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; $r = Get-Content ".github/workflows/README.md"; "README_NUMERIC=" + ...; "README_CLIENTID=" + ...; "NUMSTAT=" + (git diff --numstat 231e1c0b55105aeb626bf5a6e8d0266a567cacad -- .github/workflows/README.md)'; git diff 231e1c0b55105aeb626bf5a6e8d0266a567cacad -- .github/workflows/README.md
EXIT_CODE: 0
Output Summary:
- README_NUMERIC=0 (P0-T19 measured 1)
- README_CLIENTID=1 (at least 1)
- NUMSTAT=1	1	.github/workflows/README.md (1 added, 1 deleted)
- Diff (hunk @@ -113,7 +113,7 @@):
  - `-| `DEPENDABOT_REPAIR_APP_ID` | the numeric App identifier |`
  - `+| `DEPENDABOT_REPAIR_APP_ID` | the App's Client ID, passed to the token action's `client-id` input (the numeric App ID is not stored) |`
- The table shape is unchanged and the edit introduces no backticked three-part version literal (the AC26 test in DependabotConfig.Tests.ps1 reads every such literal in this file).
