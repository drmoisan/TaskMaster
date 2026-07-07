# C# Coverage Comparison — Issue #251

Timestamp: 2026-07-07T00-08

Sources:
- `docs/features/active/2026-07-06-quickfiler-darkmode-stale-subscription-251/evidence/baseline/csharp-vstest-coverage-baseline.2026-07-06T23-08.md`
- `docs/features/active/2026-07-06-quickfiler-darkmode-stale-subscription-251/evidence/regression-testing/targeted-vstest-coverage.2026-07-06T23-08.md`
- `docs/features/active/2026-07-06-quickfiler-darkmode-stale-subscription-251/evidence/qa-gates/csharp-vstest-coverage-final.2026-07-06T23-08.md`

EXIT_CODE: 0

Output Summary:

| Stage | Total Tests | Passed | Repo-wide combined line-rate | `QuickFiler` package line-rate |
|---|---|---|---|---|
| Baseline (P0-T7) | 486 | 486 | 20.18% (22083/109433) | 72.42% (0.7242424242424242) |
| Targeted post-fix (P1-T9) | 2 (targeted filter) | 2 | n/a (targeted run only) | n/a |
| Final full-suite (P2-T4) | 488 | 488 | 20.23% (22150/109500) | 72.42% (0.7242424242424242) |

No repository-wide regression: repo-wide combined line-rate increased marginally (20.18% -> 20.23%), driven entirely by the two new fully-covered test methods contributing to the `QuickFiler.Test` package's own line-rate (0.9464 -> 0.9517); the `QuickFiler` production package's line-rate and complexity count (258) are bit-for-bit unchanged between baseline and final, because `QfcCollectionController` — the sole production class touched by this fix — carries a pre-existing `[ExcludeFromCodeCoverage]` attribute (confirmed in Phase 0 investigation, `investigation-notes.2026-07-06T23-08.md`) and was not added or removed by this change. The changed lines (`Cleanup()`, `CleanupAsync()`, `DarkMode_CheckedChanged`) are therefore outside the coverage denominator for both baseline and final measurements, consistent with the CLAUDE.md COM/VSTO/WinForms coverage exemption for Outlook Interop event handler classes. This is a pre-existing, documented exemption rather than a new suppression introduced by this change — no `[ExcludeFromCodeCoverage]` attribute was added or removed by this plan.

Test count: 486 -> 488 (net +2, the two new regression tests). All 488 tests pass in the final full-suite run. Satisfies the coverage portion of AC7.
