# P11-T11 — Final per-file line counts after the format pass

Timestamp: 2026-08-28T02-31
Command: (Get-Content -LiteralPath <path>).Count for every path returned by `git diff --name-only <BASELINE_SHA> -- QuickFiler/ QuickFiler.Test/`, plus the five P0-T15 baseline paths absent from that diff
EXIT_CODE: 0

Loop iteration: **1**. This audit runs **after** the P11-T2 format pass, because formatting can change
line counts. The pass rewrote nothing (P11-T2 recorded 0 files with a changed SHA-256), so these
counts are also the post-implementation counts.

Counts are taken with `(Get-Content -LiteralPath <path>).Count`, not `Measure-Object -Line`; the two
disagree on files without a trailing newline.

`BASELINE_SHA` is `cecd78130a489fcfdc2ddac7970f344256f4a75a`. The diff returns 25 paths, which
already include the four new test files, so the "diff plus the four new test files" audit set is
those 25.

## `path = baseline -> final`, one row per file

Baselines are the values `evidence/baseline/phase0-file-line-counts.2026-08-27T23-31.md` measured.
`NEW` means the file did not exist at baseline.

```
QuickFiler/Viewers/ItemViewer.cs                                        = 432 -> 400   (-32)
QuickFiler/Viewers/ItemViewer.Designer.cs                               = 6224 -> 6223 (-1)
QuickFiler/Viewers/ItemViewer.DisplayState.cs                           = 81 -> 81     (0)
QuickFiler/Viewers/ItemViewer.FolderSearch.cs                           = 81 -> 81     (0)
QuickFiler/Viewers/ItemViewerExpanded.cs                                = 181 -> 154   (-27)
QuickFiler/Viewers/ItemViewerExpanded.Designer.cs                       = 821 -> 816   (-5)
QuickFiler/Viewers/IItemViewer.cs                                       = 143 -> 172   (+29)
QuickFiler/Controllers/QfcItemController.EventHandlers.cs               = 223 -> 228   (+5)
QuickFiler/Controllers/QfcItemController.EventWiring.cs                 = 482 -> 483   (+1)
QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs               = 338 -> 358   (+20)
QuickFiler/Controllers/QfcItemController.MailActions.cs                 = 257 -> 259   (+2)
QuickFiler/Controllers/QfcItemController.FolderHandling.cs              = 235 -> 235   (0)
QuickFiler.Test/QuickFiler.Test.csproj                                  = 493 -> 497   (+4)
QuickFiler.Test/Viewers/ToolStripMenuItemCbTests.cs                     = NEW -> 165
QuickFiler.Test/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs    = 132 -> 325   (+193)
QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs             = 477 -> 477   (0)
QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs           = 500 -> 500   (0)
QuickFiler.Test/Controllers/QfcItemController.ThemeMarshallingTests.cs  = NEW -> 129
QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs       = 499 -> 499   (0)
QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.Part2.cs = NEW -> 81
QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.cs       = 498 -> 498   (0)
QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.Part2.cs = NEW -> 141
QuickFiler.Test/Controllers/QfcItemController.SeamDispatcherTests.cs    = 352 -> 352   (0)
QuickFiler.Test/Controllers/QfcItemController.FolderSuggestionsTests.cs = 191 -> 191   (0)
QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs    = 498 -> 498   (0)
```

The five paths P0-T15 baselined that do **not** appear in the diff were re-measured as well, to
confirm the diff is not hiding a change:

```
QuickFiler/Viewers/ItemViewer.WebViewThread.cs                          = 37 -> 37     (0)
QuickFiler/Viewers/ItemViewer.Commands.cs                               = 109 -> 109   (0)
QuickFiler/Viewers/ToolStripMenuItemCb.cs                               = 87 -> 87     (0)
QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs     = 497 -> 497   (0)
QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs             = 500 -> 500   (0)
```

`QfcItemController.FocusAndThemeTests.cs` finishes at **497**, the figure spec AC21 pins, and
`QfcCollectionControllerTests.cs` at **500**, the figure sibling 468 pins. Both are unchanged.

## Acceptance part (a) — no file this feature adds or grows exceeds 500 lines

| File | Final | Under 500? |
|---|---:|---|
| `QuickFiler.Test/QuickFiler.Test.csproj` | 497 | Yes |
| `QuickFiler/Controllers/QfcItemController.EventWiring.cs` | 483 | Yes |
| `QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs` | 358 | Yes |
| `QuickFiler.Test/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs` | 325 | Yes |
| `QuickFiler/Controllers/QfcItemController.MailActions.cs` | 259 | Yes |
| `QuickFiler/Controllers/QfcItemController.EventHandlers.cs` | 228 | Yes |
| `QuickFiler/Viewers/IItemViewer.cs` | 172 | Yes |
| `QuickFiler.Test/Viewers/ToolStripMenuItemCbTests.cs` (new) | 165 | Yes |
| `QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.Part2.cs` (new) | 141 | Yes |
| `QuickFiler.Test/Controllers/QfcItemController.ThemeMarshallingTests.cs` (new) | 129 | Yes |
| `QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.Part2.cs` (new) | 81 | Yes |

The largest grown file is the project file at **497** and the largest grown source file is
`QfcItemController.EventWiring.cs` at **483**; the largest new file is 165. Every added or grown file
is at or under 500. Satisfied.

The two `*.Designer.cs` files are exempt from the 500-line clause and are asserted only against their
own baselines, which they must not exceed: `ItemViewer.Designer.cs` 6224 -> **6223** and
`ItemViewerExpanded.Designer.cs` 821 -> **816**. Both shrink, so neither exceeds its baseline. Both
were already above the ceiling at baseline; that excess is pre-existing out-of-scope finding O5 and
this feature only removes lines from them.

