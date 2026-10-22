# P2-T19 — Maintainer follow-up record (issue 929 Summary item 4)

Timestamp: 2026-09-30T11-18
Command: pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; "DEFERRED_UNCHECKED=" + @(Select-String -Path "docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/spec.md" -Pattern "^- \[ \] \*\*AC(18|19|20) ").Count'
EXIT_CODE: 0
Output Summary:
- DEFERRED_UNCHECKED=3 (AC18, AC19 and AC20 remain unchecked in the 911 spec; this plan changed nothing there)

This item is not an acceptance criterion and not a merge gate.

- Acceptance criteria AC18, AC19 and AC20 of issue 911 remain deferred to the maintainer until a GitHub App credential is provisioned. Nothing in this change marks them passed.
- The secret store remains unconfirmed: whether the Dependabot-triggered workflow_run that runs .github/workflows/dependabot-repair.yml reads repository Actions secrets or only Dependabot secrets is for the maintainer to confirm.
- After this change merges, the secret named DEPENDABOT_REPAIR_APP_ID must hold the App's Client ID, not the numeric App ID: the workflow now passes that secret as the client-id input of actions/create-github-app-token. The private key stays in DEPENDABOT_REPAIR_APP_PRIVATE_KEY.
- Runbook: docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/runbooks/github-app-installation-token.runbook.md (Part B step 10 and Part D step 22 now instruct storing the Client ID).
- Spec: docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/spec.md (AC18 to AC20).
