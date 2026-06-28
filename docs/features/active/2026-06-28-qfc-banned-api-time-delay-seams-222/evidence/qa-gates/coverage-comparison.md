# QA Gate — Coverage Comparison (P5-T5)

Timestamp: 2026-06-28T20-27

Sources: baseline = evidence/baseline/baseline-tests.md (P0-T9); post-change = evidence/qa-gates/final-tests.md (P5-T4). Both runs used the identical command (QuickFiler.Test only, /Settings:TaskMaster.runsettings /InIsolation /EnableCodeCoverage), so the figures are directly comparable.

## Coverage table

| Metric | Baseline | Post-change | Delta |
|--------|----------|-------------|-------|
| Single-run overall line-rate (NOT true repo-wide) | 11.46% | 11.72% | +0.26 |
| QuickFiler assembly (package) | 30.95% | 31.67% | +0.72 |
| QfcHomeController.cs class | 90.18% | 90.18% | 0.00 |
| QfcHomeController.Metrics.cs class | 54.93% | 69.44% | +14.51 |
| QfcDatamodel | exempt ([ExcludeFromCodeCoverage]) | exempt | n/a |

## New/changed-code coverage for QfcHomeController (per-line)

Changed coverable production lines: 9.
- Covered (6): Metrics.cs seam property (L17) and all five timestamp-read lines (L27, L107, L108, L110, L122).
- Not covered (3):
  - QfcHomeController.cs L54 (LaunchAsync `controller.TimeProvider = timeProvider ?? TimeProvider.System;`) and L77 (LaunchAsync catch-block timestamp). Both are reachable only by invoking the static lifecycle factory `LaunchAsync`, which constructs the controller and runs `InitAsync` -> `QfcDatamodel.LoadAsync` -> live Outlook `ActiveExplorer` COM plus WinForms control creation, with no injectable seam at the LaunchAsync boundary. Formally exempt under CLAUDE.md COM/VSTO exemption clauses (a) VSTO lifecycle/entry points and (c) Outlook Interop dependence without an injectable seam. Documented: evidence/regression-testing/launchasync-test-scope.md (P4-T6).
  - QfcHomeController.Metrics.cs L222 (NonBlockingProducer 20 ms retry delay). Defensive branch that BlockingCollection semantics make unreachable deterministically (OCE implies token cancellation, which breaks before the delay). Documented: evidence/regression-testing/nonblockingproducer-delay-branch-scope.md (P4-T3).

Testable changed-code coverage = 6/6 = 100% (>= 90% target met under the CLAUDE.md testable-denominator rule). Raw changed-line coverage including the 3 exempt/unreachable lines = 6/9 = 66.7% (reported transparently).

## Threshold assessment

- QfcHomeController new/changed TESTABLE code >= 90%: PASS (100%; the 3 uncovered lines are formally exempt COM-bound lifecycle / unreachable defensive code with dossiers).
- No regression on changed lines: PASS. All previously-covered lines remain covered; QfcHomeController.Metrics.cs class coverage increased +14.5 points; QuickFiler package +0.72.
- Repo-wide >= 80% floor: NOT MEASURABLE from this single-assembly QuickFiler.Test run (the 11.72% single-run figure is not a repo-wide denominator). This change is additive (5 new tests) plus behavior-preserving call-site swaps; it does not remove or reduce coverage of any existing code, so the repo-wide floor (measured across all test assemblies) is not regressed by this change.

## Verdict
PASS under the governing CLAUDE.md coverage policy (testable denominator with documented COM/VSTO and unreachable-branch exemptions). No remediation required for coverage. The exemption applicability for the three uncovered LaunchAsync/NonBlockingProducer lines is escalated for reviewer ratification.
