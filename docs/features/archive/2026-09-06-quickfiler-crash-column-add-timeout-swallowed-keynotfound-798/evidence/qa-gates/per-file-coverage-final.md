# P8-T5 — Post-change per-file coverage for the production write set

Timestamp: 2026-09-07T05-46
Task: [P8-T5]
Issue: #798

## Source document

`docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/coverage-final.cobertura.xml`,
the sanitised evidence copy produced by P8-T4. The document contains 7838 `class` elements, up from
the baseline document's 7787.

## Aggregation rule applied

Identical to P0-T10. For each target file, every `class` element whose `filename` attribute ends with
the target suffix was selected, and for each selected element only the `line` elements that are
**direct children of that class element's own `lines` element** were counted. `line` elements nested
under a `method` element duplicate the same source lines and were excluded. A line counts as covered
when its `hits` attribute is not `0`.

The six suffixes matched are repository-relative with **backslash** separators, matching the
`filename` form the sanitised document carries:

```
Extensions\DfDeedle.cs
Extensions\DfDeedle.QfcColumns.cs
Controllers\QfcDatamodel.FrameBuilding.cs
Controllers\QfcDatamodel.cs
Ribbon\RibbonCommandBoundary.cs
Ribbon\RibbonViewer.cs
```

A forward-slash suffix matches no element in this document. It would record all six paths as
`NOT INSTRUMENTED` and would fail the first two clauses of P8-T6 against a correct change.

### Extractor validated against the baseline before use

The same extractor was first run against the P0-T9 baseline document and reproduced the P0-T10
figures exactly: 7787 `class` elements in total, 282 with a `filename` beginning
`QuickFiler\Controllers\`, 13 beginning `TaskMaster\Ribbon\`, and `Extensions\DfDeedle.cs` matching 9
`class` elements with covered=226 valid=239. Reproducing the baseline with the same code that
produces the post-change figures removes counting-method drift as a source of any observed delta.

## Post-change readings

| Path | class elements | covered | valid | rate |
|---|---|---|---|---|
| `UtilitiesCS/Extensions/DfDeedle.cs` | 7 | 157 | 159 | 0.987421383647799 |
| `UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs` | 5 | 164 | 168 | 0.976190476190476 |
| `QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs` | 0 | 0 | 0 | NOT INSTRUMENTED |
| `QuickFiler/Controllers/QfcDatamodel.cs` | 0 | 0 | 0 | NOT INSTRUMENTED |
| `TaskMaster/Ribbon/RibbonCommandBoundary.cs` | 2 | 55 | 61 | 0.901639344262295 |
| `TaskMaster/Ribbon/RibbonViewer.cs` | 0 | 0 | 0 | NOT INSTRUMENTED |

Line-oriented restatement:

- `UtilitiesCS/Extensions/DfDeedle.cs`: covered=157 valid=159 rate=0.987421383647799
- `UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs`: covered=164 valid=168 rate=0.976190476190476
- `QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs`: covered=0 valid=0 NOT INSTRUMENTED
- `QuickFiler/Controllers/QfcDatamodel.cs`: covered=0 valid=0 NOT INSTRUMENTED
- `TaskMaster/Ribbon/RibbonCommandBoundary.cs`: covered=55 valid=61 rate=0.901639344262295
- `TaskMaster/Ribbon/RibbonViewer.cs`: covered=0 valid=0 NOT INSTRUMENTED

## Verification that the three zero readings are attribute-driven, not assembly-driven

The owning assemblies were checked directly in the post-change document, exactly as P0-T10 checked
them at baseline:

- The `QuickFiler` package contributes 282 `class` elements whose `filename` begins with
  `QuickFiler\Controllers\`, unchanged from baseline.
- The `TaskMaster` package contributes 15 `class` elements whose `filename` begins with
  `TaskMaster\Ribbon\`, up from 13 at baseline. The two added elements are the new
  `RibbonCommandBoundary` type and its compiler-generated async state machine.

Both assemblies are instrumented, so the three zero readings are produced by the type-level
`[ExcludeFromCodeCoverage]` attributes on those types — `RibbonViewer` on its own declaration, and
both `QfcDatamodel` partials through the single class-level attribute on the `QfcDatamodel`
declaration — and not by any assembly-level exclusion. P8-T6 states this is the expected outcome for
those three paths and not a defect.

Output Summary: Both new production files are instrumented and measured. `DfDeedle.QfcColumns.cs`
reads covered=164 valid=168 rate=0.976190476190476 and `RibbonCommandBoundary.cs` reads covered=55
valid=61 rate=0.901639344262295, both at or above the 0.90 obligation. `DfDeedle.cs` reads
covered=157 valid=159 after losing lines to the new partial. The three `[ExcludeFromCodeCoverage]`
production files read `NOT INSTRUMENTED` on both sides, with their owning assemblies confirmed
instrumented.
