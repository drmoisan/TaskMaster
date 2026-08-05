# SVGControl coverage uplift

- Date captured: 2026-08-05
- Author: Dan Moisan
- Status: Potential — not promoted
- Origin: **issue #418** (`svg-renderer-null-document-nre`), remediation cycle 1, task `[P1-T18]`
- Origin feature folder: `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418`
- Measurement source: `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/evidence/qa-gates/coverage-delta.2026-08-05T01-50.md`

## Summary

The `SVGControl` assembly carries a large block of production code that no test exercises. Issue #418's
remediation deliberately did **not** close it: R-4 in
`remediation-inputs.2026-08-04T20-25.md` is bounded to two targeted items and carries an explicit scope
boundary forbidding an attempt at the 85% modified-file floor in that cycle. This entry owns the residual
so it is tracked rather than lost.

**The specific gap this entry owns is the `>= 85%` modified-file line-coverage floor on
`SVGControl/SvgRenderer.cs`.** None of the members and files enumerated below is part of issue #418.

## Residual on `SVGControl/SvgRenderer.cs`

Figures are Cobertura `<method>` line counts measured at issue #418 head, before this cycle's additions.

| Member | Covered / Valid | Note |
|---|---|---|
| `AddMargins(int, int)` | **0 / 15** | unreferenced helper, pre-existing |
| `Render()` | **18 / 26** | pre-existing partial |
| `.ctor(SvgDocument, Size, AutoSize)` | **0 / 8** | pre-existing |
| `.ctor(SvgDocument, Size, Padding, AutoSize)` | **0 / 8** | pre-existing |

Writing tests for these four is what reaching 85% on the file would require. Two smaller pre-existing
residuals in the same file, recorded for completeness: `get_Margin()` 0/1 and
`AdjustSizeProportionately(Size, Size)` 22/23.

## Residual in the rest of the `SVGControl` assembly

All measured at **0.000%** both before and after issue #418's change, and all untouched by it. These are
the dominant drag on the `SVGControl` package rate (47.0857% at issue #418 head).

| File / type | Covered / Valid |
|---|---|
| `DropDownEditor` | **0 / 99** |
| `SVGParser` | **0 / 122** |
| `ToggleSwitch` | **0 / 62** |
| `ToggleSwitch` designer | **0 / 23** |
| `SvgFileNameEditor` | **0 / 104** |
| Converter 1 (`SvgOptionsConverter`) | **0 / 48** |
| Converter 2 (`SvgOptionsConverter2`) | **0 / 48** |
| Converter 3 (`SvgResourceConverter`) | **0 / 26** |

Total uncovered in this group: **532 lines.**

## Why it was deferred rather than absorbed

1. **Scope.** Issue #418 is a `minor-audit` bug fix for a `NullReferenceException` in one parse path.
   Every member above is pre-existing untested code that the fix did not touch, so covering it would widen
   a bug fix into a coverage project.
2. **No regression was introduced.** Issue #418 *improved* `SVGControl/SvgRenderer.cs` from 62.559% to
   72.109% and, after this cycle's R-4 and R-6 work, to a higher figure still; no changed line lost
   coverage in either cycle.
3. **The enforced gates already pass.** Repository-wide line and branch coverage both clear their floors
   (`>= 85%` and `>= 75%`) and both improved. The modified-file floor is the one that does not clear, and
   it is the gap this entry owns.
4. `WinForms`-derived and designer-generated types (`ToggleSwitch` and its designer, the editors) may
   qualify for the `CLAUDE.md` COM/VSTO/WinForms coverage exemption. That determination has **not** been
   made and requires maintainer ratification; it is part of the work this entry proposes, not an
   assumption it makes.

## Proposed approach when promoted

1. Classify each file above as testable-seam versus WinForms/designer-exempt per `CLAUDE.md`
   § General Unit Test Policy, and obtain maintainer ratification for any claimed exemption. Note that
   `.claude/rules/general-unit-test.md` § Coverage Exclusion Policy forbids excluding a production source
   path from measurement, so the remedy for an untestable file is extraction of its logic, not exclusion.
2. Cover the four pure or near-pure `SvgRenderer` members first (`AddMargins`, `Render()`, the two
   `SvgDocument` constructor overloads). These need no new seam and are the cheapest path to the 85%
   modified-file floor on that file.
3. Cover `SVGParser` next: 122 lines, and parsing logic is the most likely to be pure and directly
   testable.
4. Treat `DropDownEditor`, `SvgFileNameEditor`, and `ToggleSwitch` last; each will need a seam or an
   extraction before it is unit-testable at all.

## Acceptance ideas (for the promoted entry to refine)

- [ ] `SVGControl/SvgRenderer.cs` reaches `>= 85%` line coverage.
- [ ] Every `SVGControl` production file is either covered or has a maintainer-ratified, documented
      exemption; none is excluded from measurement.
- [ ] Repository-wide line coverage stays `>= 85%` and branch coverage `>= 75%`.
- [ ] No existing assertion is weakened and no test is deleted to reach any figure.
