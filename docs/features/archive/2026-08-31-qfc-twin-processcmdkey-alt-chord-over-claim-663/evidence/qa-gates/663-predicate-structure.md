# Phase 5 — Viewer predicate structure ([P5-T2])

Timestamp: 2026-09-01T23-25

Three `Select-String` invocations were run against `QuickFiler/Viewers/QfcFormViewer.cs`, each under
`pwsh -NoProfile -Command`.

Command 1: `Select-String -Path QuickFiler/Viewers/QfcFormViewer.cs -Pattern 'ClaimsAltChord'`
Command 2: `Select-String -Path QuickFiler/Viewers/QfcFormViewer.cs -Pattern 'Keys\.Alt'`
Command 3: `Select-String -Path QuickFiler/Viewers/QfcFormViewer.cs -Pattern 'IsAltKeyCommand'`

EXIT_CODE: 0 for every invocation.

## Reading 1 — `ClaimsAltChord`

Match count: **1**, exactly one as required. Matched line:

```
L58: if (Controllers.QfcFormKeyHandler.ClaimsAltChord(_keyboardHandler, keyData))
```

The matched line lies inside the `ProcessCmdKey` method body. The method's own boundaries were measured
in the same task rather than assumed: `Select-String -Pattern 'ProcessCmdKey'` over the file returns

```
L56: protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
L69: return base.ProcessCmdKey(ref msg, keyData);
```

so the override is declared on line 56 and its fall-through return is on line 69. Line 58 lies between
them.

This is a change detector: `[P0-T14]` recorded zero matches before the fix and this run records one.

## Reading 2 — `Keys.Alt`

Match count: **0**, zero as required.

This clause is an invariant guard rather than a change detector: `[P0-T14]` recorded zero matches at
branch head and AC-7 requires that the rewritten guard introduce none, so it fails only if a modifier
test is inlined into the viewer. It reads zero before and zero after.

## Reading 3 — `IsAltKeyCommand`

Match count: **0**, zero as required.

This is a change detector in the opposite direction: `[P0-T14]` recorded a single match, on line 60,
before the fix, and this run records none. The viewer no longer references the broad predicate.

Output Summary: `QfcFormViewer.ProcessCmdKey` delegates its claim decision to `ClaimsAltChord` and
contains no independent Alt test. `ClaimsAltChord` returns exactly one match, on line 58, inside the
`ProcessCmdKey` body that spans lines 56 through 69; `Keys.Alt` returns zero matches, unchanged from the
pre-change reading; and `IsAltKeyCommand` returns zero matches, down from the single match `[P0-T14]`
recorded. All three AC-7 clauses hold.
