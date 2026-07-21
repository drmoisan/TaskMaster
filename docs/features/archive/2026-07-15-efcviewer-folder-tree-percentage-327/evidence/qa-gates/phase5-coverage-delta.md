# Phase 5 — Coverage Delta / Threshold Verification (P5-T5)

Timestamp: 2026-07-16T02-35

Measurement method: identical tooling and settings for baseline and post-change (dotnet-coverage
collect -> Cobertura, both UtilitiesCS.Test and QuickFiler.Test, module excludes mirroring
TaskMaster.runsettings, MSTest Workers=4). Comparing like-for-like so any denominator nondeterminism
cancels.

## Repository coverage (baseline vs post-change)

| Metric | Baseline (P0-T5) | Post-change (P5-T4) | Delta |
|---|---|---|---|
| Line coverage | 77.4641% (109085/140820) | 77.5388% (109553/141288) | +0.0747 pts |
| Branch coverage | 52.9436% (13004/24562) | 53.1184% (13099/24660) | +0.1748 pts |

Repository line and branch coverage did not regress; both increased. The denominator grew (140820 ->
141288 lines) because the new host-neutral modules were added and are covered; the exempt WinForms
Designer/Form and controller code carry [ExcludeFromCodeCoverage] and are not in the denominator.

## New-code coverage (target >= 90% line and branch)

| Module | Line | Branch | Meets >= 90% |
|---|---|---|---|
| UtilitiesCS.FolderSuggestionNode | 100% | 100% | Yes |
| UtilitiesCS.FolderSuggestionTree | 98.45% | 96.43% | Yes |
| UtilitiesCS.PercentageFormatter | 100% | 100% | Yes |
| UtilitiesCS.FolderProbabilityAdapter | 100% | 100% | Yes |
| UtilitiesCS.IFolderProbabilitySource | interface-only (no executable lines) | n/a | n/a (excluded per policy) |

## No-regression on changed lines

The changed/added production lines are the five new host-neutral modules (covered at 96.43%-100%).
The other modified files (EfcViewer.Designer.cs, EfcViewer3.Designer.cs, EfcViewer3.cs,
EfcFormController.cs) are coverage-exempt via [ExcludeFromCodeCoverage] per CLAUDE.md
(WinForms Form-derived, Designer-generated, and COM-bound controller) and are verified by build +
manual QA. No previously-covered production line lost coverage; the repository line rate increased.

## Outcome

PASS. New host-neutral modules meet the >= 90% line and branch target, the repository floor is not
regressed (coverage increased), and no changed line loses coverage.
