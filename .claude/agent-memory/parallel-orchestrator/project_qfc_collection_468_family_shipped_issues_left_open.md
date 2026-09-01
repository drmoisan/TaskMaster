---
name: qfc-collection-468-family-shipped-issues-left-open
description: All seven qfc-collection defects (286, 468, 469, 470, 471, 473, 474) are delivered on main; the bookkeeping backlog is now partially cleared — 468/469/470 are CLOSED, while 286/471/473/474 remain OPEN despite being shipped
metadata:
  type: project
---

Seven `QfcCollectionController` defect issues — #286, #468, #469, #470, #471, #473, #474 — were
all fixed and merged under the SINGLE feature `qfc-collection-controller-defects-468`. The fix
commits use `fix(470):`-style subjects, which carry no GitHub closing keyword, so nothing
auto-closed them and every sibling was orphaned as bookkeeping debt.

**Closure state as of 2026-08-31 (re-verify; this half decays fastest):** #468 CLOSED
2026-08-28T07:38:53Z, #470 CLOSED 2026-08-31T17:53:40Z, #469 CLOSED 2026-08-31T23:25:55Z — all
three `COMPLETED`. #286, #471, #473, #474 are still OPEN (#474 re-confirmed OPEN 2026-08-31). The backlog is being drained
incrementally, so the OPEN set shrinks over time while the DELIVERED set does not change: all
seven are delivered regardless of issue state.

Verified 2026-08-29 against `origin/main` at `ecdb1c84`: every one of the six has merged fix
commits reachable from `main` (`fix(286)` x1, `fix(469)` x2, `fix(471)` x1, `fix(473)` x2,
`fix(474)` x2, `fix(470)` x3), and for #470 all three defect guards were read directly out of
`QuickFiler/Controllers/QfcCollectionController.cs` on `main`.

**Why:** An OPEN state on these six is NOT evidence of outstanding work. Admitting one into a
parallel run spends a full preparation cycle — research, spec, plan, preflight — and yields an
empty branch that would produce an empty PR. Observed: a `/parallel-add 470` preparation ran ~8
minutes and 137k tokens before halting on the falsified premise. A concurrent `/parallel-add 469`
was mid-flight at the same time against the same already-shipped work.

**#469 has since had a SECOND, dedicated delivery, and it is also complete.** The `/parallel-add 469`
that was mid-flight on 2026-08-29 was not wasted: it found genuine residual scope beyond the two
`fix(469)` code commits — comment and documentation accuracy around the retained `stackMovedItems`
contract — and shipped it as its own run. Verified 2026-08-31: branch
`bug/qfc-collection-move-diagnostics-defects-469` merged as PR #704 ("Correct #469 move-diagnostics
documentation") at 15:18:17Z, the branch is an ancestor of `origin/main`, and its three-dot diff
against `main` is EMPTY. Its feature folder
`docs/features/active/2026-08-07-qfc-collection-move-diagnostics-defects-469/` on `main` carries the
full completed chain (plan, two feature-audits, two code-reviews, two policy-audits, an
audit-reconciliation) and 14 of 14 real ACs checked in `spec.md`. The only unchecked boxes there are
the `Blocker`/`High`/`Medium` SEVERITY checklist, which `feature-promotion-lifecycle` explicitly
excludes from the AC source — do not misread those three as residual scope. The issue is still OPEN.

Two lessons that generalize. First, an "already shipped under a sibling" verdict can still leave a
thin residual scope, so the pre-check decides whether a NEW parallel item is warranted, not whether
the issue was ever touched. Second, once that residual run has itself merged, the empty three-dot
diff is what makes a later re-add conclusively rejectable — re-run it rather than reasoning from the
family membership alone.

**#474 verified delivered in depth on 2026-08-31 and rejected in five tool calls, no preparation.**
Both of its defects were read straight out of `git show origin/main:QuickFiler/Controllers/QfcCollectionController.cs`:
defect 1 — `_parent` is now `private IQfcFormController _parent;` (`:64`) and the call site is a
plain `await _parent.SkipGroupAsync();` (`:1096`), with ZERO `(QfcFormController)` casts left in the
file; defect 2 — readiness is split into `TryGetMoveReadiness(out string notifications)` which
presents nothing, with the modal moved behind an injectable `private Action<string> _notifyNotReady`
seam, so `ReadyForMove` is now the notify-on-false wrapper. Three ancestor commits: `122dcd8d`,
`4938779a`, `5f8026aa`. Unlike #469, #474 has NO residual scope — see the AC-table check below.

**A re-add of an already-rejected candidate costs almost nothing once this memory exists.** A second
`/parallel-add 470` on 2026-08-31 was rejected in two tool calls: `gh issue view 470` returned
`CLOSED`/`COMPLETED`, and the three `fix(470)` commits were confirmed as ancestors of `origin/main`.
Contrast ~137k tokens for the same verdict on 2026-08-29. Expect repeat invocations against this
family — the OPEN state invites them — and answer with the pre-check rather than a preparation cycle.

**How to apply:** Treat any of these seven issue numbers as presumptively delivered and check before
preparing. The remaining genuine work is AC-28 of the #468 feature — closing the still-open
siblings — which is bookkeeping owned by that feature and needs no branch or parallel item.
Re-verify before relying on this: the closure half goes stale the moment someone closes another, so
confirm with `gh issue view <N> --json state,stateReason` plus the delivery pre-check in
[[verify-delivery-before-preparing-an-admission]]. Note the promoted record for #470 names a
destination folder `docs/features/active/qfc-collection-conversation-index-defects/` that was never
created, since the work landed under the 468 folder — a documentation inaccuracy, not a missing
deliverable.
