# P2-T2 — Conformance verification of the manual observation artifact

Timestamp: 2026-09-07T13-47
Task: [P2-T2]
Issue: #796
Channel used: A

## Source artifact

Path: docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/evidence/other/2026-09-07T12-19-dropdown-close-ordering-observation.md

That is the only file under the feature's evidence/other/ directory whose filename
begins with an ISO-8601 `yyyy-MM-ddTHH-mm` timestamp. The directory holds three other
files (preflight-round-1-delta.md, preflight-round-1-delta-b3-adjudication.md and
preflight-round-2-delta.md), none of which begins with a timestamp, so the P2-T1
"exactly one such artifact" condition is met.

## Checks P2-T2 names

### Check 1 — lines from both required logger names appear in the transcript

The runbook's Prerequisites section names two required logger names. Both produce
lines in the transcribed excerpt, and the excerpt shows the logger name inline on each
non-elided line.

- `QuickFiler.Controllers.QfcFormController` — present on the `ParkFocusAndCancelSelectors`
  entry line of every gesture and on the per-item lines. Non-elided instances appear in
  the Gesture A, Gesture B and Gesture C blocks.
- `QuickFiler.Viewers.BreadcrumbDropDownHost` — present on the `OnDropDownClosed entered.`
  line of every gesture, one per gesture.

RESULT: PASS. Both required logger names are represented, and each is represented within
each of the three gesture blocks, so neither of the runbook's two single-site
inconclusive conditions is triggered.

Corroboration that the observed build was the instrumented build: the runbook records
that `QuickFiler.Viewers.BreadcrumbDropDownHost` emits nothing at all before the AC6
instrumentation lands, because that type declares no logger. Lines from that logger are
present, so the running build carried the instrumentation.

### Check 2 — all lines under consideration carry the same thread name

The source artifact states that every line under consideration carries the thread name
`VSTA_Main`, and every non-elided excerpt line displays `[VSTA_Main]` inline. The elided
runs are declared in the artifact's Elision notice as consecutive per-item
`ParkFocusAndCancelSelectors reached item.` lines differing only in `ItemNumber` and the
millisecond timestamp, and two of the four elided runs additionally display
`[VSTA_Main]` inline.

RESULT: PASS. The runbook's same-thread premise holds, so file order is the ordering and
the ordering claim is not required to be reported as inconclusive on thread grounds.

### Check 3 — a per-gesture confirmed-or-refuted statement for every one of the three gestures

The artifact's `## Per-gesture verdicts` section carries one subsection per gesture, each
stating a status for all three candidates and a single first cause.

| Gesture | Candidate 1 | Candidate 2 | Candidate 3 | First cause stated |
|---|---|---|---|---|
| A — arrow click | CONFIRMED | REFUTED | NOT DIRECTLY OBSERVABLE | candidate 1 |
| B — Down key from search box | CONFIRMED | REFUTED | NOT DIRECTLY OBSERVABLE | candidate 1 |
| C — type then mouse-click a row | CONFIRMED | REFUTED | NOT DIRECTLY OBSERVABLE | candidate 1 |

RESULT: PASS. All three gestures carry an explicit statement. `NOT DIRECTLY OBSERVABLE`
for candidate 3 is the status the runbook's Verification section prescribes when the
optional third instrumentation site was not added, and the artifact additionally states,
per gesture, whether the observed ordering leaves room for a third close, which is the
second half of that same prescription.

## Additional P2-T1 shape conditions re-checked here

- `Timestamp:` field present, value `2026-09-07T12-19`.
- Commit SHA of the build recorded: `ec674e0c`, with the instrumentation commit
  `0dfcb402f4e3323c7f652b63701edd9bc5eb9fe0` recorded alongside it.
- Excerpts for Gesture A, Gesture B and Gesture C present in file order, in one fenced
  block carrying the three gesture separators, with an explicit statement that ordering
  is read from file order rather than from millisecond timestamps.
- Redaction statement present; the transcript carries no absolute host path, no user
  name, no machine name and no mailbox address.

## Build-identity equivalence, verified rather than accepted

P2-T1 requires the build to be produced from the commit recorded in
evidence/qa-gates/p1-t15-instrumentation-commit.md, which records
`0dfcb402f4e3323c7f652b63701edd9bc5eb9fe0`. The artifact records the build as `ec674e0c`
and asserts that `0dfcb402` is its parent and that `ec674e0c` changes no compiled source.
Both halves of that assertion were verified against the repository rather than taken on
trust.

Command: `git log --oneline -8`

Observed: `ec674e0c` is listed immediately above `0dfcb402`, so `0dfcb402` is its parent.

Command: `git diff --name-only 0dfcb402 ec674e0c`

Observed output, both paths and no others:

```
docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/evidence/qa-gates/p1-t15-instrumentation-commit.md
docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/plan.2026-09-06T21-59.md
```

EXIT_CODE: 0

No .cs file, no project file and no resource file appears, so the compiled source of the
observed build is identical to that of the recorded instrumentation commit.

## Recorded limitations that do not affect conformance

Two properties of the artifact are recorded here so that a later reader does not mistake
either for an unreported gap. Neither is a P2-T2 check and neither changes the result.

1. **Elision.** The excerpt is not the complete line set. Runs of consecutive per-item
   lines are collapsed into a single summarising line placed at the position that run
   occupies in file order. The artifact states that no line carrying
   `SelectorWasOpen=True`, no `ParkFocusAndCancelSelectors entered.` line and no
   `OnDropDownClosed` line is elided, so every line the runbook's decision rules read is
   present verbatim, and the relative order of the non-elided lines is unaffected.
2. **Candidate 3 is not directly observable.** The optional third instrumentation site at
   `QfcItemController.TextBoxSearch_Leave` was not added. AC6 does not require it and the
   runbook explicitly admits its absence. The consequence is carried forward to Phase 3
   rather than resolved here: the transcript can support no claim about whether that
   handler ran, because a handler with no logging site produces no line whether it ran or
   not.

## Result

MANUAL-OBSERVATION: CONFORMANT

Output Summary: The manual observation artifact at
evidence/other/2026-09-07T12-19-dropdown-close-ordering-observation.md satisfies all three
P2-T2 checks and all five P2-T1 shape conditions. Both required logger names appear, every
line under consideration carries the thread name `VSTA_Main`, and all three gestures carry
an explicit per-candidate confirmed-or-refuted statement. Execution proceeds to Phase 3.
