# efc-viewer-processcmdkey-swallows-alt-mnemonics (Issue #467)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/efc-viewer-processcmdkey-swallows-alt-mnemonics/ (Issue #467)
- Work Mode: full-bug

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #467
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/467
- Last Updated: 2026-08-08
## Summary

`EfcViewer.ProcessCmdKey` returns `true` for **every** Alt-modified key whenever a keyboard handler is
attached, so `base.ProcessCmdKey` never runs for any Alt combination. This disables the standard
WinForms mnemonic path for both of the form's menu strips.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Runtime: .NET Framework 4.8.1 WinForms VSTO add-in
- UI path: `QuickFiler/Viewers/EfcViewer.cs` (Email Filer viewer form)
- Data source or fixture: n/a — pure input-routing path

## Steps to Reproduce

1. Open the Email Filer viewer (`EfcViewer`) so that `_keyboardHandler` is attached.
2. Press any Alt-modified accelerator that maps to a menu mnemonic on either menu strip.
3. Observe that the menu does not open and the keyboard-dialog toggle is invoked instead.

## Expected Behavior

Alt combinations that the QuickFiler keyboard handler does not claim should fall through to
`base.ProcessCmdKey` so WinForms can resolve menu mnemonics and standard accelerators normally.

## Actual Behavior

`EfcViewer.cs:94-105`:

```csharp
protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
{
    if ((_keyboardHandler is not null) && (keyData.HasFlag(Keys.Alt)))
    {
        object sender = FromHandle(msg.HWnd);
        var e = new KeyEventArgs(keyData);
        _keyboardHandler.ToggleKeyboardDialogAsync(sender, e);
        return true;
    }

    return base.ProcessCmdKey(ref msg, keyData);
}
```

The guard tests only `keyData.HasFlag(Keys.Alt)` — it does not ask the handler whether it actually
claims this key. Returning `true` reports the key as fully handled, so no further processing occurs.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Code-read evidence recorded above (verified 2026-08-07 against the working tree).

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Keyboard-only users lose the menu mnemonic path on this form. The severity is bounded because the
menus remain reachable by mouse.

## Suspected Cause / Notes

The guard conflates "this is an Alt chord" with "the keyboard handler owns this Alt chord".
`ToggleKeyboardDialogAsync` is also invoked without awaiting and its result is discarded, so a fault
inside it is unobserved.

Related: `EfcViewer.ProcessCmdKey` is the only branch in the file, and both of its false outcomes flow
into `base.ProcessCmdKey`. This is relevant to coverage work under issue #452 (epic #136), which must
pin the **current** behavior with characterization tests rather than correct it.

## Proposed Fix / Validation Ideas

- [ ] Narrow the guard so it consults the handler for a claim on `keyData` before returning `true`
- [ ] Unit coverage: Alt chord claimed by handler; Alt chord not claimed; non-Alt chord; null handler
- [ ] Manual verification: menu mnemonics open both menu strips with the viewer focused

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
