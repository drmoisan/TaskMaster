# Phase 0 — Baseline per-file coverage for the production write set

Timestamp: 2026-09-07T01-01
Task: [P0-T10]
Issue: #798

## Source document

`docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/baseline/coverage-baseline.cobertura.xml`,
the evidence copy produced by P0-T9. The document contains 7787 `class` elements across 23 packages.

## Aggregation rule applied

For each target file, every `class` element whose `filename` attribute ends with the target suffix
was selected, and for each selected element only the `line` elements that are **direct children of
that class element's own `lines` element** were counted. `line` elements nested under a `method`
element duplicate the same source lines and were excluded. A line counts as covered when its `hits`
attribute is not `0`.

The `filename` attributes in the evidence copy are repository-relative with backslash separators, so
the suffixes below match as written.

## Existing production files

- `UtilitiesCS/Extensions/DfDeedle.cs`: matched 9 `class` elements; covered=226 valid=239
- `QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs`: matched 0 `class` elements; covered=0 valid=0 NOT INSTRUMENTED
- `QuickFiler/Controllers/QfcDatamodel.cs`: matched 0 `class` elements; covered=0 valid=0 NOT INSTRUMENTED
- `TaskMaster/Ribbon/RibbonViewer.cs`: matched 0 `class` elements; covered=0 valid=0 NOT INSTRUMENTED

## Verification that the three zero readings are attribute-driven, not assembly-driven

A `NOT INSTRUMENTED` reading would be misleading if the whole owning assembly had been excluded from
measurement, so the owning assemblies were checked directly:

- The `QuickFiler` package is present in the document and contributes 282 `class` elements whose
  `filename` begins with `QuickFiler\Controllers\`, including sibling controller files such as
  `QuickFiler\Controllers\EfcDataModel.cs`.
- The `TaskMaster` package is present and contributes 13 `class` elements whose `filename` begins
  with `TaskMaster\Ribbon\`, including `TaskMaster\Ribbon\EngineGatedCommandRunner.cs`.

Both assemblies are therefore instrumented, and the three zero readings are produced by the
type-level `[ExcludeFromCodeCoverage]` attributes on those types: `RibbonViewer` on its own
declaration, and both `QfcDatamodel` partials through the single class-level attribute on the
`QfcDatamodel` declaration. This is the outcome P8-T6 states is expected, and it is not a defect.

## Files absent at baseline

- `UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs`: ABSENT AT BASELINE
- `TaskMaster/Ribbon/RibbonCommandBoundary.cs`: ABSENT AT BASELINE

Both are created by this change, in P1-T1 and P1-T5 respectively.

## Consequences for the P8-T6 comparison

- The relocation adjustment applies to `UtilitiesCS/Extensions/DfDeedle.cs`. Its baseline `covered=`
  value is 226. P8-T6's fourth clause requires the sum of the post-change `covered=` values for
  `UtilitiesCS/Extensions/DfDeedle.cs` and `UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs` to be at
  or above 226.
- For `QuickFiler/Controllers/QfcDatamodel.cs`,
  `QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs` and `TaskMaster/Ribbon/RibbonViewer.cs`,
  both sides are expected to read `NOT INSTRUMENTED`, in which case P8-T6 records `NOT APPLICABLE`
  for each rather than a numeric comparison.

Output Summary: `UtilitiesCS/Extensions/DfDeedle.cs` reads covered=226 valid=239 at baseline. The
three `[ExcludeFromCodeCoverage]` production files read covered=0 valid=0 and are recorded as
NOT INSTRUMENTED, with their owning assemblies confirmed instrumented so the reading is attribute
driven. The two new production files are recorded as ABSENT AT BASELINE.
