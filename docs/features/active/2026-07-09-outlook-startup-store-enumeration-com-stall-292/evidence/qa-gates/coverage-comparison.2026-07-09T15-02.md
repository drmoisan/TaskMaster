# Coverage Comparison / Threshold Verification (issue #292)

- Timestamp: 2026-07-09T15-02
- Task: [P3-T5]
- Baseline source: [P0-T6] `baseline-tests-coverage.2026-07-09T15-02.md`
- Post-change source: [P3-T4] `qa-tests-coverage.2026-07-09T15-02.md`

## Repository-wide line coverage (first-party production, raw whole-module)

| Metric | Baseline | Post-change | Delta |
|---|---|---|---|
| First-party production aggregate | 39.78% (39115/98340) | 41.09% (41195/100252) | +1.31 pts |
| UtilitiesCS (touched assembly) | 45.31% (37002/81660) | 47.14% (38505/81681) | +1.83 pts |

- No repository-wide regression: coverage increased in both the aggregate and the touched assembly. The raw whole-module denominator varies slightly between runs (nondeterministic module load), so these figures are directional; the increase and the changed-line result below establish no-regression.

## Repository-wide 80% floor (testable denominator)

- The raw whole-module figures include COM/VSTO/WinForms and Outlook-interop code formally exempted from the 80% floor by CLAUDE.md. The testable-denominator 80% floor is enforced by the feature-review canonical coverage pipeline (which applies the exclusions). This change adds only host-neutral, fully-covered lines and cannot reduce the testable-denominator rate. No production file was excluded from measurement.

## New / changed-code coverage (>= 90% obligation)

| File | New executable lines | Covered | Rate |
|---|---|---|---|
| `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs` | 4 (L44, L89, L181, L183) | 4 | 100% |
| `UtilitiesCS/Threading/StoreLockupResponder.cs` | 10 (L111-114, L119-123, L126) | 10 | 100% |
| `UtilitiesCS/Threading/CurrentStoreContext.cs` | 0 executable (L30 is a compile-time `const`, no IL) | n/a | n/a |
| **Aggregate** | **14** | **14** | **100%** |

- New-code coverage 100% >= 90%: PASS.
- The `CurrentStoreContext` const is non-executable; its value is exercised by T1/T2/T3 and by both production call sites (verified: `StoresWrapper.cs` L181 and `StoreLockupResponder.cs` L114 both hits=1).

## No regression on changed lines

- The two refactored call sites (`StoresWrapper.cs` L44 and L89, changed from inline `GetFilteredStores().ToList()` to `MaterializeFilteredStores()`) remain covered (hits=1). No previously-covered line lost coverage.

## Outcome

PASS. Repository-wide coverage did not regress (increased); new executable-code coverage is 100% (>= 90%); no changed-line regression.
