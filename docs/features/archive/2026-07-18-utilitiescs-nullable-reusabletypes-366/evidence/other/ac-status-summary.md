# Acceptance Criteria Status Summary (P9-T11)

Timestamp: 2026-07-19T22-03

- Source: `docs/features/active/2026-07-18-utilitiescs-nullable-reusabletypes-366/issue.md` (`## Acceptance Criteria`)
- Total AC items: 6
- Checked off (delivered): 6
- Remaining (unchecked): 0

| AC | Status | Supporting evidence |
|---|---|---|
| AC1 — every CS86xx-emitting ReusableTypeClasses file carries `#nullable enable` and compiles with zero nullable diagnostics under the per-file pragma + `TreatWarningsAsErrors` | PASS | `evidence/qa-gates/batch-1..8-nullable-gate.md`; `evidence/qa-gates/batch-8-nullable-gate.md` (0 CS86xx / 0 CS8714 in cluster incl. four waived lines); `evidence/qa-gates/final-nullable-pragma-gate.md` (isolated-cluster operative PASS) |
| AC2 — no project-level `<Nullable>` element in `UtilitiesCS.csproj` | PASS | `evidence/qa-gates/final-ac2-csproj-check.md` (0 occurrences); `evidence/baseline/baseline-csproj-nullable-absent.md` |
| AC3 — no behavior change; existing tests still pass | PASS | `evidence/qa-gates/final-tests-coverage.md` (5702/5702); `evidence/regression-testing/batch-8-tests.md` |
| AC4 — no coverage regression on changed lines | PASS | `evidence/qa-gates/final-coverage-delta.md` (line 83.79% -> 83.88%; branch stable; changed lines are annotation-only, non-executable) |
| AC5 — public signatures remain behavior-compatible; annotations reflect actual null behavior | PASS | `evidence/qa-gates/final-signature-compat.md`; `evidence/qa-gates/final-scope-guards.md` (no record/init, no file split) |
| AC6 — non-opted-in files elsewhere are not cross-blocked | PASS | `evidence/qa-gates/final-nullable-pragma-gate.md` (all solution-wide nullable/vendored errors are sibling-owned/vendored, none in a #366 file); `evidence/qa-gates/final-constraint-and-exemption-check.md` (three exempt WinForms files null-oblivious; only four NewtonsoftHelpers waiver files touched by #366) |

## Notes

- The `where TKey : notnull` constraint (ratified [P6-T2]) is applied to the THREE truly generic
  bases (`ConcurrentObservableDictionary`, `ScoDictionaryNew`, `ScDictionary`) and the FOUR
  cross-child NewtonsoftHelpers waiver consumers (`WrapperScoDictionary.cs`,
  `ScoDictionaryConverter.cs`, `WrapperScDictionary.cs`, `ScDictionaryConverter.cs`) under the
  epic-authorized Option-A'' four-file waiver. `ScoDictionaryStatic` (non-generic `static class`)
  and the `ConcurrentBag<T>`-based `ConcurrentObservableBag`/`ScBag` are correctly NOT constrained.
- P9-T3 solution-wide pragma gate carries an EXPECTED cross-child fan-in deviation (~148 CS86xx in
  sibling-owned `EmailIntelligence`/`OutlookObjects` files + 2 vendored SVGControl CS0649),
  attributable to sibling children and vendored code, NOT a #366 failure; the #376 capstone owns the
  solution-wide close-out.
- No post-condition attributes and no polyfill were introduced
  (`evidence/qa-gates/final-no-postcondition-attrs.md`).
