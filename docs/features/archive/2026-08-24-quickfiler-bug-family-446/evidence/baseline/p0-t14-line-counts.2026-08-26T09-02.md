# [P0-T14] Baseline Line Counts

Timestamp: 2026-08-26T09-02

Task: [P0-T14]
Feature: docs/features/active/quickfiler-bug-family-446

Command: `wc -l` over the thirteen paths this plan may touch, run from the workspace root at
merge-base HEAD `61edc19befcf6c4e95b5acd32542f2dcdab41b78`.

EXIT_CODE: 0

## Thirteen Recorded Paths

### Production (6)

| Path | Lines |
| --- | --- |
| `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs` | 177 |
| `QuickFiler/Controllers/QfcDatamodel.cs` | **496** |
| `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` | 177 |
| `QuickFiler/Controllers/QfcFormController.Actions.cs` | 302 |
| `QuickFiler/Controllers/QfcHomeController.Iteration.cs` | 86 |
| `QuickFiler/Interfaces/IQfcDatamodel.cs` | 59 |

### Test (7)

| Path | Lines |
| --- | --- |
| `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs` | 424 |
| `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part2.cs` | 460 |
| `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part3.cs` | 152 |
| `QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs` | **464** |
| `QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs` | 378 |
| `QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs` | 136 |
| `QuickFiler.Test/Controllers/QfcDatamodelTests.cs` | 317 |

Thirteen paths, each with a numeric line count. The recorded count for
`QuickFiler/Controllers/QfcDatamodel.cs` is **496**, satisfying this task's acceptance condition.

## Budget Notes Carried Forward

- `QfcDatamodel.cs` at 496 of 500 leaves **4 lines** of headroom. Per D3, `[P1-T5]` relocates
  `ScoreRemainingQueueMailItemAsync` out of this file before any widening, and `[P1-T5]`'s
  acceptance requires the post-change count to be strictly less than 496.
- `QfcHomeControllerIterationTests.cs` at 464 of 500 leaves 36 lines. Per D4, `[P1-T14]`
  deduplicates it into an `ArrangeIterate` helper before it absorbs the #446 caller tests, and
  `[P1-T14]`'s acceptance requires the post-change count to be strictly less than 464.
- `QfcStreamingDequeueConfidenceGateTests.Part2.cs` at 460 must not grow: `[P1-T11]`'s acceptance
  requires its post-change count to equal **460** exactly.
- `QfcStreamingDequeueConfidenceGateTests.cs` at 424 has 76 lines of headroom for the four gate
  tests `[P1-T1]` through `[P1-T4]` add, after `[P1-T1]` removes the four-step `GetConstructor`
  fallback chain.

Note: the research document (§8) records `QfcStreamingDequeueConfidenceGateTests.cs` at 373 and
`...Part2.cs` at 455; both files grew when PR #610 landed (D-Plan-9). The counts above are the
current tree's and supersede the research figures.

## Output Summary

Thirteen baseline line counts recorded. `QuickFiler/Controllers/QfcDatamodel.cs` is at 496 as the
acceptance condition requires. Three files sit close to the 500-line cap (496, 464, 460) and
carry explicit budget tasks in Phase 1.
