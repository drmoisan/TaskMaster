# Pre-Change Banner-Prefix Declaration Inventory (P0-T16)

Timestamp: 2026-09-01T15-49

Command: `git grep -nE -- 'const +string +[A-Za-z_]*BannerPrefix' -- '*.cs'`

EXIT_CODE: 0

Command: `git grep -nE -- 'const +string +[A-Za-z_]*[Bb]anner[A-Za-z_]*' -- '*.cs'`

EXIT_CODE: 0

Output Summary:

## Primary query — exactly three lines

```
QuickFiler/Controllers/EfcSelectionGuard.cs:15:        private const string BannerPrefix = "===";
UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs:19:        public const string BannerPrefix = "====";
UtilitiesCS/OutlookObjects/Folder/FolderSuggestionTree.cs:16:        private const string BannerPrefix = "====";
```

Member set: `QuickFiler/Controllers/EfcSelectionGuard.cs:15`,
`UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs:19`,
`UtilitiesCS/OutlookObjects/Folder/FolderSuggestionTree.cs:16`. This is exactly
the set this task's acceptance requires.

## Cross-check query — a superset of nine lines

The cross-check is deliberately broadened to a wider name family so that a
differently named banner constant would surface rather than being missed by a
narrow pattern. It returned nine lines, which is a strict superset of the
primary query's three.

### Partition: the three members whose declared name ends in `BannerPrefix`

| Path | Line | Declared identifier |
|---|---|---|
| `QuickFiler/Controllers/EfcSelectionGuard.cs` | 15 | `BannerPrefix` |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs` | 19 | `BannerPrefix` |
| `UtilitiesCS/OutlookObjects/Folder/FolderSuggestionTree.cs` | 16 | `BannerPrefix` |

### Partition: the six non-members

| Path | Line | Declared identifier |
|---|---|---|
| `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbSelectionSessionHighlightTests.cs` | 283 | `BannerText` |
| `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterReplaceItemsTests.cs` | 22 | `BannerText` |
| `UtilitiesCS.Test/OutlookObjects/Folder/FolderProbabilityAdapterTests.cs` | 19 | `Banner` |
| `UtilitiesCS.Test/OutlookObjects/Folder/FolderSuggestionTreeHierarchyTests.cs` | 19 | `SuggestionsBanner` |
| `UtilitiesCS.Test/OutlookObjects/Folder/FolderSuggestionTreeHierarchyTests.cs` | 20 | `SearchBanner` |
| `UtilitiesCS.Test/OutlookObjects/Folder/FolderSuggestionTreeStateTests.cs` | 18 | `Banner` |

Every non-member is a test-fixture constant in a `*.Test` project, and none of
the six declares a banner *prefix*; each holds a full banner text or a sentinel
row. No differently named banner-prefix constant exists in a production project
that the primary pattern would have missed.

All six non-members lie in `*.Test` assemblies, which confirms the issue's
statement that the primary declaration pattern has zero matches in a test
assembly and that no test project is in the sweep scope.
