Timestamp: 2026-07-20T13-10

## Candidate defect-surface baseline notes (capture only; diagnosis conclusion is P1-T1's job)

Verbatim citations from the plan's Confirmed Facts section:

- `QuickFiler/Controllers/QfcItemController.FolderHandling.cs:161-206` — `AssignFolderComboBox()`:
  when `_folderHandler.FolderArray.Length > 0` (line 170) and no predetermined-folder match is
  present/selected, the `else` branch (lines 200-203) unconditionally calls
  `_itemViewer.SetFolderSelectedIndex(1)` (line 202) regardless of `FolderArray.Length`.

- `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs:127` — `SelectRow(int index)` forwards the
  index unchanged into `BreadcrumbStateModel.SelectRow`.

- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs:233-246` — `SelectRow(int index)`
  validates `index` is `-1` or in `[0, RowCount-1]` and throws `ArgumentOutOfRangeException` for a
  single-row model (`RowCount == 1`) when `index == 1`. This validation is correct defensive
  behavior and must NOT be changed by this plan.

## Source verification performed in this session

- `QfcItemController.FolderHandling.cs` line 202: confirmed literal `_itemViewer.SetFolderSelectedIndex(1);` inside the `else` branch (lines 200-203) of `AssignFolderComboBox()`.
- `QfcItemController.FolderHandling.cs` line 228: confirmed literal `comboBox.SelectedIndex = predeterminedIndex >= 0 ? predeterminedIndex : 1;` inside the static `PopulateAndSelectFolder(...)` helper (lines 217-230).
- `BreadcrumbBridgeCoordinator.cs` line 127 (file lines 118-129 region, `SelectRow` method at line 125): confirmed `SelectRow(int index)` forwards `index` unchanged into `_router.SelectRow(index)` (which routes to `BreadcrumbStateModel.SelectRow`).
- `BreadcrumbStateModel.cs` lines 233-246: confirmed `SelectRow(int index)` throws `ArgumentOutOfRangeException` when `index < -1 || index >= _rows.Count`, with message `"Row selection requires -1 or an index in [0, {_rows.Count - 1}]."` — this matches the issue.md stack trace exactly (`Row selection requires -1 or an index in [0, 0].`) and is confirmed correct defensive behavior, out of scope-lock for modification.

No conclusion is drawn here beyond the plan's pre-recorded Confirmed Facts; the diagnosis conclusion is deferred to P1-T1.
