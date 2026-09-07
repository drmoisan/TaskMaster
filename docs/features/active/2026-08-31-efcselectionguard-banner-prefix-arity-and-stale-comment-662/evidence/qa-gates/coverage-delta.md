# Coverage Delta — Baseline vs Post-Change (P2-T10)

Timestamp: 2026-09-01T16-47

Command: comparison of the two captured Cobertura documents —
`evidence/baseline/coverage-baseline.cobertura.xml` (P0-T13) and
`evidence/qa-gates/coverage-postchange.cobertura.xml` (P2-T9).

EXIT_CODE: 0

Output Summary:

## PostProcessed discriminator — the precondition for comparing at all

- `PostProcessed:` recorded by P0-T13: **yes**
- `PostProcessed:` recorded by P2-T9: **yes**

The two values agree, so the gate is not BLOCKED on this precondition. Had they
differed, the comparison would be meaningless: the raw document's root
attributes are computed over all instrumented modules while the post-processed
document's are recomputed over the first-party package allowlist only, so the
two figures would measure different denominators and their difference would not
be a coverage delta.

## Repository aggregate line percentage (recorded, NOT gated)

| Capture | `lines-covered` | `lines-valid` | Line percentage |
|---|---|---|---|
| Baseline (P0-T13) | 54967 | 64382 | 85.38 |
| Post-change (P2-T9) | 54969 | 64386 | 85.37 |

Signed difference: **-0.01 percentage points**.

This figure is recorded but is **not gated**, exactly as this task specifies:
the aggregate is a full-suite figure whose denominator this plan has not
measured across repeated runs, and for which no allowance value has an
evidential basis. The denominator moved from 64382 to 64386, a rise of 4
executable lines, which is consistent with the new test method's four assertion
lines entering the measured set; the numerator rose by 2. The -0.01 point
movement is arithmetic on a shifted denominator, not a coverage regression on
any changed line.

## Class half of the gate

For each filename, every `<class>` node whose `filename` ends with that name,
ordered by the node's `name` attribute, with its `line-rate`:

### `EfcSelectionGuard.cs`

| Capture | `name` | `line-rate` |
|---|---|---|
| Baseline | `QuickFiler.Controllers.EfcSelectionGuard` | 1 |
| Post-change | `QuickFiler.Controllers.EfcSelectionGuard` | 1 |

### `FolderSuggestionTree.cs`

| Capture | `name` | `line-rate` |
|---|---|---|
| Baseline | `UtilitiesCS.FolderSuggestionTree` | 0.9844961240310077 |
| Post-change | `UtilitiesCS.FolderSuggestionTree` | 0.9849624060150376 |

**Class-half verdict: PASS.**

- The two captures carry the same set of `name` values for each filename. For
  `EfcSelectionGuard.cs` the set is `{QuickFiler.Controllers.EfcSelectionGuard}`
  in both; for `FolderSuggestionTree.cs` it is
  `{UtilitiesCS.FolderSuggestionTree}` in both. No `name` is present in one
  capture and absent from the other, so the BLOCKED branch for a set mismatch
  does not arise.
- For every `name` present in both, the post-change `line-rate` is not lower
  than the baseline `line-rate`: `EfcSelectionGuard` holds at exactly 1, and
  `FolderSuggestionTree` rises from 0.9844961240310077 to 0.9849624060150376.
- Neither filename is recorded as `NOT APPLICABLE` in either capture, so the
  BLOCKED branch for a 0/0 denominator does not arise.

## Changed-code coverage

The three changed executable statements, identified by their enclosing member
rather than by a fixed line number, and resolved to their post-format line spans
from the files as they stand after P2-T1:

| Statement | Enclosing member | Identified by | Post-format span |
|---|---|---|---|
| A | `EfcSelectionGuard.IsValidFilingSelection` | the `return` reading `StartsWith(BannerRejectionPrefix` | `EfcSelectionGuard.cs:72-73` |
| B | `EfcSelectionGuard.IsValidCreationSelection` | the `return` reading `StartsWith(BannerRejectionPrefix` | `EfcSelectionGuard.cs:97-99` |
| C | `FolderSuggestionTree.IsBanner` | the `return` reading `BreadcrumbRowBuilder.BannerPrefix` | `FolderSuggestionTree.cs:196-200` |

All three resolved spans are recorded above. Each span runs from the line
carrying the `return` keyword through the line carrying that statement's
terminating semicolon. Statement B's span begins on the line carrying the
minimum-length comparison, because the renamed call site is that statement's
second operand and so the statement's first line is not the line this change
touches. Statement C's span is five lines rather than one because CSharpier
wrapped the reader in P2-T1.

Every line element whose `number` falls inside each span, with its `hits` value,
read from the post-change capture:

**Statement A — `EfcSelectionGuard.cs:72-73`** — 4 elements: line 72 hits 1,
line 73 hits 1, line 72 hits 1, line 73 hits 1. At least one element carries
`hits` greater than zero, so the statement is **covered**.

**Statement B — `EfcSelectionGuard.cs:97-99`** — 6 elements: line 97 hits 1,
line 98 hits 1, line 99 hits 1, line 97 hits 1, line 98 hits 1, line 99 hits 1.
**Covered**.

**Statement C — `FolderSuggestionTree.cs:196-200`** — 10 elements: lines 196,
197, 198, 199, 200 each hits 1, listed twice. **Covered**.

Each line number appears twice because the Cobertura document carries each
`<line>` element both under the class-level `<lines>` collection and under the
enclosing `<method>`'s own `<lines>` collection.

No span contains zero line elements, so the BLOCKED branch for an empty span
does not arise for any of the three.

**Changed-code coverage figure: 3/3, that is 100.00%.**

## Gate verdict

**PASS.** The changed-code figure is `3/3` as the gate requires, and the class
half of the gate passes as defined in the three closing sentences of P2-T10: the
two captures carry the same `name` set for each filename, every shared `name`'s
post-change `line-rate` is not lower than its baseline value, and neither
filename is `NOT APPLICABLE` in either capture.
