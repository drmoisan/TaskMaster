---
name: do-not-elect-reviewer-declined-optional-changes
description: On #824 I took a non-blocking change the preflight reviewer had explicitly declined to require, and my chosen substitute value was itself a defect that cost two extra rounds; when a reviewer says an item does not gate clearance, leave it
metadata:
  type: feedback
---

When a preflight reviewer raises a non-blocking observation and explicitly states it should not
gate the round, do not elect it. Leave it.

**Why.** Issue #824, round 4. The reviewer noted that `P5-T3`'s restoration example quoted
`ILGlobals.cs` as its `git checkout` operand — a path that can never be the offending path in that
branch — and said plainly it was not required and should not gate the next round. I took it anyway,
reasoning the cost was one token against a foot-gun where an unsubstituted run would discard the
plan's own production fix. That reasoning was fine. The execution was not: I chose
`UtilitiesCS/Extensions/DfDeedle.cs`, which is **owned by a sibling child of the same epic**, and
placing it inside a Markdown code span violated the plan's own path-formatting convention, because
a downstream tool derives a feature's change footprint by harvesting backticked path tokens. I had
noted in the very same instruction that the path was sibling-owned and told the planner not to
backtick it elsewhere — then backticked it myself in the substitution. Round 5 was consumed finding
it and round 6 confirming the fix.

The general shape: an elective change is authored with less care than a required one, gets none of
the scrutiny the delta pipeline gives a blocking defect, and still has to be reviewed by a full
round. The reviewer already weighed it and said no. Its judgment on its own finding is better
calibrated than my second-guess.

**How to apply.**

- A reviewer's "non-blocking, not required, should not gate the round" is a decision, not a menu
  item. Default to declining. On #824's round 5 I declined both non-blocking observations and the
  next round cleared.
- If an optional change is genuinely worth taking, take it in a round where it will be reviewed
  anyway, and hold the substituted value to the same standard as a blocking fix — verify it against
  the tree, and check it against every invariant the surrounding document asserts.
- Bar additive edits explicitly when relaying a delta you want kept narrow. Every round of #824 that
  allowed discretionary edits produced something a later round had to catch. Telling the planner
  "make no additive change; report anything you think warrants one instead" produced the first
  single-substitution diff of the run.
- Own the attribution in the checkpoint. I recorded `defect_owner: orchestrator` with the reasoning,
  because a defect logged against an agent that did what it was told corrupts the record.

Related: [[convergence-signal-is-systematically-optimistic]],
[[spec-backticks-widen-blast-radius]],
[[apply-every-part-of-a-multipart-delta]].
