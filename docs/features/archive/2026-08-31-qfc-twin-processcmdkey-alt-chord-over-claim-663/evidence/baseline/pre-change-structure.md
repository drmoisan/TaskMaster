# Phase 0 — Pre-change structural state ([P0-T14])

Timestamp: 2026-09-01T22-31

This artifact records the false-before readings that every later structural gate is compared against.

Command: the six `Select-String` invocations transcribed below, each run under
`pwsh -NoProfile -Command`.

EXIT_CODE: 0 for every invocation.

## Reading 1 — `IsAltKeyCommand` in the viewer

`Select-String -Path QuickFiler/Viewers/QfcFormViewer.cs -Pattern 'IsAltKeyCommand'`

Match count: **1** (expected one). Matched line:

```
L60: && Controllers.QfcFormKeyHandler.IsAltKeyCommand(keyData)
```

`[P5-T2]` requires this to read zero after the fix, so this is a change detector reading one before.

## Reading 2 — `ClaimsAltChord` in the viewer

`Select-String -Path QuickFiler/Viewers/QfcFormViewer.cs -Pattern 'ClaimsAltChord'`

Match count: **0** (expected zero). No matched lines.

`[P1-T2]` and `[P5-T2]` require this to read exactly one after the fix, so this is a change detector
reading zero before.

## Reading 3 — `Keys.Alt` in the viewer

`Select-String -Path QuickFiler/Viewers/QfcFormViewer.cs -Pattern 'Keys\.Alt'`

Match count: **0** (expected zero). No matched lines.

`[P5-T2]` requires this still to read zero after the fix. It is an invariant guard rather than a change
detector: AC-7 requires that the rewritten guard introduce no inline modifier test into the viewer, so it
fails only if a modifier test is inlined there.

## Reading 4 — VC-2 against the viewer

`Select-String -Path QuickFiler/Viewers/QfcFormViewer.cs -Pattern 'FromHandle|new KeyEventArgs'`

Match count: **2** (expected two, on lines 64 and 65). Matched lines:

```
L64: object sender = FromHandle(msg.HWnd);
L65: var e = new KeyEventArgs(keyData);
```

The alternation pipe is deliberately unescaped. `Select-String -Pattern` takes a .NET regular expression,
in which `\|` is an escaped literal pipe that would match nothing.

`[P1-T2]`, `[P5-T7]` and AC-14 require this still to read two after the fix, one match per literal, both
inside `ProcessCmdKey`. It pins the survival of the pre-existing unused locals, whose retention is an
explicit non-goal of the spec.

## Reading 5 — VC-1 against the test file

`Select-String -Path QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs -Pattern 'new Form|: Form|Thread\.Sleep|Task\.Delay|GetTempFileName|GetTempPath'`

Match count: **0** (expected zero). No matched lines.

The `\.` sequences are correct and retained: there the backslash escapes a literal dot. Only the
alternation pipes are left unescaped.

`[P2-T1]` and `[P5-T5]` require this still to read zero after the seven new tests are added.

## Reading 6 — existing test-method declarations

`Select-String -Path QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs -Pattern 'IsAltKeyCommand_'`

Match count: **4** (expected four declaration matches, one per existing test method). Matched lines:

```
L16: public void IsAltKeyCommand_WithAltKey_ReturnsTrue()
L29: public void IsAltKeyCommand_WithAltPlusOtherKey_ReturnsTrue()
L42: public void IsAltKeyCommand_WithControlKey_ReturnsFalse()
L55: public void IsAltKeyCommand_WithNone_ReturnsFalse()
```

These four methods are not modified by any task in this plan. AC-8 requires them to survive unchanged.

Output Summary: All six pre-change readings were taken and all six match the counts the plan states.
`IsAltKeyCommand` appears once in the viewer, on line 60; `ClaimsAltChord` and `Keys.Alt` appear zero
times there; VC-2 returns two matches on lines 64 and 65; VC-1 returns zero matches over the test file;
and the four existing `IsAltKeyCommand_*` declarations are present on lines 16, 29, 42 and 55. These are
the false-before readings for the `[P1-T1]`, `[P1-T2]`, `[P2-T1]`, `[P5-T2]`, `[P5-T5]` and `[P5-T7]`
gates.
