---
name: parent-session-can-commit-into-child-worktree
description: A delegating parent session can keep writing AND committing into the child's worktree and branch mid-run — re-read HEAD and the reflog before assuming your working tree is the only state
metadata:
  type: feedback
---

When resumed in place in an existing worktree, do not assume you are the only writer, even after the parent asserts the worktree is idle. On epic child 442 (2026-08-27) the parent epic-orchestrator, having delegated a resume-in-place, went on to:

- launch a coverage-enabled `vstest` run into the child worktree ~7 minutes before the resume began, which then **hung** and blocked the child's own toolchain;
- write its own parallel set of Phase 6 gate artifacts into the child's feature folder, four minutes after the child wrote its set;
- **commit** those artifacts onto the child's branch (`e7b74e35`), silently moving `HEAD` from `c1826965` while the child was mid-Phase-7.

The commit was benign — purely additive, touching neither `plan.md` nor `spec.md`, so nothing uncommitted was displaced — but that was luck, not design. It was only detected because `git diff --name-only <base>` began listing files that `git status` had reported as untracked minutes earlier: `git diff <sha>` only reports tracked files, so a path moving from `??` into the diff output means **someone committed it**.

**Why:** two writers on one branch can interleave. Had the parent's commit touched a file the child held modified in its working tree, the child's later `git add` of its own version would have silently reverted the parent's hunks with no conflict — the same failure mode as [[stale-base-deletes-silently-on-fan-in]], but inside a single worktree.

**How to apply:**
- Treat a path that disappears from `git status --porcelain` without your having staged it as evidence of a concurrent commit. Confirm with `git rev-parse HEAD` and `git reflog -6`, then `git show --name-status <new-sha>` to check whether it touched anything you hold modified.
- Re-read `HEAD` before every commit, push, and `gh pr create`, not just at resume.
- Do not delete or "clean up" the other writer's artifacts. Retain both sets and write a short reconciliation artifact naming which set is authoritative and tabulating where the two agree and differ — duplicate evidence with no explanation is worse for an auditor than either set alone.
- Their artifacts may carry defects yours do not: the parent's set labelled its filenames with **local** time (`10-23` for a `14:23Z` write) and attributed the analyzer gate to `[P6-T2]` and nullable to `[P6-T3]`, both off by one against the plan. Prefer your own set for plan reconciliation and say why.

See [[one-executor-per-worktree]] for the related rule about not *launching* a second worker, and [[feedback_stale_checkpoint_is_not_a_dead_agent]].
