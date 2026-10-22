# P8-T7 — Test call-site rename line-neutrality

Timestamp: 2026-08-28T01-23
Command: (Get-Content -LiteralPath <path>).Count for each of the six files, immediately before and immediately after the rename edits
EXIT_CODE: 0
ExpectedExitCode: 0

## Per-file line counts, pre-task versus post-task

```
QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs             = 477 -> 477
QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs           = 500 -> 500
QuickFiler.Test/Controllers/QfcItemController.SeamDispatcherTests.cs    = 352 -> 352
QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.cs       = 498 -> 498
QuickFiler.Test/Controllers/QfcItemController.FolderSuggestionsTests.cs = 191 -> 191
QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs    = 498 -> 498
```

Every one of the six is unchanged across this task. The rename is token-for-token — `Set` to `Add`,
both three characters, inside an unchanged member name of unchanged length — so no line grows, no
line wraps and no line is added. Under the Part2 rerouting these pre-task figures also coincide with
the P0-T15 baseline for all six files: `QfcItemController.MailActionsTests.cs` still measures its
baseline 498 with only 2 spare lines to the 500-line ceiling, which is exactly why the Phase 7
additions went to `QfcItemController.MailActionsTests.Part2.cs`, and
`QfcItemController.FolderHandlingTests.cs` likewise still measures its baseline 498.
`BreadcrumbDropDownIntegrationTests.cs` sits exactly at the 500-line ceiling before and after, so a
non-neutral edit there would have breached it.

## The fourteen renamed sites

Located and confirmed by `git grep -n "SetFolderItems" -- QuickFiler.Test/` before the edits, at the
exact line numbers the task prints:

```
BreadcrumbSelectorOpenRetryTests.cs:261
BreadcrumbDropDownIntegrationTests.cs:170, :248, :341
QfcItemController.SeamDispatcherTests.cs:193
QfcItemController.MailActionsTests.cs:67, :88
QfcItemController.FolderSuggestionsTests.cs:131, :159, :183
QfcItemController.FolderHandlingTests.cs:349, :407, :433, :476
```

That is 1 + 3 + 1 + 2 + 3 + 4 = 14. The `:67` and `:88` in the mail-actions parent are the
current-head numbers; the `:66` and `:87` the plan originally printed were pre-sibling-growth values.
The renames were applied by replacing the invocation prefixes `v.SetFolderItems(` and
`Viewer.SetFolderItems(` only, so no identifier that merely contains the token could be caught.

## Diff shape against BASELINE_SHA

```
4	4	QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs
3	3	QuickFiler.Test/Controllers/QfcItemController.FolderSuggestionsTests.cs
3	3	QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.cs
1	1	QuickFiler.Test/Controllers/QfcItemController.SeamDispatcherTests.cs
3	3	QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs
1	1	QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs
```

Added equals deleted in every row, which is the diff signature of a line-neutral edit. Five of the
six rows equal that file's rename count exactly. The sixth, `QfcItemController.MailActionsTests.cs`
at `3`, is its 2 renames plus P7-T3's line-neutral `partial` modifier, which is the only other edit
this plan makes to that file.

## The four protected identifiers and three protected comments are unchanged

```
QfcItemController.FolderSuggestionsTests.cs:111:        public void AssignFolderComboBox_RetainsSetFolderItemsAndIndexOneSelection()
QfcItemController.FolderSuggestionsTests.cs:169:        public void MarkItemForDeletion_StillAppendsTrashToDeleteViaSetFolderItems()
QfcItemController.FolderSuggestionsTests.cs:16:    /// <see cref="IItemViewer.SetFolderItems"/> call sites (index-1/predetermined selection and the
QfcItemController.FolderSuggestionsTests.cs:130:            // Assert — the retained SetFolderItems(string[]) population and index-1 selection remain.
QfcItemController.EventHandlersTests.cs:315:        /// once, <c>SetFolderItems</c> once, <c>SetFolderSelectedIndex(1)</c> once,
```

No test method was renamed. Renaming one would change its node ID and invalidate a sibling
acceptance condition, and the two names above are pinned by P8-T11.

Output Summary: All fourteen test call sites are renamed invocation-only across the six named files.
Every one of the six files measures **the same line count before and after** this task — 477, 500,
352, 498, 191 and 498 — and every file's `git diff --numstat` row against `BASELINE_SHA` has added
equal to deleted. The two protected test method names and the three protected comments are
unchanged.
