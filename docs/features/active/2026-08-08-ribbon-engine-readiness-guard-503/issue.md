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

## Delivered Outcome (2026-08-08)

Status: **Delivered** on branch `bug/ribbon-engine-readiness-guard-503` (merge-base `003c5715055d7d1933db68a742531332756e30b2`). Full delivery notes and deviations are in `spec.md` under `## Delivery Notes and Deviations`.

The initialization race is closed by a per-engine-key readiness signal computed from the existing `IAppItemEngines.InboxEngines` member, implemented in four new host-neutral types under `TaskMaster\Ribbon\` that are deliberately not `[ExcludeFromCodeCoverage]`:

- `EngineCommandCatalog` — the single control-id to engine-key map (8 entries).
- `EngineReadinessGate` — the per-key readiness predicate, recomputed on every query.
- `EngineGatedCommandRunner` — the `getEnabled` decision and the click guard; defers the engine dereference into a `Func<Task>` lambda.
- `EngineCommandRefreshPlanner` — the post-initialization invalidation set.

Wiring: `getEnabled="EngineCommand_GetEnabled"` on the eight engine-backed `<button>` elements in `RibbonExplorer.xml`; one new Office-typed shim `public bool EngineCommand_GetEnabled(Office.IRibbonControl)` on a new `RibbonViewer` partial; one refresh call in `ThisAddIn.cs` immediately after `await _globals.LoadAsync(false)`, marshalled explicitly through `UiThread.Dispatcher`.

Corrections to this issue's original text, established by research and carried into `spec.md`:

- The affected set is exactly **eight** handlers (`Spam` x3, `Triage` x5), not "every engine in `InboxEngines`". No ribbon callback dereferences the `Project`, `Context`, or `Actionable` engines.
- `TestSpam_Click` throws `KeyNotFoundException` (dictionary indexer), not `NullReferenceException`. Both types are covered by the regression tests.
- The readiness signal is **not** added to `AppItemEngines` / `IAppItemEngines`: .NET Framework 4.8.1 has no default interface members, so any new interface member could only be bodied inside the `[ExcludeFromCodeCoverage]` `AppItemEngines` and would be uncoverable. Both files take a **zero-line diff**, which is stronger R4 compliance than the original phrasing required.

Constraints honoured: `AppItemEngines.InitAsync()` untouched; `SB`/`Triage`/`TrainAsync` ready-path expressions byte-identical to before.

Verification: 6338 tests pass (up from 6293), zero failed, zero skipped. All four new types at 100% line coverage. `csharpier check .` exit 0 over 1498 files; analyzer build 0 errors; nullable build 0 errors. Evidence under `evidence/baseline/`, `evidence/regression-testing/`, `evidence/qa-gates/`, `evidence/manual-verification/`, and `evidence/issue-updates/`.

Outstanding: AC19, AC20, AC21 are MANUAL-ONLY and remain unchecked pending maintainer execution of `evidence/manual-verification/ac19-ac21-checklist.2026-08-08T15-00.md` against a live Outlook profile.
