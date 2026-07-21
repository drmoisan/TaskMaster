# Phase 2 — Coverage Delta (P2-T5)

Timestamp: 2026-07-20T22-25

Command / derivation method: Compared the P0-T6 baseline Cobertura (evidence/baseline/tests-coverage.2026-07-20T21-41.md) against the P2-T4 post-change Cobertura (evidence/qa-gates/tests-coverage.2026-07-20T21-41.md). Per-file line/branch aggregated from the Cobertura XML (primary class + compiler-generated async state-machine classes) via a Python aggregation over `<line>` hits and `condition-coverage`. New/changed-code coverage derived by mapping the uncovered-line set onto the diff of the two changed production files.

EXIT_CODE: 0

Output Summary:

Overall (UtilitiesCS.dll + QuickFiler.dll scope):
- Baseline: line 86.54%, branch 80.25%.
- Post-change: line 86.54%, branch 80.26%.
- Delta: line +0.00 pt, branch +0.01 pt. No overall regression.

Per touched file (baseline -> post-change):
- FolderBreadcrumbBridgeRouter.cs: line 96.10% -> 97.55% (+1.45 pt); branch 88.33% -> 90.00% (+1.67 pt).
- BreadcrumbStateModel.cs: line 100.00% -> 100.00% (0.00 pt); branch 94.23% -> 94.64% (+0.41 pt).
- BreadcrumbBridgeCoordinator.cs: line 97.32% -> 97.32% (unchanged file, not modified); branch 83.33% -> 83.33%.

New / changed production code coverage (the >= 90% target):
- New method BreadcrumbStateModel.ReplaceRows (lines 222-238): 100% line-covered, both selection branches (preserve and clamp) plus the null-guard exercised by three dedicated ReplaceRows tests.
- Changed method FolderBreadcrumbBridgeRouter.SetSuggestionsAsync (lines 48-87, rewritten to build-local-then-atomic-swap): 100% of its lines covered; the scored, unresolvable-fallback, and non-scored branches are all exercised. No uncovered line falls within the changed range.
- Field change `_rows` (now swappable) covered by all existing model tests.
- New/changed-code line coverage: 100% (>= 90% target met).
- No regression on changed lines: every previously covered changed line remains covered; the two changed files' rates were maintained or improved.

Determination: New/changed code meets the >= 90% coverage requirement (100% line) and there is no coverage regression on changed lines. Outcome: PASS.
