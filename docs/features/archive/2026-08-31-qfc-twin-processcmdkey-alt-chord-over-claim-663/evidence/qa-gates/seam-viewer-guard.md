# Phase 1 — Viewer guard routed through the predicate ([P1-T2])

Timestamp: 2026-09-01T22-34

The four-line guard formerly on lines 58 through 61 of `QuickFiler/Viewers/QfcFormViewer.cs` was replaced
by the single-line condition quoted verbatim in the plan's reading guide. The body of the branch is
unchanged, including the pre-existing locals and the parameterless dispatch.

`ProcessCmdKey` after the edit, lines 56 through 70:

```csharp
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (Controllers.QfcFormKeyHandler.ClaimsAltChord(_keyboardHandler, keyData))
            {
                SynchronizationContext.SetSynchronizationContext(UiSyncContext);
                object sender = FromHandle(msg.HWnd);
                var e = new KeyEventArgs(keyData);
                //_keyboardHandler.ToggleKeyboardDialog(sender, e);
                e.Handled = true;
                _ = _keyboardHandler.ToggleKeyboardDialogAsync();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
```

The existing qualification form `Controllers.QfcFormKeyHandler` is retained; the file's namespace is
`QuickFiler`, so the relative qualification resolves. The dispatch stays the parameterless
`ToggleKeyboardDialogAsync()`.

Command: the three `Select-String` invocations transcribed below, each run under
`pwsh -NoProfile -Command`.

EXIT_CODE: 0 for every invocation.

## Acceptance reading 1 — exactly one `ClaimsAltChord`, inside `ProcessCmdKey`

`Select-String -Path QuickFiler/Viewers/QfcFormViewer.cs -Pattern 'ClaimsAltChord'`

Match count: **1**, exactly one as required. Matched line:

```
L58: if (Controllers.QfcFormKeyHandler.ClaimsAltChord(_keyboardHandler, keyData))
```

Line 58 lies inside the `ProcessCmdKey` method body, which spans lines 56 through 70 as transcribed
above. No comment repeats the identifier, which is why the count is one rather than more.

## Acceptance reading 2 — `IsAltKeyCommand` gone from the viewer

`Select-String -Path QuickFiler/Viewers/QfcFormViewer.cs -Pattern 'IsAltKeyCommand'`

Match count: **0**, zero as required. This is a change from the single match on line 60 that `[P0-T14]`
recorded before the edit.

## Acceptance reading 3 — VC-2 still returns two

`Select-String -Path QuickFiler/Viewers/QfcFormViewer.cs -Pattern 'FromHandle|new KeyEventArgs'`

Match count: **2**, still two as required. Matched lines:

```
L61: object sender = FromHandle(msg.HWnd);
L62: var e = new KeyEventArgs(keyData);
```

The two lines moved from 64 and 65 to 61 and 62 because the four-line guard collapsed to one line, a
three-line reduction. Their text is unchanged. Their retention is deliberate: removing them is an
explicit non-goal of the spec and AC-14 pins their survival through this pattern.

Output Summary: The viewer guard now routes through `ClaimsAltChord`. All three acceptance readings hold:
`ClaimsAltChord` returns exactly one match and that match is inside `ProcessCmdKey`, `IsAltKeyCommand`
returns zero matches, and VC-2 still returns two matches for the retained pre-existing locals.
