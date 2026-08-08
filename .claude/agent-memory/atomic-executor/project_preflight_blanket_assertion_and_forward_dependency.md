---
name: preflight-blanket-assertion-and-forward-dependency
description: Two recurring atomic-plan preflight blockers — directory-wide compliance assertions that collide with a pre-existing violation, and a task whose acceptance needs an artifact produced by a later phase
metadata:
  type: project
---

Two defect classes account for most preflight blocks on large per-file coverage plans in this repo.
Check both mechanically before signalling ALL CLEAR.

1. **Blanket directory-wide compliance assertion.** A final-QC task asserts a property over a whole
   directory (e.g. "no test file under `QuickFiler.Test/Helper Classes/` exceeds 500 lines") while a
   pre-existing, untouched file already violates it. Example: `#434` F4 preflight —
   `QuickFiler.Test/Helper Classes/ConversationResolverTests.cs` is 578 lines at baseline and is
   never modified by the plan, making `[P13-T8]` unsatisfiable. Fix is to scope the assertion to
   files the feature created or modified and record the pre-existing violation as a promoted
   follow-up.

2. **Forward-phase artifact dependency.** A Phase N task's acceptance cites an artifact produced by
   a Phase N+k task. Example: `#434` F4 `[P12-T3]` required the coverage report produced at
   `[P13-T5]`. Executors run tasks in order, so the task can never be checked off.

**Why:** Both survive planner self-review because each individual clause reads correct; only a
cross-check against real baseline file state (class 1) or against task ordering (class 2) exposes
them. Both are the same failure shape as the `#418` 500-line gate that was unsatisfiable at
authoring time — see [[project_418_500line_gate_vs_plan_content]].

**How to apply:** During preflight, (a) measure every file the plan makes a blanket claim about,
using actual `wc -l` on the branch, not the plan's own tables; (b) parse every `[P#-T#]` reference
inside a task body and flag any that points to a later phase and is a required input rather than an
informational note.
