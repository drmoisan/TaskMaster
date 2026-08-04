# P5 Open Coordinator Extraction Ledger

Timestamp: 2026-07-22T09:45:55.8573251Z

Command: Read-only line-count, SHA-256, test-inventory, include-count, exclusion-count, forbidden-dependency, host-neutral-body, prohibited-resource, configuration-hash, and `git diff --check` reconciliation against `p5-numeric-coverage-scope-ledger.2026-07-22T09-33.md`.

EXIT_CODE: `0`

Output Summary: PASS. The J1 change is confined to the exact authorized three-production/two-test tuple plus one adjacent include for each new file. All other P5 production and test source hashes match P5-T104. No configuration, runsettings, package, designer, threshold, or exclusion surface changed.

## Exact J1 tuple

| Source | Action | Lines | Tests | SHA-256 |
|---|---|---:|---:|---|
| `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` | added | 277 | 0 | `A4B2822F8D27AC79609AE36DB1DC10CF25CAFAEC7CEE86D4956FC9405CE4BD9C` |
| `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` | modified | 396 | 0 | `8D102C659C2B22E0C27684BDCA84152400CCED4CF937ED3DF3122A7DA1863DA0` |
| `QuickFiler/Viewers/BreadcrumbWebViewSurfaceFactory.cs` | modified | 226 | 0 | `28726E027BEEF4FE4633BA5BBF00AF6DA7E6C0D59CF6093657545997B3D574C9` |
| `QuickFiler.Test/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs` | preserved J0 five-case fixture | 132 | 5 | `56391C38A5A4EF599AA82D8BB981A29C61A968753D1604D3B7081CB49090C817` |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.cs` | added | 396 | 10 | `A0EBF0CD7FE46B90E02386A2C83C907456328DC831DC57348954AE289E9777A3` |

The production include appears exactly once immediately after `BreadcrumbDropDownOpenLifetime.cs`; the test include appears exactly once immediately after `ItemViewerBreadcrumbDropDownContractTests.cs`. The inventory is exactly 5 contract cases plus 10 non-data coordinator cases.

## Scope and measurement checks

- `ItemViewer.Breadcrumb.cs` is 396 lines, below 460.
- The coordinator and its test are 277 and 396 lines, each below 480.
- The factory is 226 lines, below 500.
- The coordinator has zero references to `ItemViewer`, `Control`, `Screen`, `WebView2`, `CoreWebView2`, or `SynchronizationContext`.
- `ItemViewer.Breadcrumb.cs` has zero occurrences of the removed `OpenBreadcrumbDropDownAsync`/request method, request-size calculation, or readiness observation body.
- The changed ItemViewer surface is limited to the collapsed direct core/dispatcher/navigation adapter, host construction and messenger event ownership glue, coordinator field/properties, and minimal theme/drop-down/reset/state/release delegation.
- The new test has zero live UI construction, timing, temporary-file, or external-process operations.
- All non-J1 production and applicable-test SHA-256 values match the P5-T104 ledger.

## Exclusions and protected configuration

- `ItemViewer.cs` retains its one class exclusion and SHA-256 `498D1781BE7DF3665D799A4DFC9837AD4F81D6A47B0DEC1CB1C0A84D025AB0E2`.
- `ItemViewer.Breadcrumb.cs` retains exactly two method exclusions, now at lines 71 and 84; both remain direct collapsed-surface adapters.
- `BreadcrumbPopupUiOperations.cs` retains exactly seven method exclusions with the same names and bodies recorded at P5-T104.
- No exclusion is present on `BreadcrumbDropDownOpenCoordinator`.
- `coverage.config` remains `B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943`.
- `TaskMaster.cli.runsettings` remains `98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57`.
- Production and test project changes are limited to their single adjacent includes; their resulting hashes are `1B9B9F0DA440D3CEA918CB6B178EAC1B603D0886D08E57552C90E89CDC54550E` and `59DC70BC44CE50E9556738A1BB80B280977576E693D9F23B29943188B2AC96FC`.

`git diff --check` reported no whitespace error across the ledger boundary. No contradiction requiring replanning was found.
