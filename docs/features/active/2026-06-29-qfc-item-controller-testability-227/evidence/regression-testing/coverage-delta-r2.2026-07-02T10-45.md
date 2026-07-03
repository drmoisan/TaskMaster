# Cycle-2 Coverage Delta (P8-T5, AC5)

Timestamp: 2026-07-02T10-45
Baseline source: `evidence/remediation-baseline/baseline-tests-coverage.2026-07-01T21-37.md` (P0-T5)
Final source: `evidence/qa-gates/final-r2-tests-coverage.2026-07-02T10-45.md` (P8-T4)

## Delta table

| Metric | Baseline (P0-T5, cycle-1 end) | Final (cycle-2) | Delta |
|---|---|---|---|
| Repo-wide line coverage | 8353/61046 = 13.68% | 11163/71036 = 15.71% | +2.03 pts |
| Affected `QfcItemController` NON-EXEMPT denominator | 226/239 = 94.56% | 885/1051 = 84.21% | denominator +812 lines |
| Passing test count | 233 | 328 | +95 |
| `QfcItemController` exemptions | 103 | 41 (38 members + 3 adapter shims) | -62 |

## Interpretation (AC5)

- **Affected non-exempt denominator >= 80%: MET (84.21%).** The apparent percentage decrease from
  94.56% to 84.21% is the intended, desirable effect of Option A: ~62 members were moved OUT of the
  exempt set and INTO the tested non-exempt denominator, which grew the denominator 4.4x (239 -> 1051
  lines) while the coverage percentage remained above the 80% floor. A large, well-covered denominator
  is strictly stronger evidence of testability than a tiny denominator inflated by blanket exemptions.
- **New/extracted code (including the new seam types) >= 90%: MET.** The extracted controller methods
  (`WireIntentEvents`, the five `*Core` methods, `HandleWebViewInitializedAsync`) are 100% covered by
  the dedicated `Seam*Tests`. The new interfaces have no executable lines; the adapter shims are exempt
  DI-adapter forwarders with smoke tests.
- **Changed lines do not regress: MET.** Every line changed this cycle (dispatcher/COM/factory
  re-points, the `WireEvents` split, the thin-delegator extractions, the de-exempted member bodies) is
  exercised by a passing test; no previously-covered non-exempt line lost coverage (per-partial figures:
  `QfcItemController.cs` 98.63%, `Initialization` 99.12%, `Navigation` 97.89%, `ViewerSetup` 98.25%,
  `FolderHandling` 89.66%, `Conversation` 82.47%; the aggregate rose vs. the Phase 5 gate figure of
  83.71% to 84.21%).
- **Repo-wide floor:** satisfied-with-documented-exception under the #223 authority-scoped precedent;
  residual repo-wide uplift is tracked under #197. Repo-wide coverage still improved (+2.03 pts) as a
  side effect of the added tests.

Output Summary: AC5 in-scope thresholds all MET — affected non-exempt denominator 84.21% (>= 80%),
new/extracted code 100% (>= 90%), no changed-line regression. Outcome: PASS (not remediation-required).
