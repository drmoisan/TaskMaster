# P2-T12 — Documentation-token re-verification after formatting

Timestamp: 2026-09-02T01-42

This task runs **after P2-T1**, because a formatter pass is the only step that could move a
token onto a second line. CSharpier does not reflow comment text, but the check is made
against the post-format tree rather than assumed.

## Search method

Each count is an **occurrence** count, taken by repeated ordinal `String.IndexOf` over the
file's full text, not a matching-line count. Both figures are reported so a token that landed
twice on one line, or once across two lines, would be visible as a disagreement between them.
The equivalent command shape is:

```powershell
$text = [System.IO.File]::ReadAllText($Path)
# repeated $text.IndexOf($Token, $i, [System.StringComparison]::Ordinal)
@(Select-String -LiteralPath $Path -Pattern $Token -SimpleMatch).Count   # matching lines
```

## The eight required counts

| # | File | Token / literal | Required | Occurrences | Matching lines | Result |
|---|---|---|---|---|---|---|
| 1 | `QuickFiler/Controllers/QfcHomeController.cs` | `#678 R1` | exactly 1 | **1** | 1 | PASS |
| 2 | `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` | `#678 R1` | exactly 1 | **1** | 1 | PASS |
| 3 | `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` | `#678 R2` | exactly 1 | **1** | 1 | PASS |
| 4 | `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` | `#678 R3` | exactly 1 | **1** | 1 | PASS |
| 5 | `QuickFiler/Controllers/QfcQueue.Enqueue.cs` | `#678 R1a` | exactly 1 | **1** | 1 | PASS |
| 6 | `QuickFiler/Controllers/QfcQueue.Enqueue.cs` | `#678 R1b` | exactly 1 | **1** | 1 | PASS |
| 7 | `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` | `describe one dequeue rather than two` | exactly 0 | **0** | 0 | PASS |
| 8 | `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` | `A null or empty archive root` | exactly 0 | **0** | 0 | PASS |

In every one of the six positive cases the occurrence count equals the matching-line count,
which is what establishes that each token sits wholly on a single line and was not split by
the formatter.

## Why the two zero-count clauses are falsifiable

Neither is a search for a literal that was never present. Both were present exactly once
before this cycle edited the file, so each count genuinely moved from 1 to 0:

- `describe one dequeue rather than two` was on one line of the pre-P1-T4
  `QfcDatamodel.QueueProcessing.cs` doc block.
- `A null or empty archive root` was on one line of the pre-P1-T8
  `QfcItemController.FolderHandling.cs` doc block.

## Supporting counts, recorded for completeness

| File | Token | Occurrences | Note |
|---|---|---|---|
| `QuickFiler/Controllers/QfcQueue.Enqueue.cs` | `#678 R1` | 2 | **not a required clause.** These two occurrences are the prefixes of the single `#678 R1a` and the single `#678 R1b`. The plan deliberately asserts no `#678 R1` count in this file so that the shared prefix creates no confound; rows 5 and 6 assert the two distinct suffixed tokens instead. |
| `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs` | `ReferenceEquals` | 1 | the identity-first matching clause DR1 requires; asserted by P1-T3, re-checked here as unmoved by the formatter |
| `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` | `cancel.ThrowIfCancellationRequested();` | 1 | the R3 guard; asserted by P1-T9, re-checked here |

## Output Summary

All eight required counts hold after formatting: six tokens at exactly 1 occurrence on exactly
1 line each, and two superseded literals at exactly 0. No token was split across lines by the
CSharpier pass.
