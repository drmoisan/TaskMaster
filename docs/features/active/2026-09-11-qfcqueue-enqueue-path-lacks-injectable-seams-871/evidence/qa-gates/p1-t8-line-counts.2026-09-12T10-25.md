# P1-T8 — Measured line counts of the three Phase 1 files

Timestamp: 2026-09-13T15-21
Command: (Get-Content -LiteralPath <P>).Count for each of the three repository-relative paths below
EXIT_CODE: 0
Output Summary: All three measured counts are strictly less than the repository's 500-line ceiling.
The measurement was taken after the scoped format in P1-T4, so it reflects formatter-final content.

## Measured counts (CMD-LINECOUNT)

- QuickFiler/Controllers/QfcQueue.cs = 251
- QuickFiler/Controllers/QfcQueue.Tlp.cs = 255
- QuickFiler/Controllers/QfcQueue.UiIdle.cs = 64

## Ceiling check

| Path | Measured | Ceiling | Strictly under |
|---|---|---|---|
| QuickFiler/Controllers/QfcQueue.cs | 251 | 500 | yes |
| QuickFiler/Controllers/QfcQueue.Tlp.cs | 255 | 500 | yes |
| QuickFiler/Controllers/QfcQueue.UiIdle.cs | 64 | 500 | yes |

Every figure above is a measurement taken from the tree by the command named in this artifact. No
predicted figure from the spec or the research was used, as this task's text requires. The base part
stood at 507 at the recorded anchor, seven lines over the ceiling; it now stands at 251.
