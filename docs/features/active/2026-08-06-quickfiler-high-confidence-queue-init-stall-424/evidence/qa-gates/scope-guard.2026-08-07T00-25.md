# [P5-T3] Scope Guard — PASS

- **Issue:** #424
- **Task:** [P5-T3]

Timestamp: 2026-08-07T00-25

Command: `git status --porcelain` (against the `[P0-T3]` baseline; the pre-existing `.claude/agent-memory/` and feature-folder entries recorded there are permitted and filtered out)

EXIT_CODE: 0

Output Summary: **16 changed files — 11 modified, 5 new. Every entry is justified against the plan's expected-files set; zero forbidden-path changes.**

## Forbidden-path scan — all zero

| Guarded path / symbol | Changed files |
|---|---|
| `.claude/rules/` | **0** |
| `.claude/skills/` | **0** |
| `.github/instructions/` | **0** |
| `QfSettings` | **0** |
| `IAppQuickFilerSettings` | **0** |
| `Settings.Designer.cs` | **0** |
| `TaskMaster/Ribbon/` | **0** |
| `QuickFiler/Controllers/QfcHomeController.Iteration.cs` | **0** |
| `FolderPredictor` (UtilitiesCS scoring/prediction internals) | **0** |
| `MailItemHelper` (UtilitiesCS scoring/prediction internals) | **0** |

## Changed-file justification

### Production (6 files + 1 project file)

| File | Justification |
|---|---|
| `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs` | [P1-T3] first-batch deadline; [P2-T2] progress callback. Requirements-table file. |
| `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` | [P3-T3] liveness flag declaration + `sourceActive`/`WaitForQueue` rewire; [P4-T4] new overload implementation. Requirements-table file. |
| `QuickFiler/Controllers/QfcDatamodel.cs` | [P3-T3] flag set before both `RunWorkerAsync()` sites and clear in `Worker_DoWork`'s `finally`. Requirements-table file. |
| `QuickFiler/Controllers/QfcHomeController.cs` | [P4-T5] mapper construction, new overload call, O1 poll 1000 -> 200 ms at the pre-UI call site. Requirements-table file. |
| `QuickFiler/Interfaces/IQfcDatamodel.cs` | [P4-T4] new four-argument overload. Requirements-table file. |
| `QuickFiler/Controllers/QfcScanProgressBandMapper.cs` *(new)* | [P4-T2] new testable band-mapping module. |
| `QuickFiler/QuickFiler.csproj` | [P4-T2] `<Compile Include>` for the new mapper (legacy non-SDK project, no globbing). |

### Tests (7 files + 1 project file)

| File | Justification |
|---|---|
| `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs` | [P1-T1] `partial` keyword; [P1-T3]/[P2-T2] `CreateGate` reflection-helper shape updates. |
| `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part2.cs` *(new)* | [P1-T1] and Phase 1 deadline/cancellation/logging tests. |
| `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part3.cs` *(new)* | [P5-T2] pre-decided fallback relocation of the Phase 2 progress-callback tests. |
| `QuickFiler.Test/Controllers/QfcDatamodelTests.cs` | [P3-T5] pinned polling test retargeted to the flag; [P3-T6] `WaitForQueue` test region (lines 281-309) retargeted to the flag; [P5-T2] liveness region relocated out. |
| `QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs` *(new)* | [P5-T2] pre-decided fallback relocation of the Phase 3 liveness tests. |
| `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.cs` | [P4-T6] exact-argument overload update; [P4-T8] band-mapping and empty-batch tests. |
| `QuickFiler.Test/Controllers/QfcScanProgressBandMapperTests.cs` *(new)* | [P4-T3] mapper unit tests. |
| `QuickFiler.Test/Controllers/QfcHomeControllerIssue218Tests.cs` | [P4-T7] overload-shape hunks only (4 hunks, `Setup`/`Verify` matchers). Reclassified as in-scope per Decisions Record item 14. |
| `QuickFiler.Test/QuickFiler.Test.csproj` | `<Compile Include>` entries for the four new test files. |

### Baseline allowance (filtered, not part of this change)

The 11 `.claude/agent-memory/**` entries and the feature folder `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/` were present at the `[P0-T3]` baseline and are permitted by that task's recorded allowance. They do not trip this gate.

## Out-of-scope defects — recorded, not fixed

Per `spec.md` Scope & Non-Goals, research §11, and the execution directive, the following were left untouched and remain follow-up candidates:

- `EmailMoveMonitor` hook retention for gate-rejected items.
- Post-`Show()` double-scoring of accepted items / dormant `QfcPreScoredItem` carrier path.
- `Worker_RunWorkerCompleted` early UI enablement and further `BackgroundWorker` lifecycle rework.
- Frame building (`InitDf*`/`DfDeedle`).
- Pre-existing `CS2002` duplicate `PercentageFormatterTests.cs` `<Compile>` include in `UtilitiesCS.Test.csproj` (observed in `[P0-T5]`).
- Pre-existing flaky `UtilitiesCS.Test` dispatcher test under coverage instrumentation (characterized in `[P0-T7]`).

## Recorded in-scope behavior change

The legacy synchronous `DequeueNextItemGroup(int)` path and the post-UI iteration call site (`QfcHomeController.Iteration.cs:23`) both now inherit `DefaultFirstBatchDeadline` (12 s) through the two-argument overload's delegation and parameter defaults. `QfcHomeController.Iteration.cs` itself is **byte-unmodified** (0 changed files above), and the `DequeueNextItemGroupAsync(8, 2000)` exact-argument pin passes ([P5-T1]). This inheritance is deliberate and documented in [P4-T4].
