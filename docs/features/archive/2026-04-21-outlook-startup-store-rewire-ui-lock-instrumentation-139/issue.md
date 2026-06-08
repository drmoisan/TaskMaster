# outlook-startup-store-rewire-ui-lock-instrumentation (Issue #139)

- Date captured: 2026-04-21
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/outlook-startup-store-rewire-ui-lock-instrumentation/ (Issue #139)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #139
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/139
- Last Updated: 2026-04-21
- Work Mode: minor-audit

## Summary

Outlook UI can remain unresponsive for several seconds during add-in startup while `StoresWrapper.RewireOlObjectsAsync()` rewires stores on the main STA thread. The code path is correct for COM affinity, but we currently lack per-store and per-call timing data to identify the exact Outlook store or COM boundary causing the stall.

## Environment

- OS/version: Windows 10/11 with Outlook desktop
- Runtime: .NET Framework VSTO Outlook add-in
- Startup path: `ThisAddIn.Application_Startup()` -> `IdleAsyncQueue.AddEntry(true, ...)` -> `_globals.LoadAsync(false)`
- Data source or fixture: Live Outlook profile with multiple configured stores/providers

## Steps to Reproduce

1. Start Outlook with the TaskMaster add-in enabled and a profile containing multiple stores.
2. Let startup reach `AppOlObjects.LoadStoresAsync()` and the deserialization callback into `StoresWrapper.RewireOlObjectsAsync()`.
3. Observe Outlook UI becoming unresponsive while the store-rewire loop runs on the main STA thread.
4. Attempt to step through the `foreach (var store in stores)` loop in the debugger and note that execution appears not to advance while a COM call is in progress.

## Expected Behavior

Startup logs should identify which specific store and COM call account for the wall-clock delay so the follow-up fix can target the correct bottleneck.

## Actual Behavior

The existing logs show startup pauses clustered around store rehydration, including a prior 11+ second gap between store-related COM logging and `Finished loading globals`, but they do not identify which store iteration or COM boundary is slow. As a result, the code path can be diagnosed only to the method level, not to the individual provider/store call.

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: `2026-03-14 14:17:21.777` store-related COM logging followed by `Finished loading globals` only at `14:17:33.002`; current research note at `artifacts/research/20260421-outlook-startup-store-rewire-ui-lock-research.md`

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

## Suspected Cause / Notes

Research indicates the UI lock occurs because `[OnDeserialized] RewireOlObjects(...)` runs inline during synchronous JSON deserialization on the Outlook main STA thread. The likely blocking calls are `GetFilteredStores()`, `StoreWrapper.Init()`, `GetSmtpAddressFromStore()`, and `FolderMinimalWrapper.RestoreFromRelativePath()`. Relevant files:

- `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs`
- `UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs`
- `UtilitiesCS/OutlookObjects/Folder/FolderMinimalWrapper.cs`

Prior fix folder `docs/features/active/2026-04-13-outlook-store-com-thread-crash-126/` removed invalid `Task.Run` wrappers so COM access stays on the calling STA thread. This follow-up change is diagnostic instrumentation only.

## Acceptance Criteria

- [x] `StoresWrapper.RewireOlObjectsAsync()` logs total filtered-store timing, total rewire timing, and per-store loop timing with the `[Startup timing]` prefix.
- [x] `StoreWrapper.Init()` and `StoreWrapper.GetSmtpAddressFromStore()` log per-call elapsed milliseconds for the targeted Outlook COM boundaries identified in the research note.
- [x] `StoreWrapper.Restore()` and `FolderMinimalWrapper.RestoreFromRelativePath()` log timing needed to distinguish folder-restoration delays from store-init delays.
- [x] The diagnostic code compiles cleanly, uses the existing `log4net` infrastructure, and does not change the functional startup behavior beyond additional debug logging.

## Proposed Fix / Validation Ideas

- [ ] Add temporary `[Startup timing]` `logger.Debug(...)` timing entries around the per-store loop and targeted COM calls listed in the research note
- [ ] Run Outlook startup on the affected machine and capture filtered timing logs
- [ ] Confirm the exact store name and COM boundary responsible for the stall
- [ ] Remove instrumentation after diagnosis or convert the findings into a targeted fix plan

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch