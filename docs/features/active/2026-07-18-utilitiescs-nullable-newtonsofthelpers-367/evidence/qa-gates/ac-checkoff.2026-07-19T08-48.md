# Acceptance Criteria Check-Off (P9-T7)

- Timestamp: 2026-07-19T08-48
- Work Mode: full-feature — AC sources are `spec.md` (`## Definition of Done`) AND `user-story.md` (`## Acceptance Criteria`). Both updated to `[x]`.

## spec.md — Definition of Done

| DoD item | Satisfying task(s) | Evidence |
|---|---|---|
| Every `.cs` under `NewtonsoftHelpers/` emitting CS86xx carries `#nullable enable`, zero CS86xx under per-file pragma with `TreatWarningsAsErrors` | Phases 1-8 + P9-T3 | `qa-gates/batch1..8-nullable-build.*.md`, `qa-gates/final-nullable-build.*.md` |
| No project/solution `<Nullable>` element; csproj retains none | P9-T5 | `qa-gates/csproj-no-nullable.*.md` |
| Annotation/null-safety only; no behavior change/refactor | Phases 1-8 + P9-T1/T2 + git diff (19 `.cs` only) | batch `*-nullable-build.*.md`, `final-csharpier.*.md`, `csproj-no-nullable.*.md` |
| Framework-override signatures MATCHED to Newtonsoft 13.0.4 nullability | P3-T1/T2, P4-T1/T2/T3, P7-T2/T3/T4, P8-T1 | `qa-gates/batch3/4/7/8-nullable-build.*.md` |
| All existing MSTest tests pass; no coverage regression on changed lines | P9-T4/T6 | `qa-gates/final-coverage.*.md`, `qa-gates/coverage-delta.*.md`, batch `regression-testing/*` |
| Full C# toolchain passes on final pass, pragma-only type-check | P9-T1..T4 | `qa-gates/final-csharpier/analyzer-build/nullable-build/coverage.*.md` |
| Three wrapper 500-line pre-existing violations flagged, not split | P6-T4 | `other/maintainer-flags.*.md` (P6-T4 section) |
| Duplicate `PeopleScoConverter` confirmed live before finalizing; only in-scope copy annotated | P7-T1 | `other/maintainer-flags.*.md` (P7-T1 section) |
| `NLogTraceWriter.cs` annotated in place, GLOBAL namespace unchanged | P3-T2/T3 | `qa-gates/batch3-nullable-build.*.md`, `other/maintainer-flags.*.md` (P3-T3) |
| `NonRecursiveConverter.cs` pragma normalized to top, confirmed zero CS86xx | P4-T4 | `qa-gates/batch4-nullable-build.*.md` |

## spec.md — Seeded Test Conditions

| Item | Satisfying task | Evidence |
|---|---|---|
| Existing MSTest suite for UtilitiesCS still passes post-annotation | P9-T4 | `qa-gates/final-coverage.*.md` (4511 passed) |
| No coverage regression on changed lines | P9-T6 | `qa-gates/coverage-delta.*.md` |
| Nullable gate passes for opted-in files (pragma-only build) | P9-T3 | `qa-gates/final-nullable-build.*.md` |

## user-story.md — Acceptance Criteria

| AC item | Satisfying task(s) | Evidence |
|---|---|---|
| Every `.cs` under `NewtonsoftHelpers/` emitting CS86xx carries `#nullable enable`, zero nullable diagnostics under per-file pragma with `TreatWarningsAsErrors` | Phases 1-8 + P9-T3 | batch `*-nullable-build.*.md`, `final-nullable-build.*.md` |
| No project-level `<Nullable>` element in `UtilitiesCS.csproj` | P9-T5 | `qa-gates/csproj-no-nullable.*.md` |
| No behavior change; existing tests still pass; no coverage regression on changed lines | Phases 1-8 tests + P9-T4/T6 | `regression-testing/batch*-tests.*.md`, `qa-gates/final-coverage.*.md`, `qa-gates/coverage-delta.*.md` |

All spec.md (13: 10 DoD + 3 Seeded) and user-story.md (3) checkboxes are flipped to `[x]`. Criterion text preserved verbatim; only the checkbox changed.
