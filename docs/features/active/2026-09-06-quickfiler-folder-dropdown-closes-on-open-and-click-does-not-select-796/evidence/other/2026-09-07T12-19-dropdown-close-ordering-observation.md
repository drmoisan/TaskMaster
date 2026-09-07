# Manual observation — drop-down close ordering (issue #796, plan task P2-T1)

Timestamp: 2026-09-07T12-19
Runbook: docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/runbooks/confirm-dropdown-close-ordering.runbook.md
Build SHA under test: ec674e0c
Instrumentation commit: 0dfcb402f4e3323c7f652b63701edd9bc5eb9fe0
Performed by: repository maintainer (human-interaction exception HI-796-1)
Recorded by: orchestrator, transcribing the maintainer's reported observation verbatim

## Provenance

The maintainer executed the runbook end to end against a Debug build produced from branch head
ec674e0c. `0dfcb402` is the parent of `ec674e0c` and is the commit that added the AC6
instrumentation, so the instrumentation is present in the build that was observed. `ec674e0c` adds
documentation and plan check-offs only and changes no compiled source relative to `0dfcb402`.

Both required logger names produced lines in the observed segment:

- `QuickFiler.Controllers.QfcFormController`
- `QuickFiler.Viewers.BreadcrumbDropDownHost`

The optional third instrumentation site, `QuickFiler.Controllers.QfcItemController`
(`TextBoxSearch_Leave`), was not added, so candidate 3 is not directly observable in this run. AC6
does not require it.

Every line under consideration carries the thread name `VSTA_Main`. The runbook's same-thread premise
therefore holds and file order is the ordering. Ordering below is read from file order, not from
millisecond timestamps.

## Redaction

The excerpted lines contain no absolute host paths, no user names, no mailbox addresses, and no
folder names identifying a person, so redaction is a no-op on this transcript. Field names, decision-
carrying field values, and line order are intact.

## Elision notice

This excerpt is not the complete line set. The runs written as
`ItemNumber=n..m SelectorWasOpen=False` stand in for consecutive per-item
`ParkFocusAndCancelSelectors reached item.` lines that are identical except for the `ItemNumber`
value and the millisecond timestamp. Each elided run is stated explicitly at the position it occupies
in file order, so the ordering of the non-elided lines is unaffected by the elision. No line carrying
`SelectorWasOpen=True`, no `ParkFocusAndCancelSelectors entered.` line, and no `OnDropDownClosed`
line is elided. The maintainer holds the full transcript and can supply every line verbatim on
request.

## Excerpt, in file order

```
--- Gesture A ---
2026-09-07 12:19:29,628 [VSTA_Main] DEBUG QuickFiler.Controllers.QfcFormController [(null)] - Issue #796: QfcFormController.ParkFocusAndCancelSelectors entered. WebView2Focused=False ActiveFormNull=False Groups=9
2026-09-07 12:19:29,629 [VSTA_Main] DEBUG QuickFiler.Controllers.QfcFormController [(null)] - Issue #796: QfcFormController.ParkFocusAndCancelSelectors reached item. ItemNumber=1 SelectorWasOpen=False
2026-09-07 12:19:29,630 [VSTA_Main] DEBUG QuickFiler.Controllers.QfcFormController [(null)] - Issue #796: QfcFormController.ParkFocusAndCancelSelectors reached item. ItemNumber=2 SelectorWasOpen=True
2026-09-07 12:19:29,639 [VSTA_Main] DEBUG QuickFiler.Viewers.BreadcrumbDropDownHost [(null)] - Issue #796: BreadcrumbDropDownHost.OnDropDownClosed entered. CloseReason=CloseCalled ProgrammaticClose=True OpenState=False AutoClose=True Disposed=False PendingClose=False
2026-09-07 12:19:29,647 .. 29,653 [VSTA_Main] ParkFocusAndCancelSelectors reached item. ItemNumber=3..9 SelectorWasOpen=False
--- Gesture B ---
2026-09-07 12:23:35,014 [VSTA_Main] DEBUG QuickFiler.Controllers.QfcFormController [(null)] - Issue #796: QfcFormController.ParkFocusAndCancelSelectors entered. WebView2Focused=True ActiveFormNull=False Groups=9
2026-09-07 12:23:35,020 .. 35,021 ItemNumber=1..2 SelectorWasOpen=False
2026-09-07 12:23:35,022 [VSTA_Main] DEBUG QuickFiler.Controllers.QfcFormController [(null)] - Issue #796: QfcFormController.ParkFocusAndCancelSelectors reached item. ItemNumber=3 SelectorWasOpen=True
2026-09-07 12:23:35,028 [VSTA_Main] DEBUG QuickFiler.Viewers.BreadcrumbDropDownHost [(null)] - Issue #796: BreadcrumbDropDownHost.OnDropDownClosed entered. CloseReason=CloseCalled ProgrammaticClose=True OpenState=False AutoClose=True Disposed=False PendingClose=False
2026-09-07 12:23:35,034 .. 35,039 ItemNumber=4..9 SelectorWasOpen=False
--- Gesture C ---
2026-09-07 12:26:07,437 [VSTA_Main] DEBUG QuickFiler.Controllers.QfcFormController [(null)] - Issue #796: QfcFormController.ParkFocusAndCancelSelectors entered. WebView2Focused=False ActiveFormNull=False Groups=9
2026-09-07 12:26:07,438 .. 07,440 ItemNumber=1..3 SelectorWasOpen=False
2026-09-07 12:26:07,441 [VSTA_Main] DEBUG QuickFiler.Controllers.QfcFormController [(null)] - Issue #796: QfcFormController.ParkFocusAndCancelSelectors reached item. ItemNumber=4 SelectorWasOpen=True
2026-09-07 12:26:07,443 .. 07,448 ItemNumber=5..9 SelectorWasOpen=False
2026-09-07 12:26:07,460 [VSTA_Main] DEBUG QuickFiler.Viewers.BreadcrumbDropDownHost [(null)] - Issue #796: BreadcrumbDropDownHost.OnDropDownClosed entered. CloseReason=CloseCalled ProgrammaticClose=True OpenState=False AutoClose=False Disposed=False PendingClose=False
2026-09-07 12:26:10,958 [VSTA_Main] DEBUG QuickFiler.Controllers.QfcFormController [(null)] - Issue #796: QfcFormController.ParkFocusAndCancelSelectors entered. WebView2Focused=True ActiveFormNull=True Groups=9  (post-gesture focus move; all SelectorWasOpen=False)
2026-09-07 12:27:10,094 .. 10,197 Cancel teardown (QuickFiler closed by the operator).
```

