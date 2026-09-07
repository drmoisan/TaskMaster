# [P7-T13] #465 delivered source-structure evidence

Timestamp: 2026-08-28T01-25
Task: [P7-T13]
Command: source inspection of the delivered `QuickFiler/Controllers/EfcFormController.cs` with `awk`
line numbering and `grep -n`; pre-change figures read with
`git show <BASELINE_SHA>:QuickFiler/Controllers/EfcFormController.cs`, not from the working tree; plus
`git diff --name-only <BASELINE_SHA> -- <path>` for the two read-only files.
EXIT_CODE: 0

BASELINE_SHA for this run: `38f097898639b054428188c9c5e266e54972c259` (the merge base with
`epic/quickfiler-bug-family-integration` after the orchestrator's base merge).

## RC8 — `RefreshSuggestionsAsync`, the cross-thread control read

### Delivered body, verbatim

```csharp
        public async Task RefreshSuggestionsAsync()
        {
            var searchText = _formViewer.SearchText.Text;

            await Task.Run(() => _dataModel.RefreshSuggestions(), Token);
            var matches = await Task.Run(
                () => MatchesForSearchText(_dataModel.FindMatches, searchText),
                Token
            );

            BindSourceFolderRows(matches);
        }
```

### The measurable statement of the RC8 remedy: a sign flip

| Measure | Pre-change (read from `BASELINE_SHA`) | Delivered |
|---|---|---|
| Occurrences of the token `_formViewer` inside the method | 1 | **1** |
| Line of that occurrence | `:799` | `:879` |
| Line of the first `Task.Run(` occurrence | `:797` | `:881` |
| **Offset (`_formViewer` line − first `Task.Run(` line)** | **+2 (positive)** | **−2 (negative)** |

Pre-change the control read sat **after** the first `Task.Run(`, inside the second `Task.Run` lambda.
Delivered, it sits **before** it, as the method's first statement. The sign flip from positive to
negative is the structural fail-before/pass-after pair for this defect, which cannot be measured
behaviourally — see the `[P7-T14]` exception dossier.

No member access on `_formViewer` remains inside either `Task.Run` lambda.

## RC9 — `BindFolderRows` separates presentation from retention

### Delivered body, verbatim

```csharp
        private void BindFolderRows(string[] rows)
        {
            var formViewer = _formViewer;
            if (formViewer == null || _router == null)
            {
                return;
            }

            _ = BindBreadcrumbRowsAsync(rows ?? Array.Empty<string>());
        }
```

| Measure | Pre-change (read from `BASELINE_SHA`) | Delivered |
|---|---|---|
| Assignments to `_folderRows` inside `BindFolderRows` | 1, the write-back at `:879` | **0** |
| Reads of `_folderRows` inside `BindFolderRows` | 1, at `:880` | **0** |
| Argument expression passed to `BindBreadcrumbRowsAsync` | `_folderRows` (the field) | **`rows ?? Array.Empty<string>()`** (the method's own parameter) |

Both figures are recorded because `[P7-T7]` removes the `:880` read as well as the `:879` assignment;
without both, a later auditor could not verify the read removal independently of the plan text. The
delivered argument expression is the third, independent confirmation.

### `_folderRows` assignment sites after the change

| Site | Line | Kind |
|---|---|---|
| declaration initializer | `:141` | retained, `= Array.Empty<string>()` |
| `ApplyDeleteGesture` | `:805` | method body |
| `BindSourceFolderRows` | `:975` | method body |

Exactly **two method bodies** assign the field, plus the retained declaration initializer.

### `BindSourceFolderRows` call sites

Declared at `:967`. Called from exactly **three** sites, all source paths:

| Call site | Line |
|---|---|
| `SearchText_TextChanged` | `:601` |
| `RefreshSuggestionsAsync` | `:887` |
| `PopulateFolderCombobox` | `:1150` |

The delete site continues to call `BindFolderRows`, through `ApplyDeleteGesture` at `:806`.

## Delivered declarations and accessibility

| Member | Line | Declaration |
|---|---|---|
| `TrashRowText` | `:784` | `internal const string TrashRowText = "Trash to Delete";` |
| `WithTrashRow` | `:787` | `internal static string[] WithTrashRow(string[] rows)` |
| `ApplyDeleteGesture` | `:803` | `internal void ApplyDeleteGesture()` |
| `MatchesForSearchText` | `:861` | `internal static string[] MatchesForSearchText(System.Func<string, string[]> findMatches, string searchText)` |
| `BindSourceFolderRows` | `:967` | `private void BindSourceFolderRows(string[] rows)` |
| `IsBannerRow` | `:1143` | `internal static bool IsBannerRow(string row)` |
| `IsSelectableFolder` | `:1151` | `internal static bool IsSelectableFolder(string selectedFolder)` |

### `IsBannerRow`, delivered body, verbatim

```csharp
        internal static bool IsBannerRow(string row) =>
            row is not null
            && row.StartsWith(
                UtilitiesCS.OutlookObjects.Folder.BreadcrumbRowBuilder.BannerPrefix,
                StringComparison.Ordinal
            );
```

`StartsWith` under `StringComparison.Ordinal`, never `Substring`, so a row shorter than the prefix
cannot throw. The prefix is `BreadcrumbRowBuilder.BannerPrefix` itself, so it agrees with the producers
by construction rather than by a duplicated literal.

### `ActionOkAsync`'s delivered guard expression, verbatim

```csharp
            // Classifies through the single owner and retains #614's rooted-path rejection.
            if (
                selectedFolder is null
                || IsBannerRow(selectedFolder)
                || !EfcSelectionGuard.IsValidFilingSelection(selectedFolder)
            )
```

Both classification sites now route through `IsBannerRow`: `IsValidSelection` is
`IsSelectableFolder(SelectedFolder)`, and `IsSelectableFolder` is
`!IsBannerRow(selectedFolder) && EfcSelectionGuard.IsValidCreationSelection(selectedFolder)`. The guard
retains `EfcSelectionGuard.IsValidFilingSelection`, so #614's rooted-path rejection
(`ArchiveStemContract.IsFullOutlookPath`) survives intact.

## Read-only files confirmed untouched

| Command | Output lines |
|---|---|
| `git diff --name-only <BASELINE_SHA> -- UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs` | **0** |
| `git diff --name-only <BASELINE_SHA> -- QuickFiler/Controllers/EfcSelectionGuard.cs` | **0** |

`EfcSelectionGuard.cs` is listed because the base-drift addendum requires RC7 to be delivered
**additively, without editing it**. It is unmodified.

## Recorded residual — reported, not fixed here

`EfcSelectionGuard.BannerPrefix` is `"==="` (three `=`), while both row producers
(`BreadcrumbRowBuilder.cs:19` and `FolderSuggestionTree.cs:16`) use `"===="` (four `=`). That is a third
arity variant, and the code comment at `EfcFormController.cs:325` still describes a `"===="` rejection
that `EfcSelectionGuard` does not implement. Both are outside this feature's owned set and are reported
rather than absorbed, per the base-drift addendum. `spec.md` criterion 979 asserts only that
**`IsBannerRow`'s** prefix agrees with `BreadcrumbRowBuilder.BannerPrefix`, which is honestly checkable
and true; no claim is made that every banner-classification site in the repository shares one arity.

Output Summary: PASS. RC8 is confirmed by the `_formViewer`-to-`Task.Run(` offset flipping from +2 at
`BASELINE_SHA` (`:799` against `:797`) to −2 delivered (`:879` against `:881`), with the token occurring
exactly once in the method both before and after. RC9 is confirmed by `BindFolderRows` reaching zero
assignments and zero reads of `_folderRows` and passing its own parameter to `BindBreadcrumbRowsAsync`,
with the field assigned in exactly two method bodies plus its retained declaration initializer, and
`BindSourceFolderRows` called from exactly three source sites. RC7 is confirmed by `IsBannerRow` using
`StartsWith` over `BreadcrumbRowBuilder.BannerPrefix` and by both classification sites routing through
it, with `BreadcrumbRowBuilder.cs` and `EfcSelectionGuard.cs` both showing a zero-line diff.
