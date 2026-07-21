Timestamp: 2026-07-20T13-50

## Root Cause Diagnosis for Issue #392

(a) `QfcItemController.FolderHandling.cs:202` — `AssignFolderComboBox()`'s `else` branch (lines
200-203, guarded by `_folderHandler?.FolderArray?.Length > 0` at line 170) unconditionally calls
`_itemViewer.SetFolderSelectedIndex(1);` whenever no predetermined-folder match is present or
selected, regardless of how many suggestions `_folderHandler.FolderArray` actually contains. When
`FolderArray.Length == 1`, index 1 is out of range for the single-row breadcrumb model.

(b) `QfcItemController.FolderHandling.cs:228` — the static `PopulateAndSelectFolder(...)` helper
contains the identical unguarded fallback:
`comboBox.SelectedIndex = predeterminedIndex >= 0 ? predeterminedIndex : 1;`. A WinForms
`ComboBox` populated with exactly one item also rejects `SelectedIndex = 1` (this is confirmed by
the existing test `PopulateAndSelectFolder_EmptyArray_ThrowsOnIndexOneSelection`, which demonstrates
the same class of out-of-range throw for the zero-item case).

(c) `BreadcrumbStateModel.SelectRow(int index)` (`UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs:233-246`)
validates that `index` is `-1` or within `[0, _rows.Count - 1]` and throws
`ArgumentOutOfRangeException` otherwise. Verified in this session: the exact exception message
format (`"Row selection requires -1 or an index in [0, {_rows.Count - 1}]."`) matches the
issue.md-reported stack trace verbatim for a single-row model (`RowCount == 1`, thrown message
`"Row selection requires -1 or an index in [0, 0]."`, actual value 1). This is correct defensive
validation protecting the breadcrumb pipeline's invariant and is explicitly NOT modified by this
plan.

(d) **Primary root-cause statement:** Both call sites in `QfcItemController.FolderHandling.cs`
((a) `AssignFolderComboBox()` line 202, and (b) the static `PopulateAndSelectFolder` helper line
228) select a hardcoded fallback index of `1` whenever no predetermined folder is preselected,
without checking how many suggestions are actually present. The fix is to clamp the fallback
selection index to `0` when exactly one suggestion exists, and retain `1` only when two or more
suggestions exist. `BreadcrumbStateModel.SelectRow`'s bounds validation is correct and is not the
defect; the defect is entirely in the two caller sites identified above. This satisfies the
diagnosis prerequisite for AC-1 (regression test authored against this diagnosed defect), AC-2
(`AssignFolderComboBox` fix), and AC-4 (`PopulateAndSelectFolder` fix).
