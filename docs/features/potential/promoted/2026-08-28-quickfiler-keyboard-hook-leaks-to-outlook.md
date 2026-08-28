# quickfiler-keyboard-hook-leaks-to-outlook (Issue #677)

- Date captured: 2026-08-28
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/quickfiler-keyboard-hook-leaks-to-outlook/ (Issue #677)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #677
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/677
- Last Updated: 2026-08-28
## Summary

Running QuickFiler causes keyboard input to native Outlook windows (Explorer/Inspector) to stop working after the user clicks out of the QuickFiler window; QuickFiler's own keyboard navigation continues to work correctly while its window has focus.

## Environment

- OS/version: Windows (VSTO add-in host, Outlook desktop)
- Component: QuickFiler (WinForms UserControl/Form hosted inside the Outlook VSTO process)
- Data source or fixture: N/A (manual interactive repro)

## Steps to Reproduce

1. Launch Outlook with the TaskMaster VSTO add-in loaded.
2. Run QuickFiler against a mail item so its filing window opens and keyboard navigation is active.
3. Click out of the QuickFiler window into a native Outlook window (Explorer list, an open Inspector, the search box, etc.) without closing QuickFiler.
4. Attempt to type characters in the native Outlook window.

## Expected Behavior

Keyboard input scoping should be limited to the QuickFiler window. Once focus moves to a native Outlook window, that window should receive keystrokes normally, exactly as if QuickFiler were not running.

## Actual Behavior

Keystrokes typed into native Outlook windows are blocked/suppressed while QuickFiler is open, even though focus is no longer on the QuickFiler window. Keyboard navigation inside QuickFiler itself works correctly. Keyboard input outside the Outlook process (other applications) is unaffected.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: N/A — behavioral repro, no exception/log signature identified yet.

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

## Suspected Cause / Notes

User hypothesis: QuickFiler's keyboard hooking (`QuickFiler/Controllers/KeyboardHandler.cs`, wired via `IQfcKeyboardHandler`) is scoped wider than the QuickFiler window — e.g. it is intercepting/suppressing keyboard messages for the whole Outlook process instead of only the QuickFiler control tree.

Initial code survey for this entry (no root cause confirmed yet — root-cause investigation is the first delegated step of the fix):
- No global low-level keyboard hook (`SetWindowsHookEx`/`WH_KEYBOARD*`) was found anywhere in the repo, so the leak is not a classic global hook.
- `KeyboardHandler` (`QuickFiler/Controllers/KeyboardHandler.cs`) implements `PreviewKeyDown`/`KeyDown` handlers with a `KbdActive` flag that gates whether keys are suppressed (`e.SuppressKeyPress = true; e.Handled = true;`). These are ordinary WinForms control event handlers, which should only fire for events raised by controls that are wired to them.
- `Form.KeyPreview` is present but commented out in `QuickFiler/Viewers/QfcFormViewer.cs`, so it is not the active mechanism.
- No `Application.AddMessageFilter`/`IMessageFilter` registration is currently active in the codebase (the one implementation, `UtilitiesCS/HelperClasses/Windows Forms/MouseDownFilter.cs`, is not wired up, and its only call site is commented out).
- Remaining plausible mechanisms to investigate: (a) `KbdActive` or the `KeyboardHandler`/action-table state persisting as effectively-global/static and being consulted from a shared or long-lived object after the QuickFiler window loses focus; (b) the QuickFiler window/control never truly releasing keyboard focus back to Outlook's main window (owner/parent/TopMost/activation handling); (c) some other subscriber to Outlook Explorer/Inspector-level events routing keystrokes through QuickFiler's handler regardless of which window is focused.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: `KeyboardHandler` activation/deactivation lifecycle (`KbdActive` scoping), any window-activation/focus-transfer logic for the QuickFiler window.
- [ ] Integration scenario to retest: open QuickFiler, click into a native Outlook Explorer/Inspector window, confirm normal typing; close QuickFiler, confirm QuickFiler's own keyboard navigation still functions during the session before closing.
- [ ] Manual verification notes: verify no regression to QuickFiler's own keyboard-driven filing workflow (arrow keys, character actions, string filter actions) after the scoping fix.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
