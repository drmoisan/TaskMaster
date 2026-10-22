# P4-T7 — Treatment of FormDeactivated_WebView2Focused_ParksFocusOnce

Timestamp: 2026-09-07T14-13
Task: [P4-T7]
Issue: #796
Channel used: A

## Branch taken

NO branch. The test FormDeactivated_WebView2Focused_ParksFocusOnce is unchanged by this task, no
paired negative test is added, and the fix keeps focus parking unconditional.

Quoted from evidence/other/close-ordering-decision.md:

> AC2-PARK-FOCUS-SUPPRESSED: NO

and from the derivation recorded beneath that line:

> On Gesture A and on Gesture C it is False, so focus was not parked at all on those two gestures.
> Yet both gestures still cancelled an open selector — Gesture A on item 2 and Gesture C on item 4,
> each reported as `SelectorWasOpen=True` — and both still reached the close. Suppressing a step
> that did not execute cannot alter either outcome, so the observation supplies no case in which
> suppressing parking would have prevented the defect.

## Consequences fixed by this branch

- FormDeactivated_WebView2Focused_ParksFocusOnce is not modified. It is untouched in the diff for
  this task.
- No paired `ParkFocusOffWebView2()` negative test is added.
- The line `PARK-FOCUS-SUPPRESSION: IN SCOPE FOR P4-T8` is deliberately ABSENT from this artifact,
  so task P4-T8 scopes its guard to the cancel loop only and focus parking stays unconditional.
- The plan's expect-fail inventory therefore stands at four rows, not five, and task P9-T5 reads it
  as four.

## `[TestMethod]` count for the file

Command:

```
pwsh -NoProfile -Command '(Select-String -Path QuickFiler.Test\Controllers\QfcFormControllerDeactivateTests.cs -SimpleMatch "[TestMethod]").Count'
```

EXIT_CODE: 0
Measured count: 9

The count is unchanged from the value task P4-T4 recorded, which is the observable consequence of
this branch adding no test.

Output Summary: NO branch taken; parking stays unconditional; no paired negative test added; the
suite holds 9 `[TestMethod]` members.
