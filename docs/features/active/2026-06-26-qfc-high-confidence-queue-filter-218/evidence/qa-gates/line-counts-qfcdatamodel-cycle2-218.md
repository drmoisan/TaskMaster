# Line Counts After QfcDatamodel Extraction (Cycle 2) — Issue #218

Timestamp: 2026-06-28T15-34

Command: `$files=@('QuickFiler/Controllers/QfcDatamodel.cs','QuickFiler/Controllers/EmailSorter.cs','QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs','QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs'); foreach($f in $files){ $n=(Get-Content -LiteralPath $f).Count; ... } | Format-Table` (foreach wrapped in subexpression for PowerShell pipe validity; line values are `(Get-Content).Count`).

EXIT_CODE: 0

Output Summary:

| File | Lines | Limit | Result |
|---|---:|---:|---|
| QuickFiler/Controllers/QfcDatamodel.cs | 432 | 500 | PASS |
| QuickFiler/Controllers/EmailSorter.cs | 85 | 500 | PASS |
| QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs | 154 | 500 | PASS |
| QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs | 146 | 500 | PASS |

- `QfcDatamodel.cs` reduced from 790 to 432 lines, now under the 500-line limit. No further BackgroundWorker-block extraction (the P1-T5 fallback) is required.
- All three new files are well under 500 lines.
- Extraction was behavior-preserving: `EmailSorter`/`IEmailSortInfo` moved verbatim to `EmailSorter.cs`; frame-building methods (`InitDf`, `InitDfAsync`, `GetEmailsInViewDfAsync`, `ToggleOfflineMode`, `SortTriageDate`, `MostRecentByConversation`) moved verbatim to the `QfcDatamodel.FrameBuilding.cs` partial; queue-processing methods (`UndoMove`, `TryUnhookOrReplace`, `DequeueNextItemGroupAsync`, `DequeueNextItemGroup`, `WaitForQueue`) moved verbatim to the `QfcDatamodel.QueueProcessing.cs` partial. `[ExcludeFromCodeCoverage]` remains on the single `QfcDatamodel.cs` part only (partial-type attribute applied once).
