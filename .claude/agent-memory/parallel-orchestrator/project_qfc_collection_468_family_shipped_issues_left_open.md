---
name: qfc-collection-468-family-shipped-issues-left-open
description: Issues 286, 469, 470, 471, 473, 474 are fully delivered on main but still OPEN — all seven qfc-collection defects shipped under the #468 feature with fix(NNN) subjects that carry no closing keyword
metadata:
  type: project
---

Seven `QfcCollectionController` defect issues — #286, #468, #469, #470, #471, #473, #474 — were
all fixed and merged under the SINGLE feature `qfc-collection-controller-defects-468`. Only #468
is `CLOSED`. The other six are still `OPEN` purely as bookkeeping debt: the fix commits use
`fix(470):`-style subjects, which carry no GitHub closing keyword, so nothing auto-closed them.

Verified 2026-08-29 against `origin/main` at `ecdb1c84`: every one of the six has merged fix
commits reachable from `main` (`fix(286)` x1, `fix(469)` x2, `fix(471)` x1, `fix(473)` x2,
`fix(474)` x2, `fix(470)` x3), and for #470 all three defect guards were read directly out of
`QuickFiler/Controllers/QfcCollectionController.cs` on `main`.

**Why:** An OPEN state on these six is NOT evidence of outstanding work. Admitting one into a
parallel run spends a full preparation cycle — research, spec, plan, preflight — and yields an
empty branch that would produce an empty PR. Observed: a `/parallel-add 470` preparation ran ~8
minutes and 137k tokens before halting on the falsified premise. A concurrent `/parallel-add 469`
was mid-flight at the same time against the same already-shipped work.

**How to apply:** Treat any of these six issue numbers as presumptively delivered and check before
preparing. The remaining genuine work is AC-28 of the #468 feature — closing the six issues — which
is bookkeeping owned by that feature and needs no branch or parallel item. Re-verify before relying
on this: the whole memory goes stale the moment someone closes them, so confirm with
`gh issue view <N> --json state` plus the delivery pre-check in
[[verify-delivery-before-preparing-an-admission]]. Note the promoted record for #470 names a
destination folder `docs/features/active/qfc-collection-conversation-index-defects/` that was never
created, since the work landed under the 468 folder — a documentation inaccuracy, not a missing
deliverable.
