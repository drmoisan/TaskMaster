# Targeted Diagnostic Verification

Timestamp: 2026-04-21T20:07:56-04:00
Source Artifact: `docs/features/active/2026-04-21-outlook-startup-store-rewire-ui-lock-instrumentation-139/evidence/qa-gates/csharp-mstest-coverage.2026-04-21T20-06-02-04-00.md`
Changed Files:
- `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs`
- `UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs`
- `UtilitiesCS/OutlookObjects/Folder/FolderMinimalWrapper.cs`
- Test files changed: none

## Acceptance Criteria Coverage
- `StoresWrapper.RewireOlObjectsAsync()` logs total filtered-store timing, total rewire timing, and per-store loop timing with the `[Startup timing]` prefix.
  - `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs:70` logs filtered-store timing.
  - `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs:92` logs per-store loop timing including whether the iteration used `Init` or `Restore`.
  - `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs:97` logs total rewire timing.
- `StoreWrapper.Init()` and `StoreWrapper.GetSmtpAddressFromStore()` log per-call elapsed milliseconds for the targeted Outlook COM boundaries identified in the research note.
  - `UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs:32`, `:38`, `:49`, and `:56` log `Init()` timing for `DisplayName`, `GetRootFolder`, `GetDefaultFolder(Inbox)`, and the aggregate SMTP lookup call.
  - `UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs:138`, `:144`, `:150`, and `:156` log `GetSmtpAddressFromStore()` timing for `CurrentUser`, `AddressEntry`, `GetExchangeUser`, and `PrimarySmtpAddress`.
- `StoreWrapper.Restore()` and `FolderMinimalWrapper.RestoreFromRelativePath()` log timing needed to distinguish folder-restoration delays from store-init delays.
  - `UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs:86`, `:92`, and `:98` log `Restore()` timing for `ArchiveRoot`, `JunkPotential`, and `JunkCertain` folder restoration paths.
  - `UtilitiesCS/OutlookObjects/Folder/FolderMinimalWrapper.cs:136` and `:172` bracket `RestoreFromRelativePath()` with start/completion timing so folder restoration delay can be distinguished from store initialization delay.
- `The diagnostic code compiles cleanly, uses the existing log4net infrastructure, and does not change the functional startup behavior beyond additional debug logging.`
  - `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs`, `StoreWrapper.cs`, and `FolderMinimalWrapper.cs` use the existing class-level `log4net` logger instances already present in those files.
  - Final QA gates passed: formatter clean pass, analyzer build `0 Warning(s)` / `0 Error(s)`, nullable build `0 Warning(s)` / `0 Error(s)`, and MSTest coverage run `3945` total tests with `3943` passed and `0` failed.
