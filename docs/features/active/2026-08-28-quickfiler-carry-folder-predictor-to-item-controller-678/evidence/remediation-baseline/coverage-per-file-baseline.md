# Baseline — Per-file coverage of the five production files this cycle touches

- Timestamp: 2026-09-02T01-09
- Issue: #678
- Task: [P0-T10]
- Derivation: D3, issued in the same `pwsh` session as D1 and D2 (P0-T9)

## Non-vacuity control

`@($doc.SelectNodes('//class[@filename]')).Count` = **561**

The control is an integer greater than zero, so the `NOT PRESENT IN REPORT` row below is a
genuine absence from the report and not the signature of a derivation that ran with an
unassigned `$doc`. Per the plan's own rule, no `NOT PRESENT IN REPORT` row may be accepted
while this control reads zero; it reads 561.

## Per-file rows

D3 emits `filename|CoveredLines|TotalLines` using
`Get-CoberturaClassLineSummary`, which deduplicates the class-level rollup against the
method-level view. Counting `.//line` directly would double-count every source line and is
not used. `Merge-CoberturaClassesByFilename` has already merged async state-machine classes
into one entry per file in this post-processed document, so there is one row per file.
Cobertura `filename` values carry native separators.

| Path (as reported) | Covered | Total | Line % |
|---|---|---|---|
| `QuickFiler\Controllers\QfcHighConfidencePreFilter.cs` | 44 | 44 | 100.00% |
| `QuickFiler\Controllers\QfcQueue.Enqueue.cs` | 28 | 100 | 28.00% |
| `QuickFiler\Controllers\QfcHomeController.cs` | 179 | 232 | 77.16% |
| `QuickFiler\Controllers\QfcDatamodel.QueueProcessing.cs` | NOT PRESENT IN REPORT | — | — |
| `QuickFiler\Controllers\QfcItemController.FolderHandling.cs` | 165 | 172 | 95.93% |

## Reason for the `NOT PRESENT IN REPORT` row

`QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` is a partial part of the
`QfcDatamodel` class, and that class carries a class-level `[ExcludeFromCodeCoverage]`
attribute at `QuickFiler/Controllers/QfcDatamodel.cs:25`:

```csharp
    [ExcludeFromCodeCoverage]
    public partial class QfcDatamodel : IQfcDatamodel
```

The attribute applies to the whole partial class, so no `class` element is emitted for any
of its parts and the file has no row. This is the expected absence the plan names. This
cycle's only edit to that file is P1-T4, which rewrites an XML documentation block and
changes no executable line, so the exemption is neither widened nor narrowed by this cycle
and no `[ExcludeFromCodeCoverage]` attribute is added or removed.

## Raw D3 rows, verbatim

```
D3|QuickFiler\Controllers\QfcHighConfidencePreFilter.cs|44|44
D3|QuickFiler\Controllers\QfcHomeController.cs|179|232
D3|QuickFiler\Controllers\QfcItemController.FolderHandling.cs|165|172
D3|QuickFiler\Controllers\QfcQueue.Enqueue.cs|28|100
```

## Output Summary

Four of the five paths have a row: `QfcHighConfidencePreFilter.cs` 44/44 (100.00%),
`QfcQueue.Enqueue.cs` 28/100 (28.00%), `QfcHomeController.cs` 179/232 (77.16%),
`QfcItemController.FolderHandling.cs` 165/172 (95.93%). `QfcDatamodel.QueueProcessing.cs` is
`NOT PRESENT IN REPORT` because its class carries `[ExcludeFromCodeCoverage]`. Non-vacuity
control 561.
