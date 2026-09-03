# Phase 4 — Coverage Delta and New-Code Figures (P4-T8)

Timestamp: 2026-09-03T03-20
Task: [P4-T8]
Command: XML aggregation over both Cobertura documents, plus `git diff --unified=0 (git merge-base origin/main HEAD) -- TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` to derive the added line numbers.
EXIT_CODE: 0
Merge base re-derived at run time: `a679cd082819af6788cd0fb35f4366786fab87e3`

Documents compared:

- Baseline: `evidence/baseline/coverage-baseline.2026-09-02T12-04.cobertura.xml`
- Final: `evidence/qa-gates/coverage-final.2026-09-02T12-04.cobertura.xml`

Both are post-processed Koverage-compatible Cobertura, so the comparison is on equal terms.

## Method

For each file, select every `class` element whose `filename` attribute, normalised to backslash
separators, ends with that file's path; collect every `.//line` descendant of those elements;
deduplicate by the `number` attribute, keeping the maximum `hits`; count a line as covered when its
`hits` is greater than zero; derive the rate from the sums. Deduplication is necessary because
compiler-generated async state-machine and display classes can emit `line` entries for the same
physical source line, and summing without it double-counts.

This is the same method the P0-T9 baseline artifact recorded, so the two figures are produced
identically.

## Required rows

### Row 1 — `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs`, baseline versus final

| | `class` elements | Covered | Total | Line coverage |
|---|---|---|---|---|
| Baseline | 1 | 133 | 135 | **98.52%** |
| Final | 1 | 143 | 143 | **100.00%** |

**Final is at or above baseline: yes.** 100.00% versus 98.52%, an improvement of 1.48 percentage
points. The two lines that were uncovered at baseline are now covered, and the eight instrumented
lines the change added are all covered.

### Row 2 — `TaskMaster/Ribbon/SpamManagerResetGate.cs`, the new-module rule

| | `class` elements | Covered | Total | Line coverage |
|---|---|---|---|---|
| Baseline | 0 | — | — | NOT APPLICABLE — file did not exist at baseline |
| Final | 1 | 33 | 33 | **100.00%** |

**At or above 90%: yes.** 100.00% against a 90% floor. Every line of the gate class is covered by
the nine tests in its fixture; there is no uncovered line to enumerate. This row is the evidence for
F2-AC6.

### Row 3 — coverage of the lines this change added to the coordinator

Derived by intersecting the added line numbers from the anchored unified-zero diff with the covered
line set of the final document.

| Quantity | Value |
|---|---|
| Added lines in the diff | 41 |
| Of those, instrumented (a `line` element exists) | 18 |
| Of the instrumented, covered | **18** |
| Of the instrumented, uncovered | **0** |
| **Added-line coverage** | **18 / 18 = 100.00%** |

Uncovered added lines, enumerated by number: **none**. The list is empty.

The 41-to-18 gap is not uncovered code: the other 23 added lines are XML-doc comment lines, blank
lines, braces and the declaration lines that the instrumenter does not emit a `line` element for.
Only executable lines are instrumented, and every executable line this change added to the
coordinator is covered.

### Row 4 — `TaskMaster/Ribbon/RibbonController.Intelligence.cs`

| | `class` elements matched |
|---|---|
| Baseline | 0 |
| Final | 0 |

**ABSENT — pre-existing type-level ExcludeFromCodeCoverage on the containing type.**

No `class` element matches this file in either document, so no number can be read and none is
invented. **No coverage credit is claimed for this file.** It is a partial of `RibbonController`,
which carries a pre-existing, already-ratified type-level `[ExcludeFromCodeCoverage]`. This change
adds no new exemption attribute and widens no existing one, which P4-T9 verifies independently. The
roughly ten residual lines inside `ClearSpamManagerAsync` are validated by the manual-verification
dossier instead of by a coverage claim.

## Additional row — the branch B extraction

Not required by the plan, but recorded because P4-T3 branch B created a new production module and
the repository's new-module rule applies to it.

| | `class` elements | Covered | Total | Line coverage |
|---|---|---|---|---|
| `TaskMaster/Ribbon/EngineTogglePressedStateCache.cs`, final | 1 | 37 | 39 | **94.87%** |

**At or above 90%: yes.**

The two uncovered lines are enumerated: **line 109** and **line 127**. Both are compare-and-swap
retry paths:

- Line 109 is the `continue` reached only when a concurrent writer wins the `TryAdd` race for a key
  that was absent a moment earlier.
- Line 127 is the loop-back reached only when a concurrent writer wins the `TryUpdate` comparand
  check.

Each requires a genuine thread collision inside a compare-and-swap window a few instructions wide.
Forcing one deterministically would require a sleep, a spin or a real race, all of which the
repository unit-test policy prohibits in unit tests. The surrounding logic — first write, newer
ticket, older ticket, equal ticket and per-key independence — is covered by the nine tests in
`EngineTogglePressedStateCacheTests.cs`.

## Repository-wide movement

| Attribute | Baseline | Final | Movement |
|---|---|---|---|
| `line-rate` | 0.853867 | 0.854109 | +0.000242 |
| `branch-rate` | 0.794649 | 0.794984 | +0.000335 |
| `lines-covered` | 55141 | 55225 | +84 |
| `lines-valid` | 64578 | 64658 | +80 |

No regression on either rate; both improved.

## Placeholder check

Every required row above carries either a numeric value or the explicit absence marker required for
that row. No row carries `UNVERIFIED` or any other placeholder.

Output Summary: `EngineToggleStateCoordinator.cs` moved from 98.52% to 100.00%, at or above baseline.
`SpamManagerResetGate.cs` is at 100.00%, meeting the 90% new-module rule. Added-line coverage on the
coordinator is 18 of 18 instrumented lines, 100.00%, with no uncovered added line to enumerate.
`RibbonController.Intelligence.cs` is ABSENT in both documents under a pre-existing type-level
exemption, with no coverage credit claimed. The branch B cache class is at 94.87%, above 90%, with
its two uncovered lines identified as the compare-and-swap retry paths.
