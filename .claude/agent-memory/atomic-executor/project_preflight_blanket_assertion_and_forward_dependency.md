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

2b. **Shared-helper forward dependency (same-phase variant).** A task that creates or extends a
   shared test-double file asserts that *consumer* test classes "compile against it" while those
   consumers are authored by later tasks in the same phase. Example: `#455` F13 preflight round 2 —
   `[P3-T12]` extends `WebViewTestDoubles.cs` and asserts "both messenger and host test classes
   compile against it", but the host test classes are created at `[P3-T13]`..`[P3-T44]`. The Phase 2
   sibling `[P2-T11]` gets this right ("the file compiles"), which makes the mismatch easy to spot by
   comparing the two helper-creation tasks against each other.

**Why:** Both survive planner self-review because each individual clause reads correct; only a
cross-check against real baseline file state (class 1) or against task ordering (class 2) exposes
them. Both are the same failure shape as the `#418` 500-line gate that was unsatisfiable at
authoring time — see [[project_418_500line_gate_vs_plan_content]].

**How to apply:** During preflight, (a) measure every file the plan makes a blanket claim about,
using actual `wc -l` on the branch, not the plan's own tables; (b) parse every `[P#-T#]` reference
inside a task body and flag any that points to a later phase and is a required input rather than an
informational note.

**Mechanical extraction for (b).** Run this over the plan to list every intra-task cross-reference,
then check each direction by hand — it reduces a 220-task sweep to a dozen lines:

```
awk 'match($0,/^- \[ \] \[P[0-9]+-T[0-9]+\]/){id=substr($0,7,RSTART+RLENGTH-7); n=0; s=$0;
  while (match(s,/\[P[0-9]+-T[0-9]+\]/)) { r=substr(s,RSTART,RLENGTH); n++;
  if (n>1) print id" -> "r; s=substr(s,RSTART+RLENGTH) } }' plan.md
```

Pair it with a case-insensitive `compil|msbuild|build` grep to confirm no acceptance inside a
declared non-compiling window actually requires a build (the `<Compile Include>` hits are noise).

**Accepted remediation shape (do not demand more).** A forward reference is fine when the task text
turns it into an explicit *non*-requirement, e.g. `[P1-T3]` / `[P4-T2]` / `[P3-T12]` in `#455` F13:
"the build is not exercised here because X until `[P#-Tn]` — the compiling build is recorded by
`[P#-Tm]`". Reinforce with a preamble bullet naming each non-compiling window by first/last task ID
and the first task that records a compiling tree. That is the pattern that cleared B5.
