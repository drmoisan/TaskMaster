# ribbon-controller-engines-null-unsafe (Issue #507)

- Date captured: 2026-08-08
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/ribbon-controller-engines-null-unsafe/ (Issue #507)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #507
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/507
- Last Updated: 2026-08-08
## Summary

`RibbonController.Engines` is declared `internal IAppItemEngines Engines => Globals.Engines;` with no null guard on `Globals`, unlike its sibling properties `SB` and `Triage` in the same file which both use `Globals?.`. Any ribbon callback that reaches `Engines` before `SetGlobals` has run throws `NullReferenceException`.

## Environment

- OS/version: Windows 11, Outlook desktop (VSTO add-in host)
- Runtime: .NET Framework 4.8.1, TaskMaster VSTO add-in
- Command/flags used: Outlook Explorer ribbon, Spam Manager and Triage configuration menus
- Data source or fixture: Live Outlook profile during add-in startup

## Steps to Reproduce

1. Reload the TaskMaster add-in so the ribbon is constructed before the controller's `Globals` is assigned.
2. Invoke any callback that routes through `RibbonController.Engines` — `TestSpam_Click`, `SpamBayesEnabled_Click`, `SpamBayesEnabled_GetPressed`, `SpamSaveNetwork_Click`, `SpamSaveLocal_Click`, `GetSaveLocation_Click`, `TriageEnabled_Click`, `TriageEnabled_GetPressed`, `TriageSaveNetwork_Click`, `TriageSaveLocal_Click`, or `TriageGetSaveLocation_Click`.
3. Observe the `NullReferenceException`.

## Expected Behavior

`Engines` behaves like its siblings and returns `null` rather than throwing when `Globals` has not yet been assigned, so callers can guard. The sibling precedent is already in the same file:

```csharp
return Globals?.Engines?.InboxEngines?.TryGetValue("Spam", out var engine) ?? false
    ? engine as SpamBayes
    : null;
```

## Actual Behavior

```csharp
internal IAppItemEngines Engines => Globals.Engines;
```

(`TaskMaster/Ribbon/RibbonController.Intelligence.cs`, Spam Manager region.)

`Globals` is unguarded, so the property throws instead of returning `null`.

## Logs / Screenshots

- [x] Attached minimal logs or snippet
- Snippet:

```text
System.NullReferenceException: Object reference not set to an instance of an object.
   at TaskMaster.RibbonController.get_Engines()
```

## Impact / Severity

- [ ] Blocker
- [ ] High
- [ ] Medium
- [x] Low

The reachable window is narrower than the one in issue #503 (it requires the callback to run before `SetGlobals` rather than merely before `InitAsync`), and the affected callbacks are in configuration submenus rather than primary commands. It is nevertheless a real inconsistency with the sibling properties and an avoidable throw.

## Suspected Cause / Notes

Inconsistent null-guard application within a single file. `SB` (Spam Manager region) and `Triage` (Triage region) both null-propagate through `Globals?.`; `Engines`, declared between them, does not.

This is related to but distinct from issue #503. #503 addresses the window where `Globals` is assigned but `InboxEngines` is not yet populated. This defect addresses the earlier window where `Globals` itself is null. The readiness gate introduced by #503 accesses engines through a `Func<IAppItemEngines>` accessor built as `() => Globals?.Engines`, which is null-safe by construction, so #503 does not depend on this fix and does not remediate it.

Found while researching issue #503; out of scope for that fix.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: assert `Engines` returns `null` rather than throwing when `Globals` is unassigned, using the existing `FormatterServices.GetUninitializedObject` controller-construction pattern in `TaskMaster.Test/Ribbon/RibbonControllerTests.cs`. Note that `RibbonController` is `[ExcludeFromCodeCoverage]`, so a genuine fix should consider whether the accessor belongs in a host-neutral testable type rather than only adding `?.`.
- [x] Integration scenario to retest: exercise the Spam and Triage configuration submenus immediately after add-in reload.
- [x] Manual verification notes: confirm no behavior change once `Globals` is assigned.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
