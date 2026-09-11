---
name: self-anchor-the-diff-base-in-epic-children
description: In an epic or parallel child, never fix a plan's diff base to a commit literal — record HEAD at execution time instead, because any literal fixed at planning time degenerates once siblings fan in
metadata:
  type: feedback
---

A plan's footprint and scope-boundary criteria must NOT anchor `git diff` to a commit literal chosen at planning time. Record the base at execution time instead: a Phase 0 task runs `git rev-parse HEAD` before any implementation edit and writes it into the baseline artifact as `BASE-SHA:`; every later diff task reads that field and transcribes the literal SHA into its own command span.

**Why:** any literal fixed while planning becomes an ancestor of the head the executor actually runs on, so a three-dot diff against it degenerates to two-dot and bills every sibling's delivery to this feature. Observed twice in one run on issue #823: a predecessor re-anchored from `6f08302a` to `c713d2a6` after measuring 11 paths versus 1 — and by the time execution planning resumed, the epic-planner had fanned in sibling preparation, so `c713d2a6...HEAD` listed 21 paths (8 sibling-owned, 10 agent-memory). The same defect recurred one generation later against the "fixed" anchor. It will recur again, because the execution worktree is branched from the integration branch after further fan-ins.

Two consequences to reconcile, or the change is incoherent:
- With `BASE-SHA == HEAD` at capture, `git diff BASE-SHA...HEAD` is EMPTY by construction, so any "inherited paths" clause built on that diff collapses to the `git status --porcelain --untracked-files=all` set alone. Update every place that defines, captures or subtracts the inherited set.
- Transcribing a recorded artifact field is not a shell variable crossing a task boundary, so a "no variable survives between tasks" rule is preserved. Say so explicitly, and cite the same mechanism the plan already uses for a resolved tool path.

**How to apply:** whenever authoring or reviewing a plan for an epic/parallel child, or resuming one. Also treat a predecessor's recorded measurement as stale on sight — re-run the diff yourself before carrying the figure forward. Related: [[merging-main-invalidates-plan-base-anchor]], [[three-dot-diff-degenerates-on-ancestor-base]], [[stale-base-anchor-passes-ancestry-vacuously]].
