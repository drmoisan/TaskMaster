# Test Split Equivalence Verification — Cycle 2, Issue #218

Timestamp: 2026-06-28T17-31

Command: Line-by-line comparison of each moved test's method name and full body (attributes, arrange/act, assertions) between the four split files and the canonical originals in `QuickFiler.Test/Controllers/QfcHomeControllerTests.cs` (compiled original = canonical). No deletion performed in this task.

EXIT_CODE: 0

## Expected 27 moved test names (by split file)

RunAsync (6): Run_ExecutesCorrectly, RunAsync_ExecutesCorrectly, HighConfidencePreFilterLoader_CanBeOverridden_ForTesting, RunAsync_HighConfidenceDisabled_DoesNotPreFilterUsesPlainOverload, RunAsync_HighConfidenceDisabled_UsesPlainOverloadOnly, Worker_RunWorkerCompleted_HandlesCompletionCorrectly.

Iteration (6): IterateQueueAsync_DataModelComplete, IterateQueueAsync_QueueEmpty, IterateQueueAsync_Queue2, Iterate_ExecutesCorrectly, Iterate2_ExecutesCorrectly, SwapStopWatch_ExecutesCorrectly.

Metrics (2): QuickFileMetrics_WRITE_WhenGetCalendarReturnsNull_DoesNotThrow, GetMoveDiagnostics_NullAppointment_DoesNotThrow.

Property (13): Cleanup_ExecutesCorrectly, Loaded_PropertyWorksCorrectly, ExplorerController_PropertyWorksCorrectly, FormController_PropertyWorksCorrectly, KeyboardHandler_PropertyWorksCorrectly, DataModel_PropertyWorksCorrectly, FilerQueue_PropertyWorksCorrectly, UiScheduler_PropertyWorksCorrectly, StopWatch_PropertyWorksCorrectly, TokenSource_PropertyWorksCorrectly, Token_PropertyWorksCorrectly, WorkerComplete_PropertyWorksCorrectly, UiSyncContext_PropertyWorksCorrectly.

## Per-test EQUIVALENT/DIVERGED (split file line range vs original line range)

| # | Test | Split (file:lines) | Original (lines) | Result |
|---|------|--------------------|------------------|--------|
| 1 | Run_ExecutesCorrectly | RunAsync 115-173 | 281-339 | EQUIVALENT |
| 2 | RunAsync_ExecutesCorrectly | RunAsync 175-258 | 341-424 | EQUIVALENT |
| 3 | HighConfidencePreFilterLoader_CanBeOverridden_ForTesting | RunAsync 317-339 | 483-505 | EQUIVALENT |
| 4 | RunAsync_HighConfidenceDisabled_DoesNotPreFilterUsesPlainOverload | RunAsync 345-379 | 511-545 | EQUIVALENT |
| 5 | RunAsync_HighConfidenceDisabled_UsesPlainOverloadOnly | RunAsync 385-409 | 551-575 | EQUIVALENT |
| 6 | Worker_RunWorkerCompleted_HandlesCompletionCorrectly | RunAsync 411-446 | 577-612 | EQUIVALENT |
| 7 | IterateQueueAsync_DataModelComplete | Iteration 68-112 | 614-658 | EQUIVALENT |
| 8 | IterateQueueAsync_QueueEmpty | Iteration 114-173 | 660-719 | EQUIVALENT |
| 9 | IterateQueueAsync_Queue2 | Iteration 175-247 | 721-793 | EQUIVALENT |
| 10 | Iterate_ExecutesCorrectly | Iteration 249-290 | 795-836 | EQUIVALENT |
| 11 | Iterate2_ExecutesCorrectly | Iteration 292-320 | 838-866 | EQUIVALENT |
| 12 | SwapStopWatch_ExecutesCorrectly | Iteration 322-350 | 868-896 | EQUIVALENT |
| 13 | QuickFileMetrics_WRITE_WhenGetCalendarReturnsNull_DoesNotThrow | Metrics 73-151 | 1202-1280 | EQUIVALENT |
| 14 | GetMoveDiagnostics_NullAppointment_DoesNotThrow | Metrics 159-239 | 1288-1368 | EQUIVALENT |
| 15 | Cleanup_ExecutesCorrectly | Property 79-103 | 931-955 | EQUIVALENT |
| 16 | Loaded_PropertyWorksCorrectly | Property 105-117 | 957-969 | EQUIVALENT |
| 17 | ExplorerController_PropertyWorksCorrectly | Property 119-130 | 971-982 | EQUIVALENT |
| 18 | FormController_PropertyWorksCorrectly | Property 132-154 | 984-1006 | EQUIVALENT |
| 19 | KeyboardHandler_PropertyWorksCorrectly | Property 156-171 | 1008-1023 | EQUIVALENT |
| 20 | DataModel_PropertyWorksCorrectly | Property 173-191 | 1025-1043 | EQUIVALENT |
| 21 | FilerQueue_PropertyWorksCorrectly | Property 193-205 | 1045-1057 | EQUIVALENT |
| 22 | UiScheduler_PropertyWorksCorrectly | Property 207-229 | 1059-1081 | EQUIVALENT |
| 23 | StopWatch_PropertyWorksCorrectly | Property 231-253 | 1083-1105 | EQUIVALENT |
| 24 | TokenSource_PropertyWorksCorrectly | Property 255-277 | 1107-1129 | EQUIVALENT |
| 25 | Token_PropertyWorksCorrectly | Property 279-302 | 1131-1154 | EQUIVALENT |
| 26 | WorkerComplete_PropertyWorksCorrectly | Property 304-319 | 1156-1171 | EQUIVALENT |
| 27 | UiSyncContext_PropertyWorksCorrectly | Property 321-343 | 1173-1195 | EQUIVALENT |

## Supporting scaffolding equivalence

Each split [TestClass] independently reproduces the required Setup scaffolding (TestInitialize Setup, SetUpMockIntelRes, fields) verbatim from the original. The RunAsync split additionally reproduces SetupMockProgressTracker, SetupQfSettings, SetPrivateField, and the private ArrangeRunAsyncController helper used by tests #4 and #5 — all byte-equivalent to the original. The Property split reproduces SetPrivateField (used by WorkerComplete test #26).

Output Summary: All 27 moved tests are EQUIVALENT to their canonical originals — method names AND full bodies (attributes, arrange/act, assertions) match exactly. ZERO divergence; no split file required correction. Supporting private scaffolding in each split file matches the original. Safe to proceed to wiring (P2-T2) and trimming (P2-T3).
