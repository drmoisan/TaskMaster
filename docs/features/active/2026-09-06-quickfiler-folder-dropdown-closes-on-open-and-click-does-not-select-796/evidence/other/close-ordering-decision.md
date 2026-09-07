# Close-ordering decision record (issue #796, plan Phase 3)

Timestamp: 2026-09-07T13-47
Issue: #796
Work Mode: full-bug

## Sole evidence source

Every decision in this record is derived from the quoted content of:

docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/evidence/other/2026-09-07T12-19-dropdown-close-ordering-observation.md

verified conformant at:

docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/evidence/other/p2-t2-manual-observation-conformance.md

No decision below is taken from the plan's expectation, from the research artifact's
prediction, or from any summary of the observation. Where the observation contradicts a
prediction, the observation is followed and the contradiction is recorded explicitly.

The three candidate close paths carry the numbering used by the runbook and the issue:

1. `QfcFormController.ParkFocusAndCancelSelectors` cancelling every item's selector when
   the QuickFiler form loses activation.
2. Native `ToolStripDropDown` auto-close reaching `BreadcrumbDropDownHost.OnDropDownClosed`
   and then `FinishClose` with an `Uncommitted` reason.
3. `QfcItemController.TextBoxSearch_Leave` closing the drop-down when the search box loses
   focus.

---

## [P3-T1] Per-gesture first cause

FIRST-CAUSE-GESTURE-A: CANDIDATE-1

Gesture A is the arrow click. Derived from this excerpt, quoted in file order:

```
2026-09-07 12:19:29,628 [VSTA_Main] DEBUG QuickFiler.Controllers.QfcFormController [(null)] - Issue #796: QfcFormController.ParkFocusAndCancelSelectors entered. WebView2Focused=False ActiveFormNull=False Groups=9
2026-09-07 12:19:29,630 [VSTA_Main] DEBUG QuickFiler.Controllers.QfcFormController [(null)] - Issue #796: QfcFormController.ParkFocusAndCancelSelectors reached item. ItemNumber=2 SelectorWasOpen=True
2026-09-07 12:19:29,639 [VSTA_Main] DEBUG QuickFiler.Viewers.BreadcrumbDropDownHost [(null)] - Issue #796: BreadcrumbDropDownHost.OnDropDownClosed entered. CloseReason=CloseCalled ProgrammaticClose=True OpenState=False AutoClose=True Disposed=False PendingClose=False
```

and from the verdict the observation states for it: "**First cause: candidate 1.**"

The deactivation entry line precedes the close line in file order; the close reports
`CloseReason=CloseCalled` with `ProgrammaticClose=True`; `Groups=9` rather than `Groups=0`;
and item 2 reports `SelectorWasOpen=True`, so a cancel actually occurred. Every confirm
condition is met and no refute condition applies.

Refutation status of the other two candidates for Gesture A:

- Candidate 2 — REFUTED. Quoted basis: "**Candidate 2 — REFUTED.** The close reason is
  `CloseCalled`, not `AppFocusChange` or `AppClicked`, and `ProgrammaticClose=True` rather
  than `False`. A `CloseCalled` reason is an explicit refutation condition for candidate 2."
- Candidate 3 — NOT DIRECTLY OBSERVABLE, and not refuted. Quoted basis: "**Candidate 3 —
  NOT DIRECTLY OBSERVABLE.** The optional `TextBoxSearch_Leave` site was not instrumented.
  The observed ordering leaves no room for it: the selector on item 2 was already cancelled
  by candidate 1 before the close arrived, so a later third close would have nothing left to
  close." The status recorded here is the observation's own wording, not a refutation.

FIRST-CAUSE-GESTURE-B: CANDIDATE-1

Gesture B is the Down key pressed from the search box. Derived from this excerpt, quoted in
file order:

```
2026-09-07 12:23:35,014 [VSTA_Main] DEBUG QuickFiler.Controllers.QfcFormController [(null)] - Issue #796: QfcFormController.ParkFocusAndCancelSelectors entered. WebView2Focused=True ActiveFormNull=False Groups=9
2026-09-07 12:23:35,022 [VSTA_Main] DEBUG QuickFiler.Controllers.QfcFormController [(null)] - Issue #796: QfcFormController.ParkFocusAndCancelSelectors reached item. ItemNumber=3 SelectorWasOpen=True
2026-09-07 12:23:35,028 [VSTA_Main] DEBUG QuickFiler.Viewers.BreadcrumbDropDownHost [(null)] - Issue #796: BreadcrumbDropDownHost.OnDropDownClosed entered. CloseReason=CloseCalled ProgrammaticClose=True OpenState=False AutoClose=True Disposed=False PendingClose=False
```

