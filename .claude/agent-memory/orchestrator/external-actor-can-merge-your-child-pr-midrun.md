---
name: external-actor-can-merge-your-child-pr-midrun
description: A human/external actor can merge your epic-child PR while your toolchain pass runs; re-read PR state before the CI gate, prove tree equality, and land stranded evidence via a docs-only follow-up PR
metadata:
  type: feedback
---

Before dispatching the CI gate or assuming you will perform the merge, re-read the PR's live state
with `gh pr view <N> --json state,mergedAt,mergeCommit`. The repository owner can merge an epic-child
PR at any moment, including while your post-base-merge toolchain pass is still executing.

**Why:** On child 501 the parent handed off "base re-merge, fresh CI gate, merge" with PR #659 OPEN
and MERGEABLE. Between the local base merge and the CI dispatch, the owner merged #659 as `4cb709db`.
Discovering this only at `git push` time (the branch showed `1 behind / 2 ahead` and the single
base-only commit was literally `Merge pull request #659`) is late but recoverable; discovering it
after opening a duplicate PR is not.

**How to apply:**

- When the base tip moves, always `git log --oneline origin/<base> ^HEAD` before re-merging. A single
  base-only commit whose subject names YOUR pr number means the merge already happened.
- Determine whether your reconciliation is already represented by comparing TREES, not commits:
  `git rev-parse <their-merge>^{tree}` vs `git rev-parse <your-merge>^{tree}`. A merge of A into B and
  a merge of B into A produce the same tree when neither side conflicted, so an externally merged
  commit can be provably equivalent to the reconciliation you just validated locally. That equality
  is what lets you claim your local toolchain pass gates the merged tree.
- Evidence you committed after the external merge is stranded on the branch. Land it with a
  **docs-only follow-up PR** into the same integration base. That also restores
  `git merge-base --is-ancestor HEAD origin/<base>`, which the epic worktree-removal gate needs; a
  branch left ahead of the integration tip can block the parent later.
- Reuse the same `artifacts/pr_body_<issue>.md` + `.receipt.json` pair with a FRESH sha256 and a fresh
  `created_at`. See [[pr-author-receipt-staleness-is-mtime-vs-created-at]] and
  [[pr-author-hook-blocks-gh-in-this-repo]]; the readiness preflight itself only checks that
  step5-8 are not pending/blocked, `blocked_reason` is `none`, and `local_execution_overrides` /
  `delegation_bypasses` are empty or absent.
- Record the external merge honestly in the checkpoint (who merged, when, and that you did not gate
  it beforehand) rather than presenting it as your own merge.
