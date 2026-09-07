# Phase 0 — Per-file Line Counts (P0-T15)

Timestamp: 2026-08-27T23-31
Command: (Get-Content -LiteralPath <path>).Count for each of the 26 paths below
EXIT_CODE: 0

Counts are taken with `(Get-Content -LiteralPath <path>).Count`, not `Measure-Object -Line`; the two
disagree on files without a trailing newline.

## Production — ItemViewer family

```
QuickFiler/Viewers/ItemViewer.cs = 432
QuickFiler/Viewers/ItemViewer.Designer.cs = 6224
QuickFiler/Viewers/ItemViewer.DisplayState.cs = 81
QuickFiler/Viewers/ItemViewer.FolderSearch.cs = 81
QuickFiler/Viewers/ItemViewer.WebViewThread.cs = 37
QuickFiler/Viewers/ItemViewer.Commands.cs = 109
QuickFiler/Viewers/ItemViewerExpanded.cs = 181
QuickFiler/Viewers/ItemViewerExpanded.Designer.cs = 821
QuickFiler/Viewers/ToolStripMenuItemCb.cs = 87
QuickFiler/Viewers/IItemViewer.cs = 143
```

## Production — QfcItemController partials

```
QuickFiler/Controllers/QfcItemController.EventHandlers.cs = 223
QuickFiler/Controllers/QfcItemController.EventWiring.cs = 482
QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs = 338
QuickFiler/Controllers/QfcItemController.MailActions.cs = 257
QuickFiler/Controllers/QfcItemController.FolderHandling.cs = 235
```

## Test files

```
QuickFiler.Test/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs = 132
QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs = 477
QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs = 500
QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs = 499
QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.cs = 498
QuickFiler.Test/Controllers/QfcItemController.SeamDispatcherTests.cs = 352
QuickFiler.Test/Controllers/QfcItemController.FolderSuggestionsTests.cs = 191
QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs = 498
QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs = 497
QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs = 500
```

## Project file

```
QuickFiler.Test/QuickFiler.Test.csproj = 493
```

Every one of the 26 listed paths has an integer row. No path was missing.

## The two generated files are already above the 500-line ceiling

`QuickFiler/Viewers/ItemViewer.Designer.cs` is **6224** lines and
`QuickFiler/Viewers/ItemViewerExpanded.Designer.cs` is **821** lines. Both are already far above the
500-line ceiling. That excess is pre-existing, is not created by this feature, and is not remediated
by it — this feature only removes lines from both. Accordingly **no "must be under 500 lines"
condition is asserted over either file** anywhere in this plan or in `spec.md`, and none may be
introduced later. This is recorded as out-of-scope finding O5.

## Capacity divergence from the planning-time figures — blocking for later phases

Several counts have moved materially since the research document measured them on 2026-08-25, in the
direction that removes headroom. The research figures are in section 7.1 of
`research/2026-08-25T02-15-itemviewer-surface-defects-research.md`.

| File | Research count | Count now | Spare to 500 | Plan routes new tests here? |
|---|---:|---:|---:|---|
| `QfcItemController.EventWiringTests.cs` | 374 | **499** | **1** | **Yes — two new tests for #486 D3** |
| `QfcItemController.MailActionsTests.cs` | 184 | **498** | **2** | **Yes — three new tests for #490 D3 and #490 D4, appended at P7-T3 and P7-T7** |
| `BreadcrumbDropDownIntegrationTests.cs` | not tabulated | **500** | **0** | rename-only edits, line-neutral |
| `QfcItemController.FolderSuggestionsTests.cs` | not tabulated | 191 | 309 | rename-only edits |
| `BreadcrumbSelectorOpenRetryTests.cs` | not tabulated | 477 | 23 | rename-only edits, line-neutral |
| `QfcItemController.EventWiring.cs` | 391 | **482** | 18 | one added wire line |
| `QfcItemController.FocusAndTheme.cs` | 326 | **338** | 162 | `HtmlDarkConverter` guard |
| `QfcItemController.MailActions.cs` | 224 | **257** | 243 | discard form, read-back removal |

The three unchanged anchors are `ItemViewerBreadcrumbDropDownContractTests.cs` at **132** (368 spare,
still the intended landing zone for every metadata-absence assertion),
`QfcItemController.FocusAndThemeTests.cs` at **497**, which matches the figure spec AC21 asserts must
be unchanged, and `QfcCollectionControllerTests.cs` at **500**, still pinned by 468.

The first two rows are the material ones. The plan routes new `[TestMethod]` bodies into
`QfcItemControllerEventWiringTests.cs` and `QfcItemController.MailActionsTests.cs` on the strength of
"126 spare" and "316 spare" respectively. The measured headroom is **1 line** and **2 lines**. Two
`[TestMethod]` bodies cannot be added to a file with one spare line without breaching the 500-line
ceiling that `.claude/rules/general-code-change.md` sets and that spec AC47 asserts. The growth is
consistent with upstreams 484 and 444 having landed, which P0-T17 measures directly.

This is recorded here as a measurement, which is what P0-T15 exists to produce. It is not resolved
here: choosing a different landing file, or splitting a file, is a planning decision outside this
task and outside Phase 0, which may edit no test file.

Output Summary: All 26 listed paths have an integer line-count row. `ItemViewer.Designer.cs` (6224)
and `ItemViewerExpanded.Designer.cs` (821) are **already above 500 lines**, so no "must be under 500
lines" condition is asserted over either. Three test files this plan appends to have materially less
headroom than the research document recorded: `QfcItemController.EventWiringTests.cs` is **499** lines
(1 spare, plan adds two tests), `QfcItemController.MailActionsTests.cs` is **498** lines (2 spare,
plan adds three tests), and `BreadcrumbDropDownIntegrationTests.cs` is at **500**. The intended
metadata landing zone `ItemViewerBreadcrumbDropDownContractTests.cs` is unchanged at 132 lines with
368 spare, and `QfcItemController.FocusAndThemeTests.cs` is 497, matching the figure spec AC21 pins.