and from the verdict the observation states for it: "**First cause: candidate 1.**"

Refutation status of the other two candidates for Gesture B:

- Candidate 2 — REFUTED. Quoted basis: "**Candidate 2 — REFUTED.** Close reason
  `CloseCalled` with `ProgrammaticClose=True`."
- Candidate 3 — NOT DIRECTLY OBSERVABLE, and not refuted. Quoted basis: "**Candidate 3 —
  NOT DIRECTLY OBSERVABLE**, and the ordering leaves no room for it, for the same reason as
  Gesture A."

FIRST-CAUSE-GESTURE-C: CANDIDATE-1

Gesture C is typing, the list expanding and staying open, then a mouse click on a row.
Derived from this excerpt, quoted in file order:

```
2026-09-07 12:26:07,437 [VSTA_Main] DEBUG QuickFiler.Controllers.QfcFormController [(null)] - Issue #796: QfcFormController.ParkFocusAndCancelSelectors entered. WebView2Focused=False ActiveFormNull=False Groups=9
2026-09-07 12:26:07,441 [VSTA_Main] DEBUG QuickFiler.Controllers.QfcFormController [(null)] - Issue #796: QfcFormController.ParkFocusAndCancelSelectors reached item. ItemNumber=4 SelectorWasOpen=True
2026-09-07 12:26:07,460 [VSTA_Main] DEBUG QuickFiler.Viewers.BreadcrumbDropDownHost [(null)] - Issue #796: BreadcrumbDropDownHost.OnDropDownClosed entered. CloseReason=CloseCalled ProgrammaticClose=True OpenState=False AutoClose=False Disposed=False PendingClose=False
```

and from the verdict the observation states for it: "**Candidate 1 — CONFIRMED, and
confirmed for the click-without-select symptom specifically.** Clicking a row inside the
popup deactivated the QuickFiler form, and the deactivation cancel ran before the row's
selection could commit." followed by "**First cause: candidate 1.**"

Refutation status of the other two candidates for Gesture C:

- Candidate 2 — REFUTED, on two independent grounds. Quoted basis: "**Candidate 2 —
  REFUTED.** Twice over: the close reason is `CloseCalled` rather than `AppFocusChange` or
  `AppClicked`, and `AutoClose=False` at the moment it fired, which is itself an explicit
  refutation condition for candidate 2."
- Candidate 3 — NOT DIRECTLY OBSERVABLE, and not refuted. Quoted basis: "**Candidate 3 —
  NOT DIRECTLY OBSERVABLE**, and the ordering leaves no room for it: the selector was
  already cancelled by candidate 1 before the close."

### Note on the two blocks that are not gestures

The observation records two further blocks in the same segment and states that neither is a
gesture: the `12:26:10,958` entry, which it reads as "the maintainer's focus moving away
from the form after Gesture C completed", and the `12:27:10,094` onward Cancel teardown
"emitted when the maintainer closed QuickFiler". Neither contributes to the three lines
above. The first block is used once, further down, as the record's only observed instance of
a deactivation the observation does not attribute to this add-in's own popup.

---

## [P3-T2] AC1 fail-before carrier

AC1-FAIL-BEFORE-CARRIER: AC2

Derivation is available and the halt branch is not taken: none of the three per-gesture
first-cause lines above reads INCONCLUSIVE, so the condition that would require the
undecidable value and a return to the human is not met.

Lines derived from: the first-cause line for Gesture A and the first-cause line for
Gesture B recorded in the section above, corroborated by the first-cause line for Gesture C.
Gestures A and B are precisely the two gestures AC1 names — "Opening the list by arrow click
or by Down in the search box leaves it open" — so they are the lines that decide which
criterion carries the AC1 fail-before regression test. Both read CANDIDATE-1. Gesture C
reads CANDIDATE-1 as well, so the assignment does not depend on which of the three lines is
weighted.

Mapping from the confirmed first cause to the criterion that carries the test:

- Candidate 1 is `QfcFormController.ParkFocusAndCancelSelectors` cancelling every item's
  selector on form deactivation. The criterion whose fix addresses that path is AC2, the
  self-inflicted deactivation seam.
- Candidate 2 is the native auto-close reaching `FinishClose` with an `Uncommitted` reason.
  The criterion whose fix addresses that path is AC3. Candidate 2 is refuted on all three
  gestures, so AC3 does not carry the fail-before test.

Supporting excerpt, quoted from the observation's Conclusion section:

