# folder-settings-store-model-null (Issue #262)

- Date captured: 2026-07-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/folder-settings-store-model-null/ (Issue #262)
- Epic: #260 (store-lockup-resilience)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #262
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/262
- Last Updated: 2026-07-07
- Work Mode: full-bug

## Summary

Opening "TaskMaster -> Settings -> Folder Settings" shows "Store settings are not available yet.
Please try again after startup completes." even though startup completed long ago. The store
settings model (`Globals.Ol.StoresWrapper`) is null for the entire session, so the readiness
guard correctly refuses to open the dialog.

## Environment

- OS/version: Windows, Outlook desktop (VSTO add-in)
- Assembly: UtilitiesCS / TaskMaster
- Command/flags used: Ribbon action -> RibbonController.FolderStoresSettings() -> StoreWrapperController.Launch()
- Data source or fixture: Globals.Ol.StoresWrapper (populated by AppOlObjects.LoadStoresAsync during startup)

## Steps to Reproduce

1. Start Outlook with the TaskMaster add-in and allow startup to fully complete.
2. Click TaskMaster -> Settings -> Folder Settings.
3. Observe the "Store settings are not available yet" message despite startup being finished.

## Expected Behavior

After startup completes, Folder Settings opens with a populated store model. If the persisted
store configuration is missing or invalid, the add-in rebuilds the model from the live Outlook
stores rather than leaving it null, and any genuine failure is surfaced clearly.

## Actual Behavior

`StoreWrapperController.EvaluateLaunchReadiness()` returns `ModelUnavailable`/`StoresUnavailable`
because `Globals.Ol.StoresWrapper` (or its `.Stores`) is null, so `Launch()` shows the
"not available yet" dialog and returns. The message implies a timing/notification problem, but
the model is null permanently for the session.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: distinguishing log lines to look for — `"StoresWrapper config not found."`
  (`AppOlObjects.cs:263`), `"Loader for StoresWrapper is null"` (`IntelligenceConfig.cs`), and
  whether `"Finished loading globals"` (`ThisAddIn.cs`) appears.

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

## Suspected Cause / Notes

Root cause is NOT a missing startup-complete notification. Issue #240 deliberately chose direct
live-state inspection over an event/flag, and the guard reads state correctly on every click.
The defect is upstream in the store load pipeline: `AppOlObjects.LoadStoresAsync`
(`:251-265`) leaves `StoresWrapper` null on two paths — (1) the config-missing branch
(`TryGetValue("StoresWrapper", ...)` false → `logger.Error("StoresWrapper config not found.")`),
and (2) a null deserialize tolerated by `AwaitStoreRewireAsync` (`:246-249`). A third path is an
exception on the `IdleAsyncQueue` load continuation before assignment. `IntelligenceConfig.ReadConfigurationAsync`
(`:140`) can drop the "StoresWrapper" key entirely when its loader deserializes to null.
Files to inspect: `TaskMaster/AppGlobals/AppOlObjects.cs`, `UtilitiesCS/EmailIntelligence/IntelligenceConfig.cs`,
`UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs`, `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs`.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: `LoadStoresAsync` builds a fresh `StoresWrapper` from live stores when
      config is missing or deserializes to null; failure is surfaced (not a silent `logger.Error`).
- [ ] Integration scenario to retest: open Folder Settings after startup with (a) missing config,
      (b) null-deserialized config, (c) valid config — dialog opens populated in all recoverable cases.
- [ ] Manual verification notes: confirm no "not available yet" after startup; confirm the model
      is populated and Folder Settings opens.

## Acceptance Criteria

- [ ] AC1: When the persisted `StoresWrapper` config is missing, `LoadStoresAsync` builds a fresh
      model from the live Outlook stores (via the existing `Init`/filter path) instead of leaving
      `StoresWrapper` null.
- [ ] AC2: When the persisted config deserializes to null, the same fresh-build fallback applies
      rather than being silently tolerated.
- [ ] AC3: A genuine, unrecoverable load failure is surfaced (logged at an actionable level and/or
      a clear user-facing message), not swallowed as a bare `logger.Error`.
- [ ] AC4: After startup completes on a recoverable path, `StoreWrapperController.Launch()` opens
      the dialog with a populated model and no longer shows "not available yet".
- [ ] AC5: A deterministic MSTest regression test reproduces the null-model paths (fails before,
      passes after) using Moq for `IApplicationGlobals`/`IOlObjects`/config; no live Outlook, no temp files.
- [ ] AC6: Full C# toolchain passes in order (csharpier -> analyzers -> nullable/TreatWarningsAsErrors
      -> MSTest with coverage); new/changed lines meet coverage targets.

## Next Step

- [ ] Promote to GitHub issue (bug-report template) via MCP tooling and link to epic #260
- [ ] Move to active fix folder / branch
