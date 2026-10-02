---
name: project-952-r3-shared-origin-main-ancestry-gate-seam
description: Issue 952 preflight round 3 seam - a no-fetch plan anchored to origin/main still needs a P0 merge-base ancestry gate because the ref is shared by every worktree; also the sibling header prose "the ref does not move because this plan never fetches" is false in a multi-worktree repo
metadata:
  type: project
---

Round 3 of #952 found one blocking defect: P0 recorded `BASE-SHA` (origin/main) and `HEAD-SHA` but never checked that origin/main is an ancestor of HEAD.

**Why:** `refs/remotes/origin/main` is one ref shared by every worktree of the repo. A fetch in any other session moves it, and a plan that runs later than it was authored can therefore start with origin/main already AHEAD of the branch merge-base. Every two-dot gate (`git diff origin/main -- path` and the whole-tree `git diff --name-only origin/main`) then lists main's post-branch-point changes as if the run made them. Neither the frozen `COMMITTED` list (three-dot `origin/main...HEAD`, which is empty in that direction) nor `INHERITED` (baseline porcelain) subtracts them, so the footprint `OUTSIDE-COUNT` ends greater than 0 with no stop label, and a no-commit / no-merge / no-rebase plan has no repair path. At authoring time HEAD equalled origin/main, so the defect was latent.

**How to apply:**
- In any plan anchored to `origin/main` that does not itself fetch, add `MERGE-BASE=$(git merge-base origin/main HEAD)` to the P0 base-ref payload and gate `MERGE-BASE equals BASE-SHA` with a named stop label (`BASE AHEAD OF BRANCH`), telling the operator to bring the branch up to origin/main OUTSIDE the plan and restart at P0-T1. Pair it with the existing late-phase `BASE REF MOVED` (re-read `git rev-parse origin/main` and compare to the P0 value): the P0 gate covers "already ahead at start", the late gate covers "moved during the run".
- Do not write "this plan never runs `git fetch`, so the ref does not move during the run" in the header: the first clause is true and the conclusion is false in a multi-worktree checkout. State the two stop labels instead. This sibling sentence survived two preflight rounds as "clean" because no round had yet named the shared-ref mechanism.
- Never assert "HEAD equals origin/main" as a plan invariant; it is a preparation-run observation that the executor re-derives.
- The merge-base payload segment is a plain double-quoted string with a `$( )` subexpression: no pipe, no apostrophe, no nested empty string, so it fits the single-quoted `-Command` channel unchanged.

Related: [[project-952-r2-label-counts-helper-enumeration-and-hex-backslash-seams]], [[project-952-r1-read-phantom-row-and-frozen-clause-a-seams]], [[never-pin-head-sha-as-plan-expectation]], [[harness-git-status-may-describe-another-worktree]].
