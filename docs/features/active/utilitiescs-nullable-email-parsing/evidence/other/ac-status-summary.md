# Acceptance Criteria Status Summary

Timestamp: 2026-07-19T07-55

Source files (full-feature work mode): `issue.md`, `spec.md`, `user-story.md` — all three
checked off identically (AC1–AC6 defined consistently across all three documents).

| AC | Status | Supporting Evidence |
|---|---|---|
| AC1 — Every `.cs` file in the cluster that emits CS86xx carries `#nullable enable` and compiles with zero nullable diagnostics under the per-file pragma with `TreatWarningsAsErrors` | PASS | `evidence/qa-gates/batch-{a,b,c,d,e,f,g}-nullable-gate.md` (per-batch, zero CS86xx); `evidence/qa-gates/final-nullable-pragma-gate.md` (all 24 files, zero CS86xx, solution-wide confirmation) |
| AC2 — No project-level `<Nullable>` element is introduced into `UtilitiesCS.csproj` | PASS | `evidence/baseline/baseline-csproj-nullable-absent.md` (baseline: absent); `evidence/qa-gates/final-ac2-csproj-check.md` (end state: still absent) |
| AC3 — No behavior change to parsing/sorting logic; existing tests still pass | PASS | `evidence/baseline/baseline-tests-coverage.md` (5702/5702 baseline); `evidence/regression-testing/batch-{a..g}-tests.md` (5702/5702 after each batch); `evidence/qa-gates/final-tests-coverage.md` (5702/5702 final) |
| AC4 — No coverage regression on changed lines | PASS | `evidence/qa-gates/final-coverage-delta.md` (baseline 87.30% → post-change 87.47% aggregate for the 24-file cluster; no per-file ratio decrease) |
| AC5 — Public signatures of the remediated types remain behavior-compatible; nullability annotations reflect actual null behavior and are consistent with the upstream `utilitiescs-nullable-extensions` annotation contracts they consume | PASS | `evidence/qa-gates/final-signature-compat.md` (per-file signature-change table, all additive) |
| AC6 — Non-remediated files remain non-opted-in and are not cross-blocked; the change is independently mergeable under the per-file pragma architecture | PASS | `evidence/qa-gates/final-ac6-no-cross-block.md` (exactly 24 files touched, no other file modified) |

## Overall Result

All 6 acceptance criteria are PASS. No AC is marked remediation-required.

## Scope-Invariant Compliance (supporting AC3/AC5/AC6)

- `evidence/qa-gates/final-scope-guards.md`: `SortEmail.cs` (1408 lines), `EmailTokenizer.cs`
  (730 lines), and `SubjectMapEntry.cs` (658 lines) were not split; `FolderStruct` and
  `SpamBayesOptions` remain plain structs (no `record`/`record struct` conversion);
  `SubjectMapMetrics.Designer.cs` was not modified.
- `evidence/qa-gates/final-no-postcondition-attrs.md`: no nullable post-condition attribute or
  `System.Diagnostics.CodeAnalysis` polyfill was introduced anywhere in the repository.

## Non-Blocking Observation (recorded, not a gap in any AC)

`evidence/qa-gates/final-analyzers.md` records that 3 pre-existing `UtilitiesCS.Test` files
already carrying their own `#nullable enable` (2 of which intentionally exercise a
production null-guard with a literal `null` argument) now surface `CS8625` warnings as a side
effect of this feature's per-file pragma opt-in — this does not violate AC1 (scoped to the
24-file cluster only, not test files), AC3 (tests still execute and pass, 5702/5702), or AC6
(these test files were not opted into `#nullable` by this feature; the cross-file interaction
is exposed, not caused). Flagged for the maintainer as a possible small follow-up outside this
plan's scope.
