# people-tag-window-autotag (Issue #322)

- Date captured: 2026-07-12
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/people-tag-window-autotag/ (Issue #322)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #322
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/322
- Last Updated: 2026-07-12
- Work Mode: minor-audit

## Summary

In Task Visualization, clicking the People field opens the tag-assignment window (Tags dialog), but the auto-tag (auto-assign) function on that window does not work. The people mapping data is verified to exist, but the auto-tag code path is not invoked.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Runtime: .NET Framework (VSTO Outlook add-in), TaskMaster solution
- Command/flags used: Interactive UI — TaskViewer people-assignment flow
- Data source or fixture: Live `_globals.TD.People` mapping (verified populated)

## Steps to Reproduce

1. Open the Task Visualization window (TaskViewer) for a task item.
2. Click the People field; the tag-assignment window (Tags.TagViewer) opens.
3. Invoke the auto-tag (auto-assign) function on that window.

## Expected Behavior

The auto-tag function runs the people classifier (`AutoAssignPeople.AutoFind` → `AutoFile.AutoFindPeople`) against the active item and toggles on the matching people tags in the dialog.

## Actual Behavior

Nothing happens. The mapping exists, but the auto-tag code that consumes it is never invoked.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: none captured; user-verified that the mapping exists and the auto-tag code is not reached.

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

## Suspected Cause / Notes

Candidate defect surface identified by initial code scout:

- `TaskVisualization/TaskController.Actions.cs` — `AssignPeople()` passes `objItemObject: _active.OlItem.InnerObject` to the prompt request (Context/Project pass the `IOutlookItem` wrapper instead).
- `Tags/TagController.cs` — `ResolveMailItem` requires `objItem is MailItem`, and `SetAutoAssignState` hides/disables the auto-assign button unless `_isMail` is true; `ButtonAutoAssign_Action` calls `_autoAssigner.AutoFindAsync(_objItem)`.
- `TaskVisualization/AutoAssignPeople.cs` — `AutoFind(object)` returns an empty list unless the argument is a `MailItemHelper`, an `IOutlookItem` mail item, or a `MailItem` RCW; a non-matching runtime type silently short-circuits before `RunPeopleClassifier`.

The silent empty-list fallthrough in `AutoFind` and/or the `_isMail` gating in `TagController` are the most likely reasons the classifier is never invoked.

## Acceptance Criteria

- [x] The root cause of the auto-tag function not being invoked from the People tag-assignment window is identified and documented in the fix commit/plan evidence.
- [x] A failing regression test is authored first that reproduces the defect deterministically (MSTest + Moq + FluentAssertions, no live Outlook process, no temporary files), and it passes after the fix.
- [x] After the fix, invoking the auto-tag function on the People tag-assignment window executes the people auto-assign path (`IAutoAssign.AutoFindAsync` reaching the people classifier seam) for the active item instead of silently returning without invoking it.
- [x] Matching auto-found people tags are toggled on in the dialog options when the mapping contains entries for the item, verified via unit test through the `TagController` auto-assign action seam.
- [x] Existing behavior for the Context and Project assignment flows is unchanged (no regression in their tests).
- [x] The full C# toolchain passes in order (CSharpier format, analyzers build, nullable build, MSTest with coverage) with no regression on changed lines, and changed/new code meets the >= 90% coverage target for testable seams.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: `AutoAssignPeople.AutoFind` branch selection; `TagController.SetAutoAssignState` and `ButtonAutoAssign_Action` invocation path.
- [ ] Integration scenario to retest: People-field click → tags window → auto-tag applies mapped people tags.
- [ ] Manual verification notes: verify auto-assign button is visible, enabled, and invokes the classifier for the active task item.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
