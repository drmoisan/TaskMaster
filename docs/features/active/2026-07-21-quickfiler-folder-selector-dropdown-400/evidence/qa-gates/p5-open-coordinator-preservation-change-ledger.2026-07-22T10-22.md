# P5 Open Coordinator Preservation Change Ledger

Timestamp: 2026-07-22T10:22:00Z

Command: One read-only PowerShell command constructed an ordered dictionary containing every exact P5-T122 path/SHA-256 pair, emitted each current SHA-256 and physical-line count, derived the changed-path set, reversed only the authorized Host guard and three identity literals in memory and re-hashed those strings, counted TestMethod/DataRow cases, enumerated matching Compile includes and ExcludeFromCodeCoverage occurrences, checked threshold/config invariants, printed the protected assertions, and ran git diff --check against BreadcrumbDropDownHost.cs and BreadcrumbDropDownIntegrationTests.cs.

EXIT_CODE: 0

Output Summary: PASS. Exactly two protected paths differ from P5-T122: BreadcrumbDropDownHost.cs and BreadcrumbDropDownIntegrationTests.cs. Reversing only the one public-boundary Host guard reproduces the exact Host baseline SHA-256. Reversing exactly two committed identity literals and one pending identity literal reproduces the exact integration-test baseline SHA-256. Host is 472 physical lines, within its 480-line limit. The integration test remains exactly 500 physical lines with ten non-data-row tests. The filtered inventory remains 5+10+8+4+10. All other protected hashes, includes, packages, runsettings, coverage configuration, filter, threshold, exclusion, designer, and assertion invariants are unchanged.

## Protected hash status

| Path | Status | Current SHA-256 | Lines |
|---|---|---|---:|
| QuickFiler/Viewers/BreadcrumbDropDownHost.cs | CHANGED, authorized | 17E186B7EE7F684A2310BD06A9787D29884F3CE6B4D25BD83EDB3000EC718C4A | 472 |
| QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs | CHANGED, authorized | B614351681956E2A9427412807FD6F22B270A6C7B6C6F2D331468241D4BFD990 | 500 |
| QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs | UNCHANGED | A5CCA5E401E3612DE406464F4F03C11B3BBD6B1CD76D86FA5AD31AF2C2D5A396 | 480 |
| QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs | UNCHANGED | A4B2822F8D27AC79609AE36DB1DC10CF25CAFAEC7CEE86D4956FC9405CE4BD9C | 277 |
| QuickFiler/Viewers/ItemViewer.Breadcrumb.cs | UNCHANGED | 8D102C659C2B22E0C27684BDCA84152400CCED4CF937ED3DF3122A7DA1863DA0 | 396 |
| QuickFiler/Viewers/BreadcrumbWebViewSurfaceFactory.cs | UNCHANGED | 28726E027BEEF4FE4633BA5BBF00AF6DA7E6C0D59CF6093657545997B3D574C9 | 226 |
| QuickFiler.Test/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs | UNCHANGED | 56391C38A5A4EF599AA82D8BB981A29C61A968753D1604D3B7081CB49090C817 | 132 |
| QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.cs | UNCHANGED | 7BA72D6CBCBC462136DF6C6D5072182CCBBF4BD09EDC8BC79CFF1008E0F6D98A | 395 |
| QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs | UNCHANGED | 46E602D89378582538FFA53B80338C186CC14BE87CF5F4E44BF550986B41B1F5 | 473 |
| QuickFiler.Test/Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs | UNCHANGED | 98DCF6E455A135C41C0ED5529C3EA0AEFAC50DF64CCE912A7CB14F2211465104 | 480 |
| QuickFiler/QuickFiler.csproj | UNCHANGED | 1B9B9F0DA440D3CEA918CB6B178EAC1B603D0886D08E57552C90E89CDC54550E | 588 |
| QuickFiler.Test/QuickFiler.Test.csproj | UNCHANGED | 59DC70BC44CE50E9556738A1BB80B280977576E693D9F23B29943188B2AC96FC | 452 |
| QuickFiler/packages.config | UNCHANGED | 8A4F9EF928E58289ED0964A220FC8B7B33C166098CC46A97F1498D25E8922485 | 110 |
| QuickFiler.Test/packages.config | UNCHANGED | 869B58018BDA096154A669DE597036FCC0452A8B5DD75A2841BEBE1C42393A83 | 168 |
| scripts/vscode/TaskMaster.cli.runsettings | UNCHANGED | 98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57 | 9 |
| coverage.config | UNCHANGED | B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943 | 24 |
| QuickFiler/Viewers/ItemViewer.Designer.cs | UNCHANGED | 0AB37A8F78804DEF674F7E41C028BD14E634E166719FCE933F8758B55D356A5F | 6224 |

## Exact-delta proof

- The Host contains one authorized guard delta. Its in-memory reversal produced baseline SHA-256 7B0A2981918DB95A83EEB077AE860EA62B28C8713CDD537EED5C0BECD9BD6F28.
- The integration test contains exactly two new committed plain:0:A literals and one new pending plain:1:B literal. Their in-memory reversal produced baseline SHA-256 455A0B76AC2606FDA73FB0CF715FC370194CBCE5D5760A3DA99FB305538AFFDB.
- No line was added to or removed from the integration test.

## Inventory invariants

- Test cases: ItemViewerBreadcrumbDropDownContractTests 5; BreadcrumbDropDownOpenCoordinatorTests 10; BreadcrumbSelectorOpenRetryTests 8; BreadcrumbSelectorToggleUiBoundaryTests 4; BreadcrumbDropDownIntegrationTests 10.
- The five production Compile entries and five test Compile entries recorded by P5-T122 remain at the same project lines, and both project hashes are unchanged.
- Both packages.config hashes, the runsettings hash, coverage.config hash, and designer hash are unchanged.
- Threshold inventory remains Host at most 480 lines, integration test exactly 500 lines with no increase, and applicable measurable line coverage at least 90 percent. coverage.config still contains zero threshold/minimum declarations.
- Exclusions remain seven declarations in BreadcrumbPopupUiOperations.cs and two declarations in ItemViewer.Breadcrumb.cs. The two test occurrences remain reflection assertions only.
- Pending-after-close remains null, GetSelectedFolder() remains output path A, selection publication remains zero, and focus return remains one.
- git diff --check returned 0.
