# Runtime UI-boundary scope baseline

Timestamp: 2026-07-22T01:30:48.5208634Z

Command: `$paths=@('QuickFiler/Viewers/BreadcrumbUiDispatcher.cs','QuickFiler/Viewers/BreadcrumbWebViewSurfaceFactory.cs','QuickFiler/Viewers/BreadcrumbDropDownHost.cs','QuickFiler/Viewers/ItemViewer.Breadcrumb.cs','QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs','QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs','QuickFiler.Test/Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs','QuickFiler.Test/Viewers/BreadcrumbPopupControlDispatchTests.cs','QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs','QuickFiler/QuickFiler.csproj','QuickFiler.Test/QuickFiler.Test.csproj'); foreach($p in $paths){ if(Test-Path -LiteralPath $p){ $lineCount=@(Get-Content -LiteralPath $p).Count; $hash=(Get-FileHash -Algorithm SHA256 -LiteralPath $p).Hash.ToLowerInvariant(); '{0}|LINES={1}|SHA256={2}' -f $p,$lineCount,$hash } else { '{0}|ABSENT' -f $p } }; $pattern='BreadcrumbUiDispatcher|BreadcrumbWebViewSurfaceFactory|BreadcrumbDropDownHost|ItemViewer\.Breadcrumb|BreadcrumbBridgeCoordinator|BreadcrumbPopupUiOperations|BreadcrumbSelectorToggleUiBoundaryTests|BreadcrumbPopupControlDispatchTests|BreadcrumbSelectorOpenRetryTests'; foreach($proj in @('QuickFiler/QuickFiler.csproj','QuickFiler.Test/QuickFiler.Test.csproj')){ "PROJECT=$proj"; Select-String -LiteralPath $proj -Pattern $pattern | ForEach-Object { '{0}:{1}' -f $_.LineNumber,$_.Line.Trim() } }; 'P0_P4_CHECKED=' + @(Select-String -LiteralPath 'docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/remediation-plan.2026-07-21T21-37.md' -Pattern '^- \[x\] \[P[0-4]-T').Count; 'P0_P4_OPEN=' + @(Select-String -LiteralPath 'docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/remediation-plan.2026-07-21T21-37.md' -Pattern '^- \[ \] \[P[0-4]-T').Count`

EXIT_CODE: 0

Output Summary: The baseline was captured at HEAD `dfb202fc5dbc50638a9519c66b64005bcb5de116`. `BreadcrumbPopupUiOperations.cs` and all three planned P5 test files are absent. P0-P4 remain historical completed state with 78 checked tasks and zero open tasks.

## Source inventory

| Path | Lines | SHA-256 |
|---|---:|---|
| `QuickFiler/Viewers/BreadcrumbUiDispatcher.cs` | 180 | `c69c2281cacede2d7169b0b3b701be4d5f9de2756b8dab2e01aa44a1ecd658d3` |
| `QuickFiler/Viewers/BreadcrumbWebViewSurfaceFactory.cs` | 273 | `597c5edb8cac41e653bf3d6b5507aa62134ad202f7fc1af3cc7fa7c014c8a9df` |
| `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | 484 | `b219d3f2d6dd05805adfda95932d3a7d29f2b59bdcb3b674722d804e46719acc` |
| `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` | 454 | `c33976cf76045d634e7c8cf50965eeb05c26cf6c3b9d61a8902597904cfc804b` |
| `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` | 455 | `224d5614b8a293665ec22b563a9c2d7421ca1e0046a369ab4d56a728347bd391` |
| `QuickFiler/QuickFiler.csproj` | 585 | `eb48cd9c1e3e89886f994d6369ea7ce9757e01625ce3f376e81f466ca2d7b0e2` |
| `QuickFiler.Test/QuickFiler.Test.csproj` | 448 | `1e898fb3b9f6663598a7f99d37193b5b693f0453210603dcd7c11526a2915c0c` |

## Planned absent paths

- `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs`
- `QuickFiler.Test/Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs`
- `QuickFiler.Test/Viewers/BreadcrumbPopupControlDispatchTests.cs`
- `QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs`

## Legacy project Compile inventory

`QuickFiler.csproj` contains exactly one current include for each existing scoped production source: `BreadcrumbBridgeCoordinator.cs` at line 390, `BreadcrumbUiDispatcher.cs` at line 391, `BreadcrumbDropDownHost.cs` at line 395, `BreadcrumbWebViewSurfaceFactory.cs` at line 397, and `ItemViewer.Breadcrumb.cs` at line 415. It contains no `BreadcrumbPopupUiOperations.cs` include.

`QuickFiler.Test.csproj` contains no include for any of the three planned P5 test files. Existing broad-name matches are only prior test sources such as `BreadcrumbBridgeCoordinatorTests.cs` and `BreadcrumbDropDownHostTests.cs`.
