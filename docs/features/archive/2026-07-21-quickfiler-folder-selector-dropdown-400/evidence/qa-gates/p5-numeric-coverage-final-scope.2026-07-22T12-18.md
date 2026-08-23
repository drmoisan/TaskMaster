# P5 numeric-coverage final scope, file-size, project, and exclusion inventory

Timestamp: `2026-07-22T12-18`

Command: `git status --short`; `(Get-Content -LiteralPath <file>).Count` per file; `sha256sum <files>`; `grep -c "ExcludeFromCodeCoverage" <files>`; `grep -c "<file>.cs" <csproj>`; `git diff --name-only $(git merge-base HEAD origin/main) HEAD -- 'QuickFiler/Viewers/*.cs' 'QuickFiler.Test/Viewers/*.cs'` (read-only deterministic inspection).

EXIT_CODE: `0`

Output Summary: PASS. The J1/J2 numeric/preservation revision changed exactly four production files and six test files. Each of the five new C# files (one coordinator production file plus four new test files) has exactly one adjacent legacy-project include, and J2 added no new file or include. All files are at most 500 lines; ItemViewer is 396 (<=460), Host is 472 (<=480), every helper/new test is at most 480, the integration test is exactly 500, and the only existing P5 file above 480 is the pre-authorized 486-line collapsed-readiness test. No package, runsettings, coverage-config, filter, threshold, designer, or exclusion surface changed. The class/method exclusion set is unchanged (ItemViewer class 1, ItemViewer.Breadcrumb 2, BreadcrumbPopupUiOperations 7, coordinator 0), so numeric coverage remains mandatory for every changed host-neutral body; only the seven unchanged Popup direct adapters, exact ItemViewer direct WebView2/WinForms adapter lines, and minimal one-line coordinator delegation are permitted as nonnumeric accounting.

## Changed production files (exactly four)

| Source | Role in J1/J2 revision | Lines | Limit | SHA-256 |
|---|---|---:|---|---|
| `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` | J1 new host-neutral coordinator | 277 | <=480 (new helper) | `a4b2822f8d27ac79609ae36db1dc10cf25cafaec7cee86d4956fc9405ce4bd9c` |
| `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` | J1 modified (host-neutral orchestration extracted out) | 396 | <=460 | `8d102c659c2b22e0c27684bdca84152400cced4cf937ed3df3122a7da1863da0` |
| `QuickFiler/Viewers/BreadcrumbWebViewSurfaceFactory.cs` | J1 modified (wrapper/null-tuple removal) | 226 | <=500 | `28726e027beef4fe4633ba5bbf00af6da7e6c0d59cf6093657545997b3d574c9` |
| `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | J2 modified (public-boundary `surfaceFactory` guard) | 472 | <=480 | `17e186b7ee7f684a2310bd06a9787d29884f3ce6b4d25bd83edb3000ec718c4a` |

## Changed test files (exactly six)

| Test source | Role in J1/J2 revision | Lines | Limit | SHA-256 |
|---|---|---:|---|---|
| `QuickFiler.Test/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs` | J0/J1 modified five-case contract fixture | 132 | <=480 | `56391c38a5a4ef599aa82d8bb981a29c61a968753d1604d3b7081cb49090c817` |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.cs` | J1 new (10 coordinator cases) | 395 | <=480 | `7ba72d6cbcbc462136df6c6d5072182ccbbf4bd09edc8bc79cff1008e0f6d98a` |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs` | J2 modified (identity/`ParamName` corrections) | 500 | exactly 500 | `b614351681956e2a9427412807fd6f22b270a6c7b6c6f2d331468241d4bfd990` |
| `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs` | Batch K new (18 cases) | 479 | <=480 | `d537569ce3c7917739008bd0138297438474649864c5c3bff0e92d098f57848e` |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownLifecycleCoverageTests.cs` | Batch L new (12 cases) | 468 | <=480 | `70d700c6f4ef145b106fdda5058fdcaea99471ce229d43448dc9917923f2b9d3` |
| `QuickFiler.Test/Viewers/BreadcrumbMessengerHubCoverageTests.cs` | Batch M new (10 cases) | 478 | <=480 | `4387e3b3f98ce0fa5db06488d117dbffe214dc7212e2518d721a0134fc631eb3` |

