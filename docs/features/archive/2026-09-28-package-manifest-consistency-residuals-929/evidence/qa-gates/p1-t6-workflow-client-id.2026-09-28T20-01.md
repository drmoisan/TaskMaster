# P1-T6 — Repair workflow passes client-id

Timestamp: 2026-09-30T10-09
Command: Edit .github/workflows/dependabot-repair.yml lines 13, 14 and 51; pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; $w = Get-Content ".github/workflows/dependabot-repair.yml"; "WF_APPID=" + ...; "WF_CLIENTID=" + ...; "WF_SECRET=" + ...; "NUMSTAT=" + (git diff --numstat 231e1c0b55105aeb626bf5a6e8d0266a567cacad -- .github/workflows/dependabot-repair.yml)'; git diff 231e1c0b55105aeb626bf5a6e8d0266a567cacad -- .github/workflows/dependabot-repair.yml
EXIT_CODE: 0
Output Summary:
- WF_APPID=0
- WF_CLIENTID=1
- WF_SECRET=1 (the secret name DEPENDABOT_REPAIR_APP_ID is unchanged, decision D3)
- NUMSTAT=3	3	.github/workflows/dependabot-repair.yml (at most 3 added and 3 deleted)
- Diff hunks, verbatim (lines 13, 14 and 51 only):

```
@@ -10,8 +10,8 @@ name: dependabot-repair
-# Credential: a GitHub App installation token minted from DEPENDABOT_REPAIR_APP_ID and
-# DEPENDABOT_REPAIR_APP_PRIVATE_KEY. When those secrets are absent the token step fails, the job
+# Credential: a GitHub App installation token minted from the App's Client ID, stored in
+# DEPENDABOT_REPAIR_APP_ID, and the private key in DEPENDABOT_REPAIR_APP_PRIVATE_KEY. When those secrets are absent the token step fails, the job
@@ -48,7 +48,7 @@ jobs:
-          app-id: ${{ secrets.DEPENDABOT_REPAIR_APP_ID }}
+          client-id: ${{ secrets.DEPENDABOT_REPAIR_APP_ID }}
```
