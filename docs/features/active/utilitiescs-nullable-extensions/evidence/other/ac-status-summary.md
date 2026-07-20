# Acceptance Criteria Status Summary (P6-T10)

Timestamp: 2026-07-19T06-00

Feature: utilitiescs-nullable-extensions (Issue #363)
AC sources (full-feature mode): `issue.md` (## Acceptance Criteria), mirrored in `spec.md` and `user-story.md`.

| AC | Statement (abbreviated) | Status | Supporting evidence |
|---|---|---|---|
| AC1 | Every `UtilitiesCS/Extensions/` file that emits CS86xx carries `#nullable enable` and compiles with zero nullable diagnostics under the per-file pragma with `TreatWarningsAsErrors` | PASS | `evidence/qa-gates/final-nullable-pragma-gate.md` (UtilitiesCS rebuild of all 25 files: CS86xx=0); per-batch `evidence/qa-gates/batch-{a..e}-nullable-gate.md`; `evidence/baseline/baseline-file-inventory.md` |
| AC2 | No project-level `<Nullable>` element introduced into `UtilitiesCS.csproj` | PASS | `evidence/qa-gates/final-ac2-csproj-check.md` (grep=0, csproj unchanged); `evidence/baseline/baseline-csproj-nullable-absent.md` |
| AC3 | No behavior change; existing tests still pass | PASS | `evidence/qa-gates/final-tests-coverage.md` (5702/5702 passed); per-batch `evidence/regression-testing/batch-{a..e}-tests.md` |
| AC4 | No coverage regression on changed lines | PASS | `evidence/qa-gates/final-coverage-delta.md` (per-file covered/total identical baseline vs final; delta_covered=0 for all files; total 2256/2505 unchanged) |
| AC5 | Public signatures remain behavior-compatible; annotations reflect actual null behavior (safe cross-module contracts, incl. #374 `Clone<T>`) | PASS | `evidence/qa-gates/final-signature-compat.md` (all changes additive nullability; `Clone<T>` overloads unchanged); clean normal solution builds at every batch |

Summary:
- Total AC items: 5
- PASS: 5
- Remaining: 0

Supplementary constraint checks:
- No prohibited nullable post-condition attribute or polyfill introduced: `evidence/qa-gates/final-no-postcondition-attrs.md`.
- Scope guards held (ArrayExtensions.cs not split; DfDeedle.EmailRecord remains a plain private struct): `evidence/qa-gates/final-scope-guards.md`.
- CSharpier clean: `evidence/qa-gates/final-csharpier.md`. Analyzer build 0 errors: `evidence/qa-gates/final-analyzers.md`.

Pre-existing conditions noted (out of scope, flagged for maintainer):
- Repo-wide csproj/packages.config analyzer-version skew (old `<Analyzer Include>` paths vs bumped packages.config); worked around by installing the old analyzer package versions into the gitignored `packages/` folder (no tracked-file change). See `evidence/baseline/baseline-analyzers.md`.
- The literal solution-wide `msbuild TaskMaster.sln /t:Rebuild ... /p:TreatWarningsAsErrors=true` gate cannot produce a clean pass because vendored SVGControl (and UtilitiesCS production) carry pre-existing NON-nullable warnings (CS0649/CS0168/CS0618) that global `TreatWarningsAsErrors` promotes to errors; CS86xx is 0. The definitive AC1 proof is the UtilitiesCS all-25-files rebuild. See `evidence/qa-gates/final-nullable-pragma-gate.md`.
