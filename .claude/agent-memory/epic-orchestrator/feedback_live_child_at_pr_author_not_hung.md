---
name: live-child-at-pr-author-not-hung
description: A live child idle at next_step=pr-author is NOT necessarily hung; re-derive remote branch/PR truth (esp. on a push rejection) before declaring a stall — the child may complete minutes later
metadata:
  type: feedback
---

A wave child whose own orchestrator-state.json shows `next_step: pr-author` with an alive
process and stale file mtimes is NOT reliably a stall. On the folder-tree-percentage-ui epic
(2026-07-16), I observed feature 324's child (pid 14780) idle at pr-author with no worktree
file writes for ~50+ minutes while its co-launched sibling 326 had fully merged. I nearly
recorded a blocking-stall finding (per [[hung-child-recovery-blocked-by-removal-gate]]). Before
halting, a routine `git push` of the epic-status doc was REJECTED (remote advanced) — re-deriving
durable truth revealed the 324 child had, in the intervening minutes, pushed its branch and
opened + merged PR #333 into the integration branch. It was slow, not hung.

**Why:** The pr-author step (collect_pr_context, author body + provenance receipt, gh pr create,
wait for CI, gh pr merge) can take many minutes and writes little/nothing to the worktree during
the CI wait, so file-mtime idleness and a frozen `last_updated` are weak stall signals. Local
worktree state is a lagging cache; the authoritative signal is the remote branch/PR state.

**How to apply:** Before declaring any wave child stalled/hung, re-derive REMOTE durable truth:
`git ls-remote origin refs/heads/<child-branch>`, `gh pr list --state all --search "<slug> in:title"`,
and the integration branch remote tip. A push rejection on the integration branch is itself a
signal that a child just landed work — always fetch + re-derive on rejection, never assume.
Only after the child branch is absent from origin AND no PR exists AND the integration tip is
unchanged over a sustained window should the dead-end recovery in
[[hung-child-recovery-blocked-by-removal-gate]] be considered. Never kill the process, never
falsify merge_status, never force-remove a locked worktree. When a merged child's process is
still alive and holding its worktree lock, defer `git worktree remove` (record
worktree_removed_at: null with a note) — it does not block the next wave.
