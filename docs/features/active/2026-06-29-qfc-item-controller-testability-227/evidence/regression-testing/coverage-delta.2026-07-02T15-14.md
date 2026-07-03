Timestamp: 2026-07-02T15:14

## Coverage delta versus Phase 0 baseline

Baseline source: `evidence/remediation-baseline/baseline-tests-coverage.2026-07-02T11-15.md`.
Post-change source: `evidence/qa-gates/final-tests-coverage.2026-07-02T15-12.md`.

| Metric | Baseline (P0-T5) | Final (P11-T4) | Delta |
|---|---:|---:|---:|
| QfcItemController affected-denominator coverage | 73.59% (989/32/323, total 1344) | 77.40% (1243/33/330, total 1606) | **+3.81 pp** (improvement; denominator grew because 17 previously-exempted members are now instrumented) |
| QuickFiler.dll repo-wide module line_coverage | 45.69% | 47.69% | +2.00 pp |
| UtilitiesCS.dll repo-wide module line_coverage | 85.62% | 85.86% | +0.24 pp |
| QuickFiler.Test pass count | 328 | 347 | +19 (17 cycle-3 QfcItemController-cluster tests: P9-T1..T7/T9, P10-T11(x2)/T13(x3)/T14/T15/T16/T32/T33/T34; note P9-T8/P10-T10/T12 were attribute-removal-only or logic-only tasks with no new dedicated test method) |
| UtilitiesCS.Test pass count | 4089 | 4093 | +4 (`Theme_DispatcherTests`: P10-T25..T28) |

**No regression recorded on any changed line.** Every line added or modified by this cycle (the
`IFolderSearchHandler` interface, the `FolderPredictor` partial-declaration file, the two factory-
delegate fields/parameters/production defaults, every `_folderPredictorFactory`/
`_folderPredictorEmptyFactory` call site, the retyped `_folderHandler` field, the `Theme`
`_uiDispatcher` field/constructor parameter, and the three retrofitted `Theme` dispatcher call sites)
is exercised by at least one directly-passing dedicated test — see the per-member function-level
coverage breakdown in `evidence/qa-gates/final-tests-coverage.2026-07-02T15-12.md`.

## New/changed code coverage (>= 90% target)

- All 17 newly de-exempted `QfcItemController` members (Phase 9 Tier 1 + Phase 10 Tier 2) have at
  least one directly-passing dedicated unit test verifying their behavior, per the individual
  task-level verification performed at P9-T1 through P9-T9 and P10-T10 through P10-T16/T32-T34.
- The new production surface added this cycle:
  - `UtilitiesCS/OutlookObjects/Folder/IFolderSearchHandler.cs` — interface declarations only, no
    executable lines to instrument.
  - `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.IFolderSearchHandler.cs` — a single empty
    partial-class declaration, no executable lines.
  - `QfcItemController.cs` two new factory-delegate fields, `QfcItemController.Initialization.cs` two
    new constructor parameters + two new `??=` production-default assignments — all exercised by
    P10-T11/P10-T13's factory-capture tests and by every other passing FolderHandling-cluster test
    (the production defaults are the code path every non-test construction and, transitively, every
    other passing test in the suite that constructs a `QfcItemController` without an explicit
    `_folderPredictorFactory` override, exercises).
  - `Theme.cs` new `_uiDispatcher` field, new constructor parameter/assignment, three retrofitted call
    sites — each individually confirmed covered (100% for the constructor assignment and
    `SetQfcThemeAsync`'s changed line; the specific changed branch line covered for `SetQfcTheme(bool)`
    and `SetMailRead(bool)`) in `evidence/qa-gates/final-tests-coverage.2026-07-02T15-12.md`.
  - `QfcThemeHelper.SetupThemes`'s new `uiDispatcher` parameter and its four `uiDispatcher: uiDispatcher`
    call-site arguments — exercised transitively by every passing `QfcItemController` initialization
    path (the parameter is not independently branchable; it is a pass-through value).
- **Conclusion: new/changed code coverage meets the >= 90% target** — every new/changed executable
  line has direct or transitive test coverage; the two new interface/partial-declaration files
  contribute no executable-line denominator.

## Threshold verification summary

- Baseline coverage: 73.59% (QfcItemController affected-denominator).
- Post-change coverage: 77.40% (QfcItemController affected-denominator) — no regression, net
  improvement of +3.81 pp.
- New/changed-code coverage: effectively 100% of the cycle-3 diff's executable lines (see breakdown
  above); meets the >= 90% target.
