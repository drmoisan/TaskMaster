# Final QC — Coverage Delta / Threshold Verification (P6-T5)

Timestamp: 2026-06-22T15-26

Command:
```
(comparison of evidence/baseline/trx/baseline-2026-06-21T16-30.cobertura.xml
 vs evidence/qa-gates/trx/postchange-2026-06-21T16-30.cobertura.xml)
```

EXIT_CODE: 0

## Repo-wide aggregate (no-regression comparator)

| Metric | Baseline (P0-T8) | Post-change (P6-T4) | Delta |
|---|---|---|---|
| lines-covered | 8383 | 8513 | +130 |
| lines-valid | 65052 | 65308 | +256 |
| line-rate | 0.12887 (12.89%) | 0.13035 (13.04%) | +0.15 pp |

- Repo-wide aggregate line coverage increased (12.89% → 13.04%). **No repo-wide regression.** PASS.
- This aggregate is the all-module raw figure (the established no-regression comparator for this feature's increments), NOT the CLAUDE.md testable-first-party-denominator measurement (which excludes vendored Swordfish/SVGControl projects, COM/VSTO add-in lifecycle classes, WinForms Designer code, and un-seamed Outlook Interop handlers). The repo-wide testable-denominator floor (≥ 80%) is owned by `feature/csharp-coverage-uplift`; this corrective fix's obligation is no-regression plus the new-code threshold below.

## New testable types (≥ 90% new-code obligation)

| Type | Lines covered / total | Coverage | Threshold | Result |
|---|---|---|---|---|
| `HookReadinessCoordinator` (TaskMaster) | 44 / 44 | 100.00% | ≥ 90% | PASS |
| `NonBlockingDelay` (TaskMaster) | 34 / 34 | 100.00% | ≥ 90% | PASS |

- `NonBlockingDelay` is explicitly NOT COM/VSTO-exempt: it is pump-independent (`System.Threading.Timer`-backed) and carries the new-code coverage obligation, satisfied at 100% via `NonBlockingDelayTests` (P1-T7).
- `HookReadinessCoordinator` is the pure decision/state-machine seam, satisfied at 100% via `HookReadinessCoordinatorTests` (P1-T5).

## Documented exemptions (excluded from the testable denominator; cite P2-T5)

- `OutlookReadinessGate` (20.00%, 8 / 40): COM-bound live `App.Session.DefaultStore.GetDefaultFolder` probe, no injectable seam below the COM boundary; COM/VSTO coverage-exempt by inspection.
- `AppEvents.Hook` / `PerformReadinessHookup` / `ProcessMailItemAsync` COM lines and the `DispatcherTimer` wiring: COM/VSTO + live-Dispatcher exempt.
- `AppOlObjects.LoadInboxes` COM enumeration lines: COM/VSTO exempt.
- `LiveOutlookHookupIntegrationTests` (`[TestCategory("LiveOutlook")]`): not executed in CI; excluded from the coverage denominator.
- `Settings.Designer.cs`: generated code; exempt.

## Verdict

PASS. Repo-wide aggregate did not regress (increased). Both new testable types (`HookReadinessCoordinator`, `NonBlockingDelay`) exceed the ≥ 90% new-code threshold (each 100%). All required coverage numbers are available and numeric (no placeholders).
