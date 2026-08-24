---
name: project-epic-child-prs-no-ci
description: Epic child-feature PRs targeting the integration branch run zero CI checks by design; CI runs only at the integration->main PR
metadata:
  type: project
---

In this repo's epic model, child-feature PRs target the epic integration branch (e.g. `epic/store-lockup-resilience-integration`), and `.github/workflows/ci.yml` triggers only on `pull_request: branches: [main, development]`. So a child PR into the integration branch reports zero checks (`statusCheckRollup: []`, `gh pr checks` says "no checks reported"); this is by design, not a misconfiguration.

**Why:** CI is consolidated at the eventual integration->main PR (the epic-orchestrator's gate), avoiding N redundant CI runs across parallel child worktrees. Confirmed 2026-07-08 (#262 / PR #274 -> integration).

**How to apply:** For an epic child at the S9 CI gate, do not block waiting for checks that will never appear. Treat "CI-green" as vacuously satisfied when (a) the base is the integration branch, (b) ci.yml does not trigger on it, and (c) the PR is MERGEABLE/CLEAN with blocking_count==0. Merge with `gh pr merge <n> --merge` and record `epic_merge`. Any CI-relevant concern (e.g. the LiveOutlook `TestCategory` filter observation on ci.yml) is deferred to the integration->main gate, not this PR. `gh pr checks --watch` exits immediately (exit 0) when no checks are configured — do not misread that as green required checks.

## Corollary: once the epic is finished, retarget `main`

The zero-checks rule holds only while the integration branch is *live*. When the epic has already
completed and its integration branch has been merged, that branch is **spent**, and a leftover
child must not target it.

Test it directly: `git merge-base --is-ancestor origin/<integration> origin/main`. If the
integration branch is a strict ancestor of `main` (and `git log <integration>..main` is non-empty
while `git log main..<integration>` is empty), it is fully merged.

On #511 the integration branch was 7 commits behind `main` and already merged by PR #595. Targeting
it would have produced an 85-file diff carrying 7 unrelated `main` commits into a branch nothing
merges from, and **zero CI checks**. Retargeting `main` gave a 100-file additions-only diff and five
real required checks (actionlint, format-check, build-analyzers, build-nullable, mstest-coverage),
all of which passed.

When you retarget: set `epic_mode: false` with the rationale recorded (there is no live integration
branch to merge into, so the epic-mode merge-on-green gate no longer applies), flip
`ci_gate.applicable` to `true`, and verify the diff is additions-only
(`git diff --name-status origin/main..HEAD` showing no `D` or `R` rows) before pushing — a branch
cut from an older base silently deletes what `main` gained meanwhile.
