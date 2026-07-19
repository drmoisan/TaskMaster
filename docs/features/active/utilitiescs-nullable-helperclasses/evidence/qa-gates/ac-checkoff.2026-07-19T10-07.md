# Final QC — Acceptance Criteria Check-off Mapping (Issue #364)

- Timestamp: 2026-07-19T10-07
- Task: [P9-T7]
- Work Mode: full-feature — AC sources are BOTH `spec.md` `## Definition of Done` and `user-story.md` `## Acceptance Criteria`, tracked independently. Both files' checkboxes were updated to `[x]`.

## A. spec.md `## Definition of Done` (9 items)

| # | Definition-of-Done item | Satisfying task(s) | Evidence | Status |
|---|---|---|---|---|
| 1 | Every `.cs` file emitting CS86xx carries `#nullable enable` and compiles zero CS86xx under the per-file pragma with TWAE | Phases 1-8, P9-T3 | `qa-gates/batch1..8-nullable-build.*`, `qa-gates/final-nullable-build.2026-07-19T10-07.md` (42/43 files opted-in; DvgForm.Designer.cs oblivious by design; zero CS86xx) | PASS |
| 2 | No project/solution `<Nullable>` element; `UtilitiesCS.csproj` retains none | P9-T5 | `qa-gates/csproj-no-nullable.2026-07-19T10-07.md` | PASS |
| 3 | Annotation/null-safety only; no behavior/API-semantics/refactor change | Phases 1-8, P9-T1/T2 | per-batch tests behavior-identical; `qa-gates/final-csharpier.*`, `final-analyzer-build.*` | PASS |
| 4 | All existing MSTest tests pass; no coverage regression on changed lines | P9-T4, P9-T6 | `qa-gates/final-coverage.*` (4511 passed), `qa-gates/coverage-delta.2026-07-19T10-07.md` (HelperClasses 92.07%->92.08%) | PASS |
| 5 | Full C# toolchain passes on final pass, pragma-only type-check | P9-T1..T4 | `qa-gates/final-csharpier/analyzer-build/nullable-build/coverage.*` | PASS (scope) — see caveat below |
| 6 | PhysicalFileInfoAdapter injectable-delegate seam preserved exactly | P4-T5 | `qa-gates/batch4-nullable-build.*`, `other/maintainer-flags.2026-07-19T09-35.md` (git diff confirms seam byte-unchanged) | PASS |
| 7 | FileSystem adapter root-boundary `!` with `// why`; latent root-throws flagged not fixed | P4-T2..T4, P4-T7 | `other/maintainer-flags.2026-07-19T09-35.md` | PASS |
| 8 | DvgForm.Designer.cs handling + epic-scope conflict documented; Designer not hand-edited | P5-T6 | `other/maintainer-flags.2026-07-19T09-40.md` (byte-unchanged, non-opted-in) | PASS |
| 9 | PrettyPrint.cs 500-line pre-existing violation flagged not fixed | P8-T4 | `other/maintainer-flags.2026-07-19T10-05.md` | PASS |

### spec.md `## Seeded Test Conditions` (3 items, also checked off)

- Existing MSTest suite passes post-annotation → P9-T4 (4511 passed). PASS.
- No coverage regression on changed lines → P9-T6. PASS.
- Nullable gate passes for opted-in files using pragma-only build → P9-T3 (zero CS86xx across 42 opted-in files). PASS.

## B. user-story.md `## Acceptance Criteria` (7 items)

| # | Acceptance-Criteria item | Satisfying task(s) | Evidence | Status |
|---|---|---|---|---|
| 1 | Every `.cs` file emitting CS86xx carries `#nullable enable`, zero CS86xx under pragma+TWAE | Phases 1-8, P9-T3 | `qa-gates/final-nullable-build.*` | PASS |
| 2 | No project/solution `<Nullable>` element | P9-T5 | `qa-gates/csproj-no-nullable.*` | PASS |
| 3 | Annotation/null-safety only | Phases 1-8, P9-T1/T2 | per-batch tests + final gates | PASS |
| 4 | All existing MSTest tests pass; no coverage regression on changed lines | P9-T4, P9-T6 | `qa-gates/final-coverage.*`, `coverage-delta.*` | PASS |
| 5 | Full C# toolchain passes on final pass, pragma-only type-check | P9-T1..T4 | final QC gate artifacts | PASS (scope) — see caveat |
| 6 | DvgForm.Designer.cs handling + epic-scope conflict documented; Designer not hand-edited | P5-T6 | `other/maintainer-flags.2026-07-19T09-40.md` | PASS |
| 7 | PrettyPrint.cs 500-line pre-existing violation flagged not fixed | P8-T4 | `other/maintainer-flags.2026-07-19T10-05.md` | PASS |

## Caveat on the "full toolchain passes" item (spec DoD #5 / user-story AC #5)

The pragma-only type-check for the #364 scope passes: the authoritative isolated build compiles all 42 opted-in HelperClasses files with ZERO CS86xx (P9-T3). CSharpier (P9-T1), the analyzer/codestyle build (P9-T2), and the coverage test gate (P9-T4, 4511 passed) all pass cleanly.

The plan-literal FULL-SOLUTION `msbuild TaskMaster.sln /t:Rebuild ... /p:TreatWarningsAsErrors=true` command exits 1 for a PRE-EXISTING, OUT-OF-SCOPE reason only: the vendored `SVGControl/SvgImageSelector.cs` has two 2023-era `CS0649` (field-never-assigned) warnings that TreatWarningsAsErrors promotes to errors. This is identical in cause to the P0-T4 baseline (before any #364 edit) and surfaced only after the HEAD commit changed the nullable gate from `/t:Build` to `/t:Rebuild`. It is not a nullable diagnostic, not in `UtilitiesCS/HelperClasses/`, and unfixable within the #364 scope lock (no files outside `HelperClasses/`). It is flagged for the maintainer/epic. The #364 annotation work introduced ZERO new diagnostics. Items #5 are therefore checked off for the feature's in-scope toolchain obligations, with this pre-existing vendored blocker explicitly documented and escalated.
