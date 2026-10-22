# P5-T6 — Post-format physical line counts, final measurement

Timestamp: 2026-09-13T16-53
Command: (Get-Content -LiteralPath <P>).Count, applied to each of the seven paths below
EXIT_CODE: 0
LoopPass: 1

This measurement is taken after the repository-wide format in P5-T1 of this same loop pass and after the
CSharpier check in P5-T2 confirmed the tree is clean. It is the authoritative measurement for the
file-size acceptance criterion; a count taken before the formatter has run is not load-bearing, because
CSharpier can change a file's physical line count by chain-wrapping a fluent expression past its
100-column default or by inserting a blank line before a comment that follows a statement.

## Measured counts, one labelled numeric line per file

QfcQueueLineCount: 269
QfcQueueEnqueueLineCount: 200
QfcQueueTlpLineCount: 329
QfcQueueUiIdleLineCount: 108
IUiIdleDispatcherLineCount: 35
QfcQueueEnqueueTestsLineCount: 425
QfcQueueEnqueueTestsHarnessLineCount: 343

| Path | Lines | Ceiling | Headroom | Under 500 |
|---|---|---|---|---|
| `QuickFiler/Controllers/QfcQueue.cs` | 269 | 500 | 231 | yes |
| `QuickFiler/Controllers/QfcQueue.Enqueue.cs` | 200 | 500 | 300 | yes |
| `QuickFiler/Controllers/QfcQueue.Tlp.cs` | 329 | 500 | 171 | yes |
| `QuickFiler/Controllers/QfcQueue.UiIdle.cs` | 108 | 500 | 392 | yes |
| `QuickFiler/Interfaces/IUiIdleDispatcher.cs` | 35 | 500 | 465 | yes |
| `QuickFiler.Test/Controllers/QfcQueueEnqueueTests.cs` | 425 | 500 | 75 | yes |
| `QuickFiler.Test/Controllers/QfcQueueEnqueueTests.Harness.cs` | 343 | 500 | 157 | yes |

All seven measured counts are strictly less than 500. The remediation branch of this task — moving whole
test methods or arrangement between the two test-file parts and re-running P5-T1 through P5-T6 as a
fresh pass — is not entered, because no count is at or over the ceiling.

## Movement against the counts this item started from

`QuickFiler/Controllers/QfcQueue.cs` stood at 507 lines at the recorded anchor, seven lines over the
hard ceiling before any seam was added, which is why the Phase 1 split was a precondition rather than an
option. It now measures 269. The Tlp Manipulation region moved into
`QuickFiler/Controllers/QfcQueue.Tlp.cs`, which measures 329, and the Helper Methods region moved into
`QuickFiler/Controllers/QfcQueue.UiIdle.cs`, which measures 108 after P2-T5 added the production adapter
class to it. `QuickFiler/Controllers/QfcQueue.Enqueue.cs` measures 200, unchanged from the 200 P0-T13
recorded, because the seam substitutions in Phase 2 and Phase 3 replaced expressions in place rather
than adding statements.

The two test files were held under 470 by P4-T20 precisely so that a format-induced growth could not
push either over 500 here. Neither grew: P4-T20 measured 425 and 343 after the scoped format in P4-T21,
and the repository-wide format in P5-T1 of this pass rewrote nothing, so both counts are unchanged. The
tighter 470 trigger therefore proved unnecessary in the event but was not thereby wrong: it was a
precaution against a growth that did not occur.

Output Summary: All seven files measure strictly under the 500-line ceiling after the final format: 269,
200, 329, 108, 35, 425 and 343, with headroom of 231, 300, 171, 392, 465, 75 and 157 respectively. The
tightest is the new test-class part at 425 with 75 lines of headroom. No remediation was required.
Acceptance met.