> The form-deactivation cancel in `QfcFormController.ParkFocusAndCancelSelectors` (the issue
> #677 behaviour) fires first on all three gestures. Candidate 2 is refuted on all three.

This assignment was made from the recorded first-cause findings and not from the research
artifact's expectation. The research artifact records only that both mechanisms will very
likely be needed and that the log decides which is first; it does not itself decide, and its
prediction was not used as an input here.

---

## [P3-T3] AC3 mechanism decisions

AC3-ENFORCEMENT-SITE: HOST

Derived from the `PendingClose=` and `ProgrammaticClose=` fields on the close line of every
gesture. Quoted, one per gesture, in file order:

```
2026-09-07 12:19:29,639 ... OnDropDownClosed entered. CloseReason=CloseCalled ProgrammaticClose=True OpenState=False AutoClose=True Disposed=False PendingClose=False
2026-09-07 12:23:35,028 ... OnDropDownClosed entered. CloseReason=CloseCalled ProgrammaticClose=True OpenState=False AutoClose=True Disposed=False PendingClose=False
2026-09-07 12:26:07,460 ... OnDropDownClosed entered. CloseReason=CloseCalled ProgrammaticClose=True OpenState=False AutoClose=False Disposed=False PendingClose=False
```

Two observed field values decide the site.

1. `PendingClose=False` on all three gestures. That field reports the open-lifetime member
   `IsPendingClose`, which is the state the open coordinator consults when a close intent
   arrives while an open is still in flight. It is false at the moment of every observed
   close, so on every gesture the open had already completed and the coordinator held no
   in-flight open for the close to race. A latch placed in the coordinator would therefore
   sit on a path the observation shows was not taken, and it could not intercept any of the
   three observed closes.
2. `ProgrammaticClose=True` with `CloseReason=CloseCalled` on all three gestures. Every
   observed close arrived at the host's own handler as a close the add-in itself initiated,
   downstream of the cancel. The handler that observes it, and the completion point that
   decides whether to cancel, are both in the host.

The enforcement site is therefore the host, meaning `FinishClose` in
`QuickFiler/Viewers/BreadcrumbDropDownHost.cs`. This decision leaves
`QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` with no required change; it
remains in the write set as a bound, not as an obligation.

AC3-HTML-POINTERDOWN: NOT REQUIRED

The value REQUIRED is admissible only when the Gesture C transcript shows that no activation
message was produced. It does not, and it cannot: the transcript is silent on activation
altogether, and its silence carries no information.

The reason is which sites were instrumented. The observation states it directly:

> Both required logger names produced lines in the observed segment:
>
> - `QuickFiler.Controllers.QfcFormController`
> - `QuickFiler.Viewers.BreadcrumbDropDownHost`

An activation message is the `selectorActivate` post from the breadcrumb page, handled by
the bridge coordinator and the router before it reaches the selection session. Neither of
those types is among the loggers that produced lines, and neither was instrumented at all,
so no activation would have produced a transcript line whether one was posted or not. An
absent line is therefore consistent with an activation that occurred and with an activation
that did not, and it discriminates between them not at all. Reading the absence as proof of
absence would be exactly the inference this observation exists to replace.

Because the admissibility condition for REQUIRED cannot be met by this transcript, the value
recorded is the other admitted one. That is not merely the residual choice; the observation
supplies a positive account of the Gesture C symptom that needs no change to the page:

> **Candidate 1 — CONFIRMED, and confirmed for the click-without-select symptom
> specifically.** Clicking a row inside the popup deactivated the QuickFiler form, and the
> deactivation cancel ran before the row's selection could commit.

The observed cause of a row click failing to select is the deactivation cancel running
first, which is the path AC2 closes. Moving the row listener from `click` to a pointer-down
event is a change to the page whose only justification in the plan is a demonstrated absence
of the activation message, and that demonstration does not exist. The page
`QuickFiler/Resources/FolderBreadcrumb.html` is therefore not changed by this item, and the
sibling contention recorded against it does not need to be exercised.

Recorded limitation, so a later reader does not mistake this for a settled negative: this
decision states that the evidence does not support the page change, not that the page change
has been shown unnecessary. If the AC2 seam lands and a row click still fails to select, the
question is reopened, and settling it then requires instrumenting the activation path rather
than re-reading this transcript.

---

## [P3-T4] AC2 mechanism decisions

AC2-PARK-FOCUS-SUPPRESSED: NO

This is an explicit decision, taken from the evidence, not a default. The parking behaviour
does not change, and `FormDeactivated_WebView2Focused_ParksFocusOnce` at
QuickFiler.Test/Controllers/QfcFormControllerDeactivateTests.cs line 134 is not modified.

Derived from the `WebView2Focused=` field on each gesture's entry line. Quoted, in file
order:

```
2026-09-07 12:19:29,628 ... ParkFocusAndCancelSelectors entered. WebView2Focused=False ActiveFormNull=False Groups=9
2026-09-07 12:23:35,014 ... ParkFocusAndCancelSelectors entered. WebView2Focused=True ActiveFormNull=False Groups=9
2026-09-07 12:26:07,437 ... ParkFocusAndCancelSelectors entered. WebView2Focused=False ActiveFormNull=False Groups=9
```

That field reports the same expression that gates the parking call, so the field decides
whether parking ran. On Gesture A and on Gesture C it is False, so focus was not parked at
all on those two gestures. Yet both gestures still cancelled an open selector — Gesture A on
item 2 and Gesture C on item 4, each reported as `SelectorWasOpen=True` — and both still
reached the close. Suppressing a step that did not execute cannot alter either outcome, so
the observation supplies no case in which suppressing parking would have prevented the
defect. Gesture B is the one gesture where parking did run, and its close is the same shape
as the other two, so parking is not what distinguishes it either.

The cancel loop is thus shown to be independent of the parking step: the cancel is what
closes the selector, on gestures where parking runs and on gestures where it does not. The
AC2 seam therefore gates the cancel loop only, and the paired negative test that the plan's
expect-fail inventory holds conditional on this decision is not brought into scope.

Contradiction recorded against the research artifact, because the observed values are the
reverse of its source-derived expectation and a later reader should not assume a
transcription error. Research section 3.2 derives that the parking chain "is therefore live
only when a WebView2 is the active leaf — which is the arrow-click case", and that on the
search path the leaf is a `TextBox` so parking is skipped. Observation reverses both: the
arrow-click gesture reports `WebView2Focused=False` and the Down-key-from-the-search-box
gesture reports `WebView2Focused=True`. The decision above does not depend on which gesture
carries which value — it depends only on parking having been skipped on two gestures that
nevertheless exhibited the defect — but the reversal is load-bearing for the reachability
question recorded further down and is stated here once.

AC2-ITEMVIEWER-WIRING: REQUIRED

Derived from the `ActiveFormNull=` field, compared across the three gestures and the one
non-gesture block the observation identifies as a deactivation it does not attribute to this
add-in's own popup. Quoted, in file order:

```
2026-09-07 12:19:29,628 ... ParkFocusAndCancelSelectors entered. WebView2Focused=False ActiveFormNull=False Groups=9
2026-09-07 12:23:35,014 ... ParkFocusAndCancelSelectors entered. WebView2Focused=True ActiveFormNull=False Groups=9
2026-09-07 12:26:07,437 ... ParkFocusAndCancelSelectors entered. WebView2Focused=False ActiveFormNull=False Groups=9
2026-09-07 12:26:10,958 ... ParkFocusAndCancelSelectors entered. WebView2Focused=True ActiveFormNull=True Groups=9  (post-gesture focus move; all SelectorWasOpen=False)
```

and from the observation's reading of the fourth line:

> `12:26:10,958` — a second `ParkFocusAndCancelSelectors` entry with `WebView2Focused=True`,
> `ActiveFormNull=True` and no selector open on any item. This is the maintainer's focus
> moving away from the form after Gesture C completed.

The field reports whether `Form.ActiveForm` was null at entry. The spec and the research
artifact both proposed it as a low-cost discriminator, on the reasoning that a
`ToolStripDropDown` is not a `Form`, so a self-inflicted deactivation would show a null
active form and a genuine one a non-null active form. The observation refutes that
discriminator. All three self-inflicted gestures report `ActiveFormNull=False`, and the one
block the observation attributes to focus moving away from the form reports
`ActiveFormNull=True`. The values are not merely uninformative; they run opposite to the
predicted direction on all four observations.

The consequence for the AC2 seam is direct. The seam's planned implementation was a member
on the form-viewer interface implemented in the concrete form viewer "as the only site that
reads non-injectable activation state", and `Form.ActiveForm` was the activation state it
was to read. That reading is refuted, so the concrete form viewer has no observed signal
from which to derive self-inflicted-versus-genuine on its own. The value must instead be
supplied by the code that knows the popup is being opened, which is the wiring in
`QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` that already assigns
`host.MayTakeFocus = MayRestoreBreadcrumbFocus;` at line 212. A popup-owns-activation
assignment beside that existing assignment is therefore required rather than optional.

The refuted discriminator is not inverted and re-used. One observation of a single genuine
deactivation is too thin a basis on which to depend on the opposite of a framework heuristic
that has already been observed to behave contrary to its documented rationale, and an
explicitly assigned state does not depend on the heuristic at all.

---

## [P3-T5] AC4 mechanism decision

AC4-MECHANISM: SearchOwnedDismissalLatch

AC4-NEW-MEMBER: REQUIRED

### Observed reachability of the leave handler

The record is required to state whether the Gesture A and Gesture C transcripts show a
`TextBoxSearch_Leave` entry at all. They do not. Neither transcript contains any such entry,
and neither does the Gesture B transcript.

That absence supports no conclusion about whether the handler ran. The observation states
why:

> The optional third instrumentation site, `QuickFiler.Controllers.QfcItemController`
> (`TextBoxSearch_Leave`), was not added, so candidate 3 is not directly observable in this
> run. AC6 does not require it.

A handler carrying no logging site emits nothing whether it executes or not, so the missing
entry is equally consistent with a handler that ran and a handler that did not. What the
absence establishes is only that the transcript is silent. It is not evidence that the
handler was unreached, and it must not be read as corroborating research section 3.2's
source-derived prediction that candidate 3 is not reached on either reproduction path. That
prediction remains a derivation from source and is untested by this run.

The observation's own indirect statement is recorded here in its own terms rather than
strengthened. For each gesture it states that candidate 3 is "NOT DIRECTLY OBSERVABLE" and
that the observed ordering "leaves no room for it", on the ground that the selector was
already cancelled by candidate 1 before the close arrived. That argument bears on whether a
third close could have been the FIRST cause. It does not bear on whether the leave handler
runs at all, which is the question AC4 addresses.

One observed value weakens the source-derived prediction rather than supporting it. Research
section 3.2's reachability argument turns on which gesture has a WebView2 as the active
leaf, because the parking assignment is what can synchronously raise the search box's
`Leave`. It predicts a WebView2 leaf on the arrow-click path and a `TextBox` leaf on the
keyboard path. Observation reverses that on both gestures, as recorded in the AC2 section
above. The prediction's premise is therefore observed to be wrong in the specific respect its
conclusion depends on, so the AC4 gap must be treated as open and closed on its own terms.

### Mechanism

The gap AC4 names is that `TextBoxSearch_Leave` takes dismissal ownership of the drop-down
regardless of which gesture opened it. The existing issue #680 latch
`_searchLeaveHandoffPending` is one-shot and read-and-cleared, and its single producer is the
`Keys.Down` branch, so it covers exactly one leave on one gesture path and cannot express
ownership over a popup's lifetime.

The mechanism recorded above is a provenance latch: a lifetime flag recording that the
currently open drop-down is one this controller opened from the search box, set by the two
search-driven open sites and consulted by the leave handler, which dismisses only when the
flag is set. A mouse-driven open never sets it, so the leave no longer dismisses a popup the
mouse opened, and the existing one-shot handoff latch keeps its present meaning unchanged.

The mechanism is confined to the single production file the write set allows for AC4,
`QuickFiler/Controllers/QfcItemController.EventHandlers.cs`. All three sites it touches are
declared in that file: the search-typing open path `TextBoxSearch_TextChanged` at line 173,
the Down-arrow open path `TextBoxSearch_KeyDown` at line 190, and the consumer
`TextBoxSearch_Leave` at lines 217-228. No other production file participates, no interface
changes, and the mouse open path is not modified — it is covered by not setting the flag,
which requires no edit anywhere outside this file.

The mechanism is chosen so that it does not depend on the leave handler's reachability, which
this run leaves unobserved. It is correct whether or not the handler executes on any given
gesture: if it never runs, the flag is never read and nothing changes; if it runs, it
dismisses only a popup the search box owns.

### Why a new member is required

The existing `_searchLeaveHandoffPending` field cannot carry this meaning. It is consumed
destructively on its first read, by design — its in-source comment records the read-and-clear
as the mechanism by which "the Down-arrow handoff's own Leave is consumed exactly once" — so
it is false again immediately after the handoff it guards, while the popup is still open.
Provenance must persist for as long as the popup is open, which is a different lifetime.
Overloading the one-shot field would break the #680 contract that the plan requires be
preserved.

The new member is therefore one additional private boolean field on the concrete
`QfcItemController`, declared in the same file beside the existing latch. It is a private
field on an internal partial class, so it changes no interface and no public surface, and it
disturbs no implementor anywhere in the tree.
