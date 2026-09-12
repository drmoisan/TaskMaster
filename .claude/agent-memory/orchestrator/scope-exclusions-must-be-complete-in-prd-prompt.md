---
name: scope-exclusions-must-be-complete-in-prd-prompt
description: Every item you route to a separate issue must appear in the prd-feature prompt's out-of-scope list, or the spec will assert work the plan is forbidden to do and produce an unsatisfiable acceptance criterion
metadata:
  type: feedback
---

When delegating to `prd-feature`, the out-of-scope list in the prompt must enumerate **every** item
being routed elsewhere — not just the ones that feel like defects.

**Why:** During epic child F6 preparation (#435), research found a dead 139-line region in
`QfcExplorerController.cs`. I routed its deletion to issue #449 (to avoid two children editing one
file), but my `prd-feature` prompt listed only three latent defects as out-of-scope and omitted the
deletion. `prd-feature` reasonably wrote AC-7 as "the only production code deleted is the caller-free
dead region", and projected the file at "323 → ~180-200 after deleting" — asserting work the plan was
then forbidden to perform. The atomic plan deleted nothing, so AC-7 was false as written and the
executor's final check-off task could neither honestly check it nor block. `atomic-executor` caught it
at preflight iteration 1.

The failure mode is specific: research proposes X, the orchestrator defers X, and the deferral is
recorded in the orchestrator's own head and checkpoint but not in the requirements prompt. The spec
then encodes the pre-deferral plan as an acceptance criterion.

**How to apply:**

1. Before delegating to `prd-feature`, list every research recommendation and mark each in-scope or
   deferred. Paste the deferred list verbatim into the prompt's out-of-scope section with the issue
   number for each.
2. If a deferral has a cost to an acceptance criterion — here, retaining the dead region may put the
   80% floor out of reach for that file — state the cost in the prompt so the spec records it rather
   than discovering it at execution.
3. If the error is caught at preflight, **fix the requirement during preparation**, not with a plan
   task that amends `spec.md` mid-execution. The executor proposed the latter; amending requirements
   to match what was built inverts the spec-implementation relationship. Correct `spec.md` and
   `user-story.md` directly, add an out-of-scope row, then tell the planner the requirement is already
   fixed and no amendment task is needed.

Related: [[feedback_promote_latent_defects_to_issues]], [[evidence-and-lifecycle-for-every-change]]
