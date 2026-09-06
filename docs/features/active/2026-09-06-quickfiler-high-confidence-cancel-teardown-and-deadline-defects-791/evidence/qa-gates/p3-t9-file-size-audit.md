# [P3-T9] File-size audit (post-format)

Timestamp: 2026-09-06T15-11

Command: `foreach ($p in <the seventeen paths>) { (Get-Content -LiteralPath $p).Count }`

EXIT_CODE: 0

CEILING: 500 (applies to *.cs only)

This audit is taken **after** the final CSharpier format ([P3-T1] pass 2 and the [P3-T2] check),
because CSharpier can change line counts. It supersedes the pre-format interim measurement in
[P2-T16].

## Production `.cs` (seven Write Set paths)

| Path | Baseline ([P0-T13]) | Post-format | Headroom |
|---|---|---|---|
| `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs` | 262 | 373 | 127 |
| `QuickFiler/Interfaces/IQfcDatamodel.cs` | 133 | 168 | 332 |
| `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` | 298 | 413 | 87 |
| `QuickFiler/Controllers/QfcDatamodel.cs` | 480 | 483 | 17 |
| `QuickFiler/Controllers/QfcFormController.EventHandlers.cs` | 408 | 490 | 10 |
| `QuickFiler/Controllers/QfcFormController.Deactivate.cs` | 60 | 73 | 427 |
| `QuickFiler/Controllers/QfcHomeController.cs` | 469 | 496 | 4 |

## Test `.cs` (five modified, four created)

| Path | Baseline ([P0-T13]) | Post-format | Headroom |
|---|---|---|---|
| `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs` | 477 | 487 | 13 |
| `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part2.cs` | 465 | 498 | 2 |
| `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part3.cs` | 280 | 289 | 211 |
| `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part4.cs` | 0 (new) | 347 | 153 |
| `QuickFiler.Test/Controllers/QfcFormControllerCancelTeardownTests.cs` | 0 (new) | 393 | 107 |
| `QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs` | 0 (new) | 118 | 382 |
| `QuickFiler.Test/Controllers/QfcDatamodelTeardownTests.cs` | 0 (new) | 235 | 265 |
| `QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs` | 413 | 418 | 82 |
| `QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs` | 477 | 497 | 3 |

MAX-CS-LINE-COUNT: 498
ALL-CS-AT-OR-BELOW-500: YES
SMALLEST-REMAINING-HEADROOM: 2 lines, at `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part2.cs` (498 of 500)

Every listed `.cs` count is at or below 500, which is this task's acceptance. The three next-tightest
files are `QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs` at 497 (headroom 3),
`QuickFiler/Controllers/QfcHomeController.cs` at 496 (headroom 4), and
`QuickFiler/Controllers/QfcFormController.EventHandlers.cs` at 490 (headroom 10).

## PROJECT-FILE (exempt)

PROJECT-FILE (exempt): QuickFiler.Test/QuickFiler.Test.csproj = 528 (baseline 524, +4)

Recorded but not asserted against the ceiling, for the reason [P0-T13] states under the same heading
(R8): `.claude/rules/general-code-change.md` caps production code, test code and reusable script
files, and `.csharpierignore` lines 9-14 record project files as owned by Visual Studio and not C#
source, listing `*.csproj` at line 12. The file was already 524 lines at `BASE-SHA`, so asserting it
against the ceiling would be unsatisfiable whatever this plan did. The +4 is exactly the four
`<Compile Include>` entries [P1-T7] added.

## Effect of the format on the counts

Comparing this audit with the [P2-T16] pre-format measurement, CSharpier changed six of the sixteen
`.cs` counts: the gate lost one line and five test files gained between one and six lines each. The
two production files that had been trimmed to fit — `QfcFormController.EventHandlers.cs` at 490 and
`QfcHomeController.cs` at 496 — were unchanged by the format and stayed inside the ceiling, so no
post-format trimming and no further toolchain restart was required.
