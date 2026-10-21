# Phase 0 — baseline per-file coverage of the touched paths (P0-T11)

Timestamp: 2026-09-01T22-12

Derivation: D3 over the post-processed `coverage/coverage.cobertura.xml`, using
`Get-CoberturaClassLineSummary`, which deduplicates the class-level rollup against the method-level
view. `.//line` was not counted directly, because that double-counts every source line.
`Merge-CoberturaClassesByFilename` has already merged async state-machine classes into one entry per
file in a post-processed document, so D3 yields one row per file.

Cobertura `filename` values carry native (backslash) separators after `ConvertTo-KoverageRelativePath`.
The paths are written below with forward slashes to match the plan's spelling; the lookup was
performed against the backslash form.

## Per-file covered-over-total

| # | Path | Covered | Total | Line % |
|---:|---|---:|---:|---:|
| 1 | QuickFiler/Controllers/QfcHighConfidencePreFilter.cs | 35 | 35 | 100.00 |
| 2 | QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs | 112 | 115 | 97.39 |
| 3 | QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs | NOT PRESENT IN REPORT | — | — |
| 4 | QuickFiler/Controllers/QfcHomeController.cs | 170 | 223 | 76.23 |
| 5 | QuickFiler/Controllers/QfcHomeController.Iteration.cs | 60 | 60 | 100.00 |
| 6 | QuickFiler/Controllers/QfcItemGroup.cs | 10 | 11 | 90.91 |
| 7 | QuickFiler/Controllers/QfcCollectionController.cs | NOT PRESENT IN REPORT | — | — |
| 8 | QuickFiler/Controllers/QfcQueue.cs | 158 | 381 | 41.47 |
| 9 | QuickFiler/Controllers/QfcItemController.cs | 73 | 73 | 100.00 |
| 10 | QuickFiler/Controllers/QfcItemController.Initialization.cs | 245 | 258 | 94.96 |
| 11 | QuickFiler/Controllers/QfcItemController.FolderHandling.cs | 141 | 148 | 95.27 |
| 12 | QuickFiler/Controllers/QfcItemController.ViewerSetup.cs | 189 | 209 | 90.43 |

All twelve paths listed by P0-T11 have a row. Ten carry a covered-over-total figure; two carry
`NOT PRESENT IN REPORT` with the reason recorded below.

## Reason for the two `NOT PRESENT IN REPORT` rows

- **`QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs`** is a partial part of `QfcDatamodel`,
  which carries `[ExcludeFromCodeCoverage]` at `QuickFiler/Controllers/QfcDatamodel.cs:25`. The
  attribute is applied at the class level and therefore suppresses instrumentation of every partial
  part of that class, so no `class` node with this `filename` exists in the report. Its absence is
  the expected consequence of a ratified exemption, not a measurement gap.
- **`QuickFiler/Controllers/QfcCollectionController.cs`** carries `[ExcludeFromCodeCoverage]` at
  `QuickFiler/Controllers/QfcCollectionController.cs:21`, immediately above the class declaration at
  `:22`. Same mechanism.

Lines this change adds to either of those two classes cannot be pinned by a coverage figure. The
plan's coverage-threshold reconciliation section names the tests that pin their behaviour instead,
and P2-T7 lists each new or modified member in an exempt class as exempt together with the named
test that pins it.

## Notes for the P2-T7 comparison

- `QfcQueue.cs` at 41.47 % is the lowest of the ten measured paths. P1-T6 moves `EnqueueAsync` and
  `LoadControllersViewersAsync` out of it into a new partial part. Because the two files are
  compared per file, a reduction in `QfcQueue.cs`'s figure that is explained by relocating covered
  or uncovered lines into the new part is a line deletion in that file, and P2-T7 must state it as
  such rather than as a regression. The new part carries its own row.
- The same applies to `QfcCollectionController.cs`, except that it is exempt and has no row on
  either side.
- `QfcHighConfidencePreFilter.cs` is at 100 % over 35 measured lines. The 35 lines are the
  non-exempt surface of that file: `QfcHighConfidencePreFilter.FilterAsync`, `QfcPreScoredItem` and
  `IFolderScoringService`. `FolderScoringService` in the same file is exempt and contributes no
  measured line, which is why the total is 35 rather than the file's 191 lines.
