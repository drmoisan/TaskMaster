# P3-T12 — physical line counts after the Phase 3 seams

Timestamp: 2026-09-13T15-47

Command: pwsh -NoProfile -Command 'foreach ($f in the five paths below) { (Get-Content -LiteralPath $f).Count }'

EXIT_CODE: 0

Output Summary:
- All five measured counts are strictly less than the repository's 500-line ceiling.
- The largest is `QuickFiler/Controllers/QfcQueue.Tlp.cs` at 329, which gained the four Phase 3
  seams from a starting 255; headroom to the ceiling is 171 lines.
- `QuickFiler/Controllers/QfcQueue.Enqueue.cs` measures 200, the same figure P0-T13 recorded for it
  at the anchor. The background-template substitution in P3-T6 collapsed a three-line expression to
  one line, and the formatter's re-wrap of the item-group statement in P3-T8 expanded one line to
  three, so the two changes cancel.
- Every figure below is a measurement taken after CMD-FORMAT-SCOPED ran in P3-T8. No figure is
  predicted from the plan, the spec or the research.

QuickFiler/Controllers/QfcQueue.Tlp.cs: 329
QuickFiler/Controllers/QfcQueue.Enqueue.cs: 200
QuickFiler/Controllers/QfcQueue.cs: 269
QuickFiler/Controllers/QfcQueue.UiIdle.cs: 108
QuickFiler/Interfaces/IUiIdleDispatcher.cs: 35

Ceiling: 500

| File | Measured | Under 500 | Headroom |
|---|---|---|---|
| `QuickFiler/Controllers/QfcQueue.Tlp.cs` | 329 | yes | 171 |
| `QuickFiler/Controllers/QfcQueue.Enqueue.cs` | 200 | yes | 300 |
| `QuickFiler/Controllers/QfcQueue.cs` | 269 | yes | 231 |
| `QuickFiler/Controllers/QfcQueue.UiIdle.cs` | 108 | yes | 392 |
| `QuickFiler/Interfaces/IUiIdleDispatcher.cs` | 35 | yes | 465 |

The authoritative measurement for the file-size acceptance criterion is taken again after the final
repository-wide format in P5-T6; this record is the Phase 3 checkpoint.