## Blocks that are not gestures

Two blocks appear in the segment after Gesture C and must not be read as gestures:

- `12:26:10,958` — a second `ParkFocusAndCancelSelectors` entry with `WebView2Focused=True`,
  `ActiveFormNull=True` and no selector open on any item. This is the maintainer's focus moving away
  from the form after Gesture C completed. No selector was open, so no cancel occurred and no
  `OnDropDownClosed` follows it.
- `12:27:10,094` onward — the ordinary Cancel teardown emitted when the maintainer closed QuickFiler.

## Per-gesture verdicts

The decision rules applied are those in the runbook's Verification section.

### Gesture A — arrow click (12:19:29)

File order: `ParkFocusAndCancelSelectors entered.` (`WebView2Focused=False`, `ActiveFormNull=False`,
`Groups=9`), then item 2 with `SelectorWasOpen=True`, then `OnDropDownClosed entered.` with
`CloseReason=CloseCalled`, `ProgrammaticClose=True`, `OpenState=False`, `AutoClose=True`.

- **Candidate 1 — CONFIRMED.** The `ParkFocusAndCancelSelectors` entry line appears before the
  `OnDropDownClosed` line, and that close reports `CloseCalled` with `ProgrammaticClose=True`. The
  refutation conditions do not apply: the handler was entered, it reported `Groups=9` rather than
  `Groups=0`, item 2 reported `SelectorWasOpen=True` so a cancel did occur, and the entry is strictly
  before the close.
- **Candidate 2 — REFUTED.** The close reason is `CloseCalled`, not `AppFocusChange` or `AppClicked`,
  and `ProgrammaticClose=True` rather than `False`. A `CloseCalled` reason is an explicit refutation
  condition for candidate 2.
- **Candidate 3 — NOT DIRECTLY OBSERVABLE.** The optional `TextBoxSearch_Leave` site was not
  instrumented. The observed ordering leaves no room for it: the selector on item 2 was already
  cancelled by candidate 1 before the close arrived, so a later third close would have nothing left
  to close.

**First cause: candidate 1.**

### Gesture B — Down key from the search box (12:23:35)

File order: `ParkFocusAndCancelSelectors entered.` (`WebView2Focused=True`, `ActiveFormNull=False`,
`Groups=9`), then item 3 with `SelectorWasOpen=True`, then `OnDropDownClosed entered.` with
`CloseReason=CloseCalled`, `ProgrammaticClose=True`, `OpenState=False`, `AutoClose=True`.

- **Candidate 1 — CONFIRMED.** Same shape as Gesture A: entry before close, close reason
  `CloseCalled` with `ProgrammaticClose=True`, `Groups=9`, and a cancel actually performed on item 3.
- **Candidate 2 — REFUTED.** Close reason `CloseCalled` with `ProgrammaticClose=True`.
- **Candidate 3 — NOT DIRECTLY OBSERVABLE**, and the ordering leaves no room for it, for the same
  reason as Gesture A.

**First cause: candidate 1.**

### Gesture C — type, list expands and stays open, then mouse-click a row (12:26:07)

File order: `ParkFocusAndCancelSelectors entered.` (`WebView2Focused=False`, `ActiveFormNull=False`,
`Groups=9`), then item 4 with `SelectorWasOpen=True`, then the remaining items, then
`OnDropDownClosed entered.` with `CloseReason=CloseCalled`, `ProgrammaticClose=True`,
`OpenState=False`, **`AutoClose=False`**.

- **Candidate 1 — CONFIRMED, and confirmed for the click-without-select symptom specifically.**
  Clicking a row inside the popup deactivated the QuickFiler form, and the deactivation cancel ran
  before the row's selection could commit. The entry precedes the close, the close reports
  `CloseCalled` with `ProgrammaticClose=True`, `Groups=9`, and item 4 reported `SelectorWasOpen=True`
  so a cancel occurred.
- **Candidate 2 — REFUTED.** Twice over: the close reason is `CloseCalled` rather than
  `AppFocusChange` or `AppClicked`, and `AutoClose=False` at the moment it fired, which is itself an
  explicit refutation condition for candidate 2.
- **Candidate 3 — NOT DIRECTLY OBSERVABLE**, and the ordering leaves no room for it: the selector was
  already cancelled by candidate 1 before the close.

**First cause: candidate 1.**

## Conclusion carried forward to Phase 3

The form-deactivation cancel in `QfcFormController.ParkFocusAndCancelSelectors` (the issue #677
behaviour) fires first on all three gestures. Candidate 2 is refuted on all three. Candidate 3 is not
directly observable and the observed ordering leaves no room for it on any gesture.

The three gestures agree; no gesture is inconclusive. This artifact replaces the INFERRED Win32
activation-ordering label in `spec.md` with an observation, which is what AC6 requires.
