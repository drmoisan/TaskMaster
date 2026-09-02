# ribbon-engine-toggle-defects (Issue #735)

- Date captured: 2026-09-02
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/ribbon-engine-toggle-defects/ (Issue #735)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #735
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/735
- Last Updated: 2026-09-02
## Summary

Three consolidated findings from a code-review sweep, all in the TaskMaster Ribbon subsystem (`TaskMaster/Ribbon/`): dead XML-to-handler bindings, an unguarded `Globals` dereference chain, and a toggle-state race — grouped as one issue since they share the same small file set and would otherwise cost three separate orchestration cycles for a combined ~10 lines of fix.

## Environment

- OS/version: Windows 11 Pro (repo default)
- Python version: n/a — C#/.NET Framework 4.8.1 WinForms VSTO add-in, Office Ribbon XML
- Command/flags used: n/a — findings are from code review
- Data source or fixture: n/a

## Steps to Reproduce

Not applicable in the usual sense — each finding below is a static code-review finding. See "Actual Behavior."

## Expected Behavior

Each finding's expected behavior is stated inline below.

## Actual Behavior

**1. Dead Ribbon XML-to-handler bindings (Source: #504).** `TaskMaster/Ribbon/RibbonExplorer.xml` declares five `onAction` values with no matching public method on `RibbonViewer`: `BtnMigrateIDs_Click` (no implementation exists anywhere in the assembly) and a `_Clicked`-vs-`_Click` suffix mismatch on `MoveEntireConversation_Clicked`, `SaveAttachments_Clicked`, `SaveEmailCopy_Clicked`, `SavePictures_Clicked` (the actual `RibbonViewer.cs` methods are named `..._Click`, without the "ed"). VSTO compiles fine and silently does nothing when any of these five controls is clicked — confirmed still present on `origin/main` (`RibbonExplorer.xml:268,274,280,286` and the `RibbonViewer.cs` method list). This was previously investigated during issue #503/#505 and explicitly deferred rather than fixed, per `docs/features/archive/2026-08-08-ribbon-engine-readiness-guard-503/`.

**2. `RibbonController.Intelligence.cs` `ClearSpamManagerAsync` unguarded `Globals` deref (Source: #524).** `TaskMaster/Ribbon/RibbonController.Intelligence.cs:216-231` (`ClearSpamManagerAsync`) dereferences `Globals.AF.Manager.Configuration`, `Globals.AF.Manager[...]`, and `Globals.Engines.RestartEngineAsync` with no null guard on `Globals`, `Globals.AF`, or `Globals.Engines`. If any of these is null when the user clicks "Clear Spam Manager" (e.g. before startup completes), this throws an unhandled `NullReferenceException` from a UI event handler.

**3. `EngineToggleStateCoordinator.ApplyPrimeAsync` toggle-state race (Source: #525).** `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs:303-311`: `ApplyPrimeAsync` reads the engine's real activation state once and writes it unconditionally — `_pressedState[engineName] = active;` — with no check for whether a concurrent `ExecuteToggleAsync` call already wrote a more recent value. A prime that completes after a user-initiated toggle can silently overwrite the toggle's result with stale data.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: n/a — see file/line citations inline above; each finding was verified directly against `origin/main` during this consolidation pass (2026-09-02).

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

High: finding 2 is an unhandled-exception crash path reachable from a routine UI action. Findings 1 and 3 are Medium individually (silent feature loss; a toggle-state data race); the bundle is rated at the severity of its most severe member, per this repo's established practice for consolidated multi-defect issues.

## Suspected Cause / Notes

Each finding traces to a specific prior issue, cited inline above. All three were deliberately deferred by their originating investigations rather than fixed in-branch, because fixing them would have exceeded that investigation's declared scope.

## Proposed Fix / Validation Ideas

- [ ] Rename the four `_Clicked` XML `onAction` values to `_Click` (or add the matching `_Clicked`-suffixed methods, whichever direction is intended); decide whether `BtnMigrateIDs` should be implemented or removed from the XML
- [ ] Add a null guard on `Globals`/`Globals.AF`/`Globals.Engines` in `ClearSpamManagerAsync`, failing with a clear diagnostic instead of an unhandled NRE
- [ ] Add a last-writer-wins guard to `ApplyPrimeAsync` (e.g. a generation counter or in-flight marker) so a stale prime cannot overwrite a newer toggle result

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
