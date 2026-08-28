---
name: expect-fail-tests-break-substring-scoped-run-gates
description: A scoped-run acceptance gate asserting "0 failed" under a substring TestCaseFilter becomes unsatisfiable once any earlier task lands an [expect-fail] test in the same class; the fix is an explicit single-name carve-out
metadata:
  type: feedback
---

In a failing-first (RED-then-GREEN) plan, an acceptance gate of the form "a scoped run filtered with
`/TestCaseFilter:FullyQualifiedName~SomeTests` records ... **0 failed**" is unsatisfiable at its
position in plan order whenever an EARLIER task in the same phase added an `[expect-fail]` test to
that same class. The substring filter matches the deliberately-red test, so the gate can never pass
until the much later task that greens it.

The correct idiom is an explicit single-name carve-out that still fails on any other regression:

> records the named tests as Passed **and records no failed test other than** `<ExactTestName>`,
> which `[P#-T#]` tagged `[expect-fail]` and which stays Failed until `[P#-T#]` lands the fix

**Why:** On epic child 446 this defect class consumed two of five preflight rounds. Round 4's own
insertion created round 5's only blocking defect (`[P1-T19]` asserted `0 failed` over
`QfcHomeControllerIterationTests` while `[P1-T16]` had already put an expect-fail test in that class),
and the bound was spent before the fix could be verified. A sixth confirming round cleared it.

**How to apply:**

- When authoring or preflighting a plan, enumerate every `[expect-fail]` task and its host class,
  then check every scoped-run gate's filter against that list **at the gate's position in plan
  order** — not at the end state, where everything is green.
- Verify the carve-out is COMPLETE, not merely non-empty. Round 5's fix named one test; that was
  correct only because exactly one expect-fail task targeted that class. Confirm the count.
- Check substring semantics against the tree (`git grep` the class name): `~` is a substring match,
  so a second class whose name contains the first would silently widen the filter.
- Confirm nothing between the gate and its stated resolution task changes the failed set.

A correction in this class is itself high-risk: **verify that a fix has not introduced a successor
defect**, because on this plan a correction created the next round's only defect. Never ship a fix
applied at bound exhaustion without one confirming round — see
[[feedback_verify_child_preflight_clearance]]. Related ordering hazard:
[[preflight-sweep-task-ordering-and-citation-arity]].
