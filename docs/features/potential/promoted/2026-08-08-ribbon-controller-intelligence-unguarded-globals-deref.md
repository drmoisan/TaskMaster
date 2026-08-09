# ribbon-controller-intelligence-unguarded-globals-deref (Issue #524)

- Date captured: 2026-08-08
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/ribbon-controller-intelligence-unguarded-globals-deref/ (Issue #524)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Work Mode: full-bug

- Issue: #524
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/524
- Last Updated: 2026-08-09
## Summary

`TaskMaster/Ribbon/RibbonController.Intelligence.cs` contains ribbon-callback-reachable code paths that dereference `Globals` without a guard. Before `SetGlobals` has run, `Globals` is unassigned, so each site raises a `NullReferenceException` out of an `async void` Office handler, where it is neither reported nor observable by the user.

This is the same defect class as #518, but at different call sites. #518 was scoped to the ten `Controller.Engines.<member>` sites in `RibbonViewer.EngineCommands.cs`; these sites are in the controller partial and were explicitly held out of that scope (that file is a protected zero-line-diff path in the #505/#506/#518 delivery).

Discovered during the research phase of the bundled #505/#506/#518 delivery (research section 10, item 2).

## Environment

- OS/version: Windows 11, Outlook desktop (VSTO add-in host)
- Runtime: .NET Framework 4.8.1, TaskMaster VSTO add-in
- Command/flags used: Outlook Explorer ribbon, Spam Manager and QuickFiler settings menus
- Data source or fixture: Live Outlook profile during add-in startup

## Steps to Reproduce

1. Reload the TaskMaster add-in so the ribbon is constructed before the controller's `Globals` is assigned.
2. Invoke a ribbon callback that reaches `RibbonController.Intelligence.cs` — for example the Spam Manager "Clear Spam Manager" command, or any of the QuickFiler settings toggles.
3. Observe a `NullReferenceException` raised inside the controller partial.

## Expected Behavior

No ribbon callback raises a `NullReferenceException` when invoked before initialization completes. Each site degrades gracefully, consistent with the seam pattern established by #503 and extended by #505: host-neutral, unit-tested decision logic behind an injected accessor, with the COM-touching glue left in the `[ExcludeFromCodeCoverage]` shim.

## Actual Behavior

Every listed site dereferences `Globals` immediately with no guard. Verified against `origin/main` at `f910ff2f`:

| Line | Member | Expression |
|---|---|---|
| 220 | `ClearSpamManagerAsync` | `Globals.AF...` |
| 230 | `ClearSpamManagerAsync` | `Globals.Engines.RestartEngineAsync(...)` |
| 29-58 | QuickFiler-settings toggle callbacks | `Globals...` |

The list is indicative rather than exhaustive. The fix should begin with a full enumeration of `Globals` dereferences in that file that are reachable from a ribbon callback.

## Logs / Screenshots

- [x] Attached minimal logs or snippet
- Snippet: see the site table above. As with #507 and #518, Office does not surface the failure to the user; the exception escapes an `async void` handler unobserved.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [ ] Medium
- [x] Low

Same narrow reachable window as #507 and #518: the callback must run before `SetGlobals`.

## Suspected Cause / Notes

The `Globals` property is assigned by `SetGlobals` during add-in initialization, but the ribbon is constructed earlier, so every callback is reachable in an unassigned-`Globals` window. #507 made the `Engines` property itself null-safe (`Globals?.Engines`) and #505/#506/#518 guarded the ten viewer-side `Engines` call sites, but neither touched the direct `Globals` dereferences in the controller partial.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: the host-neutral guard/readiness logic extracted behind an injected accessor. Note that `RibbonController` carries `[ExcludeFromCodeCoverage]` under the ratified VSTO/COM ribbon-handler exemption, so extracted logic must be tested and the exemption must not be widened.
- [ ] Integration scenario to retest: invoke each affected callback in a live Outlook profile before initialization completes.
- [ ] Manual verification notes: run Outlook with "Show add-in user interface errors" enabled.

Suggested approach: reuse the existing seams rather than adding ad-hoc `?.` operators, which the maintainer disrecommended on #518 — `EngineReadinessGate` / `EngineGatedCommandRunner` / `EngineCommandCatalog` for readiness-gated commands, and `EngineToggleStateCoordinator` / `EngineToggleCatalog` for configuration-backed toggles.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
