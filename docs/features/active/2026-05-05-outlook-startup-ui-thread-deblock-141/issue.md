# outlook-startup-ui-thread-deblock (Issue #141)

- Date captured: 2026-05-05
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/outlook-startup-ui-thread-deblock/ (Issue #141)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #141
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/141
- Last Updated: 2026-05-05
- Work Mode: full-bug

## Summary

Outlook add-in startup can block the main STA/UI thread for several seconds while startup coordination, store rewire, and other initialization work run without yielding often enough. The likely fix direction is to keep Outlook COM access on the UI thread while moving computation, configuration loading, and disk-backed initialization onto background threads so Outlook remains responsive during startup.

## Environment

- OS/version: Windows 10/11 with Outlook desktop and the TaskMaster VSTO add-in enabled
- Python version: Not applicable; this path is a .NET Framework Outlook add-in startup path
- Command/flags used: Standard Outlook launch into `ThisAddIn.Application_Startup()` with no special flags
- Data source or fixture: Live Outlook profile with multiple configured stores/providers and the normal TaskMaster persisted startup data/config files

## Steps to Reproduce

1. Start Outlook with the TaskMaster add-in enabled on a profile that has multiple Outlook stores.
2. Let startup reach `ThisAddIn.Application_Startup()`, which queues `_globals.LoadAsync(false)` through `IdleAsyncQueue.AddEntry(true, ...)`.
3. During startup, try to interact with Outlook while store rewire and related initialization phases are running.
4. Observe whether the Outlook window stops repainting or ignores input until the current startup phase finishes.

## Expected Behavior

Outlook should remain responsive during add-in startup even if total initialization takes longer. Background-safe work such as configuration loading, deserialization, and disk I/O should run off the UI thread, while Outlook COM access remains on the main STA thread and yields between heavy phases.

## Actual Behavior

Outlook can remain unresponsive for several seconds during startup while the add-in performs synchronous UI-thread coordination and COM-bound store rewire work. There is typically no explicit error dialog; the observable failure is a startup UI freeze, with prior timing evidence showing an 11+ second gap around store-related startup work before `Finished loading globals` is logged.

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: Prior startup timing evidence and the current analysis are captured in `docs/features/active/2026-04-21-outlook-startup-store-rewire-ui-lock-instrumentation-139/issue.md` and `artifacts/research/20260504-outlook-startup-ui-thread-deblock-research.md`, including the previously observed gap between store-related COM logging and `Finished loading globals`.

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

## Suspected Cause / Notes

`Application_Startup()` currently routes `_globals.LoadAsync(false)` through `IdleAsyncQueue.AddEntry(true, ...)`, so the startup coordinator runs on the UI thread. Some sub-steps already use `Task.Run`, but their continuations resume on the dispatcher, and `_olObjects.LoadAsync()` / `StoresWrapper.RewireOlObjectsAsync()` still perform required Outlook COM work on the STA thread without enough cooperative yielding. Prior fixes for issues `#124`, `#126`, `#128`, and `#139` indicate the fix must preserve UI-thread COM access while splitting background-safe computation and disk I/O away from the UI-bound phases. Additional research notes two follow-up hazards to inspect before implementation: `AppToDoObjects.LoadIdListAsync()` and `LoadProjInfoAsync()` both pass the Outlook `Application` object into background work and may hide latent cross-thread COM access.

## Proposed Fix / Validation Ideas

- [x] Validate a phased startup design where only COM-bound segments stay on the UI thread and background-safe config/file work is explicitly offloaded
- [ ] Retest Outlook startup with a multi-store profile and confirm the UI continues repainting and accepting input between startup phases
- [ ] Re-verify prior COM-safety regressions by confirming store access, event hookup, and mail-item materialization still occur on the STA/UI thread
- [ ] Capture before/after startup timing around `_globals.LoadAsync(false)`, `_olObjects.LoadAsync()`, and per-store rewire work to confirm responsiveness improves even if total startup duration increases

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch