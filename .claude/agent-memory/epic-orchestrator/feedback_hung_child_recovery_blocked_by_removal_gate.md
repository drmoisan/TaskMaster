---
name: hung-child-recovery-blocked-by-removal-gate
description: On resume, a stalled/orphaned wave child has no clean autonomous recovery — the worktree-removal gate blocks reset and orphaned background agents can't be re-attached; halt and report
metadata:
  type: feedback
---

When resuming an epic and a wave child is found orphaned/hung (prior-session background
Agent(orchestrator) that stalled — e.g. reached Phase 0 baseline then no file writes for
hours, no commits, no PR), there is no clean autonomous recovery path. Halt and report to
the user with precise durable state; do not improvise destructive cleanup.

**Why:** Three constraints compose into a dead end:
1. Orphaned background children launched in a *prior* session are not children of the
   current conversation, so they cannot be re-attached, waited on, or messaged.
2. `enforce-epic-worktree-removal-gate.ps1` denies `git worktree remove` unless the
   matching `features[]` record has `merge_status` in {merged, worktree_removed}. A stalled
   child is still `worktree_created`, so nuke-and-recreate is blocked by design.
3. The canonical feature branch (e.g. `feature/<slug>-<issue>`) is still checked out in the
   stale worktree, so a fresh `isolation:"worktree"` re-delegation collides on branch
   setup; and the branch can't be deleted while checked out in a worktree I can't remove.
The gate's fail-closed design is a deliberate signal that destroying an unmerged child's
worktree is not sanctioned.

**How to apply:** On resume, re-derive durable truth first (git worktree list --porcelain,
git branch, gh pr view, plus child mtime/idle check and child orchestrator-state.json
next_step). If a wave child is stalled with no commits/PR, record the finding in the epic
checkpoint (session-local, gitignored) and stop blocked. Recovery requires human
intervention: release the stale worktrees/branches (outside my gated tools) or revive the
orphaned runs. Never run child lifecycle inline (see [[inline-child-lifecycle-prohibited]]),
never falsify `merge_status` to bypass the gate, and do not kill sibling `claude.exe`
processes — a shared machine routinely has several live. See also
[[orchestrator-subagent-not-registered]] for verifying spawn/liveness per run.

**Confirmed recovery path (folder-tree-percentage-ui epic, 2026-07-16):** The blocked-stop
worked as intended. Between sessions the maintainer released the two stalled wave-0
worktrees and deleted their feature branches. On the next resume, durable re-derivation
showed the worktrees pruned, the child branches gone locally+remotely, integration tip
unchanged, and no child PR. The correct action was NOT to stay blocked: reset the affected
features to `not_started` (clear the dead worktree_path/worktree_created_at) to match durable
truth, then cleanly re-delegate the wave via `Agent(orchestrator, isolation:"worktree")`.
Lesson: the blocked state is a checkpoint, not a terminal state — always re-derive on the
next run and clear the block when the external release has happened.

**Second confirmed stall, same epic, wave 1 (2026-07-16T12:10Z):** Feature 325 (PR #335) —
the last unmerged wave-1 child — stalled a different way. Its child (pid 14780) finished
implementation + feature-review, opened PR #335, then on fan-in ran
`git merge --no-commit origin/<integration>` and hit ONE conflict
(`UU UtilitiesCS/UtilitiesCS.csproj` — 325 and 327 both add files to the same .csproj; classic
fan-in conflict when a sibling merges first). It wrote `remediation-inputs`/`remediation-plan`,
set its own `next_step=remediation.cycle_1.execute`, then went silent ~6h with MERGE_HEAD still
present and the one UU path unresolved. Blocked-stopped per this memory; same recommended
maintainer action (release pid 14780 + worktree).

**Key discriminator (refines [[live-child-at-pr-author-not-hung]]):** a stall at an *execute*
step (`remediation.cycle_N.execute`, atomic-executor actively editing files) is NOT the same as
idle-at-`pr-author` (a CI wait that legitimately writes nothing for many minutes). At an execute
step, zero worktree writes for hours + a mid-flight `MERGE_HEAD` + unresolved `UU` paths is a
genuine stall, not slowness. Confirm via: child's own `orchestrator-state.json` next_step,
`git rev-parse -q --verify MERGE_HEAD` in the child worktree, `git status --porcelain | grep '^UU'`,
and last-write mtime vs now. Do NOT spawn a second orchestrator into a worktree still held by a
live (even if inert) prior-session pid while a merge is mid-flight — concurrent writes to that
index are unrecoverable. Halt, report, recommend release.
