# [P2-T9] Interim Line-Cap Audit (End of Phase 2)

Timestamp: 2026-08-26T10-11

Task: [P2-T9]
Feature: docs/features/active/quickfiler-bug-family-446

Command: `git diff --name-only 61edc19befcf6c4e95b5acd32542f2dcdab41b78...HEAD -- "QuickFiler" "QuickFiler.Test"` (plus the uncommitted Phase 2 working-tree changes)
EXIT_CODE: 0

Command: `wc -l <each file>`
EXIT_CODE: 0

Merge base (`<mb>`, from `[P0-T3]`): `61edc19befcf6c4e95b5acd32542f2dcdab41b78`.

## Production files touched so far (5)

| Lines | File | Cap |
|---|---|---|
| 245 | `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs` | 500 |
| 480 | `QuickFiler/Controllers/QfcDatamodel.cs` | 500 |
| 288 | `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` | 500 |
| 95 | `QuickFiler/Controllers/QfcHomeController.Iteration.cs` | 500 |
| 133 | `QuickFiler/Interfaces/IQfcDatamodel.cs` | 500 |

## Test files touched so far (6)

| Lines | File | Cap |
|---|---|---|
| 391 | `QuickFiler.Test/Controllers/QfcDatamodelTests.cs` | 500 |
| 497 | `QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs` | 500 |
| 262 | `QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs` | 500 |
| 468 | `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs` | 500 |
| 460 | `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part2.cs` | 500 |
| 270 | `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part3.cs` | 500 |

All counts are taken after `csharpier format`, so they are the counts a formatting gate will see.

## Output Summary

Every one of the eleven recorded counts is **at most 500**. The maximum is 497
(`QfcHomeControllerIterationTests.cs`). The D3-constrained file
`QuickFiler/Controllers/QfcDatamodel.cs` is **480**, at most 500, with net growth from its 496-line
base held negative because `[P1-T5]` relocated `ScoreRemainingQueueMailItemAsync` out of it into
`QfcDatamodel.QueueProcessing.cs` before any widening.
