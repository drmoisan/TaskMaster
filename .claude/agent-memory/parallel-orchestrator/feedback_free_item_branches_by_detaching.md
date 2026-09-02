---
name: free-item-branches-by-detaching
description: Stale planner-preparation worktrees hold the item branches checked out; detach their HEAD rather than removing them, because the worktree-removal gates fail closed on every removal
metadata:
  type: feedback
---

When a parallel run reaches execution, the planner's per-item preparation worktrees are
usually still present and still have the item feature branches checked out. Git refuses a
second checkout of the same branch, so every child execution worktree fails at
`git checkout <item-branch>` until the branch is freed. Free it with `git checkout --detach`
inside the stale preparation worktree — never with `git worktree remove`.

**Why:** Two PreToolUse hooks fire on `git worktree remove` and denials are conjunctive, so
both must allow. `.claude/hooks/enforce-parallel-worktree-removal-gate.ps1` and
`.claude/hooks/enforce-epic-worktree-removal-gate.ps1` each authorize a removal ONLY from a
checkpoint record whose `worktree_path` matches the target and whose `merge_status` is
`merged` or `worktree_removed`. A preparation worktree is in no `items[]` record at all, so
both gates fail closed with `PARALLEL_WORKTREE_REMOVAL_BLOCKED` /
`EPIC_WORKTREE_REMOVAL_BLOCKED`. Detaching is not gated, loses nothing when the worktree is
clean, and leaves the pushed branch untouched.

**How to apply:** Before launching a cohort batch, run `git worktree list --porcelain` and
look for `refs/heads/<item-branch>` on a worktree that is not one of yours. Confirm the
worktree is idle before detaching — a lock naming a live pid is often THIS session's own
finished preparation subagent, so pid liveness alone proves nothing. Check that
`git status --porcelain` is empty and that the worktree's `.git/worktrees/<id>/index` mtime
is stale across two samples. Then detach. Verify with `git branch --list <branch>`: the `+`
prefix disappears when the branch is free. See [[parallel-run-execution-playbook]].
