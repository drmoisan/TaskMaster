# Coverage remediation delta — remediation required

- Timestamp (UTC): 2026-07-27T04:51Z
- Task: P9-T6
- Command: Parsed P9-T4 `coverage-final-remediation.2026-07-27T04-49.cobertura.xml` and P0 `coverage-remediation-baseline.2026-07-21T22-13.cobertura.xml` by exact Cobertura source filename plus source-line range. Every `(filename,line)` point was deduplicated by maximum hit count. No `Merge-CoberturaClassesByFilename` class identity was used.
- Result: remediation required. The historical failed `coverage-remediation-delta.2026-07-27T03-35.md` remains unchanged.

## Repository and changed/new-line accounting

| Scope | Covered/valid | Coverage | Result |
| --- | ---: | ---: | --- |
| P0 remediation baseline repository | 89,240/106,048 | 84.1506% | Baseline |
| P9-T4 final repository | 91,895/108,736 | 84.5120% | PASS, >=80% |
| Prior full-P9 source points on live P0 merge-base diff | 3,110/3,208 | 96.9451% | Historical comparison |
| P9-T4 source points on the same live P0 merge-base diff | 3,157/3,208 | 98.4102% | No regression (+1.4651 points) |

The P0 baseline's 1,141/1,143 changed/new production-point scope predated later remediation additions; the comparable current-source calculation above uses the same P0 merge base and current source ranges for the prior and final full-P9 Cobertura files.

## Filename and source-range attribution

| Source file | Source range/member | Covered/valid | Coverage | Status |
| --- | --- | ---: | ---: | --- |
| `BreadcrumbDropDownOpenLifetime.cs` | `BreadcrumbDropDownOpenLifetime`, lines 23-476 | 332/335 | 99.10% | PASS |
| `BreadcrumbCoordinatorUpgradeLifetime.cs` | `BreadcrumbCoordinatorUpgradeLifetime` source range, lines 9-307 | 202/204 | 99.02% | PASS |
| `BreadcrumbDropDownOpenCoordinator.cs` | `BreadcrumbDropDownOpenCoordinator`, lines 12-307 | 216/220 | 98.18% | PASS |
| `BreadcrumbDropDownOpenCoordinator.cs` | `CloseCore`, lines 237-267 | 25/26 | 96.15% | PASS |
| `BreadcrumbDropDownOpenCoordinator.cs` | `<Reset>b__24_0`, lines 138-147 | 10/10 | 100.00% | PASS |
| `BreadcrumbDropDownHost.cs` | `BreadcrumbDropDownHost`, lines 22-479 | 288/290 | 99.31% | PASS |
| `BreadcrumbDropDownHost.cs` | `InstalledPopupControl` setter, line 211 | 1/1 | 100.00% | PASS |
| `BreadcrumbDropDownHost.cs` | `<DisposeCoreAsync>b__4`, lines 327-330 | 4/4 | 100.00% | PASS |
| `BreadcrumbBridgeCoordinator.cs` | `BreadcrumbBridgeCoordinator`, lines 25-486 | 278/280 | 99.29% | PASS |
| `BreadcrumbBridgeCoordinator.cs` | `PostRenderAndSelectorAsync`, lines 262-273 | 9/11 | 81.82% | **FAIL** |

`PostRenderAndSelectorAsync` source lines 263 and 264, the stale-lease return path, remain uncovered. This is the same 9/11 filename/range result that the historical source-range ledger identified; it is not a class-identity lookup failure.

## Required plan revision delta

Replace the final sentence of `[P9-T6]` with the following addition before any P9-T7 execution:

`When a named source range is below 90%, modify only the existing deterministic test that owns that range to cover the uncovered source lines, without adding production code, a new test method, or any coverage-policy change. For PostRenderAndSelectorAsync, update only BreadcrumbCoordinatorLifecycleTests.PostRenderAndSelectorAsync_StaleLeaseReturnsCompletedWithoutPublishing so it deterministically invokes the coordinator's stale-lease return path at source lines 263-264 and asserts no messenger publication. Re-run P9-T1 through P9-T6 in order after that in-scope test correction. Keep P9-T6 unchecked until the fresh P9-T4 Cobertura reports every named range at least 90%.`

P9-T6 is intentionally unchecked. P9-T7 and later tasks must not begin until this revision is applied and the required restart sequence succeeds.
