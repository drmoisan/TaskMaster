# Baseline — Per-File Coverage of the Owned Production Files (P0-T15)

Timestamp: 2026-08-27T20-06

Source: `FF/evidence/baseline/baseline.cobertura.2026-08-27T20-01.xml` (the P0-T14 baseline artifact).

## Counting method

- Every `<class>` element sharing the same `filename` attribute is aggregated into a single row. Async
  state machines and closures are emitted by some collectors as separate `<class>` elements and must
  not be reported separately.
- Line rate = (count of `<line>` elements with `hits > 0`) / (count of `<line>` elements), counted per
  `<line>` element. Cobertura repeats a line number when several `<class>` elements cover it, so the
  per-element count is the denominator this repository's coverage arithmetic uses. It is why a file's
  `<line>` count can exceed its physical line count.
- Branch rate = sum of the numerator over sum of the denominator of each `<line>` element's
  `condition-coverage="P% (n/m)"` attribute.
- Total `<class>` elements in the document: 554.

Observation worth recording for P7-T6: after the Koverage post-processing step this repository's
coverage runner applies, each of the four owned files resolves to **exactly one** `<class>` element
(`DISTINCT_FILENAMES=1`, `CLASSES=1` in every row). The aggregation was still performed as specified,
so the arithmetic is correct whether or not a later run emits multiple elements per file.

## Rows

| File | `<class>` elements | Lines covered / total | Baseline line-rate | Branches covered / total | Baseline branch-rate |
| --- | ---: | ---: | ---: | ---: | ---: |
| `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` | 1 | 428 / 435 | **98.39%** | 170 / 184 | **92.39%** |
| `QuickFiler/Viewers/BreadcrumbCoordinatorUpgradeLifetime.cs` | 1 | 210 / 212 | **99.06%** | 51 / 56 | **91.07%** |
| `QuickFiler/Viewers/BreadcrumbMessengerHub.cs` | 1 | 449 / 449 | **100.00%** | 172 / 176 | **97.73%** |
| `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` | 1 | 504 / 504 | **100.00%** | 147 / 168 | **87.50%** |

Four rows, one per file path required by the task, each with a numeric line-rate and a numeric
branch-rate. No value is the placeholder `UNVERIFIED`.

The `BreadcrumbBridgeCoordinator.cs` row is the baseline for the COMBINED SR-1 split-pair row that
P7-T6 must compute, since P2-T1 relocates members out of this file into
`QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Suggestions.cs`, which has no baseline row by
construction.
