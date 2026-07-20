---
name: merged-child-worktree-still-locked-defer-removal
description: A cleanly-merged child's worktree may refuse non-force removal for TWO reasons — framework lock (shared session pid) OR uncommitted files (untracked evidence/agent-memory); both exit 128 — defer, never force autonomously mid-wave. Locks release PIECEMEAL so retry every deferral each resume; a child may own a named-feature worktree + a secondary agent worktree
metadata:
  type: feedback
---

When a child completes and its PR merges into the integration branch, a gated non-force
`git worktree remove <path>` can still fail with exit 128 for either of two distinct reasons:

1. **Framework lock.** `git worktree list --porcelain` shows
   `locked claude agent agent-<id> (pid <N>)`. That pid is the live parent session process, SHARED
   across every running sibling worktree, so the lock's presence does not prove the child is
   running — but the framework has not released the worktree. Error:
   `fatal: cannot remove a locked working tree ... use 'remove -f -f' to override or unlock first`.
2. **Uncommitted files (observed with #369, agent-a01d6eefe1f9bff5a).** The worktree is UNLOCKED
   (child released its lock) but the tree still holds modified/untracked content — typically child
   `.claude/agent-memory` writes plus untracked coverage-evidence cobertura XMLs under the feature
   folder that were never committed into the merged PR. Error:
   `fatal: '<path>' contains modified or untracked files, use --force to delete it`.

Rule: do NOT `remove -f -f` / `--force` autonomously in either case. Record `merge_status:
"merged"` (with PR number, `pr_url`, `merge_commit_sha`, `merge_confirmed_at`) and a
`worktree_removal_deferred` note naming which of the two causes applies; leave `merge_status` at
`merged`.

**Why:** `merge_status: "merged"` already satisfies both the wave barrier (accepts
`{merged, worktree_removed}`) and the epic completion requirements. Worktree removal is optional
cleanup, not a gate. Force in the lock case risks disturbing shared framework state for live
siblings; force in the uncommitted-files case discards child agent-memory and coverage evidence
that was never committed. Neither has a completion benefit. Consistent with
[[feedback_hung_child_recovery_blocked_by_removal_gate]] — avoid risky worktree operations mid-wave.

**How to apply:** On any epic resume that records a child merge, attempt the gated non-force
`git worktree remove`; inspect the exit-128 message to classify the cause (locked vs. uncommitted
files — run `git -C <path> status --short` for the latter), record the deferral with the cause, and
move on. Reclaim only when the blocker clears: for the lock case, after framework release
(session/agent teardown); for the uncommitted-files case, after the user authorizes `--force` or the
content is committed/confirmed disposable. Then set `worktree_removed` + `worktree_removed_at`.

**Locks release PIECEMEAL — always retry every deferral each resume (observed 2026-07-19, #372).**
Even while pid 34848 stays alive with several siblings still locked, an individual merged child's
lock CAN release once that child agent tears down, and its non-force removal then succeeds (exit 0).
At the #365/#372-completion resume the #372 agent worktree (agent-a7a2a151417dd0bc4) removed cleanly
while #367/#368/#370/#371/#374 under the same pid stayed locked. So do not assume "pid still alive =
all locks held"; retry the non-force removal on EVERY prior deferral each resume and record which
cleared.

**A child may own TWO worktrees: a named-feature worktree + a secondary agent worktree (observed
with #365).** #365 ran in a NAMED worktree (`...-wt/utilitiescs-nullable-outlook-folder-store-365`,
carrying the feature branch, unlocked) PLUS a secondary `agent-<id>` worktree used for PR artifacts
(framework-locked). The named feature worktree removed cleanly; the secondary agent worktree stayed
locked. Set `merge_status: worktree_removed` once the PRIMARY feature worktree (the one carrying the
child branch) is gone, and record the residual locked secondary agent worktree as a separate
optional-cleanup deferral (e.g. a `secondary_agent_worktree_removal_deferred` note) rather than
holding the whole feature at `merged`.
