# Acceptance-Criteria Check-Off Mapping (P10-T7)

- Timestamp: 2026-07-19T10-50
- Work Mode: full-feature (AC sources: `spec.md` AND `user-story.md`, tracked independently)
- Both source files updated to `[x]` for every satisfied checkbox (spec.md: 7 DoD + 6 AC + 4 Seeded Test Conditions = 17; user-story.md: 6 AC).

## spec.md — `## Definition of Done`

| DoD item | Status | Satisfying task(s) / evidence |
|---|---|---|
| Acceptance criteria documented and mapped to tests or demos | [x] | This artifact (P10-T7); AC1–AC6 mapped below. |
| Behavior matches acceptance criteria in all documented environments | [x] | All 4511 UtilitiesCS tests green (P10-T4); per-batch behavior-identical (Phases 1–9 test tasks). |
| Tests updated/added (unit/integration as applicable) | [x] | Annotation-only work; no new tests required. Existing tests treated as the spec and all pass (P10-T4). |
| Edge cases and error handling covered by tests | [x] | Existing edge/error tests unchanged and green; no behavior/edge changes introduced (P10-T4). |
| Docs updated (README, docs/features/active/... links) | [x] | Plan checklist + all batch/final evidence artifacts under `evidence/`; maintainer-flags for pre-existing conditions. |
| Telemetry/logging added or updated (if applicable) | [x] | Not applicable — no telemetry/logging added (annotation-only). |
| Toolchain pass completed (format -> lint -> type-check -> test) | [x] | P10-T1 CSharpier (0), P10-T2 analyzer (0 errors), P10-T3 pragma-only nullable (0 CS86xx), P10-T4 tests (4511/4511). |

## spec.md — AC1–AC6

| AC | Status | Satisfying task(s) / evidence |
|---|---|---|
| AC1 — every in-scope `.cs` emitting CS86xx carries `#nullable enable`, zero CS86xx under `/p:TreatWarningsAsErrors=true` per-file pragma | [x] | Phases 1–9 batch nullable builds; P10-T3 final gate — 0 CS86xx across all 30 files (`evidence/qa-gates/final-nullable-build.*`). |
| AC2 — no project/solution `<Nullable>` element | [x] | P10-T5 (`evidence/qa-gates/csproj-no-nullable.*`) — grep exit 1; UtilitiesCS.csproj/TaskMaster.sln git-clean. |
| AC3 — no behavior change; existing MSTest tests pass | [x] | P10-T4 — 4511/4511 tests green (`evidence/qa-gates/final-coverage.*`); per-batch regression tests (Phases 1–9). |
| AC4 — no coverage regression on changed lines | [x] | P10-T6 (`evidence/qa-gates/coverage-delta.*`) — in-scope 87.07% baseline == 87.07% final; no per-file rate regression. |
| AC5 — public signatures behavior-compatible; annotations reflect actual null behavior and consume #363/#364 contracts | [x] | Upstream gates P5-T1/P7-T1/P7-T2/P8-T1/P8-T2/P9-T1 verified; behavior-compat confirmed by keeping ETL/GetTableInViewAsync/EtlPrepAsync public tuples non-null so nullable-enabled out-of-scope consumers (DfDeedle/FrameUtilities) still compile (fix commit 2f6f3fec; total UtilitiesCS CS86xx = 0). |
| AC6 — COM-bound classes annotated for null-safety, COM/VSTO coverage exemption respected (no new tests forced around non-seamed COM code) | [x] | Phases 3–9 acceptance notes; no new tests added around COM-bound reflection/`InvokeMember`/`SaveAsFile` paths; seams (`EmailDetailsWrapper`/`IEmailDetailsWrapper`, `OutlookItemTry`/`OutlookItemTryGet`/`OutlookItemFlaggableTry`) preserved. `CidImageResolver.cs` (non-exempt) held to normal coverage (94.7%, P2-T4/P10-T6). |

## spec.md — Seeded Test Conditions

| Condition | Status | Evidence |
|---|---|---|
| Existing `UtilitiesCS.Test/OutlookObjects/` suite (incl. legacy-named duplicates) stays green | [x] | P10-T4 (4511/4511); per-batch tests covered both current-layout and legacy-named files. |
| Changed-line coverage does not regress vs baseline | [x] | P10-T6 (`coverage-delta.*`). |
| Pragma-driven gate produces zero CS86xx without `/p:Nullable=enable` | [x] | P10-T3 (`final-nullable-build.*`) — 0 CS86xx, `/p:Nullable=enable` NOT passed. |
| No new tests forced around non-seamed COM members; `CidImageResolver.cs` held to normal coverage | [x] | AC6 evidence above; P2-T4. |

## user-story.md — `## Acceptance Criteria` (independent tracking)

| AC | Status | Satisfying task(s) / evidence |
|---|---|---|
| AC1 — per-file `#nullable enable`, zero CS86xx under TWAE per-file pragma | [x] | P10-T3 (`final-nullable-build.*`); Phases 1–9. |
| AC2 — no `<Nullable>` element | [x] | P10-T5 (`csproj-no-nullable.*`). |
| AC3 — no behavior change; existing MSTest tests pass | [x] | P10-T4 (4511/4511). |
| AC4 — no coverage regression on changed lines | [x] | P10-T6 (`coverage-delta.*`). |
| AC5 — public signatures behavior-compatible; consume #363/#364 contracts | [x] | Upstream gates + behavior-compat fix (commit 2f6f3fec). |
| AC6 — COM/VSTO coverage exemption respected, no new tests forced | [x] | Phases 3–9 acceptance notes; seams preserved. |

## AC Status Summary

- Sources: `spec.md` (Definition of Done + AC1–AC6 + Seeded Test Conditions) and `user-story.md` (Acceptance Criteria).
- spec.md: 17/17 checkboxes checked off (0 remaining).
- user-story.md: 6/6 AC checked off (0 remaining).
