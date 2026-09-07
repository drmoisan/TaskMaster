# folder-settings-never-persist-and-user-email-error-loading (Issue #797)

- Date captured: 2026-09-06
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/folder-settings-never-persist-and-user-email-error-loading/ (Issue #797)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #797
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/797
- Last Updated: 2026-09-07
- Work Mode: full-bug

## Summary

Values chosen in Settings -> Folder Settings (Archive Root Outlook, Archive Root File System, Junk Potential, Junk Email) survive only for the current Outlook session and are lost on restart, because the settings file `StoresWrapper.json` has never been created and the save path silently does nothing when no file path is configured. In the same dialog, User Email renders "Error Loading" because the Exchange SMTP lookup throws a COM exception that is caught, logged, and rendered as a generic placeholder with no fallback and no retry.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Runtime: .NET Framework 4.8 VSTO Outlook add-in, debug build loaded from `TaskMaster\bin\Debug`, HEAD `c431dc32` (2026-09-06)
- Command/flags used: Outlook ribbon -> Settings -> Folder Settings (`RibbonController.FolderStoresSettings`, `StoreWrapperController.Launch`)
- Data source or fixture: live Exchange mailbox `dmoisan@realgoodfoods.com`; one included store, one Google Workspace store excluded by the GWSO filter

## Steps to Reproduce

1. Confirm `%LocalAppData%\TaskMaster\StoresWrapper.json` does not exist (it has never existed on this machine; every other TaskMaster JSON file is present in that folder).
2. Start Outlook, open Settings -> Folder Settings. Observe: Archive Root Outlook and File System show "Please select an archive"; Junk Potential and Junk Email show "Please select a folder"; User Email shows "Error Loading"; Inbox shows `\\dmoisan@realgoodfoods.com\Inbox`; Root Folder shows `\\dmoisan@realgoodfoods.com`.
3. Select a value for Archive Root Outlook and click Save. Reopen the dialog in the same session: the value is retained.
4. Close Outlook, reopen it, open Folder Settings again: the value is gone and the placeholder is back.
5. Confirm `StoresWrapper.json` still does not exist.

## Expected Behavior

- A saved Folder Settings value is written to `%LocalAppData%\TaskMaster\StoresWrapper.json` and restored on the next Outlook start.
- A save that cannot be written is reported as an error in the log, never silently dropped.
- User Email shows the mailbox SMTP address. When the Exchange user lookup fails, the dialog shows a specific unavailability message with the reason, falls back to another source for the address, and retries the lookup when the dialog is opened rather than only at startup.
- Cosmetic: Inbox and Root Folder display without the leading `\\` store prefix.

## Actual Behavior

- The file is never created; every Outlook start rebuilds an empty stores wrapper and the placeholders return.
- No error is logged when Save runs with no file path.
- User Email shows "Error Loading" every time.
- Inbox and Root Folder show Outlook's native `\\<store>\<folder>` form.

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet (`TaskMaster\bin\Debug\logs\debug_2026-09-06.log`):

```
2026-09-06 17:29:59,517 [VSTA_Main] WARN  TaskMaster.AppOlObjects - StoresWrapper config deserialized to null; rebuilding from live stores.
2026-09-06 17:29:59,560 [VSTA_Main] DEBUG UtilitiesCS.OutlookObjects.Store.StoresWrapper - [store-filter] displayName=dmoisan@realgoodfoods.com exchangeStoreTypeMs=0.0 filePathMs=0.0 included=true rule=Included
2026-09-06 17:29:59,592 [VSTA_Main] ERROR UtilitiesCS.OutlookObjects.Store.StoreWrapper - Error retrieving PrimarySmtpAddress from secondary inbox. The operation failed.
   at UtilitiesCS.OutlookObjects.Store.StoreWrapper.GetSmtpAddressFromStore() in ...\UtilitiesCS\OutlookObjects\Store\StoreWrapper.cs:line 184
```

- Filesystem evidence (2026-09-06): `%LocalAppData%\TaskMaster` contains `ManagerFolder.json`, `9999999RecentsFile.json`, `UsedIDList.json`, etc. A recursive search of the user profile finds no `StoresWrapper.json` anywhere.

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

The Folder Settings dialog cannot persist any per-store setting on a machine where the file has never been created, which is every fresh install. The archive root and junk folder settings it manages feed filing and junk-mail workflows. The silent no-op on save means the defect produces no diagnostic signal.

## Suspected Cause / Notes

Root cause 1 (verified by code read and by the log line above): a bootstrap gap between the loader and the serializer.

- `TaskMaster\AppGlobals\AppOlObjects.StoreLoading.cs:35-65` (`LoadStoresAsync`) deserializes via `SmartSerializable.Deserialize<StoresWrapper, SmartSerializableLoader>(config)`.
- `UtilitiesCS\ReusableTypeClasses\NewSmartSerializable\SmartSerializableBase.cs:167-188` (`Deserialize<T,U>(loader)`) calls `DeserializeJson<T>(loader.Config.Disk, ...)`, which returns null when the file does not exist (`:335-342`). The loader's disk configuration is copied onto the instance only inside `if (instance is not null)` (`:176-180`), so it is discarded on the null path.
- The loader then calls `BuildFreshStoresWrapper()` (`:64`) = `new StoresWrapper(_globals).Init()`. The fresh instance's `Config.Disk.FilePath` is the `FilePathHelper` default `""` (`UtilitiesCS\HelperClasses\FileSystem\FilePathHelper.cs:72-102`); nothing assigns the resource-defined path (`UtilitiesCS\IntelligenceResources.resx:176-203`, `FileName: StoresWrapper.json`, `SpecialFolderName: AppData`).
- `StoreWrapperController.SaveChanges` (`UtilitiesCS\OutlookObjects\Store\StoreWrapperController.cs:348-357`) calls `Model.Serialize()`. `SmartSerializable<T>.Serialize()` (`UtilitiesCS\ReusableTypeClasses\NewSmartSerializable\SmartSerializable.cs:442-448`) is `if (Config.Disk.FilePath != "") RequestSerialization(...)`, so with an empty path it returns without writing and without logging.
- Because the file is never written, every subsequent start takes the same null path. In-session persistence works only because the values live in the in-memory `StoreWrapper` (`SaveChanges` lines 350-353).
- Contrast: the overload `Deserialize<T,U>(loader, askUserOnError, altLoader)` at `SmartSerializableBase.cs:190-240` copies the loader config onto the instance regardless (`:236`) and writes the instance when `writeInstance` is set. `RecentFolders` uses the `askUserOnError: true` variant (`TaskMaster\AppGlobals\AppAutoFileObjects.cs:217-222`) and its file exists.

Root cause 2 (verified by the log): `StoreWrapper.GetSmtpAddressFromStore` (`UtilitiesCS\OutlookObjects\Store\StoreWrapper.cs:179-217`) threw `COMException` "The operation failed." at line 184 (`RootFolder?.Session?.CurrentUser`), caught and converted to null. `StoreWrapperController.PopulateWithCurrent` (`StoreWrapperController.cs:294-296`) renders null as "Error Loading". The lookup runs once in `StoreWrapper.Init` (`:83`) and is never retried. The Outlook-side cause of the COM failure is not determinable from the log. The same session's `ThreadMonitor` captured the UI thread inside `_ExchangeUser.get_PrimarySmtpAddress()` at 17:35:21, so a second caller of this chain also blocks on it.

Not a defect: the `\\` prefix on Inbox and Root Folder is Outlook's native `MAPIFolder.FolderPath` read directly at `StoreWrapperController.cs:294-295`. Trimming is a cosmetic acceptance criterion only.

Related latent defects in the same files, to be fixed in the same change:

- `SmartSerializable.RequestSerialization` (`SmartSerializable.cs:550-559`) defers the write by a 3-second single-shot timer. Closing Outlook within that window loses the save. A shutdown flush or synchronous write on explicit Save is needed.
- `StoreWrapperController.PersistJunkFolderSelections` (`StoreWrapperController.cs:391-418`) reaches `AppOlObjects.ApplyJunkFolderSelections` by reflection and silently returns with only a warning when the method is not found; the junk folders are therefore persisted twice (per-store JSON and `.NET` user settings at `TaskMaster\AppGlobals\AppOlObjects.JunkFolders.cs:27-34`) by two mechanisms that can diverge.
- `StoreWrapperController.cs:169` can assign a null `Current`; `PopulateWithCurrent` dereferences it unguarded at `:288-291` before the null-safe reads at `:294-296`, so an unmatched store selection throws instead of showing the placeholder.
- `StoreWrapperController.GetRelativeFsPath` (`:456-474`) uses `&` rather than `&&` at `:464`.

## Proposed Fix / Validation Ideas

Acceptance criteria settled with the maintainer on 2026-09-06:

- [ ] AC1: When `StoresWrapper.json` is absent, the fresh-build path adopts the resource-defined disk configuration so `Config.Disk.FilePath` resolves to `%LocalAppData%\TaskMaster\StoresWrapper.json`, and the first Save creates the file.
- [ ] AC2: `SmartSerializable<T>.Serialize()` logs an error (not a silent return) when invoked with an empty or null `Config.Disk.FilePath`.
- [ ] AC3: A value saved in Folder Settings is present after an Outlook restart (manual verification).
- [ ] AC4: An explicit Save is not lost if Outlook closes within the 3-second deferred-write window (flush on save or on shutdown).
- [ ] AC5: The junk-folder double-persistence path is either removed or made to fail loudly; the reflection lookup is replaced by a typed seam.
- [ ] AC6: User Email shows the SMTP address; on lookup failure it shows a specific message including the reason, falls back to an alternative source (the account SMTP address or the store display name when it is an SMTP address), and the lookup is retried when the dialog opens.
- [ ] AC7: Inbox and Root Folder are displayed without the leading `\\` (cosmetic).
- [ ] AC8: A null `Current` store selection renders the placeholder text instead of throwing.

Validation:

- [ ] Unit coverage areas: `SmartSerializableBase.Deserialize<T,U>` null-file path copies loader config onto the fallback instance; `SmartSerializable.Serialize()` with empty path logs an error (Moq on the logger seam or an injectable log sink); `StoreWrapperController.PopulateWithCurrent` null-`Current` path; SMTP fallback ordering; `\\` trim helper.
- [ ] Integration scenario to retest: fresh profile with no `StoresWrapper.json`, save archive root, restart, reopen dialog.
- [ ] Manual verification notes: confirm the file is created on first Save and the log contains no serializer error; confirm User Email populates or shows the specific message.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
