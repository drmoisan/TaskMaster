# P9 member source-range attribution

Timestamp: 2026-07-27T03-38
Command: Parsed only `evidence/qa-gates/coverage-final-remediation.2026-07-27T03-32.cobertura.xml` source nodes by exact `filename` and source-line range. No `Merge-CoberturaClassesByFilename` invocation or modification was used.
EXIT_CODE: 0
Output Summary: The reproducible filename/range ledger resolves the type-name collision that made the prior class-name lookup report `BreadcrumbDropDownOpenLifetime` as missing. All named below-90 source ranges are identified for the test-only remediation tasks.

| Source file | Source range/member | Covered/valid | Coverage | Status |
| --- | --- | ---: | ---: | --- |
| `QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs` | `BreadcrumbDropDownOpenLifetime` | 332/335 | 99.10% | Pass |
| `QuickFiler/Viewers/BreadcrumbCoordinatorUpgradeLifetime.cs` | `BreadcrumbCoordinatorUpgradeLifetime` | 161/204 | 78.92% | Below 90 |
| `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` | `CloseCore` | 21/26 | 80.77% | Below 90 |
| `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` | `<Reset>b__24_0` | 8/9 | 88.89% | Below 90 |
| `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | `InstalledPopupControl` setter | 0/1 | 0.00% | Below 90 |
| `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | `<DisposeCoreAsync>b__4` | 3/4 | 75.00% | Below 90 |
| `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` | `PostRenderAndSelectorAsync` | 9/11 | 81.82% | Below 90 |

The failed historical P9-T6 evidence remains unchanged at `evidence/qa-gates/coverage-remediation-delta.2026-07-27T03-35.md` with SHA-256 `E062FF891B5B8464B81E0EC25A1CC49A5AFE1F6F4784A00B29E942FA1C73CAB9`.
