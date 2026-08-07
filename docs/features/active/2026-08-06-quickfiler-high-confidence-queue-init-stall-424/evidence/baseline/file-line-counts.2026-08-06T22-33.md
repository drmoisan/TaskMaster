# [P0-T8] Baseline File Line Counts — Baseline Evidence

- **Issue:** #424
- **Task:** [P0-T8]
- **Limit under enforcement:** 500 lines per file (`.claude/rules/general-code-change.md`, `CLAUDE.md` § Module & File Structure)

Timestamp: 2026-08-06T22-33

Command: `for f in <8 paths>; do wc -l < "$f"; done` (run from repo root `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-04T18-38`)

EXIT_CODE: 0

Output Summary: All 8 files measured; **every count matches Decisions Record item 8 exactly — zero discrepancies.** Two production files sit within 25 lines of the 500-line limit and carry pre-decided fallback splits in `[P5-T2]`.

| # | File | Baseline lines | Decisions item 8 | Match | Headroom to 500 |
|---|---|---|---|---|---|
| 1 | `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs` | 106 | 106 | yes | 394 |
| 2 | `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` | 139 | 139 | yes | 361 |
| 3 | `QuickFiler/Controllers/QfcDatamodel.cs` | 479 | 479 | yes | **21** |
| 4 | `QuickFiler/Controllers/QfcHomeController.cs` | 477 | 477 | yes | **23** |
| 5 | `QuickFiler/Interfaces/IQfcDatamodel.cs` | 40 | 40 | yes | 460 |
| 6 | `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs` | 300 | 300 | yes | 200 |
| 7 | `QuickFiler.Test/Controllers/QfcDatamodelTests.cs` | 313 | 313 | yes | 187 |
| 8 | `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.cs` | 254 | 254 | yes | 246 |

## New files this plan creates (baseline: do not exist)

| File | Baseline state | Created by |
|---|---|---|
| `QuickFiler/Controllers/QfcScanProgressBandMapper.cs` | absent | [P4-T2] |
| `QuickFiler.Test/Controllers/QfcScanProgressBandMapperTests.cs` | absent | [P4-T3] |
| `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part2.cs` | absent | [P1-T1] |

## Pre-decided fallback splits (no ad-hoc splits permitted)

Recorded here so `[P5-T2]` has an unambiguous reference:

| If this exceeds 500 | Relocate | To |
|---|---|---|
| `QuickFiler/Controllers/QfcHomeController.cs` | `RunAsync` verbatim | new partial `QuickFiler/Controllers/QfcHomeController.Run.cs` |
| `QuickFiler/Controllers/QfcDatamodel.cs` | `ScoreRemainingQueueMailItemAsync` verbatim | existing partial `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` |
| `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part2.cs` | Phase 2 progress-callback tests verbatim | new partial `...GateTests.Part3.cs` (no `[TestClass]`) |
| `QuickFiler.Test/Controllers/QfcDatamodelTests.cs` | Phase 3 liveness tests verbatim | new `[TestClass]` `QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs` |

Each new test file additionally requires an explicit `<Compile Include>` item in `QuickFiler.Test/QuickFiler.Test.csproj` (legacy non-SDK project; no globbing).
