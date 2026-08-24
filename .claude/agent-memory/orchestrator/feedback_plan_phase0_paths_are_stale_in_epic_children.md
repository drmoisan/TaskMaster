---
name: plan-phase0-paths-are-stale-in-epic-children
description: Epic-child atomic plans cite the planning worktree's absolute paths; redirect the executor at execution time, but fix it in the PLAN at preparation time
metadata:
  type: feedback
---

Epic-child atomic plans are authored during epic planning in a different (planning-time) git worktree. Their Phase 0 policy-read tasks (P0-T1..T4) hard-code that worktree's absolute paths, e.g. `<repo-root>-wt-2026-07-07-13-21\CLAUDE.md`, which do not exist when the feature is later executed in a fresh session worktree.

**Why:** the plan is frozen at planning time; the execution worktree is created later with a different `agent-<hash>` path. An executor that reads the cited paths verbatim fails P0 and can wrongly report BLOCKED.

**How to apply:** when delegating an epic-child atomic plan to `atomic-executor`, explicitly override the stale Phase 0 paths in the delegation prompt — point CLAUDE.md and the `.claude/rules/*.md` reads at the CURRENT working-directory worktree, and tell the executor to record the actual paths read in the P0-T5 evidence artifact. This is separate from the evidence-output path rule (evidence still goes to `<FEATURE>/evidence/<kind>/`). Confirmed on #265 (F5, store-lockup-resilience epic): plan cited `TaskMaster-wt-2026-07-07-13-21`; execution ran in `agent-aa788d7e018d8924e`. See also [[pr-context-summary-unreliable-gh-and-classification]] (collect_pr_context writing to the main checkout is the same class of worktree-path drift).

**In PREPARATION mode, fix it in the plan instead.** The override above works only when you already hold the execution worktree. Under `route_id: preparation` there is none — `epic-orchestrator` creates a THIRD worktree later — so no absolute literal is correct and "redirect the executor" has no target. Have `atomic-planner` replace the pinned path with an instruction to resolve the workspace root at execution time from `git rev-parse --show-toplevel`. Confirmed on #445 (quickfiler-suite-determinism-foundation): the plan pinned `agent-aa16be3c847acea9b` inside a block headed "verified; use these, do not re-derive", and preflight raised it as BLOCKING precisely because that header forbids the one micro-action that would repair the value. The distinction is the severity rule: a stale path an executor may silently re-derive is advisory; a stale path under a do-not-re-derive instruction is blocking. See also [[preflight-catches-vacuous-gates]].