## New-file include accounting (five new C# files, one include each)

- `QuickFiler/QuickFiler.csproj` contains exactly one `BreadcrumbDropDownOpenCoordinator.cs` `Compile` include (grep count 1); production csproj SHA-256 `1b9b9f0da440d3cea918cb6b178eac1b603d0886d08e57552c90e89cdc54550e` (unchanged since J1).
- `QuickFiler.Test/QuickFiler.Test.csproj` contains exactly one include each for `BreadcrumbDropDownOpenCoordinatorTests.cs`, `BreadcrumbPopupBoundaryCoverageTests.cs`, `BreadcrumbDropDownLifecycleCoverageTests.cs`, and `BreadcrumbMessengerHubCoverageTests.cs` (grep count 1 each); test csproj SHA-256 `ccc27a208c1c66c72ec53ccdf51918b6bdfe868faf2f387b61380ce09b8d627f`.
- J2 (Host + integration test) added no new file and no new include.

## File-size compliance

- All ten changed files are at most 500 lines.
- ItemViewer.Breadcrumb.cs: 396 <= 460.
- BreadcrumbDropDownHost.cs: 472 <= 480.
- New helper and new/coverage tests: 277, 395, 479, 468, 478 — all <= 480.
- Integration test: exactly 500.
- Existing P5 headroom file above 480: only `BreadcrumbCollapsedSurfaceReadinessTests.cs` at 486, which is the pre-authorized exception.

## Exclusion and protected-configuration integrity

- `ItemViewer.cs`: exactly 1 `ExcludeFromCodeCoverage` (class-level); SHA-256 `498d1781be7df3665d799a4dfc9837ad4f81d6a47b0dec1cb1c0a84d025ab0e2` (unchanged).
- `ItemViewer.Breadcrumb.cs`: exactly 2 method exclusions (unchanged direct collapsed-surface adapters).
- `BreadcrumbPopupUiOperations.cs`: exactly 7 method exclusions; SHA-256 `a5cca5e401e3612de406464f4f03c11b3bbd6b1cd76d86fa5ad31af2c2d5a396`.
- `BreadcrumbDropDownOpenCoordinator.cs`, `BreadcrumbDropDownHost.cs`, `BreadcrumbWebViewSurfaceFactory.cs`: 0 exclusions.
- `coverage.config`: `b9cd80356c6bdbe03807a0b8cb106ae03d24efbdbb2515097fbf003099050943` (unchanged).
- `scripts/vscode/TaskMaster.cli.runsettings`: `98ef03a8d3b0ebb2ed7a765e3b5e1b58e774d20202df2f294c03a7260b9cef57` (unchanged).
- `QuickFiler/Viewers/ItemViewer.Designer.cs`: `0ab37a8f78804def674f7e41c028bd14e634e166719fce933f8758b55d356a5f`; absent from the merge-base `.cs` change set (no designer runtime change).
- `QuickFiler/packages.config`: `8a4f9ef928e58289ed0964a220fc8b7b33c166098cc46a97f1498d25e8922485` (unchanged).
- `QuickFiler.Test/packages.config`: `869b58018bda096154a669de597036fcc0452a8b5dd75a2841bebe1c42393a83` (unchanged).

## Nonnumeric-accounting scope

Because no exclusion attribute was added, removed, or widened, every changed host-neutral body remains in the numeric coverage denominator. Nonnumeric accounting is permitted in P5-T159 only for: (a) the seven unchanged `BreadcrumbPopupUiOperations` direct WebView2/WinForms adapters, (b) exact ItemViewer direct WebView2/WinForms adapter lines, and (c) minimal one-line coordinator delegation. Per the P5-T116 extraction ledger, no host-neutral popup-open body remains inside excluded ItemViewer code; excluded host-neutral bodies are rejected.

## Result

Scope inventory PASS. No contradiction, no unauthorized file/include/config/exclusion change, and no file-size violation. Ready to proceed to the P5-T154 replacement-pass CSharpier gate.
