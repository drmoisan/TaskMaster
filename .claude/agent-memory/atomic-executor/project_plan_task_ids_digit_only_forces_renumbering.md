---
name: plan-task-ids-digit-only-forces-renumbering
description: Preflight deltas that insert a task cannot use suffixed IDs (P3-T5a) — the validator requires digit-only sequential IDs, so insertion renumbers all downstream tasks in that phase plus every cross-reference
metadata:
  type: project
---

When a preflight delta asks the planner to **insert** a task mid-phase, do not propose a suffixed ID such as `[P3-T5a]`. `mcp__drm-copilot__validate_orchestration_artifacts` (`artifact_type: "plan"`) and the planner-output hook require digit-only, sequential-per-phase task IDs, so a suffixed ID fails structural validation.

**Why:** #424 cycle 2 — two inserted tasks (`WaitForQueue` test update, gate cancellation tests) each forced a cascade: `P3-T6/T7` → `P3-T7/T8` and the Phase 1 suite run `P1-T8` → `P1-T9`. Every acceptance clause, Decisions Record item, and traceability row pointing at the old numbers had to move with them.

**How to apply:**
- Phrase insertion deltas as "insert as `[P#-T<n>]`, renumber `T<n>`..`T<last>` upward, and update every cross-reference" rather than naming a suffixed ID.
- When re-validating a renumbered plan, verify mechanically, not by reading: enumerate line-start definitions (`^- \[ \] \[P#-T#\]`), check for duplicates, then enumerate *all* `[P#-T#]` mentions and confirm every referenced ID also exists as a definition. Cross-references hide in acceptance clauses, the Decisions Record, the traceability table, and the revision log — not just in task bodies.
- Ordering is a correctness constraint, not cosmetics: an inserted authoring task must land **before** the phase's suite-run task, and a test that exercises a new seam must land **after** the task that implements it.

Related: [[project_418_500line_gate_vs_plan_content]] (the other case where a delta had to be restructured rather than applied literally).
