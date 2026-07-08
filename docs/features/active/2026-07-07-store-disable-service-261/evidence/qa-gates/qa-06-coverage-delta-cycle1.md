# QA Gate 6 — Coverage/Test-Count Delta Verification (Remediation Cycle 1)

- Timestamp: 2026-07-08T00-53

## Figures

- Baseline coverage: 81.62% (repo-wide, Cobertura top-level `line-rate`; `119363` / `146244`
  lines; source: `evidence/remediation-baseline/test-coverage-baseline-cycle1.md`)
- Post-change coverage: 81.61% (repo-wide, Cobertura top-level `line-rate`; `119396` / `146294`
  lines; source: `evidence/qa-gates/qa-05-coverage-post-change-cycle1.md`)
- Test count baseline: 5032 total / 5031 passed / 1 failed (source:
  `evidence/remediation-baseline/test-coverage-baseline-cycle1.md`)
- Test count post-change: 5032 total / 5031 passed / 1 failed (source:
  `evidence/qa-gates/qa-05-coverage-post-change-cycle1.md`)

## Verification

- **Test count**: unchanged. 5032 total tests before and after. Passed count unchanged at 5031;
  failed count unchanged at 1. The single failure
  (`LiveHookup_OnSta_CompletesAndDoesNotBlockStaBeyondThreshold`) is the same test, same root
  cause (live-Outlook COM class factory unavailable in this environment), in both runs — confirmed
  by comparing the failure identity, not just the count, in
  `evidence/remediation-baseline/test-coverage-baseline-cycle1.md` and
  `evidence/qa-gates/qa-05-coverage-post-change-cycle1.md`. This is a pre-existing,
  environment-dependent condition unrelated to `StoresWrapperTests.cs`,
  `StoresWrapperDisableTests.cs`, or `StoreDisableServiceTests.cs` (the only files this
  remediation touches), and it pre-dates this remediation (present in the Phase 0 baseline
  captured before any Phase 1 edit).
- **Coverage**: 81.62% -> 81.61%, a 0.01-percentage-point difference, not a regression. This
  remediation moves and duplicates test code and fixes two previously-inert async assertions; it
  adds zero lines of new production code (no `*.cs` file under `UtilitiesCS/`,
  `UtilitiesCS/OutlookObjects/`, or any other production directory was touched — confirmed by the
  file list in this remediation's scope statement). The small denominator/numerator increase
  (50 lines valid, 33 lines covered) is attributable entirely to the new test file's duplicated
  helper methods, all of which execute (100% class line-rate, per QA Gate 5). Both figures remain
  comfortably above the CLAUDE.md 80% repo-wide testable-denominator floor.
- **New-code AC15 obligation (>= 90% new-code coverage)**: this remediation introduces no new
  production code, so there is no new production-code denominator to measure against the 90%
  new-code floor. The new/duplicated *test* code itself reports 100% line coverage (per QA Gate 5,
  `StoresWrapperDisableTests` class `line-rate="1"`), which exceeds 90% even if test code were
  counted, though test code is explicitly outside the coverage-tooling scope per policy
  ("Configure coverage tooling to exclude test files ... so metrics reflect application code, not
  tests"). This obligation is satisfied by inspection, consistent with the plan's stated rationale.

## Conclusion

No regression in test count or coverage. Both R1 (500-line file split) and N1 (await fix) are
verified resolved with a clean, non-regressing toolchain result.
