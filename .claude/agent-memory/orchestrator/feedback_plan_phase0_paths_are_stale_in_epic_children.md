---
name: plan-phase0-paths-are-stale-in-epic-children
description: Epic-child atomic plans cite the planning worktree's absolute paths in Phase 0 policy-read tasks; redirect the executor to the CURRENT worktree's files
metadata:
  type: feedback
---

Epic-child atomic plans are authored during epic planning in a different (planning-time) git worktree. Their Phase 0 policy-read tasks (P0-T1..T4) hard-code that worktree's absolute paths, e.g. `C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-07-13-21\CLAUDE.md`, which do not exist when the feature is later executed in a fresh session worktree.

**Why:** the plan is frozen at planning time; the execution worktree is created later with a different `agent-<hash>` path. An executor that reads the cited paths verbatim fails P0 and can wrongly report BLOCKED.

**How to apply:** when delegating an epic-child atomic plan to `atomic-executor`, explicitly override the stale Phase 0 paths in the delegation prompt — point CLAUDE.md and the `.claude/rules/*.md` reads at the CURRENT working-directory worktree, and tell the executor to record the actual paths read in the P0-T5 evidence artifact. This is separate from the evidence-output path rule (evidence still goes to `<FEATURE>/evidence/<kind>/`). Confirmed on #265 (F5, store-lockup-resilience epic): plan cited `TaskMaster-wt-2026-07-07-13-21`; execution ran in `agent-aa788d7e018d8924e`. See also [[pr-context-summary-unreliable-gh-and-classification]] (collect_pr_context writing to the main checkout is the same class of worktree-path drift).
