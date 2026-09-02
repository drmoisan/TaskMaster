# Phase 3 — Minimal fix applied to the predicate ([P3-T1])

Timestamp: 2026-09-01T22-51

The intermediate body of `ClaimsAltChord` in `QuickFiler/Controllers/QfcFormKeyHandler.cs` was replaced by
the final source form quoted verbatim in the plan's reading guide, which adds the null and Alt-flag guard,
the `keyData & Keys.KeyCode` mask, and the acceptance of `Keys.Menu` or `Keys.None` only.

Final body as written:

```csharp
internal static bool ClaimsAltChord(IQfcKeyboardHandler handler, Keys keyData)
{
    if (handler is null || !keyData.HasFlag(Keys.Alt))
    {
        return false;
    }

    Keys keyCode = keyData & Keys.KeyCode;
    return keyCode == Keys.Menu || keyCode == Keys.None;
}
```

Nothing else in the file changed. `IsAltKeyCommand` was not modified. No `[ExcludeFromCodeCoverage]`
attribute was added.

Command: the three `Select-String` invocations transcribed below, each run under
`pwsh -NoProfile -Command`.

EXIT_CODE: 0 for every invocation.

## Acceptance reading 1 — the key-code mask is present

`Select-String -Path QuickFiler/Controllers/QfcFormKeyHandler.cs -Pattern 'Keys\.KeyCode'`

Match count: **1**, which is at least one as required. Matched line:

```
L35: Keys keyCode = keyData & Keys.KeyCode;
```

This is a change from the zero matches `[P1-T1]` recorded for the intermediate seam.

## Acceptance reading 2 — the `Keys.Menu` arm is present

`Select-String -Path QuickFiler/Controllers/QfcFormKeyHandler.cs -Pattern 'Keys\.Menu'`

Match count: **1**, which is at least one as required. Matched line:

```
L36: return keyCode == Keys.Menu || keyCode == Keys.None;
```

The `Keys.Menu` arm matters because `Keys.Menu` is documented as "The ALT key" and is the key code a
physical bare Alt press produces; `Keys.None` is the key code the synthetic `Keys.Alt` value used in unit
tests produces. `ClaimsAltChord_WithMenuKeyCodeAndAltFlag_ReturnsTrue` pins the first shape and
`ClaimsAltChord_WithBareAltFlagAndHandler_ReturnsTrue` pins the second, so neither arm is
untested-and-therefore-removable.

## Acceptance reading 3 — `IsAltKeyCommand` still exactly one match

`Select-String -Path QuickFiler/Controllers/QfcFormKeyHandler.cs -Pattern 'IsAltKeyCommand'`

Match count: **1**, still exactly one as required. Matched line:

```
L19: internal static bool IsAltKeyCommand(Keys keyData) => keyData.HasFlag(Keys.Alt);
```

Byte-identical to the branch-head text. AC-8 requires the member to survive unchanged.

## File size

`QuickFiler/Controllers/QfcFormKeyHandler.cs` is **39 lines**, within the repository's 500-line limit.

Output Summary: The final predicate body is in place. All three acceptance readings hold:
`Keys.KeyCode` returns one match, up from the zero the intermediate seam produced; `Keys.Menu` returns one
match; and `IsAltKeyCommand` still returns exactly one match with its body unchanged.
