# P2-T17 — AC6 check-off

Timestamp: 2026-09-30T11-16
Command: Read the cited artifacts; Edit issue.md changing `- [ ] AC6:` to `- [x] AC6:`
EXIT_CODE: 0
Output Summary:
- Evidence read:
  - docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/qa-gates/p1-t8-runbook-client-id.2026-09-28T20-01.md — RB_APPID=0, RB_CLIENTID=1, RB_SECRET=2, RB_PARTB=0, PARTD_CLIENTID=2
  - docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/qa-gates/p1-t9-readme-client-id.2026-09-28T20-01.md — README row updated (README_NUMERIC=0, README_CLIENTID=1)
  - docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/evidence/regression-testing/p1-t11-tree-test-pass-after.2026-09-28T20-01.md — the runbook secret-name test passing; it extracts the secret name from the workflow's client-id line and asserts it in the runbook sample and in Part D
- Decision D3: the secret name DEPENDABOT_REPAIR_APP_ID is kept and now holds the App's Client ID; the workflow passes it as client-id. Renaming would require maintainer credential action; the secret is not yet provisioned, so the change is merge-safe without maintainer action.
- Change to issue.md: only the AC6 checkbox.