## Acceptance part (b) — every file that grew is on the intentional-growth list, with its task

Exactly **seven** files exceed their P0-T15 baseline, and all seven are on the list of seven:

| # | File | Baseline -> final | Grown by | Plan's authoring-time expectation |
|---|---|---|---|---|
| 1 | `QuickFiler.Test/QuickFiler.Test.csproj` | 493 -> 497 (+4) | P1-T2, P1-T4, P5-T2, P7-T3 | 477 — **mismatch, recorded below** |
| 2 | `QuickFiler/Controllers/QfcItemController.EventWiring.cs` | 482 -> 483 (+1) | P2-T6 | 391 — **mismatch** |
| 3 | `QuickFiler/Controllers/QfcItemController.EventHandlers.cs` | 223 -> 228 (+5) | P2-T5 | 223 — matches |
| 4 | `QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs` | 338 -> 358 (+20) | P6-T1 | 326 — **mismatch** |
| 5 | `QuickFiler/Viewers/IItemViewer.cs` | 143 -> 172 (+29) | P9-T3, P9-T4 | 143 — matches |
| 6 | `QuickFiler.Test/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs` | 132 -> 325 (+193) | P1-T3, P3-T1, P5-T3, P7-T1, P7-T2 | 132 — matches |
| 7 | `QuickFiler/Controllers/QfcItemController.MailActions.cs` | 257 -> 259 (+2) | P8-T8 | 224 — **mismatch** |

The csproj grew by **exactly 4**, one entry per the four tasks named, as the list requires. The
`MailActions.cs` growth of **+2** is exactly the "up to 2 lines" the list allows: P8-T8 replaces a
two-line pair with a three-line local-hold form in each of `FlagAsTask` and `FlagAsTaskAsync`, and
P8-T3 and P8-T6 are line-neutral in that file.

### Recorded mismatches between the plan's expectations and the measured baselines

The plan states that "the value P0-T15 actually measures governs; the figure here is the
authoring-time expectation and a mismatch is recorded, not silently adopted". Four of the seven
expectations do not match what P0-T15 measured, and the mismatch is recorded here rather than
adopted:

- `QuickFiler.Test.csproj`: expected 477, measured **493** (+16). The expectation predates the
  merged-sibling 444 and 493 entries.
- `QfcItemController.EventWiring.cs`: expected 391, measured **482** (+91).
- `QfcItemController.FocusAndTheme.cs`: expected 326, measured **338** (+12).
- `QfcItemController.MailActions.cs`: expected 224, measured **257** (+33).

All four are upward divergences consistent with merged siblings 484, 444 and 493 having grown these
files after this plan was authored, which is the same cause P0-T15 recorded for the test files. Every
comparison in parts (b) and (c) above is made against the **measured** P0-T15 value, never against
the authoring-time expectation. No acceptance outcome changes: all seven grow, all seven are on the
list, and all seven finish at or under 500.

## Acceptance part (c) — no file outside the list exceeds its baseline

Every remaining pre-existing file in the audit set is line-neutral or shrinks:

- **Zero delta (11 files):** `ItemViewer.DisplayState.cs`, `ItemViewer.FolderSearch.cs`,
  `QfcItemController.FolderHandling.cs`, `BreadcrumbSelectorOpenRetryTests.cs`,
  `BreadcrumbDropDownIntegrationTests.cs`, `QfcItemController.EventWiringTests.cs`,
  `QfcItemController.MailActionsTests.cs`, `QfcItemController.SeamDispatcherTests.cs`,
  `QfcItemController.FolderSuggestionsTests.cs`, `QfcItemController.FolderHandlingTests.cs`, plus the
  five non-diff paths listed above.
- **Negative delta (5 files):** `ItemViewer.cs` (-32), `ItemViewerExpanded.cs` (-27),
  `ItemViewerExpanded.Designer.cs` (-5), `ItemViewer.Designer.cs` (-1).

The two parents deliberately kept off the growth list finish **exactly at their P0-T15 baselines**:
`QfcItemController.EventWiringTests.cs` at 499 and `QfcItemController.MailActionsTests.cs` at 498,
1 and 2 lines of spare respectively. Every edit they received — P1-T4's and P7-T3's `partial`
modifiers and P8-T7's renames — is line-neutral, exactly as the list argues, and the new tests were
routed to the `Part2` continuation files instead. Part (c) holds them and they hold.

The four new files have no baseline to exceed and are governed by part (a) alone, which they satisfy.

Output Summary: The line-count audit **passes** on all three parts. (a) Every file this feature adds
or grows is at or under 500 lines; the largest are the project file at 497 and
`QfcItemController.EventWiring.cs` at 483, and the largest new file is 165. The two `*.Designer.cs`
files are exempt from the ceiling and both **shrink** against their own baselines, 6224 to 6223 and
821 to 816. (b) Exactly seven files exceed their P0-T15 baseline and all seven are on the
intentional-growth list, each named with the task that grew it; the csproj grew by exactly the 4
entries its four tasks add and `MailActions.cs` by exactly the 2 lines allowed. Four of the plan's
seven authoring-time baseline expectations do not match what P0-T15 measured — 477 against 493, 391
against 482, 326 against 338 and 224 against 257 — and each mismatch is **recorded** rather than
adopted, with every comparison made against the measured value. (c) No file outside the list exceeds
its baseline: 16 are line-neutral and 4 shrink, and the two deliberately excluded parents finish
exactly at their baselines of 499 and 498. `QfcItemController.FocusAndThemeTests.cs` is unchanged at
497 and `QfcCollectionControllerTests.cs` at 500.
