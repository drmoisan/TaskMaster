---
name: preparation-mode-plans-need-repo-relative-paths
description: A preparation-mode epic-child plan is executed later in a DIFFERENT worktree, so every path it cites must be repo-relative; tell the planner explicitly or it emits stale absolute worktree paths
metadata:
  type: feedback
---

When running `route_id: preparation` for an epic child, state in the planner delegation prompt that the plan is executed later, by `epic-orchestrator`, in a different checkout — and that **every path in the plan must be repo-relative**. Forbid absolute paths containing `C:\Users\...` or `.claude\worktrees\...` anywhere, including Phase 0 policy-read tasks.

**Why:** Preparation happens in an isolated agent worktree; execution happens elsewhere, after the upstream dependency child has merged to the integration branch. Any absolute path baked in at planning time is stale at execution time. This is the same failure mode already recorded in [[feedback_plan_phase0_paths_are_stale_in_epic_children]], but caught at the planning end rather than patched at the execution end. On #430 (epic #136 child F3, 2026-08-07) the instruction was given up front and the resulting 236-task plan had zero absolute paths — the executor preflight verified this explicitly.

**How to apply:** Include in every preparation-mode planner prompt: (1) plan is executed later in a different worktree, (2) all paths repo-relative, (3) the upstream child's outputs do not exist on disk yet and their absence is NOT a preflight failure — the plan must *defer* to them rather than assume their content. Then tell the preflight executor the same thing, so it validates the deferral instead of reporting the missing upstream artifacts as blocking prerequisites. Also relay the two-round expectation: budget for a `PREFLIGHT: REVISIONS REQUIRED` round, apply the delta via the planner (not yourself), then re-run both the MCP validator and preflight.
