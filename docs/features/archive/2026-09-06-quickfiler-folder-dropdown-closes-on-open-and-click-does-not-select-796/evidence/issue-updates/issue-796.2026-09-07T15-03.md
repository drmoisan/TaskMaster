# Issue #796 — acceptance-criteria status mirror

Timestamp: 2026-09-07T15-03
Task: [P8-T9]
Issue: #796

POSTING BLOCKED

Reason: this executor does not post to GitHub. The orchestrator owns all GitHub
interaction for this item, including the issue update and the pull request. The text
below is the exact text intended for the issue and is recorded here so the posting can
be made from it verbatim without re-deriving it.

The same six checkboxes have been mirrored into the local feature `issue.md`, whose
acceptance-criteria block now matches `spec.md` checkbox for checkbox.

## Exact text intended for the issue

All six acceptance criteria for issue #796 are delivered and verified on branch
`bug/quickfiler-folder-dropdown-closes-on-open-796`.

- [x] AC1: Opening the list by arrow click or by Down in the search box leaves it open until Escape, Left, a second arrow click, an item selection, or selection of a different QfcItem.
- [x] AC2: A deactivation of the QuickFiler form caused by the popup taking focus does not cancel the selector session; a deactivation caused by any other window still does (the #677 contract is preserved for genuine deactivation).
- [x] AC3: A mouse click on a row in the open list selects that row and closes the list; the selection is committed before any auto-close cancel runs.
- [x] AC4: The #680 leave-handoff latch covers the mouse open path as well as the Down-arrow path.
- [x] AC5: Row-set refreshes while open (search, late decoration) continue not to close the list (#438 AC-3 regression guard).
- [x] AC6: The first implementation step instruments `ParkFocusAndCancelSelectors` and `OnDropDownClosed` with debug log lines so the runtime ordering is confirmed before the fix is chosen.

Evidence, per criterion, under the feature folder
`docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796`:

| AC | Evidence |
|---|---|
| AC1 | evidence/regression-testing/p7-t3-ac1-ac5-guards.md, plus the fail-before artifact named by the `AC1-FAIL-BEFORE-CARRIER: AC2` line of evidence/other/close-ordering-decision.md, namely evidence/regression-testing/p4-t5-ac2-fail-before.md |
| AC2 | evidence/regression-testing/p4-t5-ac2-fail-before.md, evidence/regression-testing/p4-t9-ac2-pass-after.md |
| AC3 | evidence/regression-testing/p5-t3-ac3-fail-before.md, evidence/regression-testing/p5-t7-ac3-pass-after.md, evidence/regression-testing/p5-t6-scoping-guard.md |
| AC4 | evidence/regression-testing/p6-t4-ac4-fail-before.md, evidence/regression-testing/p6-t6-ac4-pass-after.md, and the deliberate re-pinning of the superseded issue #680 test recorded in evidence/regression-testing/p8-t1-search-dismissal-repin.md and verified green in evidence/regression-testing/p8-t2-search-dismissal-verification.md |
| AC5 | evidence/regression-testing/p7-t3-ac1-ac5-guards.md, evidence/qa-gates/p7-t4-ac5-exclusion.md |
| AC6 | evidence/regression-testing/p1-t11-ac6-instrumentation-tests.md, the human observation artifact evidence/other/2026-09-07T12-19-dropdown-close-ordering-observation.md, and the derived decision record evidence/other/close-ordering-decision.md |

AC6 was satisfied first, as the plan's ordering constraint requires: the instrumentation
landed before any behavioural change, a human ran the runbook against a Debug build of
that instrumented commit, and the confirmed first-cause close path recorded in the
decision record is what selected the fixes for AC2 through AC4 rather than an inference.

## Local mirror

`docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/issue.md`
acceptance-criteria block updated in the same task. Its six checkboxes match the six in
`spec.md`, which is the authoritative acceptance-criteria source for this full-bug item.

Output Summary: six of six acceptance criteria checked off in spec.md and mirrored into
issue.md; posting to GitHub is the orchestrator's step and has not been performed by this
executor.
