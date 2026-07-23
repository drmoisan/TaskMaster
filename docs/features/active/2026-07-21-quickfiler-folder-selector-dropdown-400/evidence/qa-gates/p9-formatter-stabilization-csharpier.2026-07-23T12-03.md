# Phase 9 Formatter-Stabilization CSharpier Gate

- Timestamp: `2026-07-23T12:03:56Z`
- Command: `$base=(git merge-base HEAD origin/main).Trim(); $patterns=@('QuickFiler/**/*.cs','QuickFiler.Test/**/*.cs','UtilitiesCS/**/*.cs','UtilitiesCS.Test/**/*.cs'); $spam='UtilitiesCS/EmailIntelligence/ClassifierGroups/SpamBayes/SpamBayes.Actions.cs'; $authorized=@(@(git diff --name-only --diff-filter=ACMR $base -- $patterns)+@(git ls-files --others --exclude-standard -- $patterns)|Sort-Object -Unique|Where-Object {$_ -ne $spam}); require count/hash/protected hashes; csharpier format @authorized; csharpier check @authorized; verify protected hashes, every authorized file <=500 lines, and all retained headroom bounds`
- EXIT_CODE: `0`
- Output Summary: `P8_T22_CSHARPIER_OK format_exit=0 check_exit=0 authorized=62 path_hash=E2439D9F8A28D97A05EA3EEFB3201587904CC784FCB9EF7200632F6BEED3EBCD changed=5 protected_changes=0 max_over=0`

## Formatter Results

```text
Formatted 62 files in 16053ms.
Checked 62 files in 10434ms.
```

CSharpier changed exactly the five expected authorized paths:

| Path | Pre-format SHA-256 | Post-format SHA-256 | Post-format lines |
|---|---|---|---:|
| `QuickFiler/Viewers/BreadcrumbMessengerHub.cs` | `AE307D76F01FB5C50289E9F50B6FC5F05C770A81EA4827BA010C00336A1006B2` | `BCC9C08DC0222754508EBBD15AB426876DB87F70176496DD9CD6031A09508559` | 456 |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownCoverageThresholdTests.cs` | `25EE741353DB8CFA625F5783ED7CA17697768FBAB826865F53D72F0DF4BBBD77` | `A3D6FD01550C8E50BFA9E70FE2DC889777926D08CA2144EAACA0013CB60D9434` | 477 |
| `QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs` | `46E602D89378582538FFA53B80338C186CC14BE87CF5F4E44BF550986B41B1F5` | `80492FD30AD7DD5B60517D8213BEF429FEBA305A328DFDF370242DA78B908987` | 470 |
| `QuickFiler.Test/Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs` | `98DCF6E455A135C41C0ED5529C3EA0AEFAC50DF64CCE912A7CB14F2211465104` | `5BBA5553A25D5CA27E422EF51DE09A9971F9211D34AD558ED0A14E4DCC544998` | 478 |
| `QuickFiler.Test/Viewers/BreadcrumbPopupControlDispatchTests.cs` | `8A21B50B684A1B0D2D471A24FCAB8CC2EE59B2C6FD7D067097099487BE6264C9` | `A59DDA03D17572E9597B9146AD1E84AF8FE7A919DE5A7B611DBEDB38E9B9B356` | 479 |

The popup-dispatch test retained:

- 11 ordered test methods with name hash `DFCD8BB714DB88473F702E9E8122F15BCF4EB8B749F5A0CE9F36321DD2266981`.
- Three data rows and 13 expected discovered cases.
- 52 `.Should()` occurrences across 44 assertion-bearing lines.
- Assertion-line hash `0FA3A31B15FE6825B716DEB28E0CFAE58CE8014891AA6BA901FDD0ABD2034BEC`.

These values exactly match the P8-T20 pre-edit semantic ledger.

## Protected Scope

| Protected path | Required and observed SHA-256 |
|---|---|
| `coverage.config` | `B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943` |
| `.csharpierignore` | `362211A82C6C6887E023B1B1936408715D6C35CC42E0D91B46E8E76A29318C25` |
| `UtilitiesCS/EmailIntelligence/ClassifierGroups/SpamBayes/SpamBayes.Actions.cs` | `99AFDEB968CD88ED657807E17CD1EE804D0043AEF3879E4D30C2259ED73945DA` |

All protected hashes matched before and after formatting.

## Retained Headroom

| Path | Lines | Limit |
|---|---:|---:|
| `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` | 480 | 480 |
| `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | 480 | 480 |
| `QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs` | 477 | 480 |
| `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` | 399 | 460 |
| `QuickFiler.Test/Viewers/BreadcrumbPopupControlDispatchTests.cs` | 479 | 480 |
| `QuickFiler.Test/Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs` | 478 | 480 |
| `QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs` | 470 | 480 |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownCoverageThresholdTests.cs` | 477 | 480 |
| `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` | 309 | 480 |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.cs` | 444 | 480 |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs` | 341 | 480 |
| `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs` | 361 | 480 |
| `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.Part2.cs` | 480 | 480 |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownLifecycleCoverageTests.cs` | 468 | 480 |
| `QuickFiler.Test/Viewers/BreadcrumbMessengerHubCoverageTests.cs` | 478 | 480 |
| `QuickFiler.Test/Viewers/BreadcrumbCollapsedSurfaceReadinessTests.cs` | 485 | 486 |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs` | 500 | 500 |

No authorized C# file exceeds 500 physical lines. No production behavior, project include, configuration, filter, threshold, exclusion, or unrelated file was changed by this gate beyond the planned CSharpier output.
