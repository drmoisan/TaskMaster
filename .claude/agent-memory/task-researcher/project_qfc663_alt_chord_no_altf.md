---
name: qfc663-alt-chord-no-altf
description: Issue #663 (QFC twin of #467) - QfcFormViewer has NO menus and NO '&' mnemonic of its own; the only swallowed mnemonic is Alt+M from hosted ItemViewer/ItemViewerExpanded. Alt+F is an EFC-only chord.
metadata:
  type: project
---

**#663 correctness trap: naming `Alt+F` in the QFC spec would be false.**

`QuickFiler/Viewers/QfcFormViewer.Designer.cs` contains **zero** `MenuStrip`/`ToolStripMenuItem` and
**zero** `&`+letter. Its `ButtonFilters.Text = "Filters"` (`:113`) carries no mnemonic. The only Alt
mnemonic swallowed on the QFC surface is **`M`**, contributed by the hosted user controls:
`ItemViewer._moveOptionsMenu` = `"&Move Options"` (`ItemViewer.Designer.cs:173`) and
`ItemViewerExpanded.MoveOptionsMenu` (`ItemViewerExpanded.Designer.cs:161`), plus one per
queue-manufactured row (`Helper Classes/ItemViewerQueue.cs:105`). `ProcessCmdKey` bubbles up the whole
hierarchy, so the form-level override intercepts them all. QFC also assigns **no** `MainMenuStrip`
(EFC does, `EfcViewer.Designer.cs:4224`), so QFC's bare-Alt claim costs nothing.

**Why the #464 spec left the shared predicate alone.** `464/spec.md:487` + `:629-634` +
`research/…-464….md:562`: the recorded reason is purely *file-ownership scope* ("`QfcFormKeyHandler.cs`
is not in 464's owned set"), **not** a technical judgement that the shared predicate should stay broad.
Nothing in #464 argues against narrowing it.

**Coverage asymmetry that drives the scope call.** `QfcFormViewer` (`:17`), `EfcViewer` (`:20`),
`QfcFormViewerDark` (`:16`), `QfcFormViewerExpanded` (`:16`) all carry `[ExcludeFromCodeCoverage]`;
`QfcFormKeyHandler.cs` does **not**. So the #467-mirror placement is unmeasured, while narrowing
`QfcFormKeyHandler` is measured **and** needs no csproj edit (`QfcFormKeyHandlerTests.cs` is already at
`QuickFiler.Test.csproj:151`). Cost: it requires rewriting the bug-pinning test
`IsAltKeyCommand_WithAltPlusOtherKey_ReturnsTrue` (`QfcFormKeyHandlerTests.cs:29`).

**Test gap inherited from #467:** `EfcViewerTests.cs:112-162` pins only `Keys.Alt` (key code `None`),
never `Keys.Menu | Keys.Alt` — the shape a real keyboard produces (`Keys.Menu` == 18 == "The ALT key";
`Keys.KeyCode` == 65535). Pin both.

**Dead locals at the QFC fix site:** `QfcFormViewer.cs:64-67` builds `sender` and a `KeyEventArgs` and
sets `e.Handled`, but `:68` calls the **parameterless** `ToggleKeyboardDialogAsync()`. Both are unread.
EFC kept the equivalent lines legitimately because it calls the `(object, KeyEventArgs)` overload.

**Why: Getting Alt+F into a spec AC would produce an untestable, false acceptance criterion, and
choosing the #467 mirror placement silently forfeits the >= 90% new-method coverage demonstration.**

**How to apply:** For any Alt-mnemonic work on a QuickFiler form, enumerate mnemonics on the *hosted
user controls*, not just the form's own Designer; and check `[ExcludeFromCodeCoverage]` on the
candidate predicate host before choosing placement. Related: [[qfc680-menu-mode-keyboard-capture]],
[[qfc677-webview2-focus-hold-outlook-keyboard]], [[qfc438-search-focus-steal]].
