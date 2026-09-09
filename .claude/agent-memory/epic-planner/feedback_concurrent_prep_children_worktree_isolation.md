---
name: concurrent-prep-children-worktree-isolation
description: Concurrent preparation-mode children each need isolation:worktree; with real worktree isolation they should keep the CANONICAL orchestrator-state.json path, because the repository hooks read only that path
metadata:
  type: feedback
---

When launching multiple preparation-mode `Agent(orchestrator)` children concurrently, give each
`isolation: "worktree"` and `run_in_background: true`.

**Corrected 2026-09-08 — do NOT ask an isolated child for a child-scoped checkpoint filename.**
`artifacts/` is gitignored and each isolated worktree has its own copy of the tree, so the
canonical `artifacts/orchestration/orchestrator-state.json` is already per-child and siblings
cannot collide through it. Several repository hooks read that literal path and nothing else:
`enforce-orchestration-preimplementation-gate.ps1` gates every `git add`/`git commit` on
`Test-OrchestrationReady` reading it, and `enforce-model-routing-receipt.ps1` reads it for
receipt presence. A child told to write `orchestrator-state.<slug>.json` therefore has a
permanently unready gate and cannot commit. Instruct each child to seed the canonical file
immediately after `new_active_feature_folder` returns, with `issue-num`, a `feature-folder` under
`docs/features/active/`, `route_id`, and `lifecycle_ready: true`.

The child-scoped-path advice below applies ONLY to children launched without worktree isolation,
which is itself the thing to avoid.

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
