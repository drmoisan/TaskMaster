Timestamp: 2026-07-20T14-55

## AC-5 check-off decision for issue #392

AC-5 text: "The full C# toolchain passes in order (CSharpier format, .NET analyzers build, nullable
build, MSTest via vstest.console.exe) with zero regressions, and new/changed code meets the >= 90%
coverage target."

Component-by-component status:
- CSharpier format: PASS for the two Scope-Lock-authorized files (see
  `evidence/qa-gates/csharpier-final-392.2026-07-20T14-20.md`). Repo-wide pre-existing formatting
  noise (32 files) is unchanged from baseline and out of Scope-Lock.
- .NET analyzers build: PASS, EXIT_CODE 0 (see `evidence/qa-gates/analyzer-final-392.2026-07-20T14-24.md`).
- Nullable build: **FAIL, EXIT_CODE 1** (see `evidence/qa-gates/nullable-final-392.2026-07-20T14-28.md`).
  All 34 errors are confined to `SVGControl.csproj` (a vendored third-party control library), a
  byte-for-byte reproduction of the pre-existing baseline failure documented in
  `evidence/baseline/nullable-baseline.2026-07-20T13-35.md`. No error is attributable to either
  Scope-Lock-authorized file. Fixing this would require modifying `SVGControl.csproj`, which this
  plan's Scope-Lock explicitly forbids ("No other file may be changed by this plan").
- MSTest via vstest.console.exe: PASS, 541/541 tests passed, 0 failed (see
  `evidence/qa-gates/vstest-coverage-final-392.2026-07-20T14-32.md`).
- Zero regressions: PASS (see `evidence/qa-gates/regression-check-392.2026-07-20T14-42.md`).
- New/changed code >= 90% coverage: PASS, 100% observed (see
  `evidence/qa-gates/coverage-delta-392.2026-07-20T14-38.md`).

**Decision: AC-5 is left unchecked (`- [ ]`)** in `issue.md`, per `acceptance-criteria-tracking`'s
rule "Leave unmet items unchecked: If an AC item cannot be fully delivered or verified, leave it as
`- [ ]` and document the gap." Five of the six named toolchain/coverage components pass; the
nullable-build component fails for a pre-existing, out-of-scope, vendored-project reason unrelated
to this bugfix and not resolvable within this plan's Scope-Lock. This is recorded here as an
explicit, auditable gap rather than an overstated pass.

## Final AC status for issue #392

| AC | Status | Backing evidence |
|---|---|---|
| AC-1 | `[x]` Checked | `evidence/regression-testing/fail-before-392.2026-07-20T14-05.md`, `evidence/regression-testing/pass-after-392.2026-07-20T14-10.md` |
| AC-2 | `[x]` Checked | `evidence/other/root-cause-392.2026-07-20T13-50.md`, same fail-before/pass-after evidence |
| AC-3 | `[x]` Checked | `evidence/regression-testing/targeted-no-regression-392.2026-07-20T14-13.md` |
| AC-4 | `[x]` Checked | `evidence/other/root-cause-392.2026-07-20T13-50.md`, same fail-before/pass-after evidence |
| AC-5 | `[ ]` NOT checked — partial | Five of six components pass (see table above); nullable-build component fails on pre-existing, out-of-scope `SVGControl.csproj` vendored debt, confirmed no-regression vs. baseline. |
