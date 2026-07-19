# Acceptance Criteria Check-Off Mapping

- Timestamp: 2026-07-19T11-05
- Task: [P9-T7]
- Work mode: full-feature (AC sources: `spec.md` `## Definition of Done` and `user-story.md` `## Acceptance Criteria`)

## spec.md `## Definition of Done` (8 items) -> satisfying task / evidence

| # | DoD item | Satisfying task(s) | Evidence |
|---|---|---|---|
| 1 | Every CS86xx-emitting `Threading/` `.cs` carries `#nullable enable` and compiles with zero CS86xx under per-file pragma + TWAE | Phases 1-8 + P9-T3 | `evidence/qa-gates/batch{1..8}-nullable-build.*.md`, `evidence/qa-gates/final-nullable-build.2026-07-19T11-05.md` (0 CS86xx across 25 files) |
| 2 | No project/solution `<Nullable>` element; `UtilitiesCS.csproj` retains none | P9-T5 | `evidence/qa-gates/csproj-no-nullable.2026-07-19T11-05.md` |
| 3 | Annotation/null-safety only; no behavior/API/concurrency-semantics change | Phases 1-8 + P9-T1/T2 | batch nullable-builds + regression-testing artifacts; `evidence/other/maintainer-flags.*.md` (branch/semantics preservation) |
| 4 | All UtilitiesCS MSTest tests pass, behavior-identical; no changed-line coverage regression | P9-T4 / P9-T6 | `evidence/qa-gates/final-coverage.2026-07-19T11-05.md` (4511/4511), `evidence/qa-gates/coverage-delta.2026-07-19T11-05.md` (PASS) |
| 5 | Full C# toolchain passes final pass using pragma-only type-check | P9-T1..T4 | `final-csharpier`, `final-analyzer-build`, `final-nullable-build`, `final-coverage` (all in `evidence/qa-gates/`) |
| 6 | `StoreLockupResponder` null-branches preserved exactly; identity chain annotated around them | P7-T3 / P7-T7 | `evidence/qa-gates/batch7-nullable-build.*.md`, `evidence/other/maintainer-flags.*.md` (P7-T7 section) |
| 7 | `*.Designer.cs` + 4 `.resx` not hand-edited, left oblivious; hand-partials own-field only | P1-T5, P5-T1..T5 | `evidence/qa-gates/batch5-nullable-build.*.md`, `final-nullable-build.*.md` (Designer/resx unchanged, no pragma) |
| 8 | `TimeOutTask.cs` 500-line breach flagged (not fixed); `ApplicationIdleTimer`/`AsyncMultiTasker` breach flagged not split | P4-T7, P8-T7 | `evidence/other/maintainer-flags.*.md` (P4-T7 + P8-T7 sections) |

All 8 DoD checkboxes updated to `[x]` in `spec.md`.

## user-story.md `## Acceptance Criteria` (8 items) -> satisfying task / evidence

The 8 user-story AC items are textually identical to the 8 DoD items and are satisfied by the same tasks/evidence:

| # | AC item | Satisfying task(s) | Evidence |
|---|---|---|---|
| 1 | Per-file `#nullable enable` + zero CS86xx under pragma + TWAE | Phases 1-8 + P9-T3 | batch{1..8}-nullable-build + final-nullable-build |
| 2 | No project/solution `<Nullable>` element | P9-T5 | csproj-no-nullable |
| 3 | Annotation/null-safety only; no concurrency-semantics change | Phases 1-8 + P9-T1/T2 | batch nullable-builds + regression-testing + maintainer-flags |
| 4 | All MSTest pass, behavior-identical; no changed-line coverage regression | P9-T4/T6 | final-coverage + coverage-delta |
| 5 | Full toolchain final pass, pragma-only type-check | P9-T1..T4 | final-csharpier/analyzer-build/nullable-build/coverage |
| 6 | `StoreLockupResponder` null-branch preserved exactly | P7-T3/T7 | batch7-nullable-build + maintainer-flags (P7-T7) |
| 7 | Designer/`.resx` not hand-edited, oblivious; hand-partials own-field only | P1-T5, P5-T1..T5 | batch5-nullable-build + final-nullable-build |
| 8 | `TimeOutTask.cs` 500-line flag + `ApplicationIdleTimer`/`AsyncMultiTasker` breach flag | P4-T7, P8-T7 | maintainer-flags (P4-T7, P8-T7) |

All 8 AC checkboxes updated to `[x]` in `user-story.md`. (The 3 `## Seeded Test Conditions` checkboxes in `spec.md` are also satisfied — existing suite passes post-annotation, no changed-line coverage regression, pragma-only nullable gate passes — and were checked off; evidence: `final-coverage`, `coverage-delta`, `final-nullable-build`.)

## Summary

- spec.md: 11/11 checkboxes checked (8 Definition of Done + 3 Seeded Test Conditions).
- user-story.md: 8/8 Acceptance Criteria checked.
- All items delivered and verified; no remaining unchecked criteria.
