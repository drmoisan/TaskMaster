# efc-enter-activates-valid-highlighted-suggestion (Issue #641)

- Date captured: 2026-08-27
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/efc-enter-activates-valid-highlighted-suggestion/ (Issue #641)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #641
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/641
- Last Updated: 2026-08-27
- Work Mode: minor-audit

## Summary

In Efc, pressing Enter while a valid folder suggestion is highlighted does not invoke
the same OK action as pressing the OK button.

## Environment

- OS/version: Windows desktop TaskMaster Efc.
- Python version: Not applicable; the affected implementation is C# with a WebView-backed breadcrumb selector.
- Command/flags used: Interactive Efc keyboard navigation.
- Data source or fixture: Any suggestion list containing a valid filing destination.

## Steps to Reproduce

1. Open Efc and display folder suggestions.
2. Highlight a valid destination suggestion using the selector.
3. Press Enter.

## Expected Behavior

Enter must map to the OK action when the highlighted suggestion is a valid filing selection.
The resulting behavior must be the same as activating the Efc OK button for that selection.

## Acceptance Criteria

- [ ] With a valid highlighted Efc suggestion, Enter invokes the same OK action as the OK button.
- [ ] Enter does not invoke the OK action when the suggestion is invalid or absent.
- [ ] Focused regression coverage verifies the valid and invalid keyboard paths.

## Actual Behavior

Nothing happens when Enter is pressed, despite a valid suggestion being highlighted.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: No error is presented; the Efc dialog remains open and the valid highlighted selection is not acted upon.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

## Suspected Cause / Notes

`EfcFormController` exposes `ActionOkAsync`, but the Enter event from the
breadcrumb selector does not appear to reach that action. The existing bridge handles
selector keyboard messages separately from the form-level OK command path.

## Proposed Fix / Validation Ideas

- [ ] Add a focused regression test proving Enter invokes the same OK action for a valid highlighted suggestion.
- [ ] Verify Enter does not activate OK when the highlighted selection is invalid or absent.
- [ ] Retest activation through both the keyboard and the Efc OK button.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
