# [P5-T27] Deferral of AC-482-12

Timestamp: 2026-08-27T20-17
Command: none — this artifact records a deferral decision
EXIT_CODE: 0
Output Summary: AC-482-12 is conjunctive. Its spec half is already satisfied at branch head; its
pull-request half is outstanding because this preparation run creates no pull request. The checkbox is
deliberately left unchecked.

DEFERRED-TO-ORCHESTRATOR: AC-482-12 spec half satisfied, pull-request half outstanding

PostedAs: unknown

No GitHub issue comment or body update was posted by this task. This artifact is a local deferral
record.

## The criterion, verbatim from `spec.md`

> The correction to #482's filed trigger and severity (the filed `QfcCollectionController.cs:1439`
> trigger is unreachable; the live trigger is Right → Down → Right; the exception is caught and
> logged at `KeyboardHandler.cs:141-147` so the symptom is a dead key, not a crash) is stated in this
> spec and repeated in the PR body, so the PR does not restate an unsupported claim.

## Clause-by-clause status

| Clause | Status | Evidence |
| --- | --- | --- |
| stated in **this spec** | **satisfied at branch head** | `spec.md`, `## Repro & Evidence`, subsection `### #482 — expansion registry divergence` |
| **repeated in the PR body** | **outstanding** | no pull request exists |

Because the criterion joins the two clauses with **and**, the satisfied spec half is not sufficient.
The checkbox stays `- [ ]` with its text unmodified.

## The spec half, located precisely

`spec.md`'s `### #482 — expansion registry divergence` subsection carries all three corrections:

1. **The filed trigger is unreachable.** The subsection carries the paragraph beginning "The promoted
   document's stated trigger is unreachable, and this document corrects it," and grounds it: the
   named synchronous `ToggleExpansion()` call inside `ActivateBySelectionAsync` is guarded by
   `if (blExpanded)`, and both asynchronous callers pass a value that is always `false` — one passes
   the literal `false`, the other passes a value from `ToggleOffActiveItemAsync` whose expansion
   branch is commented out, so it returns its parameter unchanged.
2. **The live trigger is Right, then Down, then Right.** The subsection carries a three-row table
   walking the keystroke sequence through `ToggleExpansionAsync(On)`, then `SelectNextItemAsync`
   marshalling to the synchronous `SelectNextItem` and reaching the synchronous `ToggleExpansion()`,
   then the second `Right` raising `ArgumentException` from `KbdActions.Add` on an entry already
   present.
3. **The symptom is a dead key, not a crash.** The subsection records that the exception surfaces
   through the asynchronous keyboard handler whose `catch` block logs it, so the user-visible symptom
   is a `'B'` or `'D'` key that stops responding for that item.

None of that text was authored by this task; it was present in `spec.md` at branch head, which is why
the spec half is recorded as already satisfied rather than as delivered here.

## Where the pull-request text lives

`docs/features/active/quickfiler-keyboard-action-defects-444/evidence/other/p5-t9-pr-body-inputs.2026-08-27T20-13.md`
holds it under the heading
`## Item 2 — Correction to #482's filed trigger and severity`, restating all three corrections in the
form the PR body needs.

## Why this matters beyond bookkeeping

The purpose of the criterion is stated in its own final clause: "so the PR does not restate an
unsupported claim." If the integration pull request repeats the filed trigger and the filed severity,
it asserts a reachable crash path that the analysis shows to be dead code with a logged, non-fatal
symptom. That would be a factually incorrect claim in the permanent review record, which is exactly
what the criterion exists to prevent.

## What the orchestrator must do

Carry `## Item 2` of the `[P5-T9]` artifact into the integration pull-request body. Once it appears
there, both clauses hold and the criterion may be checked off by whoever authors that body.

## Acceptance

- The artifact carries the required `DEFERRED-TO-ORCHESTRATOR:` line — met; it appears above,
  verbatim.
- That criterion in `spec.md` remains `- [ ]` with its text unmodified — met; the checkbox was not
  touched.
