# Phase 5 — Justification wording of the mnemonic test ([P5-T8])

Timestamp: 2026-09-01T23-34

Command 1: `Select-String -Path QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs -Pattern 'Move Options'`
Command 2: `Select-String -Path QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs -Pattern 'Filters menu'`

EXIT_CODE: 0 for both.

## Acceptance reading 1 — `Move Options` is named, inside the right method body

Match count: **2**, which is at least one as required. Matched lines:

```
L115: // ItemViewerExpanded controls each carry a "&Move Options" menu item.
L129: "Alt+M is the Move Options mnemonic on the hosted item viewers and must reach the base implementation"
```

Line 129 is the FluentAssertions because-string argument of
`ClaimsAltChord_WithAltM_ReturnsFalse`. The method boundaries were measured in the same task rather than
assumed:

```
L117: public void ClaimsAltChord_WithAltM_ReturnsFalse()
L136: public void ClaimsAltChord_WithAltF4_ReturnsFalse()
```

`ClaimsAltChord_WithAltM_ReturnsFalse` is declared on line 117 and the next test method is declared on
line 136, so line 129 lies inside the former's body. At least one matched line therefore lies inside the
body of `ClaimsAltChord_WithAltM_ReturnsFalse`, as required. Line 115 is that method's explanatory
comment, immediately above its `[TestMethod]` attribute.

## Acceptance reading 2 — no Filters-menu justification

Match count for `Filters menu`: **0**, zero as required.

The because-string must not name a Filters menu. `ButtonFilters.Text` on the QuickFiler surface is the
plain string `"Filters"` with no ampersand, per QuickFiler/Viewers/QfcFormViewer.Designer.cs line 113, so
a Filters-menu justification would state something false for this surface. The Email Filer twin does carry
a `"&Filters"` caption, which is why its own fixture names Alt+F as the Filters mnemonic; copying that
wording here would have been incorrect.

The QuickFiler fixture instead covers Alt+F4 as a generic non-claimed system chord, whose because-string
names the window-close behaviour and asserts no menu.

Output Summary: The because-string of `ClaimsAltChord_WithAltM_ReturnsFalse` names `Move Options`, on line
129 inside that method's body which spans lines 117 through 135, and the file contains zero occurrences of
`Filters menu`. AC-3's justification-wording requirement holds.
