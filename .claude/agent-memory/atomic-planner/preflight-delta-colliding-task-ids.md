---
name: preflight-delta-colliding-task-ids
description: When two preflight findings in the same delta pin the same task ID (one reorders a task into it, another quotes it in literal replacement text), keep the ID cited by the literal text and place the reordered task adjacent instead
metadata:
  type: feedback
---

When an `atomic-executor` preflight delta contains two findings that both claim the same task ID — typically one finding says "renumber X so it becomes `[Pn-Tk]`" while another supplies **literal replacement text** that cites `[Pn-Tk]` as a different task — do not pick one and drop the other. Satisfy both by keeping `[Pn-Tk]` assigned to the task the literal text cites, placing the reordered task in the adjacent slot, and reporting the deviation explicitly in the response.

**Why:** Preflight deltas are written finding-by-finding against the *pre-revision* numbering, so a reorder proposed in one finding silently invalidates ID references in another. In #455 F13, B2 said "current `[P4-T10]` becomes `[P4-T3]`" while B3 supplied quoted acceptance text for `[P4-T2]` ending "...until `[P4-T3]`", where `[P4-T3]` meant the csproj task. Both findings' *defects* were real; only the ID assignment collided. Placing apply-ruling at `[P4-T4]` and leaving csproj at `[P4-T3]` fixed both and preserved one more task ID than the literal instruction would have.

**How to apply:** Before editing, extract every task ID mentioned anywhere in the delta (including inside quoted replacement strings) and check for collisions against the proposed renumbering. The reorder's *purpose* is almost always ordinal ("task A must precede task B"), not positional, so the exact slot is free to move; the quoted text is not. Then re-sweep the traceability table, the Open Questions section, and every in-task cross-reference. Always list every changed ID in the report — the caller uses it to re-run validation. See [[plan-validator-task-id-sequential-constraint]] for the sequential-by-appearance requirement that forces the renumbering in the first place.
