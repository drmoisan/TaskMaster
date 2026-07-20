---
name: concurrent-prep-children-worktree-isolation
description: Concurrent preparation-mode children must each get isolation:worktree AND a child-scoped checkpoint path, or they overwrite each other's canonical orchestrator-state.json
metadata:
  type: feedback
---

When launching multiple preparation-mode `Agent(orchestrator)` children concurrently, give each
`isolation: "worktree"` AND instruct each to persist its checkpoint to a child-scoped path
`artifacts/orchestration/orchestrator-state.<slug>.json`, never the shared canonical
`artifacts/orchestration/orchestrator-state.json`.

**Why:** In the utilitiescs-nullable-remediation epic, the two canary children (extensions #363,
helperclasses #364) were launched WITHOUT per-child worktree isolation. They ran concurrently in
the SAME session worktree and git index; the canonical `orchestrator-state.json` was repeatedly
overwritten by whichever sibling wrote last, and each child fell back to a child-scoped gitignored
checkpoint on its own. A shared index also risks cross-contaminated staging when children commit.
At 8-10 way concurrency this collision would be severe.

**How to apply:** Every concurrent preparation delegation call uses `isolation: "worktree"` and
`run_in_background: true`. In each delegation prompt include an explicit CONCURRENCY ISOLATION
paragraph: child-scoped checkpoint path, do-not-touch the canonical shared checkpoint, operate only
within its own worktree and only on its own `docs/features/active/<slug>/` tree, commit with
explicit pathspecs. Respect the checkpoint's `max_parallel_features` cap (8 here) by batching:
launch cap-sized batches, launch the remainder as earlier children complete. Fan-in is a clean
disjoint fast-forward/merge because each child's tree is disjoint.
Related: [[epic-planner-state-required-fields]], [[epic-plan-tooling-not-vendored]].
