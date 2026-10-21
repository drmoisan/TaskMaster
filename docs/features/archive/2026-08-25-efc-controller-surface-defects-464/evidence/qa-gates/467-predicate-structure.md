# [P8-T6] #467 delivered predicate structure and test isolation

Timestamp: 2026-08-28T01-31
Task: [P8-T6]
Command: source inspection of the delivered `QuickFiler/Viewers/EfcViewer.cs` and
`QuickFiler.Test/Controllers/EfcViewerTests.cs` with `awk` line numbering and `grep -n`; plus `grep -rn`
over `QuickFiler/Viewers/EfcViewer.Designer.cs` for the two menu captions
EXIT_CODE: 0

## Delivered declaration

Declared at `QuickFiler/Viewers/EfcViewer.cs:96`:

```csharp
internal static bool ClaimsAltChord(IQfcKeyboardHandler handler, Keys keyData)
```

Accessibility and modifiers: **`internal static`**. A search of `QuickFiler/Interfaces/` for the name
returns **0** matching lines, so it appears on no interface.

## Delivered predicate body, verbatim

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

The predicate masks with `Keys.KeyCode` (`:102`) and accepts only `Keys.Menu` or `Keys.None` as the
resulting key code, so the claim is bare Alt and nothing else.

## Delivered `ProcessCmdKey` body, verbatim

```csharp
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (ClaimsAltChord(_keyboardHandler, keyData))
            {
                object sender = FromHandle(msg.HWnd);
                var e = new KeyEventArgs(keyData);
                _keyboardHandler.ToggleKeyboardDialogAsync(sender, e);
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
```

**The `true` return is reached only through the predicate.** The override's guard (`:108`) is a single
call to `ClaimsAltChord`, and the only `return true` in the method (`:113`) is inside that guarded
branch. Every other key reaches `return base.ProcessCmdKey(ref msg, keyData)` at `:116`. The override
still constructs the sender (`:110`) and the `KeyEventArgs` (`:111`) inside the claimed branch, unchanged
from the pre-change shape.

## The two mnemonics restored

| Chord | Menu | Designer caption |
|---|---|---|
| `Alt+F` | Filters | `QuickFiler/Viewers/EfcViewer.Designer.cs:4102` — `this.FiltersMenu.Text = "&Filters";` |
| `Alt+M` | Move Options | `QuickFiler/Viewers/EfcViewer.Designer.cs:4162` — `this.MoveOptionsMenu.Text = "&Move Options";` |

Both captions carry an ampersand accelerator, so WinForms routes the chord to the menu once
`ProcessCmdKey` stops claiming it. The **manual reviewer check of both chords against a live Outlook
session is recorded separately in `[P11-T13]`**; this artifact records only the source-level restoration.

## `CharActions` reachability preserved

Feature #444 records that `CharActions` is read by `KeyboardHandler_KeyDown` and is reached **only** from
the Alt-key `ProcessCmdKey` path, and #444 deliberately widened Alt+`B` and Alt+`D` availability. The
narrowing here is scoped to `EfcViewer` and narrows only what `EfcViewer` claims: bare Alt, the gesture
that opens the keyboard dialog through which `CharActions` is serviced, still returns `true`, as
`ClaimsAltChord_WithBareAltAndHandler_ReturnsTrue` confirms. `QuickFiler/Controllers/KeyboardHandler.cs`
is owned by #498 and was not edited.

## Per-method isolation confirmation for every test in `EfcViewerTests.cs`

The file declares exactly eight test methods and no other type. For each, the four prohibited constructs
were searched across the whole file: `new Form`, `.Show()`, `.Handle`, `CreateControl`, and a `: Form`
base-type clause. **All five searches return zero matching lines over the entire file**, so the
confirmation holds for every method individually.

| # | Test method | Line | Constructs a `Form` | Calls `Show()` | Reads `Handle` | Declares a `Form`-derived type |
|---|---|---|---|---|---|---|
| 1 | `SetControllerAndFormControllerField_AreAbsentFromEfcViewerMetadata` | `:36` | No | No | No | No |
| 2 | `EditFiltersMenuItemClick_IsAbsentFromEfcViewerMetadata` | `:65` | No | No | No | No |
| 3 | `FormEditFiltersMenuItemClick_IsStillDeclaredOnEfcFormController` | `:91` | No | No | No | No |
| 4 | `ClaimsAltChord_WithBareAltAndHandler_ReturnsTrue` | `:112` | No | No | No | No |
| 5 | `ClaimsAltChord_WithAltF_ReturnsFalse` | `:123` | No | No | No | No |
| 6 | `ClaimsAltChord_WithAltM_ReturnsFalse` | `:134` | No | No | No | No |
| 7 | `ClaimsAltChord_WithNonAltChord_ReturnsFalse` | `:145` | No | No | No | No |
| 8 | `ClaimsAltChord_WithNullHandler_ReturnsFalse` | `:156` | No | No | No | No |

Methods 1 to 3 are type-metadata assertions over `EfcViewer` and `EfcFormController`; they use
reflection only and instantiate nothing. Methods 4 to 8 call the `internal static` predicate directly
with a `Mock<IQfcKeyboardHandler>` (or `null`) and a `Keys` value; the predicate needs no window handle,
following the pattern of `QfcFormKeyHandlerTests.cs`. The fixture class `EfcViewerTests` does not derive
from `System.Windows.Forms.Form`, so
`QuickFiler.Test/NoLiveFormInTestAssemblyTests.cs` remains satisfied.

File size: `QuickFiler/Viewers/EfcViewer.cs` is **169** lines, at most 500.
`QuickFiler.Test/Controllers/EfcViewerTests.cs` is **164** lines, under 500.

Output Summary: PASS. `ClaimsAltChord` is declared `internal static` at `EfcViewer.cs:96`, appears on no
interface, masks with `Keys.KeyCode`, and accepts only `Keys.Menu` or `Keys.None`. `ProcessCmdKey`'s
guard is a single call to it and its only `return true` sits inside that guarded branch, so every other
chord reaches `base.ProcessCmdKey`. The restored mnemonics are `Alt+F` for `"&Filters"` and `Alt+M` for
`"&Move Options"`. All eight tests in `EfcViewerTests.cs` construct no `Form`, call no `Show()`, read no
`Handle`, and declare no `Form`-derived type.
