# AC15 Re-Confirmation — Remediation Cycle 1 (Issue #261)

- Timestamp: 2026-07-08T00-57
- Feature: Store Disable Service (F1, Issue #261)
- AC reference: `spec.md` §9, AC15 ("Toolchain + coverage + 500-line cap")
- Remediation cycle: 1 (this cycle)

## Summary

This remediation cycle resolved the two findings from
`remediation-inputs.2026-07-07T23-46.md`:

- **R1 (Blocking — 500-line file-size violation)**: `UtilitiesCS.Test/OutlookObjects/Store/StoresWrapperTests.cs`
  was reduced from 688 lines to 415 lines by extracting the 6 `InclusionFilters_*` tests, the 5
  F1 disabled-store tests, and the `AssertInclusionDecision` helper into a new file
  `UtilitiesCS.Test/OutlookObjects/Store/StoresWrapperDisableTests.cs` (368 lines), wired into
  `UtilitiesCS.Test.csproj` via a new `<Compile Include>` item. Both resulting files are well
  under the 500-line cap. No test assertion or behavior was changed; all moved test bodies are
  byte-identical to their pre-move source (verified in `remediation-plan.2026-07-07T23-46.md`
  P1-T3 through P1-T6 and their acceptance checks).
- **N1 (Non-blocking — unawaited async throw assertions)**: the two `ReenableAsync` guard tests in
  `UtilitiesCS.Test/OutlookObjects/Store/StoreDisableServiceTests.cs`
  (`Writes_ThrowArgumentException_ForSentinelIdentity`,
  `Writes_ThrowInvalidOperation_WhenModelIsNull`) were converted to `async Task` with `await` on
  their `ThrowAsync<...>()` assertions, so the `ReenableAsync` guard paths now genuinely execute
  and are verified (confirmed passing individually, per QA Gate 8).

## Evidence References

- Full toolchain green:
  - Format: `evidence/qa-gates/qa-01-format-cycle1.md` (EXIT_CODE 0, no files reformatted)
  - Analyzers: `evidence/qa-gates/qa-02-analyzers-cycle1.md` (EXIT_CODE 0, 0 errors, 20
    pre-existing unrelated warnings)
  - Nullable/TreatWarningsAsErrors: `evidence/qa-gates/qa-03-nullable-cycle1.md` (EXIT_CODE 0,
    0 warnings, 0 errors on the plan-specified incremental build; diagnostic forced-rebuild
    confirms pre-existing, out-of-scope nullable debt elsewhere in the solution, unrelated to the
    touched files)
  - MSTest (touched assemblies): `evidence/qa-gates/qa-04-mstest-cycle1.md` (4410 tests, 4409
    passed, 1 pre-existing environment-dependent failure unrelated to this remediation; all 13
    directly-affected test methods passed)
- Both split files <= 500 lines: `evidence/qa-gates/qa-07-file-size-final-cycle1.md`
  (`StoresWrapperTests.cs` = 415 lines, `StoresWrapperDisableTests.cs` = 368 lines)
- No test-count or coverage regression: `evidence/qa-gates/qa-06-coverage-delta-cycle1.md`
  (test count unchanged at 5032 total / 5031 passed / 1 pre-existing failure; repo-wide coverage
  81.62% -> 81.61%, not a regression, still above the 80% floor)
- N1 fix genuinely exercised: `evidence/qa-gates/qa-08-n1-verification-cycle1.md`

## Determination

R1 and N1 are both resolved. AC15 is fully satisfied for this remediation cycle: the toolchain is
green, both touched test files are under the 500-line cap, coverage and test count show no
regression against the Phase 0 baseline, and the N1 non-blocking test-quality issue is also fixed
in the same pass per the remediation plan's scope.
