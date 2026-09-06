---
name: forward-planner-handoff-records-to-preflight
description: The planner's PLANNER-INTERNAL-REVIEW and SELF-REVIEW records come to the orchestrator, not to the preflight reviewer — forward them explicitly or the reviewer reports them missing
metadata:
  type: feedback
---

When `atomic-planner` finishes a round it emits `PLANNER-INTERNAL-REVIEW: PASS` (with its
CITATION / AC-INVENTORY / AC-MAPPING / UNRESOLVED-GAPS declarations) and
`SELF-REVIEW: RE-DERIVED THIS PASS` (with its citation enumeration) **into its report to
the orchestrator**. Those records do not live in the plan file and there is no channel that
carries them to the next agent. Paste them into the `atomic-executor` preflight brief.

**Why:** the atomic-plan-contract requires both records on every plan handoff, and the
preflight reviewer checks for them. On issue #731 round 7 the reviewer correctly reported
both as absent from its handoff even though the planner had emitted both — the orchestrator
simply did not forward them. That produces a spurious contract finding in the preflight
report and costs credibility on the findings that are real.

**How to apply:** when composing a preflight delegation, copy the planner's two record
blocks verbatim into the brief. If the planner did not emit them, that IS a real defect and
the plan must go back before preflight, not through it.

Related: [[no-sendmessage-relaunch-with-resume-brief]] — there is no SendMessage tool here,
so anything the next agent needs must be in its initial prompt; nothing can be added later.
