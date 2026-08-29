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

**How to apply:** State this directive in every preparation delegation that ends at
`PREFLIGHT: ALL CLEAR`, and require the child to report its preflight round count with an
explanation whenever it exceeds 1. Note this is stricter than, and compatible with, the
two-round target in `.claude/skills/atomic-plan-contract/SKILL.md`: the contract caps
rounds at two, this directive says the second round should not normally be needed. Pairs
with [[derive-counts-exhaustively-before-approving]], whose unverified-count failure is a
prime example of what the self-review pass is meant to catch.
