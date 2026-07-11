# Phase 6 — Acceptance Criteria Mapping

Timestamp: 2026-07-10T23-59

| AC | Description | Phases/Tasks | Evidence |
|---|---|---|---|
| AC1 | `KbdActions._list` re-pointed to `List<UClass>`; no `Swordfish.NET.Collections` reference; public API unchanged; existing `KbdActions` tests pass. | Phase 1 (P1-T1..T5), Phase 5 (P5-T1), Phase 6 (P6-T1..T5) | `evidence/qa-gates/phase1-kbdactions-build.2026-07-10T20-20.md`, `evidence/regression-testing/kbdactions-regression.2026-07-10T20-20.md`, `evidence/qa-gates/final-csharpier.2026-07-10T20-20.md`, `evidence/qa-gates/final-analyzer-build.2026-07-10T20-20.md`, `evidence/qa-gates/final-nullable-build.2026-07-10T20-20.md`, `evidence/qa-gates/final-mstest-coverage.2026-07-10T20-20.md`, `evidence/qa-gates/coverage-delta.2026-07-10T20-20.md` |
| AC2 | `using Swordfish.NET.Collections;` removed from `KeyboardHandler.cs`, `FlagDetails.cs`, `FolderRemapController.cs`; solution rebuilds clean. | Phase 2 (P2-T1..T4), Phase 6 build gates (P6-T2, P6-T3) | `evidence/qa-gates/phase2-unused-using-build.2026-07-10T20-20.md`, `evidence/qa-gates/final-analyzer-build.2026-07-10T20-20.md`, `evidence/qa-gates/final-nullable-build.2026-07-10T20-20.md` |
| AC3 | Stale `"UtilitiesSwordfish.NET.General"` / `"UtilitiesSwordfish.NET.Test"` literals deleted from `TraceUtility.cs`. | Phase 3 (P3-T1..T3), Phase 6 build gates (P6-T2, P6-T3) | `evidence/qa-gates/phase3-traceutility-build.2026-07-10T20-20.md`, `evidence/qa-gates/final-analyzer-build.2026-07-10T20-20.md`, `evidence/qa-gates/final-nullable-build.2026-07-10T20-20.md` |
| AC4 | No Sco* lineage class or its consumers modified beyond the `KbdActions` swap; `UtilitiesSwordfish` project, `ProjectReference` entries, and `TaskMaster.sln` untouched. | Phase 4 (P4-T1, P4-T2) | `evidence/other/scope-boundary-diff.2026-07-10T20-20.md`, `evidence/other/post-change-swordfish-inventory.2026-07-10T20-20.md` |
| AC5 | Full C# toolchain (CSharpier, .NET analyzers, nullable, MSTest) passes; changed/new code meets coverage thresholds with no regression on changed lines. | Phase 6 (P6-T1..T5) | `evidence/qa-gates/final-csharpier.2026-07-10T20-20.md`, `evidence/qa-gates/final-analyzer-build.2026-07-10T20-20.md`, `evidence/qa-gates/final-nullable-build.2026-07-10T20-20.md`, `evidence/qa-gates/final-mstest-coverage.2026-07-10T20-20.md`, `evidence/qa-gates/coverage-delta.2026-07-10T20-20.md` |

No AC is left unmapped. All five ACs (AC1-AC5) have a complete, evidence-backed phase/task
mapping.
