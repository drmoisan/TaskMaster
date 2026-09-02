# Research — issue #663, QFC twin `ProcessCmdKey` Alt-chord over-claim

- **Issue:** #663
- **Timestamp:** 2026-09-01T01-05
- **Tree:** worktree on branch `bug/qfc-twin-processcmdkey-alt-chord-over-claim-663`, HEAD == `origin/main` == `2b85134b`
- **Scope of this document:** EFC (#467) precedent forensics; WinForms semantics of the `Keys.KeyCode` mask;
  handler-contract divergence; menu-mnemonic inventory on the QFC surface; test-surface facts; coverage posture.
- **Not covered here (owned by a parallel agent):** construction/reachability of the four viewer types. The
  compile-inclusion determination that agent produced is at
  `<repo-root>/docs/features/active/qfc-twin-processcmdkey-alt-chord-over-claim-663/evidence/other/call-site-compile-inclusion.md`
  and this document builds on it rather than repeating it.

## Tooling limitation recorded up front

**The Bash tool is disabled for this session** ("No such tool available: Bash. Bash is disabled for this
session, in subagents as well as here."). No `git` command could be executed. Consequently:

- The requested `git show 28d244e5 --stat` and the diff of its `.cs` files **could not be run**. What
  commit `28d244e5` touched is reported below from the *committed evidence artifacts of feature #464*,
  which record the same file set from two independently-run `git diff --name-only` invocations. That is
  second-hand but contemporaneous and internally cross-checked; it is **not** a direct read of the commit.
- Every other claim in this document is derived from direct file reads and Grep over the working tree, or
  from a cited Microsoft Learn URL.

---

## 1. EFC precedent forensics (#467 under feature #464)

### 1.1 The invariant #467 was written to establish, quoted

From `<repo-root>/docs/features/active/efc-controller-surface-defects-464/spec.md:669-670`
("Technical specifications / Inputs/outputs and formats"):

> `ClaimsAltChord(handler, keyData)` returns `true` if and only if `handler is not null`, `keyData` has
> the `Keys.Alt` flag, and the key-code portion of `keyData` is `Keys.Menu` or `Keys.None`.

The behavioural half, from `spec.md:641-642`:

> **RC10**: `ProcessCmdKey` returns `true` only when `ClaimsAltChord` is true; otherwise control reaches
> `base.ProcessCmdKey`, restoring both mnemonics.

The reasoning that produced it, from `spec.md:474-476`:

> **Therefore the claim is exactly: Alt with no other key code.** Any `Alt`+*key* chord is a WinForms
> mnemonic and must reach `base.ProcessCmdKey`. This settles the Alt+M question research §Q5.5 raised: Alt+M
> is a mnemonic, not a handler claim, and there is no collision with the `'M'` registration.

The premise it rests on, from `spec.md:465-470`:

> **The gesture the handler actually services is the bare-Alt toggle, not an Alt chord.** … **It never
> inspects `e.KeyData`.** The key code is discarded.

### 1.2 Why the SHARED predicate was left alone — the recorded reason

`spec.md:486-491` states it explicitly:

> The QFC twin `QfcFormViewer.cs:56-73` **shares this defect** — `QfcFormKeyHandler.IsAltKeyCommand`
> (`QfcFormKeyHandler.cs:18`) is just `keyData.HasFlag(Keys.Alt)`. **This feature does not change the QFC
> twin.** What it adopts from the twin is the **testability pattern**: an `internal static` predicate
> lifted out of the `ProcessCmdKey` override so the key-command logic can be unit tested without a live
> `Form` window handle, exercised by tests carrying no `Form` instance
> (`QfcFormKeyHandlerTests.cs`, 67 lines, four `[TestMethod]`s).

The placement decision is recorded separately at `spec.md:629-634`:

> `ClaimsAltChord` is placed on `EfcViewer` itself rather than in a new
> `QuickFiler/Controllers/EfcViewerKeyHandler.cs`. A new file would require a `QuickFiler.csproj`
> `<Compile Include>` edit, which this feature has otherwise eliminated and which would contend with
> feature #501's one-line addition after `QuickFiler.csproj:392`. `EfcViewer.cs` is owned, is 162 lines,
> and a static member is callable without instantiating the `Form`. **Extending `QfcFormKeyHandler.cs` is
> rejected — not in the owned set.**

**This is the load-bearing point for #663 scoping.** The recorded reason for not narrowing the shared
predicate is an **ownership/file-scope constraint of feature #464**, not a technical judgement that the
shared predicate should stay broad. `research/2026-08-25T12-20-efc-controller-surface-defects.md:562`
states the same thing in one line: "**Option 3: extend `QfcFormKeyHandler.cs`.** Rejected — that file is
not in 464's owned set." Nothing in #464 argues that narrowing `IsAltKeyCommand` would be *wrong*.

### 1.3 Was the QFC twin's deferral recorded, and with what reason?

Yes, in three places, all pointing at the same ownership reason:

| Location | Text |
|---|---|
| `spec.md:110` (root-cause register) | "**RC10** \| Input-routing over-claim \| #467 \| **GUARD** \| the QFC twin shares the defect and supplies only the testability pattern" |
| `spec.md:487` | "**This feature does not change the QFC twin.**" |
| `research/…-464….md:523` | "So the QFC twin **does not fix the over-claim**. What it supplies is the **testability seam pattern**" |

No artifact in the #464 folder records a *technical* objection to fixing the QFC side.

### 1.4 The test-authoring pattern, exactly

Delivered file: `<repo-root>/QuickFiler.Test/Controllers/EfcViewerTests.cs`, 164 lines, 8 `[TestMethod]`s
(3 for #466, 5 named `ClaimsAltChord_*` at `:112`, `:123`, `:134`, `:145`, `:156`).

- **Why the fixture avoids constructing a `Form`.** Two reasons are recorded. First, the structural guard
  `<repo-root>/QuickFiler.Test/NoLiveFormInTestAssemblyTests.cs:17`
  (`ExecutingAssembly_ContainsNoFormDerivedType`) fails if any `Form`-derived type is compiled into the
  test assembly. Second, `research/…-464….md:1328` lists as *unverified* "Whether
  `EfcViewer.ProcessCmdKey`'s `base.ProcessCmdKey` is safe to invoke on a `CreateUninitialized<EfcViewer>()`
  instance", and notes "§Q5.4's recommended remedy (a pure static predicate) makes the question
  unnecessary." The evidence artifact `evidence/qa-gates/467-predicate-structure.md:83-86` records that
  five searches (`new Form`, `.Show()`, `.Handle`, `CreateControl`, `: Form`) each return zero matches over
  the whole file.
- **Where the file was placed and why.** `QuickFiler.Test/Controllers/EfcViewerTests.cs`. The deviation
  note is in the file's own `<remarks>` at `EfcViewerTests.cs:24-25`, verbatim:

  > The file is deliberately placed under `Controllers/` rather than `Viewers/`;
  > the deviation from the mirrored test layout is recorded in the plan task that created it.

  **Correction available for #663:** `QuickFiler.Test` *does* have a populated `Viewers\` folder — 42
  `<Compile Include="Viewers\…">` items at `QuickFiler.Test.csproj:67-107` and `:192-193`. The `Controllers/`
  placement was a deviation from an available convention, not a forced choice. A #663 test for a member on
  `QfcFormViewer` would belong under `Viewers\`; a test for a member on `QfcFormKeyHandler` belongs under
  `Controllers\`, where `QfcFormKeyHandlerTests.cs` already sits (`QuickFiler.Test.csproj:151`).
- **Assertion / mocking shape.** `Mock<IQfcKeyboardHandler>` from Moq, FluentAssertions `.Should().BeTrue(
  "<reason>")` / `.BeFalse("<reason>")`, MSTest `[TestClass]`/`[TestMethod]`. Namespace
  `QuickFiler.Controllers.Tests`.

### 1.5 What commit `28d244e5` touched — reported at second hand

Direct verification was impossible (no Bash/git, §"Tooling limitation"). Feature #464's own evidence
records the complete non-documentation change set from two `git diff --name-only` runs against two
different bases:

- `evidence/qa-gates/changed-file-set.md:49-62` (base `38f09789`, 98 paths total)
- `evidence/other/final-commit.md:50-63` (same base, 119 paths total after the doc commit)

Both list the identical twelve non-documentation paths:

```
QuickFiler.Test/Controllers/EfcFormControllerTests.cs
QuickFiler.Test/Controllers/EfcItemController.CleanupTests.cs
QuickFiler.Test/Controllers/EfcItemControllerTests.cs
QuickFiler.Test/Controllers/EfcViewerTests.cs
QuickFiler.Test/QuickFiler.Test.csproj
QuickFiler/Controllers/EfcFormController.cs
QuickFiler/Controllers/EfcItemController.cs
QuickFiler/Controllers/QfcItemController.ViewerSetup.cs
QuickFiler/Viewers/EfcViewer.cs
QuickFiler/Viewers/EfcViewer3.Designer.cs   (deleted)
QuickFiler/Viewers/EfcViewer3.cs            (deleted)
QuickFiler/Viewers/EfcViewer3.resx          (deleted)
```

Of these, the **#467 fix itself** touched exactly two: `QuickFiler/Viewers/EfcViewer.cs` (predicate added at
`:96-104`, override rewritten at `:106-117`) and `QuickFiler.Test/Controllers/EfcViewerTests.cs` (created),
plus the `QuickFiler.Test.csproj` line that compiles the new test file. **`QuickFiler/QuickFiler.csproj` is
explicitly absent** — `changed-file-set.md:84-86` records that it appears only in the over-broad
`BASELINE_SHA` diff and is attributable to merged siblings. That is the evidentiary basis for "the EFC fix
required no production-csproj edit".

If a direct `git show 28d244e5` is required as a gate artifact, it must be produced in a session with a
working shell; this document cannot supply it.

### 1.6 Current delivered state of the EFC predicate (read directly)

`<repo-root>/QuickFiler/Viewers/EfcViewer.cs:96-104`:

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

The `<remarks>` at `:90-95` asserts the WinForms behaviour that §2 verifies.

---

## 2. WinForms semantics of the `Keys.KeyCode` mask

### 2.1 Enum values (authoritative)

Source: <https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.keys?view=netframework-4.8.1>
(Fields table).

| Member | Value | Documented description |
|---|---|---|
| `None` | 0 | "No key pressed." |
| `Menu` | 18 | **"The ALT key."** |
| `Left` | 37 | "The LEFT ARROW key." |
| `Up` | 38 | "The UP ARROW key." |
| `Right` | 39 | "The RIGHT ARROW key." |
| `Down` | 40 | "The DOWN ARROW key." |
| `F` | 70 | "The F key." |
| `M` | 77 | "The M key." |
| `LMenu` | 164 | "The left ALT key." |
| `RMenu` | 165 | "The right ALT key." |
| `KeyCode` | 65535 | **"The bitmask to extract a key code from a key value."** |
| `Modifiers` | -65536 | "The bitmask to extract modifiers from a key value." |
| `Alt` | 262144 | "The ALT modifier key." |

**`Keys.Menu` is the key code for the ALT key. Stated explicitly by the documentation: `Menu = 18`, "The
ALT key."** `Keys.Alt = 262144` is the separate *modifier* bit. The two are not interchangeable.

Enum remarks (same page): "a key value has two halves, with the high-order bits containing the key code …
and the low-order bits representing key modifiers". (Note: the prose reverses high/low relative to the
numeric values; the numeric table is authoritative and is what the mask arithmetic follows.)

### 2.2 `keyData & Keys.KeyCode` for each case

`keyData` is formed by WinForms as the virtual-key code from `wParam` OR-ed with the current modifier bits.
`Keys.KeyCode == 65535 == 0x0000FFFF`, so the mask clears every modifier bit (`Shift 0x10000`,
`Control 0x20000`, `Alt 0x40000`) and leaves the virtual-key code.

| Gesture | `keyData` | `keyData & Keys.KeyCode` | `ClaimsAltChord` verdict |
|---|---|---|---|
| Bare ALT press (real keyboard) | `Keys.Menu \| Keys.Alt` = `18 \| 262144` = `262162` | `Keys.Menu` (18) | **true** |
| Synthetic `Keys.Alt` (unit-test value) | `262144` | `Keys.None` (0) | **true** |
| Alt+F | `Keys.F \| Keys.Alt` = `70 \| 262144` | `Keys.F` (70) | false |
| Alt+M | `Keys.M \| Keys.Alt` = `77 \| 262144` | `Keys.M` (77) | false |
| Alt+Left / Up / Down / Right | `Keys.{Left,Up,Down,Right} \| Keys.Alt` | `Keys.Left` (37) / `Up` (38) / `Down` (40) / `Right` (39) | false |
| ALT released | n/a — see §2.4 | n/a | predicate not reached |

**The `Keys.Menu` remark in `EfcViewer.cs:90-95` is confirmed correct**, and the `Keys.None` half is
confirmed *necessary for the delivered tests to pass*: `EfcViewerTests.cs:117` passes the bare enum value
`Keys.Alt`, whose key-code portion is `Keys.None`, not `Keys.Menu`. See §2.6 for the gap this creates.

### 2.3 Is `ProcessCmdKey` reached for a bare modifier press? Yes.

`WM_SYSKEYDOWN` carries "The virtual-key code of the key being pressed" in `wParam`
(<https://learn.microsoft.com/en-us/windows/win32/inputdev/wm-syskeydown>). The ALT key's own virtual key is
`VK_MENU` (0x12 = 18), which is `Keys.Menu`. `Control.PreProcessMessage` accepts "WM_KEYDOWN,
WM_SYSKEYDOWN, WM_CHAR, and WM_SYSCHAR"
(<https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.control.preprocessmessage?view=netframework-4.8.1>,
Parameters section) and dispatches `WM_SYSKEYDOWN` to `ProcessCmdKey`.

**Evidence-strength caveat.** The `WM_SYSKEYDOWN` summary sentence enumerates only "presses the F10 key …
or holds down the ALT key and then presses another key"; it does not spell out the ALT-alone case. The
claim that `ProcessCmdKey` is reached for a bare ALT press is therefore supported by (a) the virtual-key /
`Keys.Menu` correspondence above and (b) **direct in-repo behavioural evidence**: the QuickFiler and Email
Filer keyboard-navigation dialog is opened today by pressing ALT alone, which is only possible if the
existing `ProcessCmdKey` override runs for that gesture (`QfcFormViewer.cs:56-73`,
`EfcViewer.cs:106-117`). It is corroborated, not quoted.

### 2.4 ALT released

`Control.PreProcessMessage`'s documented message set does not include `WM_SYSKEYUP`
(same URL, Parameters). **`ProcessCmdKey` is not reached on key-up.** No release-time behaviour changes
under any candidate fix.

### 2.5 What `base.ProcessCmdKey` does with an Alt mnemonic — the actual opening mechanism

Two documented facts combine.

1. `Control.ProcessCmdKey` bubbles up:
   <https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.control.processcmdkey?view=netframework-4.8.1>,
   Remarks: "If the command key is not a menu shortcut and the control has a parent, the key is passed to
   the parent's `ProcessCmdKey` method. The net effect is that command keys are 'bubbled' up the control
   hierarchy." **A `Form`-level override therefore sees Alt chords typed into any focused descendant
   control, not just the form itself.**
2. Mnemonics are *not* resolved in the `ProcessCmdKey` path at all. From
   <https://learn.microsoft.com/en-us/dotnet/desktop/winforms/how-keyboard-input-works> ("Preprocessing for
   a KeyDown event" and "Preprocessing for a KeyPress event"):

   > **`ProcessCmdKey`** — "This method processes a command key, which takes precedence over regular keys.
   > **If this method returns `true`, the key message isn't dispatched and a key event doesn't occur.**"

   > **`ProcessDialogChar`** — "Check to see if the character is a mnemonic (such as &OK on a button) …
   > This method, similar to `ProcessDialogKey`, is called up the control hierarchy. **If the control is a
   > container control, it checks for mnemonics by calling `ProcessMnemonic` on itself and its child
   > controls.**"

   `ProcessDialogChar` sits in the `WM_CHAR`/`WM_SYSCHAR` (KeyPress) preprocessing chain, downstream of the
   `WM_SYSKEYDOWN` that `ProcessCmdKey` sees.

**Mechanism, stated precisely.** Alt+M produces `WM_SYSKEYDOWN`. If `ProcessCmdKey` returns `true` the key
message is not dispatched, so the `WM_SYSCHAR` that would carry the mnemonic character is never generated,
so `ProcessDialogChar` → `ProcessMnemonic` never runs, so the menu never opens. Returning `false` (i.e.
falling through to `base.ProcessCmdKey`) lets the chain continue to the mnemonic stage. This is exactly the
#467 defect and exactly why the #467 fix works.

### 2.6 Gap in the delivered EFC tests, relevant to #663

`EfcViewerTests.cs:112-162` exercises `Keys.Alt`, `Keys.Alt | Keys.F`, `Keys.Alt | Keys.M`, `Keys.F`, and
`null`. **It never exercises `Keys.Menu | Keys.Alt`** — the shape a real keyboard actually produces
(§2.2). The bare-Alt positive case is passing only through the `Keys.None` arm of the predicate. A #663
test set should pin **both** `Keys.Alt` and `Keys.Menu | Keys.Alt` so the `Keys.Menu` arm is not
untested-and-therefore-removable.

---

## 3. Handler-contract divergence — what the sites would lose

### 3.1 The registries, and what is actually in them for the QuickFiler surface

Population sites (all searched under `QuickFiler/`; the six registries are declared at
`QuickFiler/Interfaces/IQfcKeyboardHandler.cs:21-26` and backed at `KeyboardHandler.cs:44-88`):

| Registry | Key type | QFC population site | Registered keys |
|---|---|---|---|
| `KeyActions` (sync) | `Keys` | `QfcItemController.EventWiring.cs:161`, `:166` | `Keys.Right`, `Keys.Left` |
| `KeyActionsAsync` | `Keys` | `QfcItemController.EventWiring.cs:223`; `QfcCollectionController.cs:1138-1139` | `Keys.Right`; `Keys.Up`, `Keys.Down` |
| `AlwaysOnKeyActionsAsync` | `Keys` | `QfcCollectionController.cs:1153` | `Keys.Return` |
| `CharActions` (sync) | `char` | `QfcItemController.EventWiring.cs:171-209`, `:310-319` | `O C A M E S T P R X F` (+ `B D` when expanded) |
| `CharActionsAsync` | `char` | `QfcItemController.EventWiring.cs:228-301`, `:324-333` | `C O M R L W E S T P Z X F` (+ `B D` when expanded) |
| `StringActionsAsync` | `string` | `QfcCollectionController.cs:1202` (`RegisterNavigation`) | digit-prefix row selectors |

**No registry is keyed on a modifier-bearing value.** Every `Keys`-keyed entry is a bare `Keys` member
(`Right`, `Left`, `Up`, `Down`, `Return`); every `char`-keyed entry is a bare uppercase letter. There is no
`Keys.Alt | …` key anywhere.

### 3.2 Could a registered key ever arrive as an Alt chord through `ProcessCmdKey`? No.

Three independent reasons:

1. **The dispatchers read the *unmodified* key code.** `KeyboardHandler_KeyDown` (`KeyboardHandler.cs:118`,
   `:124`) looks up `KeyActions[e.KeyCode]` and `CharActions[(char)e.KeyValue]`. `e.KeyCode` is already the
   `keyData & Keys.KeyCode` value. So an Alt chord and a bare press are *indistinguishable* at the lookup —
   the Alt bit contributes nothing.
2. **On the compiled QFC surface the dispatchers are not reached from `ProcessCmdKey` at all.** The one
   compiled `QfcFormViewer.ProcessCmdKey` (`QfcFormViewer.cs:68`) calls the **parameterless**
   `ToggleKeyboardDialogAsync()` (`KeyboardHandler.cs:225-236`), whose body reads only `_kbdActive` and
   calls `ToggleOffNavigationAsync()` / `ToggleOnNavigationAsync()`. It never touches any registry.
3. **The sync `KeyboardHandler_KeyDown` has no compiled caller anywhere.** Repo-wide search for
   `KeyboardHandler_KeyDown` returns production call sites only in `QfcFormViewerDark.cs:48`,
   `QfcFormViewerExpanded.cs:48` (neither file is in `QuickFiler.csproj`), `Legacy/*` and `Notes/*` (whole
   folders absent from the csproj), and `TaskVisualization/TaskViewer.cs:260` (a *different* method on a
   *different* type — see §3.4). The compiled QuickFiler wiring subscribes the **async** overload to
   control `KeyDown` events: `QfcItemController.EventWiring.cs:45`, `:422`;
   `QfcFormController.SetupDisposal.cs:164`, `:193`. This independently reconfirms finding **D10** in
   `research/…-464….md:1312`.

**Conclusion for the compiled QFC site: narrowing the Alt claim to bare Alt loses nothing.** No registered
key becomes unreachable, because no registered key was ever reachable through an Alt chord.

### 3.3 Alt+arrow — vestigial comment, not behaviour

`QfcFormViewerDark.cs:45`, `QfcFormViewerExpanded.cs:45` and `QfcFormLegacyViewer.cs:25` all carry the same
commented-out VB-era line:

```csharp
// If keyData = Keys.Up OrElse keyData = Keys.Down OrElse keyData = Keys.Left OrElse keyData = Keys.Right OrElse keyData = Keys.Alt Then
```

It is a **comment**, in three files that are **not compiled**. The equivalent test pin,
`QfcFormKeyHandlerTests.cs:29` (`IsAltKeyCommand_WithAltPlusOtherKey_ReturnsTrue`, asserting
`Keys.Alt | Keys.Left` → `true`), is a *unit test of the predicate*, not evidence of a live Alt+arrow
gesture. Against it: the arrow entries in `KeyActions` / `KeyActionsAsync` /
`AlwaysOnKeyActionsAsync` (§3.1) are all keyed on bare `Keys.Left/Right/Up/Down/Return`, and the arrow
handling in `KeyboardHandler.DdOpen_KeyDownAsync` / `DdClosed_KeyDownAsync`
(`KeyboardHandler.cs:323-411`) switches on `e.KeyCode` with no modifier test.

**Determination: Alt+arrow is vestigial on the QuickFiler surface.** No compiled code path treats Alt+arrow
differently from bare arrow, and no registry contains an Alt-modified key.

### 3.4 What the uncompiled sites would lose (hypothetically), and the out-of-project site

- **`QfcFormViewerDark.cs:41-53` / `QfcFormViewerExpanded.cs:41-53`.** Not build inputs. If they were
  compiled, narrowing to bare-Alt-only would mean their `KeyboardHandler_KeyDown(sender, e)` call fires
  only on bare Alt. Since `KeyboardHandler_KeyDown` is gated on `KbdActive` (`KeyboardHandler.cs:116`) and
  dispatches on the unmodified key code (§3.2 point 1), the only behaviour reachable there via an Alt chord
  is behaviour equally reachable via the corresponding *bare* key. Loss: none in substance.
- **`Legacy/QfcFormLegacyViewer.cs:21-33`.** Whole `QuickFiler/Legacy/` folder absent from the csproj. Its
  target `QuickFiler.Legacy.QuickFileController.KeyboardHandler_KeyDown` (`QuickFileController.cs:604`) is
  likewise uncompiled. Loss: none.
- **`TaskVisualization/TaskViewer.cs:253-265` — genuinely different, and genuinely compiled.**
  `TaskVisualization.csproj:110` compiles `TaskViewer.cs`. Its target
  `TaskController.KeyboardHandler_KeyDown` (`TaskController.Accelerator.cs:75`) branches on `e.Alt`
  (`:77`) and **toggles the accelerator overlay for *any* Alt-bearing chord**, then services subsequent
  *bare* letters/arrows while `_altActive` (`:110-119`). Narrowing that site would change real behaviour
  pinned by `TaskVisualization.Test/TaskControllerAcceleratorKeyboard.StaTests.cs:76-144`. Also,
  `TaskViewer.cs:260` discards the `bool` the handler returns, whereas `TaskViewer.cs:395` consumes it —
  an internal inconsistency worth a separate issue. **Recommend excluding site 5 from #663**, consistent
  with the parallel agent's finding that `TaskViewer.Designer.cs` declares no menu strip at all.

---

## 4. Menu inventory on the QFC surface — the correctness trap

### 4.1 `QfcFormViewer` has no menus of its own, and no `&` mnemonic of its own

- `QuickFiler/Viewers/QfcFormViewer.Designer.cs` contains **zero** occurrences of `MenuStrip`,
  `ToolStripMenuItem`, or `MainMenuStrip`.
- It contains **zero** occurrences of `&` followed by a letter (searched two ways, §6).
- Its five `Button` captions are, verbatim: `"Skip Group"` (`:102`), `"Filters"` (`:113`), `"OK"` (`:124`),
  `"CANCEL"` (`:135`), `"Undo"` (`:146`); the form caption is `"Quick File"` (`:230`). **None carries an
  ampersand.**
- The one runtime-mutated caption, `SkipButtonText`, is set only to `"Skipping..."` and `"Skip Group"`
  (`QfcFormController.EventHandlers.cs:339`, `:341`). No ampersand.

**Therefore `Alt+F` does nothing on the QFC surface today and will still do nothing after any fix.** The
QFC analogue of the EFC "&Filters" menu is `ButtonFilters`, a `Button` whose text is `"Filters"` with no
mnemonic. Naming Alt+F in a #663 acceptance criterion would create an untestable and false claim.

### 4.2 The mnemonic that *is* being swallowed lives on the hosted item viewers

`QfcFormViewer.Designer.cs` instantiates two `UserControl`s and adds them to the form's control tree:

- `:41` `this._QfcItemViewerTemplate = new QuickFiler.ItemViewer();` → added `:179`
- `:42` `this._qfcItemViewerExpandedTemplate = new QuickFiler.ItemViewerExpanded();` → added `:180`

Both carry a real `MenuStrip` in their own control tree, with a top-level `&`-mnemonic item:

| Host control | `MenuStrip` field, added to parent at | Top-level item caption | Mnemonic |
|---|---|---|---|
| `ItemViewer` | `_moveOptionsStrip`, `ItemViewer.Designer.cs:114` (`_l0vh_Tlp.Controls.Add`) | `"&Move Options"` (`:173`) | **Alt+M** |
| `ItemViewerExpanded` | `MoveOptionsStrip`, `ItemViewerExpanded.Designer.cs:104` (`L0vh_Tlp.Controls.Add`) | `"&Move Options"` (`:161`) | **Alt+M** |

Additional `ItemViewer` instances are manufactured per row at
`QuickFiler/Helper Classes/ItemViewerQueue.cs:105` (`return new ItemViewer();`), each carrying its own
`"&Move Options"`.

Because `ProcessCmdKey` bubbles up the whole hierarchy (§2.5, point 1), `QfcFormViewer.ProcessCmdKey`
intercepts Alt+M typed anywhere on the form, including inside any of these item viewers.

**The single mnemonic letter currently swallowed on the QFC surface is `M`, for "&Move Options".**

The four drop-down children (`"Move &Conversation"`, `"Save &Attachments"`, `"Save E&mail Copy"`,
`"Save &Pictures"` at `ItemViewer.Designer.cs:6125`, `:6133`, `:6141`, `:6149`, and the same four at
`ItemViewerExpanded.Designer.cs:170`, `:178`, `:186`, `:194`) are reached only once the drop-down is open,
at which point form-level `ProcessCmdKey` is not the routing path — the same reasoning `spec.md:481-483`
applied to EFC.

### 4.3 Explicit statement of the EFC/QFC difference

| | EFC (`EfcViewer`) | QFC (`QfcFormViewer`) |
|---|---|---|
| Top-level mnemonics on the form's own Designer | `"&Filters"` (`EfcViewer.Designer.cs:4102`), `"&Move Options"` (`:4162`) | **none** |
| Top-level mnemonics contributed by hosted item viewers | `ItemViewer` at `EfcViewer.Designer.cs:74` → `"&Move Options"` | `ItemViewer` (`:41`) + `ItemViewerExpanded` (`:42`) + N queue-manufactured `ItemViewer`s → `"&Move Options"` each |
| `MainMenuStrip` assigned? | Yes — `MoveOptionsStrip` (`EfcViewer.Designer.cs:4224`) | **No** — no `MainMenuStrip` assignment exists |
| Mnemonic letters restored by the fix | `F` and `M` | **`M` only** |
| Number of distinct controls owning the `M` mnemonic | 2 (form-level `MoveOptionsMenu` + hosted `ItemViewer`) | 2 + one per loaded row |

Two consequences worth spec attention:

1. **Duplicate-mnemonic ambiguity is a QFC-only risk.** WinForms cycles focus among multiple controls
   sharing one mnemonic. With N rows there are N+2 `"&Move Options"` items. Whether the *intended* row's
   menu opens on the first Alt+M press cannot be determined without a live host; it requires the same kind
   of manual reviewer check #464 recorded as `[P11-T13]`
   (`evidence/qa-gates/467-predicate-structure.md:69-70`). Note also that WinForms only offers a mnemonic
   to a control whose whole parent chain is visible and enabled, which will exclude any hidden template.
2. **A #464 statement is narrower than the tree.** `spec.md:478` says the two lost EFC chords are "both
   top-level menu mnemonics constructed in `EfcViewer.Designer.cs`". `EfcViewer.Designer.cs:74` also
   instantiates a `QuickFiler.ItemViewer`, which contributes a second `"&Move Options"` owner. The *count
   of lost chords* (2) is unaffected; the *count of mnemonic owners* was understated. Recorded for
   accuracy; it does not invalidate the #467 fix.

### 4.4 A pre-existing keyboard route to the same menu already exists on QFC

`CharActionsAsync['M']` is registered at `QfcItemController.EventWiring.cs:242-246` to
`this.KbdExecuteAsync(MenuDropDown, true)` → `QfcItemController.Navigation.cs:81-84` →
`_itemViewer.ShowMoveOptionsMenu()` → `ItemViewer.WebViewThread.cs:35` → `MoveOptionsMenu.ShowDropDown()`.
So bare `M`, while keyboard mode is active, already opens the same drop-down.

(The *sync* `CharActions['M']` at `EventWiring.cs:186-190` maps to `ToggleSaveCopyOfMail()` instead — but
per §3.2 point 3 the sync registry has no compiled reader, so it does not describe live behaviour. Do not
cite the sync registration as the QFC 'M' semantics.)

This mirrors the EFC "collision note" (`research/…-464….md:589-593`) and resolves the same way: Alt+M as a
mnemonic and bare `M` as a registered action reach the same menu, so restoring the mnemonic adds a second
route rather than conflicting with the first.

---

## 5. Test-surface facts for planning

| Fact | Value | Evidence |
|---|---|---|
| Test project path | `<repo-root>/QuickFiler.Test/QuickFiler.Test.csproj` | direct read |
| Legacy non-SDK csproj? | **Yes.** `ToolsVersion="15.0"`, `xmlns=".../developer/msbuild/2003"`, no `Microsoft.NET.Sdk` attribute, explicit `<Compile Include>` per file | `QuickFiler.Test.csproj:2`; item list `:58` onward |
| New test file requires a csproj edit? | **Yes** | no wildcard `<Compile Include>` in the project |
| Exact line format for `EfcViewerTests.cs` | `    <Compile Include="Controllers\EfcViewerTests.cs" />` — four leading spaces, backslash separator, self-closing, no child metadata | `QuickFiler.Test.csproj:128` |
| A `Viewers\` test folder already exists | Yes, 42 items | `QuickFiler.Test.csproj:67-107`, `:192-193` |
| `QfcFormKeyHandlerTests.cs` already compiled | Yes | `QuickFiler.Test.csproj:151` |
| Target framework | `v4.8.1` | `QuickFiler.Test.csproj:18` |
| MSTest | `MSTest.TestAdapter` **4.3.3**, `MSTest.TestFramework` **4.3.3** (+ `MSTest.Analyzers`) | `QuickFiler.Test/packages.config:119`, `:120`, `:114` |
| Moq | **4.20.72** | `QuickFiler.Test/packages.config:112` |
| FluentAssertions | **8.10.0** | `QuickFiler.Test/packages.config:8` |
| Mechanism reaching `internal` production members | `[assembly: InternalsVisibleTo("QuickFiler.Test")]` | `QuickFiler/Properties/AssemblyInfo.cs:5` |
| Structural guard the fixture must not violate | `ExecutingAssembly_ContainsNoFormDerivedType` | `QuickFiler.Test/NoLiveFormInTestAssemblyTests.cs:17-36` |

**Note on the `InternalsVisibleTo` mechanism.** It is an assembly-level attribute in *production* source,
already present, requiring no project-file change — the same statement `spec.md:626-627` made for #464. It
covers `QfcFormKeyHandler` (an `internal static class`, `QfcFormKeyHandler.cs:10`) and any `internal static`
member added to `QfcFormViewer`.

---

## 6. Coverage posture

| Type | File:line of `[ExcludeFromCodeCoverage]` | Measured? |
|---|---|---|
| `QfcFormViewer` | `QuickFiler/Viewers/QfcFormViewer.cs:17` | **No** |
| `QfcFormViewerDark` | `QuickFiler/Viewers/QfcFormViewerDark.cs:16` | No (and not compiled) |
| `QfcFormViewerExpanded` | `QuickFiler/Viewers/QfcFormViewerExpanded.cs:16` | No (and not compiled) |
| `QfcFormLegacyViewer` | **none — no attribute in the file** | Not compiled, so absent from the denominator regardless |
| `EfcViewer` | `QuickFiler/Viewers/EfcViewer.cs:20` | **No** |
| `QfcFormKeyHandler` | **none — no attribute in the file** | **Yes** |
| `KeyboardHandler` (for context) | `QuickFiler/Controllers/KeyboardHandler.cs:22` | No |

Assembly-level exclusions: `<repo-root>/coverage.config` excludes only Deedle, FSharp, Castle.Core,
FluentAssertions, Moq, Microsoft.Testing*, MSTest module paths. **`QuickFiler.dll` is instrumented**, so
exemption here is entirely attribute-driven and per-type.

**Direct consequence for #663.** A predicate placed on `QfcFormViewer` (the exact #467 mirror) lands inside
an `[ExcludeFromCodeCoverage]` type and produces **no `<method>` element in the Cobertura output** — it is
fully *tested* but not *measured*, so the "≥ 90% for new methods" clause of the unit-test policy cannot be
demonstrated by measurement. A predicate placed on or in `QfcFormKeyHandler` **is** measured. The
`[ExcludeFromCodeCoverage]` attribute on `QfcFormViewer` predates this work; reusing it adds no exemption
(relevant because `qfc-item-controller-defects-484/spec.md:235` forbids "Adding any new
`[ExcludeFromCodeCoverage]` attribute anywhere").

---

## 7. Numeric Derivation Evidence

### N1 — Distinct Alt mnemonic *letters* swallowed on the compiled QFC form surface

- **Complete Family:** every control that (a) is compiled into `QuickFiler.dll`, (b) is present in the
  control hierarchy rooted at a `QfcFormViewer` instance at runtime, and (c) carries a `Text` value
  containing `&` immediately followed by an ASCII letter, i.e. a WinForms mnemonic.
- **Exhaustive Search Scope:** every `.cs` file under `<repo-root>/QuickFiler/` (Designer files,
  hand-written viewer files, and controller files that assign `Text` at runtime) **plus** every `.resx`
  under `<repo-root>/QuickFiler/Viewers/` (mnemonics can be supplied through
  `ComponentResourceManager.ApplyResources` rather than an inline literal).
- **Inclusion Rules:** `&` + `[A-Za-z]` inside a string that reaches a `Text` property of a control that is
  a descendant of `QfcFormViewer`; the mnemonic must be *top-level* (reachable while no menu is open),
  since drop-down children are not routed through form-level `ProcessCmdKey`.
- **Exclusion Rules:** (i) `&&` and other boolean/bitwise `&` usages; (ii) `&lt;`/`&gt;`/`&quot;`/`&apos;`
  XML entities in doc comments and resource strings; (iii) files with no `<Compile Include>` in
  `QuickFiler/QuickFiler.csproj`; (iv) controls hosted on a *different* top-level `Form`
  (`EfcViewer`, and the `MyBoxViewer : Form` dialog at
  `UtilitiesCS/Dialogs/MyBoxViewer.cs:15` reached from `QfcItemController.MailActions.cs:81-101` — a
  separate window with its own `ProcessCmdKey` chain); (v) drop-down child items.
- **Primary Search Strategy:** Grep, content mode, pattern `\.Text = "[^"]*&[A-Za-z]`, path
  `<repo-root>/QuickFiler`. This anchors on the property-assignment syntax.
- **Primary Member Set:** 16 hits in 3 files —
  `EfcViewer.Designer.cs` {`:4102 "&Filters"`, `:4162 "&Move Options"`, `:4173`, `:4183`, `:4193`, `:4203`};
  `ItemViewer.Designer.cs` {`:173 "&Move Options"`, `:6125`, `:6133`, `:6141`, `:6149`};
  `ItemViewerExpanded.Designer.cs` {`:161 "&Move Options"`, `:170`, `:178`, `:186`, `:194`}.
  After exclusion (iv) (drop `EfcViewer.Designer.cs` — different form) and (v) (drop the eight drop-down
  children), the QFC-surface top-level set is
  **{ `ItemViewer._moveOptionsMenu` = `"&Move Options"`, `ItemViewerExpanded.MoveOptionsMenu` = `"&Move Options"` }**,
  i.e. **one distinct mnemonic letter: `M`**.
- **Primary Count:** distinct mnemonic letters on the QFC form surface = **1** (`M`).
  Distinct top-level mnemonic-bearing *control types* = **2**.
- **Cross-check Search Strategy:** a *different* pattern that is not anchored to `.Text = ` at all —
  bare `&[A-Za-z]` — run (a) repo-wide over `<repo-root>/QuickFiler/**/*.cs`, and (b) per-file over each
  Designer file of a control hosted on `QfcFormViewer`; **plus** a third, format-different query
  `&amp;[A-Za-z]` over `<repo-root>/QuickFiler/Viewers/*.resx` to close the resource-supplied path; **plus**
  a structural query `MenuStrip` over `<repo-root>/QuickFiler` to enumerate menu hosts independently of any
  text pattern.
- **Cross-check Member Set:**
  - Repo-wide bare `&[A-Za-z]` over `QuickFiler/**/*.cs` returned the same 16 `Text` hits plus 20
    non-qualifying hits, each excluded by an explicit rule: 2 `-&gt;` in `BreadcrumbBridgeCoordinator.cs`
    doc comments and 1 in `EmailMoveMonitor.cs` and 1 in `QfcFormController.Actions.cs` (rule ii);
    12 `&lt;`/`&quot;`/`&apos;` in `Properties/Resources.Designer.cs` (rule ii); 3 commented-out or
    `MyBox`-dialog button captions in `EfcItemController.cs:1109-1111` and 6 in
    `QfcItemController.MailActions.cs:81-101` (rule iv — `MyBoxViewer` is a separate `Form`).
  - Per-file: `QfcFormViewer.Designer.cs` → **0 matches**; `ItemViewer.Designer.cs` → 5;
    `ItemViewerExpanded.Designer.cs` → 5.
  - `&amp;[A-Za-z]` over `QuickFiler/Viewers/*.resx` → **0 matches**, so no mnemonic is resource-supplied.
    Corroborated by `ApplyResources` returning **0** occurrences across all of `QuickFiler/`.
  - `MenuStrip` over `QuickFiler/` → 7 files; the only `.Designer.cs` menu hosts are `ItemViewer`,
    `ItemViewerExpanded` and `EfcViewer`. `QfcFormViewer.Designer.cs` is **absent** from that list, and a
    direct `MenuStrip|ToolStripMenuItem|MainMenuStrip` search of that one file returns **0**.
- **Cross-check Count:** distinct mnemonic letters on the QFC form surface = **1** (`M`).
  Distinct top-level mnemonic-bearing control types = **2**.
- **Member-set Comparison:** normalized primary set `{ItemViewer:"&Move Options", ItemViewerExpanded:"&Move Options"}`
  equals the normalized cross-check set element-for-element. Both counts are 1 letter / 2 control types.
  The two strategies use different anchors (property-assignment syntax vs. bare character class), a
  different file format (`.resx` entity encoding), and a structural non-text query (`MenuStrip`), and they
  agree. **No disagreement.**

**Assertion cleared for spec use:** *exactly one Alt mnemonic letter, `M` ("&Move Options"), is currently
swallowed on the `QfcFormViewer` surface; `Alt+F` has no mnemonic target on that surface and must not be
named in an acceptance criterion.*

### N2 — Compiled consumers of `QfcFormKeyHandler.IsAltKeyCommand`

- **Complete Family:** every source reference to the symbol `IsAltKeyCommand` in a file that
  `QuickFiler/QuickFiler.csproj` compiles.
- **Exhaustive Search Scope:** all `*.cs` in the repository, intersected with the csproj `<Compile Include>`
  item list.
- **Inclusion Rules:** call expressions and the declaration. **Exclusion Rules:** files absent from the
  csproj item list; test-assembly references.
- **Primary Search Strategy:** symbol grep `QfcFormViewer|EfcViewer|QfcFormKeyHandler|Legacy\\` over
  `QuickFiler/QuickFiler.csproj`, cross-referenced against the reads of the four viewer `.cs` files.
- **Primary Member Set:** csproj contains `Controllers\QfcFormKeyHandler.cs` (`:324`),
  `Viewers\QfcFormViewer.cs` (`:452`), `Viewers\EfcViewer.cs` (`:389`). It contains **no**
  `QfcFormViewerDark`, **no** `QfcFormViewerExpanded`, **no** `Legacy\` item. Call sites in compiled files:
  `{QfcFormViewer.cs:60}`.
- **Primary Count:** **1** compiled consumer.
- **Cross-check Search Strategy:** a differently-shaped query — `Dark|Expanded|Legacy` over the same
  csproj, which enumerates every item whose path contains any of those tokens regardless of extension.
- **Cross-check Member Set:** 11 hits, all of which are `ItemViewerExpanded.*`, `QfcItemViewerExpanded.*`
  or `Resources\FlagDarkRed.*`. **No `QfcFormViewerDark`, `QfcFormViewerExpanded`, or `Legacy\` item
  appears.** Therefore the three non-compiled call sites are confirmed non-compiled by a second,
  independent formulation, leaving `{QfcFormViewer.cs:60}`.
- **Cross-check Count:** **1**.
- **Member-set Comparison:** both sets are exactly `{QuickFiler/Viewers/QfcFormViewer.cs:60}`. Counts agree
  at 1. This also agrees with the parallel agent's `git grep`-based determination recorded at
  `evidence/other/call-site-compile-inclusion.md:56`, giving a third concordant formulation.

---

## 8. Scope options — presented, not decided

The orchestrator makes the call. Evidence for and against each.

### Option A — mirror #467 exactly: `internal static bool ClaimsAltChord(IQfcKeyboardHandler, Keys)` on `QfcFormViewer`

**For:**
- Character-for-character precedent, including the `Keys.KeyCode` mask and the folded-in null guard.
- Touches one production file (`QfcFormViewer.cs`, 297 lines — ample headroom under the 500-line limit).
- No `QuickFiler.csproj` edit.
- Leaves `QfcFormKeyHandler.IsAltKeyCommand` and its four passing tests untouched, so no existing test is
  deleted or rewritten.

**Against:**
- `QfcFormViewer` carries `[ExcludeFromCodeCoverage]` (`:17`); the new member emits no Cobertura `<method>`
  element and the ≥ 90% new-method coverage clause cannot be demonstrated by measurement (§6).
- Leaves `IsAltKeyCommand` in the tree as an `internal static` member with **zero compiled consumers**
  (`QfcFormViewer.cs:60` would stop calling it). Whether that trips an unused-member analyzer under
  `/p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` is a plan-time check, not something this
  document verified.
- Leaves `QfcFormKeyHandlerTests.cs:29` still asserting the defective breadth (`Keys.Alt | Keys.Left` →
  `true`) for a predicate nothing compiled calls — a standing invitation to re-adopt the bug.
- A new `QuickFiler.Test/Viewers/QfcFormViewerTests.cs` still needs a `QuickFiler.Test.csproj` line.

### Option B — narrow `QfcFormKeyHandler.IsAltKeyCommand` itself

**For:**
- `QfcFormKeyHandler.cs` carries **no** `[ExcludeFromCodeCoverage]` (§6), so the predicate is *measured*;
  the ≥ 90% clause is demonstrable.
- `QfcFormKeyHandlerTests.cs` is already a csproj item (`QuickFiler.Test.csproj:151`), so extending it
  requires **no project-file edit at all** — a strictly smaller diff than Option A.
- The predicate has exactly one compiled consumer (§7 N2), so the blast radius is one call site.
- The class's own XML comment (`QfcFormKeyHandler.cs:5-9`) already describes it as "Pure routing predicates
  extracted from the QuickFiler form variants' `ProcessCmdKey` overrides", which is precisely the role.
- It fixes the three uncompiled sites too, should they ever be re-included, without editing them.

**Against:**
- Requires deleting or rewriting `IsAltKeyCommand_WithAltPlusOtherKey_ReturnsTrue`
  (`QfcFormKeyHandlerTests.cs:29-39`), which currently pins the defect. Repo policy treats existing tests
  as part of the spec, so this must be argued explicitly as *the test codifies the bug*, with the #467
  precedent as support. (Precedent for exactly this move exists: `research/…-464….md:1312` shows #464
  correcting a prior spec's claim rather than preserving it.)
- The method name `IsAltKeyCommand` would no longer describe what it does; renaming to `ClaimsAltChord`
  diverges from a name three uncompiled files reference (harmless — they are not build inputs, but it does
  leave those files referencing a removed symbol).
- The null-handler check currently sits in the *caller* (`QfcFormViewer.cs:59`). Folding it in, per the
  #467 shape, changes the predicate's signature to take the handler — a larger change than a body edit.

### Option C — new file `QuickFiler/Controllers/QfcFormViewerKeyHandler.cs`

**For:** cleanest coverage story; no existing test rewritten.
**Against:** requires a `QuickFiler.csproj` `<Compile Include>` edit, which #464 deliberately avoided
(`spec.md:630-632`), plus a `QuickFiler.Test.csproj` edit. Strictly dominated by Option B on diff size and
by Option A on precedent fidelity.

### Cross-cutting facts either option must handle

1. **Name the mnemonic correctly.** `M` only. Not `F`. (§4.1, §4.3, §7 N1.)
2. **Pin both bare-Alt shapes.** `Keys.Alt` (key-code `None`) *and* `Keys.Menu | Keys.Alt` (key-code
   `Menu`, the real-keyboard shape). The EFC suite pins only the first (§2.6).
3. **Dead locals at the fix site.** `QfcFormViewer.cs:64-67` computes `object sender = FromHandle(msg.HWnd)`
   and constructs `var e = new KeyEventArgs(keyData)` then sets `e.Handled = true`, but the dispatch at
   `:68` is `_ = _keyboardHandler.ToggleKeyboardDialogAsync()` — the **parameterless** overload. Neither
   `sender` nor `e` is ever read. Whether to remove them is a scope decision; note that #467 *kept* the
   equivalent lines on `EfcViewer` because that site calls the `(object, KeyEventArgs)` overload and
   genuinely needs them (`evidence/qa-gates/467-predicate-structure.md:57-59`). The QFC site does not.
4. **`TaskVisualization/TaskViewer.cs` is a different contract** (§3.4) and should be excluded with a
   stated reason, not silently omitted.
5. **A live-host manual check is required** for the duplicate-mnemonic behaviour described in §4.3, mirroring
   #464's `[P11-T13]`.

---

## 9. Testing implications (strategy only; no test code)

- **Framework/libraries:** MSTest 4.3.3, Moq 4.20.72, FluentAssertions 8.10.0 (§5). Match the
  `EfcViewerTests.cs` shape: `[TestClass]`/`[TestMethod]`, Arrange–Act–Assert, FluentAssertions with a
  because-string on every assertion.
- **No `Form` construction.** `NoLiveFormInTestAssemblyTests.cs:17` fails on any `Form`-derived type in the
  test assembly; the whole point of the extracted-predicate pattern is that no window handle is needed.
- **Scenario matrix the predicate should be pinned against:**
  - positive: `Keys.Alt` (key-code `None`) and `Keys.Menu | Keys.Alt` (key-code `Menu`) with a non-null
    `Mock<IQfcKeyboardHandler>`;
  - negative, mnemonic: `Keys.Alt | Keys.M` — the one real QFC mnemonic (§4.2);
  - negative, vestigial: `Keys.Alt | Keys.Left` (and optionally `Up`/`Down`/`Right`) — this is the
    *inversion* of the currently-passing `QfcFormKeyHandlerTests.cs:29`, and it is the assertion that makes
    the fix visible;
  - negative, non-Alt: `Keys.M` and `Keys.Control`;
  - negative, null handler (only if the null check is folded into the predicate).
- **Structural assertion, optional:** a reflection test asserting `QfcFormViewer.ProcessCmdKey`'s only
  `return true` is guarded by the predicate is not expressible via reflection; #464 instead recorded this
  as a source-inspection QA-gate artifact (`evidence/qa-gates/467-predicate-structure.md`). Recommend the
  same artifact-based approach rather than a brittle source-reading test.
- **Coverage:** if Option B is chosen, the predicate is measured and the ≥ 90% new-method target is
  demonstrable from the Cobertura output. If Option A is chosen, record explicitly that the member sits
  inside a pre-existing `[ExcludeFromCodeCoverage]` type, that no new exemption is added, and that coverage
  is demonstrated by named tests rather than by measurement.
- **Manual validation:** open the QuickFiler form in a live Outlook session, press Alt (dialog toggles —
  unchanged), then press Alt+M (the focused row's Move Options menu opens). Record as a reviewer-performed
  artifact under `evidence/other/`, mirroring #464's `manual-validation.md`.
- **Prohibited:** temporary files; `Thread.Sleep`/`Task.Delay`; any live-host dependency inside a unit test.

---

## 10. Open items this session could not close

| Item | Why |
|---|---|
| Direct `git show 28d244e5 --stat` and its `.cs` diff | Bash/git disabled for the session. File set reported at second hand from #464's own two-base diff artifacts (§1.5). |
| Whether removing `IsAltKeyCommand`'s last compiled caller (Option A) trips an unused-member analyzer | Requires an `msbuild` run; no shell. Plan-time check. |
| Which of the N+2 `"&Move Options"` owners WinForms selects on the first Alt+M | Requires a live host; visibility/enabled state of the Designer templates at runtime is not statically determinable. |
| Whether `TaskViewer.cs:260` discarding the `bool` from `TaskController.KeyboardHandler_KeyDown` (consumed at `:395`) is a live defect | Out of #663's scope and in a different project; recommend a separate promotion. |
