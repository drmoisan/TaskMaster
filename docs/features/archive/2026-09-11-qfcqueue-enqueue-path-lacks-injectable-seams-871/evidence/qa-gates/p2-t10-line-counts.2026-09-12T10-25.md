# P2-T10 — Measured line counts of the four Phase 2 files

Timestamp: 2026-09-13T15-33
Command: (Get-Content -LiteralPath <P>).Count for each of the four repository-relative paths below
EXIT_CODE: 0
Output Summary: All four measured counts are strictly less than the repository's 500-line ceiling. The
measurement was taken after the scoped format in P2-T6, so it reflects formatter-final content.

## Measured counts (CMD-LINECOUNT)

- QuickFiler/Controllers/QfcQueue.cs = 269
- QuickFiler/Controllers/QfcQueue.Enqueue.cs = 200
- QuickFiler/Controllers/QfcQueue.UiIdle.cs = 108
- QuickFiler/Interfaces/IUiIdleDispatcher.cs = 35

## Ceiling check

| Path | Measured | Ceiling | Strictly under |
|---|---|---|---|
| QuickFiler/Controllers/QfcQueue.cs | 269 | 500 | yes |
| QuickFiler/Controllers/QfcQueue.Enqueue.cs | 200 | 500 | yes |
| QuickFiler/Controllers/QfcQueue.UiIdle.cs | 108 | 500 | yes |
| QuickFiler/Interfaces/IUiIdleDispatcher.cs | 35 | 500 | yes |

Movement since the preceding measurement in P1-T8: the base part rose from 251 to 269, the eighteen
added lines being seam S1 and its documentation comment. The UI-idle part rose from 64 to 108, the
forty-four added lines being seam S2 with its documentation comment and the adapter class with its
own, against three former method bodies collapsed into three one-line forwards. The enqueue part is
unchanged at 200, its single substituted line being neither an addition nor a removal. The interface
file is new at 35.

The authoritative measurement for the file-size acceptance criterion is taken again in P5-T6, after
the repository-wide format of the final QC loop.
