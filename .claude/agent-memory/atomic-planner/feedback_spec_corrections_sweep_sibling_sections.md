---
name: spec-corrections-sweep-sibling-sections
description: When a finding falsifies a spec premise, sweep the WHOLE spec (Scope & Non-Goals, Out-of-scope, Rollout) for the same premise, not just the Acceptance Criteria section
metadata:
  type: feedback
---

When a remediation finding falsifies a premise stated in a spec's `## Acceptance Criteria`, plan revision tasks for EVERY spec section that asserts the same premise — `## Scope & Non-Goals` "In scope" bullets, out-of-scope bullets that say a follow-up issue "requires its own issue" (name the now-existing issue number instead), and `## Rollout & Follow-up`.

**Why:** In #511 R1 the first remediation plan revised only AC 6; the spec's three "In scope" bullets still claimed deterministic handle creation, "#571 in full", and a handle race. The orchestrator had to amend the requirements doc (Part 6 addendum + exit criterion 7) and force a plan revision, because an AC-only fix leaves the spec internally contradictory and the feature audit raises it as blocking.

**How to apply:** Before finalizing a Finding-E-style spec-wording task, grep the spec for the falsified claim's key tokens across all sections and add one atomic replacement task per contiguous edit site. Also: (a) revision denial text must avoid closing-keyword stems immediately before prohibited issue refs (write "is not delivered by", never "does not fix #571") when a `(fix|clos|resolv)[a-z]* #N` zero-scan gates the branch; (b) assert retention of the accurate bullets with exactly-1 literals; (c) re-derive every downstream exit-criteria count (six vs seven) named in Phase 0 read tasks and the final handoff index. See [[zero-hit-grep-gates-need-carveouts]] and [[terminal-phase-planner-traps]].
