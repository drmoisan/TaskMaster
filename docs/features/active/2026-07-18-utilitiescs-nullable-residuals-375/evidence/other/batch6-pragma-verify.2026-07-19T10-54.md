# Batch 6 Pragma Verification (P7-T6)

Timestamp: 2026-07-19T10-54

Batch 6 opted-in hand-written files (4, FilterOlFolders):
1. FolderInfoViewer.cs — `FolderTree { get; set; } = null!` (set via SetFolderTree); own field
   `_folderTreeView` → `FolderTreeCompatibilityView?` (nulled in FormClosed; `?.Dispose()` preserved).
2. OSBrowser.cs — pragma only; verify-only clean (only Designer-declared controls and oblivious
   BrightIdeasSoftware/MyFileSystemInfo types, no own uninitialized fields).
3. FilterOlFoldersViewer.cs — own field `_controller = null!` (set via SetController; `_controller?.`
   guards in BtnDiscard/BtnSave preserved).
4. FilterOlFoldersController.cs (344 lines) — `_folderTreeView` → `FolderTreeCompatibilityView?`
   (existing `if (_folderTreeView == null)` and `?.Dispose()` guards preserved); `PutCheckedState = null!`
   (never assigned, unused); justified `!` at two post-construction invariant sites (FolderTreeView
   property getter and Save()'s `_folderTreeView!.Roots`, both reached only after the ctor sets the
   field). Designer-declared controls not annotated.

P7-T5: The four `*.Designer.cs` files (FilterOlFoldersViewer, FolderInfoViewer, OSBrowser, OSFolder)
carry NO `#nullable` pragma and are unmodified (`git status` clean) — AC3.

## Trustworthy isolated CS86xx gate

Command: `msbuild UtilitiesCS/UtilitiesCS.csproj -t:Rebuild -p:Configuration=Debug -p:Platform=AnyCPU -p:TreatWarningsAsErrors=true -p:WarningsNotAsErrors=CS0649;CS0618;CS0168 -p:BuildProjectReferences=false`

EXIT_CODE: 0

Output Summary: Build succeeded. 0 errors, 0 CS86xx, 15 pre-existing out-of-scope warnings. First
pass surfaced 2 CS8603/CS8602 (the FolderTreeView getter and Save deref of the now-nullable field),
both resolved with justified `!` at post-construction sites. No Designer half was cross-blocked.

## Deviation note
- FilterOlFoldersController: the plan listed `_folderTreeView` → `?` and `PutCheckedState`. Making
  `_folderTreeView` nullable mechanically required justified `!` at the `FolderTreeView` property
  getter (`=> _folderTreeView!`) and the `Save()` deref (`_folderTreeView!.Roots`); both are reached
  only after the constructor assigns the field, so the `!` is a guaranteed-non-null invariant, not a
  new guard or behavior change.
