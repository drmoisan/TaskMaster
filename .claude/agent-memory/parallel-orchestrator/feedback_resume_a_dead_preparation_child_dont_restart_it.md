---
name: resume-a-dead-preparation-child-dont-restart-it
description: When a /parallel-add preparation child dies, its work is committed on the item branch — detach the stale worktree and re-delegate only the missing preparation step, never restart preparation from promotion
metadata:
  type: feedback
---

A `/parallel-add` preparation child that dies mid-run usually leaves its work COMMITTED on the item
branch, not lost. Before re-delegating, read what survived and scope the resume to the gap.

**Why:** Preparation is expensive (~8 minutes, >100k tokens) and its outputs are durable. Observed
2026-08-29 resuming `/parallel-add 637`: the item sat at `admitted` in the checkpoint and looked
abandoned, but `git log origin/main..<item-branch>` showed one commit,
`wip(637): checkpoint preparation artifacts before session pause`, carrying `issue.md`, `spec.md`,
a 1338-line atomic plan, and the research artifact. Only preflight clearance was missing. Restarting
preparation would have rebuilt all four and produced a second plan file, violating the Plan-Path
Continuity Contract.

**How to apply:**

- **Diagnose from the branch, not the checkpoint.** `git log origin/main..<item-branch> --oneline`
  and `git ls-tree -r --name-only <head> -- <feature-folder>/` tell you exactly which preparation
  outputs exist. The checkpoint's item `state` does not: it says `admitted` whether preparation
  wrote nothing or wrote everything but preflight.
- **A live lock pid does not mean a live child.** The stale worktree carried
  `locked claude agent <id> (pid <n>)` and that pid was a running `claude.exe`, because the lock pid
  is the SESSION, not the subagent. Judge liveness from the worktree's index/HEAD mtimes and from
  the child's own last commit message.
- **Free the branch by detaching the stale worktree**, then let the fresh child check the existing
  branch out by name as its first action. See [[free-item-branches-by-detaching]] — both removal
  gates fail closed, so `git worktree remove` is not the move.
- **Enumerate the done set in the delegation prompt.** Name promotion, research, feature documents,
  and plan authoring as already complete, and state the remaining step as the whole scope. Also
  instruct the child to revise the existing plan IN PLACE, or it will author a timestamped sibling.
- **Tell the child the plan may have gone stale.** The plan cites line numbers against the `main` of
  its authoring time; if `main` advanced, correcting those citations is legitimate preflight work
  rather than scope creep.

See [[defer-the-checkpoint-write-until-admission]] for why the checkpoint stays untouched while the
resumed preparation runs, and [[parallel-run-execution-playbook]].
