---
name: verify-a-removals-stated-premise-before-recording-it
description: A /parallel-remove reason can be right in its conclusion and wrong in its premise; check the premise against the item's own spec before writing it into the durable record, because the wrong premise implies a different scheduling rule
metadata:
  type: feedback
---

Check a removal's stated reason against the item's own spec before recording it. Apply the
withdrawal if the outcome is right, but never persist an unverified dependency claim.

**Why:** On run `bugs-2026-09-11` the request to withdraw item 602 was justified as "602's AC4
depends on the ResultsDirectory/LogFileName behavior #873 delivers". Verification against the
branches refuted it outright: 602's `spec.md` AC4 is an 8.3 short-name residual-search criterion
naming neither symbol, 602's plan referenced neither symbol nor item 873, and both its `issue.md`
and `spec.md` placed that half explicitly OUT of scope as the sibling's work. The claim had
originated in the intake handoff and propagated unchecked into
`planner_notes.unexpressible_ordering`, `ordering_assumption_refuted`, the manifest body, and
kickoff Open Decision 1 — four artifacts repeating one unverified sentence. The withdrawal was
still correct, but for a different and much stronger reason, and the two reasons imply DIFFERENT
SCHEDULING RULES: the stated one would justify withdrawing 602 only from a run containing 873,
while the real one scales with the number of document-adding siblings and applied to all twelve.
Recording the stated reason would have left a rule that under-fires on the next run.

**How to apply:**

- **Spend one subagent on the premise.** Two read-only `Explore` passes over the committed
  branches settled it. That is cheap against a durable record that four artifacts already
  repeated and that the next run would inherit.
- **Separate the verdict from the grounds.** An unstarted removal is the operator's call and
  the behavior table has no "reject because the reason is wrong" row, so apply it. Then record
  the verified grounds in the reason field and the refutation beside it — here as
  `items[].withdrawal.reason` plus a sibling `stated_reason_correction` key. Both, not one.
- **Look for the repository-wide criterion.** The real ground was structural and generalizes:
  six of 602's fifteen criteria were present-tense assertions over the whole tracked tree
  ("lists no tracked file"), unbounded by 602's own diff, so any sibling merging AFTER it
  falsifies them on `main` rather than merely dating them. Four sibling preparation branches
  already carried account-identifier strings inside 602's own declared globs. When an item's
  ACs assert a tree-wide property, its position in the run is load-bearing even when no
  dependency edge exists.
- **Distrust a spec's own "correct in either order" claim.** 602's AC13 and its Ordering Risk
  section asserted order-independence, but both rested on a single axis — that no criterion
  mentions the test runner's argument list. They said nothing about the tree-wide searches that
  a later merge actually breaks. The spec even conceded the mechanism elsewhere, under Known
  Contention: "because the criterion is repository-wide a partial correction leaves it unmet."
  Read the axis the claim is scoped to, not the claim's headline.
- Same family as [[verify-delivery-before-preparing-an-admission]] and
  [[never-record-an-identifier-from-a-child-report]]: a supplied assertion is a claim, not
  evidence, and the record outlives the conversation that produced it.
