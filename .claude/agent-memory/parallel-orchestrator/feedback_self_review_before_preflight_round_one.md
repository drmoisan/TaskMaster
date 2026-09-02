---
name: self-review-before-preflight-round-one
description: Run one internal self-review pass mirroring preflight's own checks before handing a plan to preflight; a preflight round count above 1 on a well-scoped item is a process defect to investigate, not normal iteration
metadata:
  type: feedback
---

Before a plan reaches the preflight-clearance stage, run one internal self-review pass
structured identically to what preflight itself checks — citation-to-tree verification,
AC-to-implementation traceability, and scope-boundary consistency — and resolve every gap
that pass finds.

**Why:** Preflight is supposed to CONFIRM a plan that already passes these checks, not be
the first place gaps are discovered. When preflight is used as the primary review, every
defect it finds costs a full planner/executor round trip, and the round count inflates
without the plan having been reviewed any more thoroughly. The operator's standard is
therefore explicit: on a well-scoped item, a preflight round count above 1 should be
treated as a signal that this pre-check was skipped or done too shallowly, and
investigated as a process defect — not absorbed as normal iteration.

**One cause dominates the overruns, and it is a SCOPING error rather than a discipline
failure.** Both multi-round cases on run `bugs-638-644-647` — 670 at five rounds, 678 at four —
had exhaustive rounds that still missed defects, because the citation set was scoped to the files
the plan NAMES rather than to the wider set its mandated edits force the compiler to touch. Once
the 678 review traced outward from the named files, the sweep found seven more instances of
classes already reported. The highest-value catch was of exactly that kind: `QfcFormControllerTests.cs`
is 827 lines and was absent from the file-size census entirely, leaving one task's
at-or-below-baseline comparison with **no operand** — a gate that could not pass however the
executor behaved. Two files were unreachable by any token grep, because their `CreateGate` lambdas
are untyped at the call site.

So make the self-review's citation set the TRANSITIVE one: every file a mandated edit forces a
signature, constructor, or call-site change in, not just the files the plan mentions. Ask what
stops compiling if this change lands.

**Expect sibling invalidation to dominate rounds 3 and up, and bundle its fix into the same
delta.** On 670, rounds 3 through 5 were almost entirely a round's own fix invalidating a
neighbouring task's assumption. On 678, round 2's fix created round 3's only substantive defect: a
loop-termination carve-out scoped in its own words to "this restart rule", which does not reach the
task recording the loop's result. A delta that changes a rule must be checked against every task
that consumes that rule, in the same pass.

**How to apply:** State this directive in every preparation delegation that ends at
`PREFLIGHT: ALL CLEAR`, and require the child to report its preflight round count with an
explanation whenever it exceeds 1. Note this is stricter than, and compatible with, the
two-round target in `.claude/skills/atomic-plan-contract/SKILL.md`: the contract caps
rounds at two, this directive says the second round should not normally be needed. Pairs
with [[derive-counts-exhaustively-before-approving]], whose unverified-count failure is a
prime example of what the self-review pass is meant to catch.
