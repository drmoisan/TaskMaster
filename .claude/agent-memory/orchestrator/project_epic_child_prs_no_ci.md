---
name: project-epic-child-prs-no-ci
description: Epic child-feature PRs targeting the integration branch run zero CI checks by design; CI runs only at the integration->main PR
metadata:
  type: project
---

In this repo's epic model, child-feature PRs target the epic integration branch (e.g. `epic/store-lockup-resilience-integration`), and `.github/workflows/ci.yml` triggers only on `pull_request: branches: [main, development]`. So a child PR into the integration branch reports zero checks (`statusCheckRollup: []`, `gh pr checks` says "no checks reported"); this is by design, not a misconfiguration.

**Why:** CI is consolidated at the eventual integration->main PR (the epic-orchestrator's gate), avoiding N redundant CI runs across parallel child worktrees. Confirmed 2026-07-08 (#262 / PR #274 -> integration).

**How to apply:** For an epic child at the S9 CI gate, do not block waiting for checks that will never appear. Treat "CI-green" as vacuously satisfied when (a) the base is the integration branch, (b) ci.yml does not trigger on it, and (c) the PR is MERGEABLE/CLEAN with blocking_count==0. Merge with `gh pr merge <n> --merge` and record `epic_merge`. Any CI-relevant concern (e.g. the LiveOutlook `TestCategory` filter observation on ci.yml) is deferred to the integration->main gate, not this PR. `gh pr checks --watch` exits immediately (exit 0) when no checks are configured — do not misread that as green required checks.
