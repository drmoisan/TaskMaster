Timestamp: 2026-07-20T15-15

## AC-5 check-off (revised) for issue #392

The plan of record was revised (P2-T3 amended) and `issue.md`'s AC-5 text was amended by the
orchestrator on 2026-07-20 with an explicit scope note: nullable enforcement is scoped to
first-party projects per `.claude/rules/csharp.md`; pre-existing, byte-identical-to-baseline
vendored `SVGControl.csproj` nullable errors are explicitly non-blocking and tracked separately in
`docs/features/potential/2026-07-07-ci-nullable-check-skipped-vendored-projects.md`.

Component-by-component status under the amended wording:
- CSharpier format: PASS for the two Scope-Lock-authorized files (`evidence/qa-gates/csharpier-final-392.2026-07-20T14-20.md`).
- .NET analyzers build: PASS, EXIT_CODE 0 (`evidence/qa-gates/analyzer-final-392.2026-07-20T14-24.md`).
- Nullable build (first-party scope): PASS under the amended AC-5 wording. Full-recompile run
  (`evidence/qa-gates/nullable-final-392.2026-07-20T15-10.md`) shows EXIT_CODE 1 overall, but the
  error-set comparison against the P0-T11 baseline shows **zero NEW errors** and **zero errors
  attributable to first-party in-scope files** — all 34 errors are byte-identical-to-baseline,
  confined to vendored `SVGControl.csproj`, and are explicitly excluded from this AC's scope per the
  amended wording.
- MSTest via vstest.console.exe: PASS, 541/541 tests passed, 0 failed (`evidence/qa-gates/vstest-coverage-final-392.2026-07-20T14-32.md`).
- Zero regressions: PASS (`evidence/qa-gates/regression-check-392.2026-07-20T14-42.md`).
- New/changed code >= 90% coverage: PASS, 100% observed (`evidence/qa-gates/coverage-delta-392.2026-07-20T14-38.md`).

**Decision: AC-5 is checked off (`- [x]`)** in `issue.md`. All six components pass under the amended,
first-party-scoped wording.

## Final AC status for issue #392

| AC | Status | Backing evidence |
|---|---|---|
| AC-1 | `[x]` Checked | `evidence/regression-testing/fail-before-392.2026-07-20T14-05.md`, `evidence/regression-testing/pass-after-392.2026-07-20T14-10.md` |
| AC-2 | `[x]` Checked | `evidence/other/root-cause-392.2026-07-20T13-50.md`, same fail-before/pass-after evidence |
| AC-3 | `[x]` Checked | `evidence/regression-testing/targeted-no-regression-392.2026-07-20T14-13.md` |
| AC-4 | `[x]` Checked | `evidence/other/root-cause-392.2026-07-20T13-50.md`, same fail-before/pass-after evidence |
| AC-5 | `[x]` Checked | `evidence/qa-gates/csharpier-final-392.2026-07-20T14-20.md`, `evidence/qa-gates/analyzer-final-392.2026-07-20T14-24.md`, `evidence/qa-gates/nullable-final-392.2026-07-20T15-10.md` (error-set comparison), `evidence/qa-gates/vstest-coverage-final-392.2026-07-20T14-32.md`, `evidence/qa-gates/regression-check-392.2026-07-20T14-42.md`, `evidence/qa-gates/coverage-delta-392.2026-07-20T14-38.md`, `evidence/qa-gates/coverage-conversion-392.2026-07-20T14-50.md` |

All 5 acceptance criteria are now checked off.
