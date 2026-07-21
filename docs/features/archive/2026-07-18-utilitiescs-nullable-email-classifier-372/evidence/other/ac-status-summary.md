# Acceptance Criteria Status Summary

Timestamp: 2026-07-19T06-40

Feature: utilitiescs-nullable-email-classifier (#372, Wave 1 of utilitiescs-nullable-remediation).
AC source files (full-feature mode): `spec.md` (near line 334) and `user-story.md` (near line 152).

| AC | Statement | Status | Supporting evidence |
|---|---|---|---|
| AC1 | Every in-scope `.cs` file emitting CS86xx carries `#nullable enable` and compiles with zero nullable diagnostics under the per-file pragma with `TreatWarningsAsErrors`. | PASS | `evidence/qa-gates/final-nullable-pragma-gate.md` (scoped gate EXIT 0, 0 CS86xx; solution-wide 0 CS86xx; 36 in-scope files carry the pragma). Per-batch gates `batch-{a..g}-nullable-gate.md`. |
| AC2 | No project-level `<Nullable>` element is introduced into `UtilitiesCS.csproj`. | PASS | `evidence/qa-gates/final-ac2-csproj-check.md` (csproj unchanged vs branch base; 0 `<Nullable>`). |
| AC3 | No behavior change; no scoring/model-path change; existing tests (incl. golden/property) pass unchanged. | PASS | `evidence/qa-gates/final-tests-coverage.md` (5702/5702 pass) + per-batch DO-NOT-ALTER constraint artifacts `batch-{a..g}-constraint.md`. |
| AC4 | No coverage regression on changed lines. | PASS | `evidence/qa-gates/final-coverage-delta.md` (repo line 83.78%->83.83%, branch 76.33%->76.36%; no previously-covered line un-covered). |
| AC5 | Public signatures of remediated members remain behavior-compatible; annotations reflect actual null behavior and honor #363 contracts. | PASS | `evidence/qa-gates/final-signature-compat.md` (additive nullability only; base/override + interface/implementer consistent; SubBayesianClassifier/SubClassifierGroup/SubCorpus contracts intact). |

## Acceptance Criteria Status
- Source: `docs/features/active/2026-07-18-utilitiescs-nullable-email-classifier-372/spec.md` and `docs/features/active/2026-07-18-utilitiescs-nullable-email-classifier-372/user-story.md`
- Total AC items: 5
- Checked off (delivered): 5
- Remaining (unchecked): 0
- Items remaining: none

## Measured remediation set (P0-T6, authoritative)
- 30 files emitted CS86xx (188 unique diagnostics); 6 additional REMEDIATE candidates were measured null-clean and still received the pragma (36 total). Both `Flags/` (Batch F, 5 files) and `Performance/` (Batch G, 3 files) were confirmed IN SCOPE. Measured count (30 emitting) aligns with the research ~30 static estimate; the epic ~18 planning figure undercounted the true CS86xx surface. The two interface-only files and the empty `Bayesian/SpamBayes.cs` stub remain EXCLUDE (interfaces were not forced by any implementer mismatch).
