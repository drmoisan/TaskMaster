# P4-T10 — File-size audit after the final format (AC11)

Timestamp: 2026-09-01T20-19
Command: `foreach ($p in @('QuickFiler/Controllers/QfcItemController.WebViewFaultBoundary.cs','QuickFiler/Controllers/QfcItemController.Initialization.cs','QuickFiler/Controllers/QfcItemController.ViewerSetup.cs','QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs','QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs')) { [pscustomobject]@{ Path = $p; Lines = (Get-Content -LiteralPath $p).Count } }`
EXIT_CODE: 0

## Measured counts

| File | Lines | Ceiling | Required | Holds |
| --- | --- | --- | --- | --- |
| `QuickFiler/Controllers/QfcItemController.WebViewFaultBoundary.cs` | 41 | 500 | at most 500 | yes |
| `QuickFiler/Controllers/QfcItemController.Initialization.cs` | 489 | 500 | **exactly 489** | yes |
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` | 499 | 500 | **exactly 499** | yes |
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs` | 498 | 500 | at most 500 | yes |
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs` | 261 | 500 | at most 500 | yes |

**All five counts are at or below the 500-line ceiling**, so AC11 passes.

The two exact-value conditions both hold:

- `Initialization.cs` is **exactly 489**, unchanged from the P0-T8 baseline measurement. This is the direct consequence of the three call-site substitutions being net-zero-line replacements with no `#670` comment added at any site. Had a comment been added, this count would have risen and every line citation in the plan downstream of the first insertion point would have shifted.
- `ViewerSetup.cs` is **exactly 499**, also unchanged from baseline, corroborating from a second direction the zero-changed-lines finding recorded in P2-T4 and required by AC8. A file with zero changed lines must have an unchanged line count, and it does.

## Timing of the measurement

These counts are taken **after** the P4-T1 repo-wide format, which is the measurement that matters: the 500-line ceiling in `.claude/rules/general-code-change.md` applies to the file as committed, and a formatter can move a count in either direction. Measuring before the final format would leave open the possibility that the committed file exceeds the ceiling even though a pre-format draft did not.

P4-T1 rewrote no file under either directory, so these counts are identical to the P3-T13 measurements. That is a confirmation rather than a redundancy: it is what establishes that the final format did not push any file over the ceiling.

## Note on `Part3.cs`

`Part3.cs` sits at 498, two lines below the ceiling and exactly 100 lines above its 398-line baseline. Reaching that figure required compacting XML documentation comments after the first drafts brought the file to 510 lines, which is recorded in full in `evidence/qa-gates/p3-t13-test-file-sizes.md`. No test was relocated to achieve it, because AC4, AC5 and AC6 each name `Part3.cs` specifically and relocating any of the three would have falsified its own acceptance criterion.

Base-ref note: this task states no `git` command. The re-anchored base used throughout this delivery run is `988d35a8f8eb7436cc46a9f6424db917ed93807a`, replacing the plan-pinned `2b85134b42872e405602e6064e02dc9cda6c319b`, which is a stale ancestor rather than the current merge base.
