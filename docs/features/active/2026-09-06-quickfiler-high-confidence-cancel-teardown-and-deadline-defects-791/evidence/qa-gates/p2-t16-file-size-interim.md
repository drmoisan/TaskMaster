# [P2-T16] Interim file-size audit (pre-format)

Timestamp: 2026-09-06T15-00

Command: `foreach ($p in <the seventeen paths>) { (Get-Content -LiteralPath $p).Count }`

EXIT_CODE: 0

CEILING: 500 (applies to *.cs only)

This measurement is taken before the final CSharpier format. CSharpier can change line counts, so
[P3-T9] re-measures the same set afterwards and is the audit that decides the ceiling.

## Production `.cs` (seven Write Set paths)

| Path | Baseline ([P0-T13]) | Now | Headroom |
|---|---|---|---|
| `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs` | 262 | 374 | 126 |
| `QuickFiler/Interfaces/IQfcDatamodel.cs` | 133 | 168 | 332 |
| `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` | 298 | 411 | 89 |
| `QuickFiler/Controllers/QfcDatamodel.cs` | 480 | 483 | 17 |
| `QuickFiler/Controllers/QfcFormController.EventHandlers.cs` | 408 | 490 | 10 |
| `QuickFiler/Controllers/QfcFormController.Deactivate.cs` | 60 | 73 | 427 |
| `QuickFiler/Controllers/QfcHomeController.cs` | 469 | 496 | 4 |

## Test `.cs` (five modified, four created)

| Path | Baseline ([P0-T13]) | Now | Headroom |
|---|---|---|---|
| `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs` | 477 | 487 | 13 |
| `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part2.cs` | 465 | 498 | 2 |
| `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part3.cs` | 280 | 287 | 213 |
| `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part4.cs` | 0 (new) | 341 | 159 |
| `QuickFiler.Test/Controllers/QfcFormControllerCancelTeardownTests.cs` | 0 (new) | 391 | 109 |
| `QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs` | 0 (new) | 118 | 382 |
| `QuickFiler.Test/Controllers/QfcDatamodelTeardownTests.cs` | 0 (new) | 230 | 270 |
| `QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs` | 413 | 418 | 82 |
| `QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs` | 477 | 497 | 3 |

MAX-CS-LINE-COUNT: 498 (`QfcStreamingDequeueConfidenceGateTests.Part2.cs`)
ALL-CS-AT-OR-BELOW-500: YES

## PROJECT-FILE (exempt)

PROJECT-FILE (exempt): QuickFiler.Test/QuickFiler.Test.csproj = 528 (baseline 524, +4)

Recorded but not asserted against the ceiling, for the reason [P0-T13] states under the same
heading: `.claude/rules/general-code-change.md` caps production code, test code and reusable script
files, and `.csharpierignore` lines 9-14 record project files as owned by Visual Studio and not C#
source. The +4 is exactly the four `<Compile Include>` entries [P1-T7] added.

## Files within ten lines of the ceiling

Four `.cs` files are inside the ten-line margin and are named explicitly, in ascending headroom:

1. `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part2.cs` — 498, headroom 2.
2. `QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs` — 497, headroom 3.
3. `QuickFiler/Controllers/QfcHomeController.cs` — 496, headroom 4.
4. `QuickFiler/Controllers/QfcFormController.EventHandlers.cs` — 490, headroom 10.

The two production files in that list were both first written over the ceiling — `EventHandlers.cs`
at 521 and `QfcHomeController.cs` at 505 — and were brought back inside it by condensing added XML
documentation and comments only. No assertion, log line, guard, stage or ordering was removed to fit.
`QfcHomeController.cs` at four lines of headroom is the D11 constraint reaching its measured limit:
D11 predicted the three-guarded-block form would measure about 505 lines, and the two-block form
this plan uses measured 505 before its documentation was condensed.

`QuickFiler/Controllers/QfcDatamodel.cs` fell from a first draft above its baseline back to 483
because [P2-T5] relocated `TryQueueRemainingMailItemAsync` out of it into the QueueProcessing
partial, which is a net removal from the tightest production file in the baseline set.
