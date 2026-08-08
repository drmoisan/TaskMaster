# ribbon-async-getpressed-signature (Issue #505)

- Date captured: 2026-08-08
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/ribbon-async-getpressed-signature/ (Issue #505)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #505
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/505
- Last Updated: 2026-08-08
## Summary

`RibbonViewer.SpamBayesEnabled_GetPressed` and `RibbonViewer.TriageEnabled_GetPressed` are declared `async Task<bool>`, but the Office ribbon `getPressed` callback contract requires a synchronous `bool GetPressed(Office.IRibbonControl control)`. Office cannot bind a callback whose return type is `Task<bool>`, so the "SpamBayes Enabled" and "Triage Enabled" toggle buttons never reflect the real engine activation state.

## Environment

- OS/version: Windows 11, Outlook desktop (VSTO add-in host)
- Runtime: .NET Framework 4.8.1, TaskMaster VSTO add-in
- Command/flags used: Outlook Explorer ribbon, Spam Manager and Triage configuration menus
- Data source or fixture: `TaskMaster/Ribbon/RibbonExplorer.xml` embedded resource

## Steps to Reproduce

1. Open Outlook with the TaskMaster add-in loaded and let initialization complete.
2. Open the Spam Manager save-options menu and observe the "SpamBayes Enabled" toggle button.
3. Toggle the engine off via `SpamBayesEnabled_Click`, then reopen the menu.
4. Repeat for the "Triage Enabled" toggle button.

## Expected Behavior

The toggle button's pressed state reflects `Globals.Engines.EngineActiveAsync(<engineName>)` — pressed when the classifier is activated, unpressed when it is not — and updates when the engine is toggled.

## Actual Behavior

Office resolves `getPressed` by name and requires the exact signature `bool GetPressed(IRibbonControl control)`. Both methods return `Task<bool>`:

```csharp
public async Task<bool> SpamBayesEnabled_GetPressed(Office.IRibbonControl control) =>
    await Controller.Engines.EngineActiveAsync(SpamBayes.GroupName);

public async Task<bool> TriageEnabled_GetPressed(Office.IRibbonControl control) =>
    await Controller.Engines.EngineActiveAsync("Triage");
```

(`TaskMaster/Ribbon/RibbonViewer.cs`, Spam Config and Triage Config regions.)

The callback does not bind, so the pressed state is not driven by the engine configuration.

## Logs / Screenshots

- [x] Attached minimal logs or snippet
- Snippet: see the source excerpt above. Office does not surface a user-visible error for a signature-incompatible callback; the failure is silent unless Office is run with the "Show add-in user interface errors" developer option enabled.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Two configuration toggles display a state that is not tied to the underlying setting. The commands themselves still work, so this is a state-display defect rather than a functional break, but it misrepresents engine configuration to the user.

## Suspected Cause / Notes

The underlying accessor `IAppItemEngines.EngineActiveAsync(string)` is asynchronous because it awaits `Globals.AF.Manager.Configuration`. The ribbon callback contract is synchronous and cannot await. Blocking on the task inside the callback is not an acceptable fix: the callback runs on the Outlook UI/STA thread and a synchronous wait on a continuation that needs that thread would deadlock.

The likely correct shape is a cached, synchronously-readable activation snapshot that the async configuration path populates, with `IRibbonUI.InvalidateControl(<controlId>)` called when it changes. That is the same refresh mechanism being introduced for issue #503, so this work should be sequenced after #503 lands and should reuse its readiness/refresh infrastructure rather than inventing a parallel one.

Found while researching issue #503 (ribbon engine readiness guard); out of scope for that fix.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: a reflection-based signature test asserting that every `getPressed`/`getEnabled` name declared in `RibbonExplorer.xml` resolves to a method returning `bool` and taking a single `IRibbonControl` parameter; unit tests for the synchronous activation-snapshot accessor.
- [x] Integration scenario to retest: toggle each engine in live Outlook and confirm the toggle button pressed state tracks the setting across menu reopen and add-in reload.
- [x] Manual verification notes: confirm no UI-thread deadlock under a cold configuration load.

Sequencing note: depends on issue #503 for the ribbon invalidation infrastructure.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
