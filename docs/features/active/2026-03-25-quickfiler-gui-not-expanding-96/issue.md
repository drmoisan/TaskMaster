# quickfiler-gui-not-expanding (Issue #96)

- Date captured: 2026-03-25
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/quickfiler-gui-not-expanding/ (Issue #96)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #96
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/96
- Last Updated: 2026-03-25
- Work Mode: minor-audit

## Summary

Pressing the Right arrow key while QuickFiler keyboard navigation is active does not expand the conversation messages beneath the selected item; instead it activates the sender's mailto: address on the focused control.

## Environment

- OS/version: Windows (any)
- Python version: N/A (C# / WinForms VSTO add-in)
- Command/flags used: Press Alt to activate QuickFiler keyboard interface; navigate with Up/Down; press Right on an item with >1 conversation member
- Data source or fixture: Any Outlook mailbox with at least one threaded email conversation

## Steps to Reproduce

1. Open Outlook with the QuickFiler add-in loaded.
2. Press Alt to activate the QuickFiler keyboard interface.
3. Use Up/Down arrows to navigate to an email that has more than one message in a conversation (LblConvCt > 0).
4. Press the Right arrow key.

## Expected Behavior

The selected item should expand to reveal all the conversation messages beneath it (equivalent to clicking the expand/collapse widget or pressing 'E').

## Actual Behavior

The Right arrow key press falls through to the focused WinForms control (a label or link showing the sender's email address). The mailto: address of the sender is displayed or activated instead of the conversation expanding.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: No error is logged; the key press is silently misrouted.

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

## Suspected Cause / Notes

Root cause identified: `QfcItemController.RegisterFocusAsyncActions()` does not register a `Keys.Right` handler in `_kbdHandler.KeyActionsAsync`. The handler was commented out when the codebase migrated from the sync `RegisterFocusActions()` path to the async `RegisterFocusAsyncActions()` path, and was never re-implemented. Because no handler suppresses the key press, WinForms routes the Right arrow event to whatever control holds focus, which renders or activates the sender's mailto: link.

Files to inspect:
- `QuickFiler/Controllers/QfcItemController.cs` — `RegisterFocusAsyncActions()` (line ~1335) and `UnregisterFocusAsyncActions()` (line ~1465)
- `QuickFiler/Controllers/KeyboardHandler.cs` — `KeyDownTaskAsync()` for the key-dispatch chain

## Proposed Fix / Validation Ideas

- [x] Add `_kbdHandler.KeyActionsAsync.Add(ItemHelper.EntryId, Keys.Right, (x) => this.ToggleExpansionAsync(Enums.ToggleState.On))` to `RegisterFocusAsyncActions()`.
- [x] Uncomment `_kbdHandler.KeyActionsAsync.Remove(ItemHelper.EntryId, Keys.Right)` in `UnregisterFocusAsyncActions()`.
- [x] Unit coverage areas: `QfcItemControllerTests.cs` — add tests asserting that `Keys.Right` is present in `KeyActionsAsync` after `RegisterFocusAsyncActions()` and absent after `UnregisterFocusAsyncActions()`.
- [ ] Integration scenario to retest: manually reproduce in Outlook after deploying the fix.
- [ ] Manual verification notes: confirm Right arrow expands conversation and that mailto: is no longer triggered.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch