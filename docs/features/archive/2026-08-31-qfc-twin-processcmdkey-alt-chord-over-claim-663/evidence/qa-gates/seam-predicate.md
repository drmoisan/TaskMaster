# Phase 1 — Behaviour-preserving predicate seam ([P1-T1])

Timestamp: 2026-09-01T22-33

The member `ClaimsAltChord` was added to the existing type `QfcFormKeyHandler` in
`QuickFiler/Controllers/QfcFormKeyHandler.cs`, in its behaviour-preserving intermediate form, together
with the `using QuickFiler.Interfaces;` directive the `IQfcKeyboardHandler` parameter type requires.

Intermediate body as written:

```csharp
internal static bool ClaimsAltChord(IQfcKeyboardHandler handler, Keys keyData) =>
    handler is not null && keyData.HasFlag(Keys.Alt);
```

That condition is exactly equivalent to the branch-head guard
`(_keyboardHandler is not null) && Controllers.QfcFormKeyHandler.IsAltKeyCommand(keyData)`, so this
intermediate state preserves behaviour.

`IsAltKeyCommand` was not modified. No `[ExcludeFromCodeCoverage]` attribute was added. An XML
documentation comment was added on the new member, which `[P1-T1]` permits; it does not repeat the
identifier `ClaimsAltChord` in its text.

Command: the three `Select-String` invocations transcribed below, each run under
`pwsh -NoProfile -Command`.

EXIT_CODE: 0 for every invocation.

## Acceptance reading 1 — the member exists

`Select-String -Path QuickFiler/Controllers/QfcFormKeyHandler.cs -Pattern 'ClaimsAltChord'`

Match count: **1**, which is at least one. Matched line:

```
L28: internal static bool ClaimsAltChord(IQfcKeyboardHandler handler, Keys keyData) =>
```

That matched line is the member declaration. This is a change from the zero matches the file returns at
branch head.

## Acceptance reading 2 — the seam is still the intermediate form

`Select-String -Path QuickFiler/Controllers/QfcFormKeyHandler.cs -Pattern 'Keys\.KeyCode'`

Match count: **0**.

Zero is what distinguishes the intermediate seam from the final form. `[P3-T1]` adds the mask and this
reading becomes at least one.

## Acceptance reading 3 — `IsAltKeyCommand` untouched

`Select-String -Path QuickFiler/Controllers/QfcFormKeyHandler.cs -Pattern 'IsAltKeyCommand'`

Match count: **1**, exactly one as required. Matched line:

```
L19: internal static bool IsAltKeyCommand(Keys keyData) => keyData.HasFlag(Keys.Alt);
```

The line number moved from 18 to 19 because the new `using QuickFiler.Interfaces;` directive was inserted
above it. The line text is byte-identical to the branch-head text; only its position changed. `[P5-T3]`
verifies by diff that no removed line contains `IsAltKeyCommand`.

Output Summary: `ClaimsAltChord` was added in its behaviour-preserving intermediate form and the required
`using QuickFiler.Interfaces;` directive was added. All three acceptance readings hold: `ClaimsAltChord`
returns one match and it is the member declaration, `Keys.KeyCode` returns zero matches, and
`IsAltKeyCommand` returns exactly one match with its body unchanged.
