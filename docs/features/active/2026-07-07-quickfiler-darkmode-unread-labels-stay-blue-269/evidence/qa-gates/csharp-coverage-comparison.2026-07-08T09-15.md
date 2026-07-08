# C# Coverage Comparison — Issue #269

- Timestamp: 2026-07-08T10-38
- Task: [P2-T5]

## Sources

- Baseline: `evidence/baseline/csharp-vstest-coverage-baseline.2026-07-08T09-15.md` (and `evidence/baseline/coverage-baseline.cobertura.xml`)
- Targeted post-fix (UtilitiesCS): `evidence/regression-testing/targeted-vstest-utilitiescs.2026-07-08T09-15.md`
- Targeted post-fix (QuickFiler): `evidence/regression-testing/targeted-vstest-quickfiler.2026-07-08T09-15.md`
- Final full-suite post-change: `evidence/qa-gates/csharp-vstest-coverage-final.2026-07-08T09-15.md` (and `evidence/qa-gates/coverage-final.cobertura.xml`)

## Comparison Table

| Scope | Baseline | Targeted post-fix | Final post-change |
|---|---|---|---|
| Whole-process line coverage | 65.73% (112696/171461) | n/a (targeted runs) | 65.73% (112727/171496) |
| `UtilitiesCS` package | 88.21% | n/a | 88.20% |
| `UtilitiesCS.Test` package | 97.75% | n/a | 97.76% |
| `QuickFiler` package | 72.51% | n/a | 72.53% |
| `QuickFiler.Test` package | 95.19% | n/a | 95.18% |
| Class `UtilitiesCS.Theme` (`Theme.cs`) | 66.95% | n/a | 66.95% (unchanged) |
| Class `UtilitiesCS.Theme` (`Theme.Rendering.cs`) | 54.05% | n/a | 56.41% (+2.36 pts) |
| Class `QuickFiler.QfcThemeHelper` | 96.45% | n/a | 96.45% (unchanged) |
| `UtilitiesCS.Test` targeted run (`Theme_MailLabelThemingTests`, 4 tests) | 3 tests pre-existing | 4/4 pass post-fix | included in final full suite |
| `QuickFiler.Test` targeted run (`QfcThemeHelperTests`, 10 tests) | 9 tests pre-existing | 10/10 pass post-fix | included in final full suite |

## Repository-Wide Regression Check

No repository-wide coverage regression: whole-process, `UtilitiesCS`, `UtilitiesCS.Test`, `QuickFiler`, and `QuickFiler.Test` package-level line rates are within +/-0.02 percentage points of baseline (rounding-level noise from the two added test methods), with no downward movement exceeding that noise band.

## Changed-Line Coverage Confirmation

- `QfcThemeHelper.cs:89` (the changed probe line, `() => controller.Mail is not null && !controller.Mail.UnRead`) is exercised by both `BuildProductionControlSet_MapsControllerAndViewerInputs` (existing, non-null `Mail` path exercised via `controlSet.MailRead` construction) and the new `BuildProductionControlSet_WithNullMail_MailReadReturnsFalseWithoutThrowing` (null `Mail` path, added in P1-T3). Class-level line rate for `QfcThemeHelper.cs` is unchanged at 96.45%, confirming the changed line was already, and remains, covered.
- `Theme.Rendering.cs:42-53` (the two-catch-clause guard block) is exercised by both `Theme_MailLabelTheming_WhenReadProbeThrows_LabelsStillReThemeToUnread` (existing `COMException` case) and the new `Theme_MailLabelTheming_WhenReadProbeThrowsNullReferenceException_LabelsStillReThemeToUnread` (`NullReferenceException` case, added in P1-T2). Class-level line rate for `Theme.Rendering.cs` increased from 54.05% to 56.41%, directly reflecting the new `catch (NullReferenceException) { isRead = false; }` branch now being covered.

## Conclusion

No coverage regression at any measured scope; the two changed production lines are covered by the new regression tests. Satisfies AC5 (coverage portion, no regression on changed lines).
