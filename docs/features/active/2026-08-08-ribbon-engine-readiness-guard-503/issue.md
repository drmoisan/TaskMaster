# ribbon-engine-readiness-guard (Issue #503)

- Date captured: 2026-08-08
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/2026-08-08-ribbon-engine-readiness-guard-503/ (Issue #503)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #503
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/503
- Last Updated: 2026-08-08
- Work Mode: full-bug

## Summary

SpamBayes-dependent ribbon commands are invokable before `AppItemEngines.InitAsync()` has populated `Globals.Engines.InboxEngines`. `RibbonController.SB` returns `null` during that window, so `RibbonViewer.TrainSpam_Click` dereferences null and throws a `NullReferenceException`. The same initialization race affects every ribbon command backed by an engine in `InboxEngines` (Triage, Project, Context, Actionable).

## Environment

- OS/version: Windows 11, Outlook desktop (VSTO add-in host)
- Runtime: .NET Framework 4.8, TaskMaster VSTO add-in
- Command/flags used: Outlook Explorer ribbon, "Train Spam" button clicked immediately after add-in reload
- Data source or fixture: Live Outlook profile; `Globals.AF.Manager.Configuration` still resolving

## Steps to Reproduce

1. Reload the TaskMaster add-in (or restart Outlook) so `AppItemEngines.InitAsync()` begins.
2. Before `InitAsync()` completes, click the "Train Spam" ribbon button in the Explorer ribbon.
3. Observe the failure in `RibbonViewer.TrainSpam_Click`.

## Expected Behavior

Engine-dependent ribbon commands are not invokable until their backing engine in `InboxEngines` is available. Clicking a not-yet-ready command produces no exception; once `InitAsync()` completes, the commands become enabled and behave exactly as they do today.

## Actual Behavior

`RibbonController.SB` evaluates `Globals?.Engines?.InboxEngines?.TryGetValue("Spam", out var engine)` against an empty `ConcurrentDictionary` and returns `null`. `RibbonViewer.TrainSpam_Click` then executes `await Controller.SB.TrainAsync(Controller.OlSelection, true)`, which throws `NullReferenceException`. Because the handler is `async void`, the exception surfaces on the message-pump synchronization context rather than at the call site.

## Logs / Screenshots

- [x] Attached minimal logs or snippet
- Snippet:

```text
System.NullReferenceException: Object reference not set to an instance of an object.
   at TaskMaster.RibbonViewer.TrainSpam_Click(IRibbonControl control)
```

Relevant source:

- `TaskMaster/Ribbon/RibbonViewer.cs` (`TrainSpam_Click`, ~line 255-256)
- `TaskMaster/Ribbon/RibbonController.Intelligence.cs` (`SB` property, ~line 190-202)
- `TaskMaster/AppGlobals/AppItemEngines.cs` (`InitAsync`, `InboxEngines`)

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

An unhandled `NullReferenceException` on a routine ribbon click immediately after add-in reload. The window is short but reliably reachable, and the failure mode is a silent/unhandled async-void throw rather than a recoverable message.

## Suspected Cause / Notes

`InboxEngines` is initialized to an empty `ConcurrentDictionary` at field-initializer time and is only populated at the end of `InitAsync()`, which first awaits `Globals.AF.Manager.Configuration` and then asynchronously constructs each engine (`SpamBayes.CreateEngineAsync`, `Triage.CreateEngineAsync`, `CategoryClassifierGroup.CreateEngineAsync`, `ActionableClassifierGroup.CreateEngineAsync`). There is no published readiness signal on `AppItemEngines`/`IAppItemEngines`, and the Explorer ribbon XML declares no `getEnabled` callback for the engine-backed buttons, so the ribbon has no way to reflect initialization progress.

`RibbonViewer.TestSpam_Click` has the same defect in a more direct form: it indexes `Controller.Engines.InboxEngines[SpamBayes.GroupName]` and will throw `KeyNotFoundException` during the same window.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: readiness signal on `AppItemEngines`/`IAppItemEngines`; `RibbonController` engine-readiness predicate; `RibbonViewer` click-handler guards; `RibbonExplorer.xml` `getEnabled` wiring validated by the existing ribbon-XML regression suite.
- [x] Integration scenario to retest: click each engine-backed ribbon command immediately after add-in reload; confirm no exception and confirm normal behavior after `InitAsync()` completes.
- [x] Manual verification notes: verify `Ribbon.InvalidateControl(...)`/`Invalidate()` refreshes the enabled state once initialization finishes.

Constraints to honor:

- Do not change the async engine construction logic, config loading, or dictionary population order inside `AppItemEngines.InitAsync()`.
- Preserve existing `SB`/`TrainAsync` behavior once engines are loaded.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [x] Move to active fix folder / branch
