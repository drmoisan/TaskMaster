# [P5-T8] Post-Format File-Size Audit

Timestamp: 2026-08-26T11-10

Task: [P5-T8]
Feature: docs/features/active/quickfiler-bug-family-446

Command: `git diff --name-only <mb>...HEAD -- "*.cs"` where `<mb>` is the merge-base sha
`61edc19befcf6c4e95b5acd32542f2dcdab41b78` recorded by `[P0-T3]`, followed by a newline count of
each returned path.
EXIT_CODE: 0

This audit runs **after** `[P5-T1]`, because a formatting pass can change line counts. The counts
below are therefore the post-format, post-change counts. Lines are counted as newline terminators
plus one for a final unterminated line, so blank lines are included; a count that omits blank
lines would understate every file against the 500-line cap.

## Audited paths and post-change line counts

| line count | path | at most 500? |
| --- | --- | --- |
| 391 | `QuickFiler.Test/Controllers/QfcDatamodelTests.cs` | yes |
| 496 | `QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs` | yes |
| 497 | `QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs` | yes |
| 262 | `QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs` | yes |
| 460 | `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part2.cs` | yes |
| 270 | `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part3.cs` | yes |
| 468 | `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs` | yes |
| 288 | `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` | yes |
| 480 | `QuickFiler/Controllers/QfcDatamodel.cs` | yes |
| 360 | `QuickFiler/Controllers/QfcFormController.Actions.cs` | yes |
| 95 | `QuickFiler/Controllers/QfcHomeController.Iteration.cs` | yes |
| 245 | `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs` | yes |
| 133 | `QuickFiler/Interfaces/IQfcDatamodel.cs` | yes |

- Audited path count: **13**
- Maximum recorded count: **497** (`QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs`)
- Every recorded count is at most 500: **yes**

## QfcDatamodel.cs budget

`QuickFiler/Controllers/QfcDatamodel.cs` records **480** lines, comfortably inside the cap. D-Plan-3
recorded it at 496 of 500 at the base with net growth capped at 4 lines; `[P1-T5]` relocated
`ScoreRemainingQueueMailItemAsync` out of the file into
`QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` to buy budget before any widening, which
is why the post-change count is 16 lines below the base rather than at the cap.

## QfcFormControllerTests.cs absence

`QuickFiler.Test/Controllers/QfcFormControllerTests.cs` **does not appear** in the audited path
list, so it is untouched by this change set. This is the outcome D-Plan-2 committed to: the file
is 827 lines and the plan deliberately leaves it unmodified, which makes the single permitted
exception of AC24 vacuously satisfied. `[P3-T9]` asserted the same absence at the end of Phase 3
and this audit confirms it still holds after the final formatting pass.

## Scope note

Markdown documentation under the feature folder is exempt from the 500-line cap per
`.claude/rules/general-code-change.md` and is not part of this audit. The audit covers exactly the
`.cs` paths in the change set.

## Output Summary

All 13 changed `.cs` files are at or below the 500-line cap after the final formatting pass; the
largest is 497 lines. `QfcDatamodel.cs` is 480. `QfcFormControllerTests.cs` is absent from the
change set.
