---
name: prep-child-upstream-dependency-must-be-nonhalting
description: A preparation child whose wave-0 upstream is still being prepared must plan the upstream as an execution-time read, or preflight/WI-0 halts on a file that legitimately does not exist yet
metadata:
  type: feedback
---

When preparing an epic child that depends on a concurrently-prepared upstream child, instruct the
planner and the preflight executor explicitly that the upstream's artifacts do not exist yet and
that their absence is NOT a defect.

Concretely, put this in both the `atomic-planner` and the `atomic-executor` preflight prompts:
- the upstream artifact path, and that it is delivered by sibling child `<F#>` at execution time;
- that the plan must consume it via a normal Phase 0 execution-time read whose acceptance rests on
  the artifact the task *produces*, not on the upstream file's presence;
- that no task may have an acceptance condition evaluable only if the upstream exists at planning
  or preflight time;
- that the preflight must return `PREFLIGHT: ALL CLEAR` against a worktree where it is absent;
- and the inverse defect to actually flag: a task that *does* gate on upstream presence.

**Why:** without this, the planner writes a dependency gate and the child halts at WI-0 on a file
that is legitimately absent — the failure mode already seen in the Swordfish epic
([[project_swordfish_epic_f5_blocked_on_old_scodictionary]]). Also tell them not to flag the
plan's own outputs (new test files, new partial files, new seams) as missing.

**How to apply:** epic-planner preparation children in wave 1+ where wave 0 is still in flight.
Verified on epic #136 child F7 (issue #433, 2026-08-07): F1's `coverage-ledger.md` and per-file
coverage harness were absent throughout preparation, and preflight returned ALL CLEAR with the
dependency modelled as Phase 0 tasks `[P0-T8]`/`[P0-T9]`.

Related: [[parallel-prep-children-subagent-saturation]],
[[feedback_plan_phase0_paths_are_stale_in_epic_children]].
