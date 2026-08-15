---
name: thread-granted-discharges-through-consumers
description: When a plan grants a measurement discharge (e.g. "branch coverage may be recorded as structurally unavailable"), every downstream task that consumes or restates that measurement must carry the same escape, or the discharge is unreachable
metadata:
  type: feedback
---

When a revision adds a conditional discharge to one task — "if the channel emits no X, record the unavailability statement instead of the number" — grep the whole plan for every other task that demands X and apply the same escape there. A discharge granted only at the baseline task and the final consumer, with an intermediate task still demanding the hard number, is a blocking defect: the intermediate task can never be checked off and the consumer's discharge clause is unreachable.

**Why:** Round 1 of the #512 delta added a branch-coverage discharge to `[P0-T16]` (baseline) and `[P6-T4]` (the delta/threshold consumer) but left `[P6-T3]` — which sits between them and supplies the post-change figures `[P6-T4]` reads — demanding a numeric `Branch Coverage:` with "No placeholders." The same round added an `MCP_DETAIL_UNAVAILABLE:` concession to two tasks while two others still asserted numeric counts from that same unmeasured channel. Both surfaced as BLOCKING findings in preflight iteration 2, costing an extra revision pass.

**How to apply:** After editing any acceptance clause that softens a measurement requirement, enumerate the producer→consumer chain for that measurement and re-read every task in it end-to-end. State the discharge in the consumer as "on the same terms <producer task> grants" so the coupling is auditable. Distinguish which obligation stays hard (line coverage) from the one being discharged (branch coverage), and say so explicitly, or the softening reads as blanket. Related: [[research-claims-as-acceptance-clauses]], [[named-coverage-exception-verify-member-body]].
