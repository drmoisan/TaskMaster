# Bug: ribbon-engines-callers-unguarded-null-deref (Issue #518)

- Work Mode: minor-audit

- Issue: #518
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/518
- Last Updated: 2026-08-08
- Status: Promoted -> docs/features/active/Bug_ribbon-engines-callers-unguarded-null-deref/ (Issue #518)
## Summary

All 11 production call sites of `RibbonController.Engines` dereference the result with no null
guard. Issue #507 changed `Engines` from `Globals.Engines` to `Globals?.Engines` so the property
returns `null` instead of throwing when `Globals` is unassigned, matching the sibling `SB`
precedent. That fix is correct and is the behavior #507 specified, but on its own it relocates the
`NullReferenceException` rather than eliminating it: the same ribbon click now throws one frame
later, at the call site, instead of inside `get_Engines()`.

Discovered during the feature review of #507 (`bug/ribbon-controller-engines-null-unsafe-507`).

## Environment

- OS/version: Windows 11, Outlook desktop (VSTO add-in host)
- Runtime: .NET Framework 4.8.1, TaskMaster VSTO add-in
- Data source or fixture: Live Outlook profile during add-in startup

## Affected Call Sites

All in `TaskMaster/Ribbon/RibbonViewer.cs`:

| Line | Callback | Expression |
|---|---|---|
| 263 | `TestSpam_Click` | `(SpamBayes)Controller.Engines.InboxEngines[SpamBayes.GroupName].Engine` |
| 277 | `SpamBayesEnabled_Click` | `Controller.Engines.ToggleEngineAsync(SpamBayes.GroupName)` |
| 280 | `SpamBayesEnabled_GetPressed` | `Controller.Engines.EngineActiveAsync(SpamBayes.GroupName)` |
| 283 | `SpamSaveNetwork_Click` | `Controller.Engines.ShowDiskDialog(SpamBayes.GroupName, false)` |
| 286 | `SpamSaveLocal_Click` | `Controller.Engines.ShowDiskDialog(SpamBayes.GroupName, true)` |
| 289 | `GetSaveLocation_Click` | `Controller.Engines.ShowSaveInfo(SpamBayes.GroupName)` |
| 331 | `TriageEnabled_Click` | `Controller.Engines.ToggleEngineAsync("Triage")` |
| 334 | `TriageEnabled_GetPressed` | `Controller.Engines.EngineActiveAsync("Triage")` |
| 337 | `TriageSaveNetwork_Click` | `Controller.Engines.ShowDiskDialog("Triage", false)` |
| 340 | `TriageSaveLocal_Click` | `Controller.Engines.ShowDiskDialog("Triage", true)` |
| 343 | `TriageGetSaveLocation_Click` | `Controller.Engines.ShowSaveInfo("Triage")` |

## Steps to Reproduce

1. Reload the TaskMaster add-in so the ribbon is constructed before the controller's `Globals` is
   assigned.
2. Invoke any of the callbacks listed above.
3. Observe a `NullReferenceException` raised at the call site rather than inside `get_Engines()`.

## Expected Behavior

Each callback guards the `Engines` result and degrades gracefully when the engines are not yet
available, rather than dereferencing `null`.

## Actual Behavior

Every call site dereferences `Controller.Engines` immediately with no guard.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [ ] Medium
- [x] Low

Same narrow reachable window as #507: the callback must run before `SetGlobals`. The affected
callbacks are configuration submenu items rather than primary commands.

## Dependencies / Sequencing

**This must land after `bug/ribbon-engine-readiness-guard-503` merges.** That branch is
concurrently relocating the entire `#region Spam Manager` and `#region Triage` blocks — which
contain all 11 call sites — out of `RibbonViewer.cs` into a partial class. Attempting this fix
before #503 merges would conflict directly with that restructuring.

Related and adjacent, also deferred to the same follow-up feature: issues #505
(`ribbon-async-getpressed-signature`) and #506 (`ribbon-toggle-engine-fire-and-forget`), which
affect `SpamBayesEnabled_Click`/`_GetPressed` and `TriageEnabled_Click`/`_GetPressed` in the same
file. Consider addressing #505, #506, and this finding together as one caller-hardening change.

The sibling `SB` property already exhibits the identical unguarded-caller pattern, so this is a
pre-existing codebase convention rather than a defect introduced by #507. Any fix should consider
whether the guard belongs at each call site or in a shared readiness check.

## Source

Discovered by feature review of issue #507. See
`docs/features/active/2026-08-08-ribbon-controller-engines-null-unsafe-507/code-review.2026-08-08T17-45.md`.
