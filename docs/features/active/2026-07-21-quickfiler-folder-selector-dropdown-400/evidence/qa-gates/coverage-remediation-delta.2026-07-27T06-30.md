# P9-T6 coverage remediation delta

Source-line attribution parsed `coverage-final-remediation.2026-07-27T06-25.cobertura.xml` by exact Cobertura filename and source-line number, deduplicating each `(filename,line)` by maximum hit count. No merged class identity was used.

Repository coverage is 91,894/108,736 = 84.5111%, above the 80% floor and above the recorded P0 baseline 89,240/106,048 = 84.1506%.

| Source range | Covered/valid | Result |
| --- | ---: | --- |
| `BreadcrumbDropDownOpenLifetime.cs` 23-476 | 332/335 | PASS |
| `BreadcrumbCoordinatorUpgradeLifetime.cs` 9-307 | 202/204 | PASS |
| `BreadcrumbDropDownOpenCoordinator.cs` 12-307 | 216/220 | PASS |
| `BreadcrumbDropDownOpenCoordinator.cs` `CloseCore` 237-267 | 25/26 | PASS |
| `BreadcrumbDropDownOpenCoordinator.cs` `<Reset>b__24_0` 138-147 | 10/10 | PASS |
| `BreadcrumbDropDownHost.cs` 22-479 | 288/290 | PASS |
| `BreadcrumbDropDownHost.cs` `InstalledPopupControl` setter line 211 | 1/1 | PASS |
| `BreadcrumbDropDownHost.cs` `<DisposeCoreAsync>b__4` 327-330 | 4/4 | PASS |
| `BreadcrumbBridgeCoordinator.cs` 25-486 | 280/280 | PASS |
| `BreadcrumbBridgeCoordinator.cs` `PostRenderAndSelectorAsync` 262-273 | 11/11 | PASS |
| `BreadcrumbBridgeCoordinator.cs` stale-return 263-264 | 2/2 | PASS |
| `BreadcrumbPopupUiOperations.cs` 29-479 | 226/244 | PASS (92.6229%) |

The stale-return lines 263 and 264 are covered in the fresh artifact. Every named applicable source range is at least 90%, so P9-T7 is authorized.

## Changed/new-line and per-type accounting

The live P0 merge base is `e63ddc7c18ca71e2c968b3329e42d965d45af1eb`. Changed/new source points were derived from its zero-context C# diff plus all source lines in the allowed untracked C# paths, then intersected with the fresh Cobertura `(filename,line)` map. The result is **3,159/3,208 = 98.4726%**, improving on the prior current-source comparison of 3,157/3,208 = 98.4102%; therefore changed/new-line coverage has not regressed.

| Filename/source range | Covered/valid | Coverage |
| --- | ---: | ---: |
| `BreadcrumbPopupUiOperations.cs` 29-479 | 226/244 | 92.6230% |
| `BreadcrumbDropDownOpenLifetime.cs` 23-476 | 332/335 | 99.10% |
| `BreadcrumbCoordinatorUpgradeLifetime.cs` 9-307 | 202/204 | 99.02% |
| `BreadcrumbDropDownOpenCoordinator.cs` 12-307 | 216/220 | 98.18% |
| `BreadcrumbDropDownHost.cs` 22-479 | 288/290 | 99.31% |
| `BreadcrumbBridgeCoordinator.cs` 25-486 | 280/280 | 100.00% |

These results use exact filename/source-range attribution only; no merged Cobertura class identity is used.

The table now contains every entry named in the historical `coverage-remediation-delta.2026-07-27T04-51.md` artifact plus the prior applicable `BreadcrumbPopupUiOperations` type from `coverage-remediation-delta.2026-07-27T03-35.md`. All named ranges and types are at least 90%.
