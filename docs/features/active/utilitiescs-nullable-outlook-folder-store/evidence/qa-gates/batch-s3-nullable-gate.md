# Batch S3 Nullable Gate (P10-T3)

Timestamp: 2026-07-19T16-05

- csharpier format EXIT 0; full /t:Build EXIT 0.
- Scoped gate: **zero CS86xx** for the 2 Batch S3 files (AC1).

## Files remediated (2): StoreWrapperController.cs (477 lines), DisabledStoresController.cs
- StoreWrapperController viewer-bound optional fields nullable: `ArchiveOutlook`, `ArchiveFS`, `JunkEmail`,
  `JunkPotential` (FolderMinimalWrapper?/FilePathHelper?). Lifecycle-set fields use the `= null!` idiom
  (`Viewer`, `Model`, `Current`, `FsConverter`) so the WinForms event-handler derefs stay clean (set during
  Launch/PopulateWithCurrent before any handler runs).
- The pre-existing `#pragma warning disable CS8625` / `restore` pair inside `StoreLaunchReadiness.NotReady`
  was re-evaluated after adding the file-level pragma: it is **confirmed still needed** — `NotReady` passes
  `null` for the non-null `model`/`displayNames` sentinel, which is CS8625 under the pragma gate; a clean
  rebuild with the pair removed fails, so it is kept (it is a warning pragma, not a prohibited post-condition
  attribute). `StoreLaunchReadiness.DisplayNames` widened to `IList<string?>` because a store's DisplayName is
  nullable (consumer only assigns it to `ComboBox.DataSource`, an `object`).
- `SelectFolder`/`SelectFsFolder` return nullable; `Current.RootFolder!`, `Model.Stores!`, guarded
  `folderPath!`/`storeId!` forgiven (WinForms lifecycle guarantees). Cross-batch: StoreWrapper's configurable
  `ArchiveRoot`/`ArchiveFsRoot`/`JunkPotential`/`JunkCertain` made nullable (SaveChanges assigns null; Restore
  uses `?.`).
- DisabledStoresController: `Viewer` (IDisabledStoresViewer) nullable-by-design (unset until Launch());
  `Viewer!.BindRows`, `object? sender`. External oblivious FilePathHelperConverter/FilePathHelper consumed at
  call sites only (not edited). No post-condition attributes; no record/init.
