# Phase 2 — Seven regression tests added ([P2-T1])

Timestamp: 2026-09-01T22-42

Seven `[TestMethod]`s were added to the existing file
`QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs`, inside the existing `QfcFormKeyHandlerTests`
class in namespace `QuickFiler.Controllers.Tests`. `using Moq;` and `using QuickFiler.Interfaces;` were
added to the file's using block.

Each new test uses MSTest `[TestMethod]`, an explicit Arrange-Act-Assert body, a
`Mock<IQfcKeyboardHandler>` for the handler argument where a non-null handler is required, and a
FluentAssertions because-string on every assertion, following the shape of the delivered Email Filer
fixture at QuickFiler.Test/Controllers/EfcViewerTests.cs lines 112 through 162.

The four existing `IsAltKeyCommand_*` methods were not modified. The class-level XML summary on lines 8
through 11 was not modified: line 9 names `IsAltKeyCommand`, so rewriting it would produce a removed line
containing that identifier and would fail the `[P5-T3]` AC-8 gate. The new methods are documented with
their own per-method comments instead.

No test constructs, shows, or derives from a `System.Windows.Forms.Form`. No test uses a temporary file,
`Thread.Sleep` or `Task.Delay`.

Command: the four `Select-String` measurements transcribed below, each run under
`pwsh -NoProfile -Command`.

EXIT_CODE: 0 for every invocation.

## Acceptance reading 1 — exactly eleven `[TestMethod]` attributes

`Select-String -Path QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs -Pattern '\[TestMethod\]'`

Match count: **11**, exactly eleven as required. Four pre-existing plus seven added.

## Acceptance reading 2 — each of the seven names appears exactly once

| Method name | Match count | Declaration line |
|---|---|---|
| `ClaimsAltChord_WithBareAltFlagAndHandler_ReturnsTrue` | 1 | 79 |
| `ClaimsAltChord_WithMenuKeyCodeAndAltFlag_ReturnsTrue` | 1 | 96 |
| `ClaimsAltChord_WithAltM_ReturnsFalse` | 1 | 113 |
| `ClaimsAltChord_WithAltF4_ReturnsFalse` | 1 | 132 |
| `ClaimsAltChord_WithAltLeft_ReturnsFalse` | 1 | 149 |
| `ClaimsAltChord_WithoutAltFlag_ReturnsFalse` | 1 | 167 |
| `ClaimsAltChord_WithNullHandler_ReturnsFalse` | 1 | 193 |

## Acceptance reading 3 — VC-1 returns zero

`Select-String -Path QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs -Pattern 'new Form|: Form|Thread\.Sleep|Task\.Delay|GetTempFileName|GetTempPath'`

Match count: **0**, zero as required, matching the `[P0-T14]` pre-change reading.

## Acceptance reading 4 — the `Keys.Control` change detector

`Select-String -Path QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs -Pattern 'Keys\.Control'`

Match count: **4**, which is at least two as required. Matched lines:

```
L47:  var keyData = Keys.Control;
L164: // key, and Keys.Control, whose key-code half is Keys.None and which would be accepted by a
L176: Keys.Control
L186: "Keys.Control carries no Alt flag even though its key-code half is Keys.None"
```

`ClaimsAltChord_WithoutAltFlag_ReturnsFalse` is declared on line 167 and the next method's comment block
begins after line 186, so lines 176 and 186 both lie inside that method's body. At least one matched line
therefore lies inside the body, as required. Line 47 is the pre-existing single match inside
`IsAltKeyCommand_WithControlKey_ReturnsFalse`, which `[P0-T14]` recorded as the only match at branch head;
line 164 is the new method's explanatory comment.

That clause is a change detector: at branch head the pattern returned exactly one match, so without it no
acceptance condition in the plan would change value if the second assertion AC-5 requires were omitted.

## The two-input body AC-5 requires

`ClaimsAltChord_WithoutAltFlag_ReturnsFalse` asserts two inputs in a single Arrange-Act-Assert body,
`Keys.M` first and `Keys.Control` second, each with its own because-string:

- `Keys.M` — "a bare letter key carries no Alt flag and is not the dialog gesture"
- `Keys.Control` — "Keys.Control carries no Alt flag even though its key-code half is Keys.None"

This closes AC-1's "every row" claim over the spec's eight-row behaviour table without changing the
eleven-`[TestMethod]` count or the seven-name enumeration.

## The justification wording AC-3 requires

The because-string of `ClaimsAltChord_WithAltM_ReturnsFalse` is:

```
Alt+M is the Move Options mnemonic on the hosted item viewers and must reach the base implementation
```

It names `Move Options`. It does not name a Filters menu: `ButtonFilters.Text` on the QuickFiler surface
is the plain string `"Filters"` with no ampersand, so a Filters-menu justification would be false for
this surface. `[P5-T8]` verifies both halves.

## File size

The file is **207 lines**, within the repository's 500-line limit.

Output Summary: Seven `[TestMethod]`s were added to the existing test file, bringing the total to exactly
eleven. Each of the seven names appears exactly once. VC-1 returns zero matches over the file. The
`Keys.Control` pattern returns four matches, up from the one recorded at branch head, with two of them
inside the body of `ClaimsAltChord_WithoutAltFlag_ReturnsFalse`. All four acceptance readings hold.
