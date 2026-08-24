---
name: feedback-commit-review-artifacts-and-step8-preflight
description: feature-review writes audit artifacts AFTER the pre-review commit, so commit them again before rebase/PR; and the PR-creation preflight requires step8_status non-pending
metadata:
  type: feedback
---

Two process facts confirmed on issue #270 / PR #272 (small-path bug, rebase-then-merge flow):

1. **feature-review's own artifacts are uncommitted after the pre-review commit.** The pre-feature-review `git add -A && commit` runs BEFORE the review, so the `policy-audit`/`code-review`/`feature-audit` artifacts the review then writes (plus any review-agent memory edits) are left uncommitted. Stage and commit them (a `docs(review):` commit) BEFORE rebasing/pushing, or the rebase aborts with "cannot rebase: You have unstaged changes" and the audit trail is incomplete on the branch. **Why:** review runs after the commit that was supposed to capture everything. **How to apply:** after feature-review returns, run `git status` and commit the new audit artifacts before the rebase/PR step. Aligns with [[evidence-and-lifecycle-for-every-change]] and commit-all-evidence.

2. **PR-creation-ready preflight requires `step8_status` to be non-pending.** `Invoke-OrchestratorStatePreflight` (the `--require-pr-creation-ready` equivalent in `.claude/lib/orchestrator-state/OrchestratorState.psm1`) fails with "step8_status is pending" even when everything else is ready. Set `step8_status` to `delegated` (PR authoring underway) BEFORE running the preflight; flip to `verified` after `gh pr create` succeeds. Steps 5-8 must all be non-pending/non-blocked. **Why:** step8 is the PR-creation step; the gate treats a pending step8 as "not started." See [[pr-author-hook-blocks-gh-in-this-repo]] for the in-thread pr-author body+SHA-256 receipt flow (Agent(pr-author) is still not a registered agent type here).

3. **`git fetch origin main:main` is refused in a linked worktree.** `main` is checked out in the primary worktree (`<repo-root>`), so the fetch-into-checked-out-branch is refused. Achieve the same intent with `git fetch origin` (updates `origin/main`) then `git -C <primary-worktree> merge --ff-only origin/main`.
