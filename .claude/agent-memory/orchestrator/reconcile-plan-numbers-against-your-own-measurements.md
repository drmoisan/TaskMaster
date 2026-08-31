---
name: reconcile-plan-numbers-against-your-own-measurements
description: The pre-preflight self-review must diff every number a plan asserts against numbers the orchestrator already measured; a stale line count sails through a citation-only review and halts Phase 0
metadata:
  type: feedback
---

When self-reviewing a plan before handing it to preflight, do not only check that citations point at
real lines. **Diff every literal number the plan asserts against the numbers you measured yourself
earlier in the same run**, and treat any disagreement as a defect regardless of which source looks
more authoritative.

**Why:** on the #469 preparation run (2026-08-29) I ran `wc -l` on the in-scope files and recorded
`QfcHomeController.Metrics.cs` at **215** lines. The plan asserted **216** in four places, including a
Phase 0 equality gate. My self-review checked citation accuracy, token vacuity and the numstat
arithmetic, and passed the plan. Preflight caught it as a blocking defect: the Phase 0 gate could
never satisfy, so the executor would have halted before doing any work. The correct value was already
in my own context — the review simply never cross-referenced the two.

The upstream cause is worth noting: the research document had recorded the figure as
"232, approximate, **unverified**" because that subagent had no shell. The planner substituted its own
count and landed on 216. An explicitly unverified figure is a marker to re-measure, not a value to
refine.

**How to apply:**

- Keep the measurements you take during scoping (line counts, occurrence counts, file sizes, test
  counts) and run an explicit numeric reconciliation pass over the plan before preflight. Grep the
  plan for digits near the identifiers you measured.
- Any figure a subagent marked `unverified` or `approximate` must be re-measured by whoever has shell
  access before it can appear in an acceptance gate. Do not let it propagate.
- An equality gate on a measured quantity is the highest-risk shape, because it fails closed on a
  one-off error. Prefer a bound where the plan only needs "does not increase".
- Preflight finding blocking defects means the pre-check was too shallow. A citation-only review is
  not a self-review; the arithmetic and the cross-file consistency are where the blocking defects
  actually live.

Related: [[preflight-catches-vacuous-gates]], [[epic-kickoff-facts-need-independent-measurement]],
[[multi-location-fact-residuals-drive-preflight-rounds]].
