# quickfiler-keyboard-hook-leaks-to-outlook (Issue #677)

- Date captured: 2026-08-28
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/quickfiler-keyboard-hook-leaks-to-outlook/ (Issue #677)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #677
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/677
- Last Updated: 2026-08-28
- Work Mode: full-bug

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

- [x] Unit coverage areas: **delivered**. The seeded `KeyboardHandler`/`KbdActive` coverage area is superseded — root-cause analysis confirmed `KeyboardHandler` is correctly scoped to QuickFiler's own control tree and is not changed by this fix (`evidence/qa-gates/keyboardhandler-unchanged.md`). The correct coverage area is the **window-activation / focus-transfer logic**, and seventeen regression tests now cover it at 100% changed-line coverage: eight in `QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.Part3.cs` for the execution-time focus-permission predicate, seven in `QuickFiler.Test/Controllers/QfcFormControllerDeactivateTests.cs` for the `Form.Deactivate` focus-parking and selector-cancel handler, and two in `QuickFiler.Test/Controllers/QfcItemController.CancelBreadcrumbSelectorTests.cs` for the fan-out hop.
- [ ] Integration scenario to retest: open QuickFiler, click into a native Outlook Explorer/Inspector window, confirm normal typing; close QuickFiler, confirm QuickFiler's own keyboard navigation still functions during the session before closing. — **still open**; requires a live Outlook session. See `evidence/other/manual-verification-pending.md`.
- [ ] Manual verification notes: verify no regression to QuickFiler's own keyboard-driven filing workflow (arrow keys, character actions, string filter actions) after the scoping fix. — **still open**; requires a live Outlook session. See `evidence/other/manual-verification-pending.md`.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [x] Move to active fix folder / branch
- [x] Fix implemented: the two-part focus fix (execution-time focus-permission predicate on `BreadcrumbDropDownHost`, plus `Form.Deactivate`-driven focus parking and selector cancellation through `QfcFormViewer`/`QfcFormController`). Full toolchain green: CSharpier check 0 violations, analyzer rebuild 0 errors, nullable rebuild 0 errors, full suite 6838/6838 passing, repo line coverage 85.28% (up from 85.27%).
- [ ] **Manual live-Outlook verification pending** — acceptance criteria AC-1, AC-2 and the manual half of AC-3 in `spec.md` remain unchecked until a maintainer runs the checklist in `evidence/other/manual-verification-pending.md`. The same session should reconfirm or rule out the secondary WinForms modal-menu-mode contributor recorded in `spec.md` Rollout & Follow-up.
