# Alt mnemonic inventory on the QuickFiler form surface (issue #663)

Timestamp: 2026-09-01T00-40
Tree: `origin/main` @ `2b85134b42872e405602e6064e02dc9cda6c319b`

Purpose: the issue #663 body asserts that "Alt-key menu mnemonics such as Alt+F and Alt+M are swallowed" on the QuickFiler form surface. That sentence was carried over from the EFC twin. It is not accurate for this surface, and an acceptance criterion written from it would assert a mnemonic that does not exist. This artifact establishes the real inventory before any acceptance criterion is authored.

## The QfcFormViewer form itself carries no menu and no mnemonic

Command: `git grep -n "MenuStrip\|ToolStripMenuItem" -- QuickFiler/Viewers/QfcFormViewer.Designer.cs`
EXIT_CODE: 1
Output Summary: no matches. The form declares no menu strip and no menu item.

Command: `git grep -n "ToolStrip" -- QuickFiler/Viewers/QfcFormViewer.Designer.cs`
EXIT_CODE: 1
Output Summary: no matches, which is the second independent formulation and agrees with the first.

Command: `git grep -n "\.Text = " -- QuickFiler/Viewers/QfcFormViewer.Designer.cs`
EXIT_CODE: 0
Output Summary: 6 assignments, none containing an `&`:

```
this._l1v1L2h5_BtnSkip.Text   = "Skip Group";
this.ButtonFilters.Text       = "Filters";
this._l1v1L2h2_ButtonOK.Text  = "OK";
this._l1v1L2h3_ButtonCancel.Text = "CANCEL";
this._l1v1L2h4_ButtonUndo.Text   = "Undo";
this.Text                     = "Quick File";
```

Note `ButtonFilters.Text` is the plain string `"Filters"`. The EFC surface spells its counterpart `"&Filters"` (`QuickFiler/Viewers/EfcViewer.Designer.cs:4102`). **There is no Alt+F mnemonic on the QuickFiler surface.** The issue body's claim that Alt+F should open a menu here is false, and no acceptance criterion may assert it.

The Designer also uses no `resources.ApplyResources` (`git grep -c "ApplyResources" -- QuickFiler/Viewers/QfcFormViewer.Designer.cs`, EXIT_CODE 1), so no mnemonic can be hiding in the `.resx`. The literal `.Text =` list above is exhaustive.

## The mnemonic lives on the child user controls the form hosts

`QfcFormViewer.Designer.cs:41-42` constructs two child controls and `:179-180` adds them to `_l1v0L2L3v_TableLayout`:

```
this._QfcItemViewerTemplate        = new QuickFiler.ItemViewer();
this._qfcItemViewerExpandedTemplate = new QuickFiler.ItemViewerExpanded();
```

Both are `UserControl` (`QuickFiler/Viewers/ItemViewer.cs:21`, `QuickFiler/Viewers/ItemViewerExpanded.cs:16`), so they are children of the form and their keystrokes traverse the form's `ProcessCmdKey`.

Command: `git grep -n 'Text = "[^"]*&' -- 'QuickFiler/Viewers/*.Designer.cs'`
EXIT_CODE: 0
Output Summary: 16 hits across 3 files. Excluding `EfcViewer.Designer.cs` (the already-fixed EFC surface), the QuickFiler-surface mnemonics are:

| Control | File:line | Text | Chord |
|---|---|---|---|
| `_moveOptionsMenu` | `QuickFiler/Viewers/ItemViewer.Designer.cs:173` | `&Move Options` | **Alt+M** |
| `_conversationMenuItem` | `ItemViewer.Designer.cs:6125` | `Move &Conversation` | C, within the open drop-down |
| `_saveAttachmentsMenuItem` | `ItemViewer.Designer.cs:6133` | `Save &Attachments` | A, within the open drop-down |
| `_saveEmailMenuItem` | `ItemViewer.Designer.cs:6141` | `Save E&mail Copy` | M, within the open drop-down |
| `_savePicturesMenuItem` | `ItemViewer.Designer.cs:6149` | `Save &Pictures` | P, within the open drop-down |
| `MoveOptionsMenu` | `QuickFiler/Viewers/ItemViewerExpanded.Designer.cs:161` | `&Move Options` | **Alt+M** |
| (four drop-down items) | `ItemViewerExpanded.Designer.cs:170,178,186,194` | as above | within the open drop-down |

The top-level item is hosted in a real menu bar, not a context menu:

- `ItemViewer.Designer.cs:43` — `this._moveOptionsStrip = new System.Windows.Forms.MenuStrip();`
- `ItemViewer.Designer.cs:154-155` — `_moveOptionsStrip.Items.AddRange(new ToolStripItem[] { this._moveOptionsMenu });`
- `ItemViewer.Designer.cs:6215` — `internal System.Windows.Forms.MenuStrip _moveOptionsStrip;`

## Determination

**The exactly one top-level Alt chord that the QuickFiler form surface has a mnemonic for is Alt+M (`&Move Options`).** The four drop-down mnemonics (C, A, M, P) are reached only after the drop-down is already open, at which point `ToolStrip` owns the input and the form's `ProcessCmdKey` is no longer the gate; they are not independently swallowed by this defect and must not be listed as separate acceptance criteria.

The defect in issue #663 is therefore real but its stated symptom is half wrong. The corrected statement is:

> `QfcFormViewer.ProcessCmdKey` returns `true` for every chord carrying `Keys.Alt`, which consumes the key before WinForms mnemonic dispatch runs. The Alt+M mnemonic for the `&Move Options` menu on the hosted `ItemViewer` / `ItemViewerExpanded` controls therefore never opens the menu. Alt+F is not a mnemonic on this surface and is not part of the defect.

Every Alt chord other than a bare Alt press is also consumed, including `Alt+F4`, which is the standard window-close chord and is delivered to `ProcessCmdKey` as `WM_SYSKEYDOWN` before the default window procedure can turn it into `WM_SYSCOMMAND`/`SC_CLOSE`.

## Comparison with the EFC precedent

`EfcViewer` carries its own menu bar with `&Filters` (`EfcViewer.Designer.cs:4102`) and `&Move Options` (`:4162`), which is why the #467 tests name Alt+F and Alt+M and why `ClaimsAltChord_WithAltF_ReturnsFalse` states "Alt+F is the Filters menu mnemonic". Copying that test pair verbatim onto the QFC surface would produce a test whose stated justification is false for the surface under test. The QFC fixture must name Alt+M as the real mnemonic and may still cover Alt+F as a generic non-claimed chord, provided its justification does not assert a menu that does not exist.
