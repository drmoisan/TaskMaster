# Changed-File Line-Count Baseline — Cycle 2 (Rebased Tree), Issue #218

Timestamp: 2026-06-28T17-31

Command: `$files=@(<14 files>); $rows = foreach($f in $files){[pscustomobject]@{File=$f;Lines=(Get-Content -LiteralPath $f).Count}}; $rows | Format-Table -AutoSize`

Note: the plan's literal `foreach(...){...} | Format-Table` form is not directly pipeable as a PowerShell statement; the foreach result is captured into `$rows` then piped. Result values are unchanged.

EXIT_CODE: 0

| File | Lines | 500-line limit |
|------|-------|----------------|
| QuickFiler/Controllers/QfcDatamodel.cs | 432 | PASS |
| QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs | 154 | PASS |
| QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs | 146 | PASS |
| QuickFiler/Controllers/EmailSorter.cs | 85 | PASS |
| QuickFiler/Controllers/QfcHomeController.cs | 454 | PASS |
| QuickFiler/Controllers/QfcHomeController.Iteration.cs | 82 | PASS |
| QuickFiler/Controllers/QfcHomeController.Metrics.cs | 226 | PASS |
| QuickFiler/Controllers/QfcRemainingQueueAdmission.cs | 58 | PASS |
| QuickFiler.Test/Controllers/QfcHomeControllerTests.cs | 1370 | FAIL (expected; test split not yet trimmed) |
| QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs | 448 | PASS |
| QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs | 352 | PASS |
| QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs | 241 | PASS |
| QuickFiler.Test/Controllers/QfcHomeControllerPropertyTests.cs | 345 | PASS |
| QuickFiler.Test/Controllers/QfcHomeControllerIssue218Tests.cs | 219 | PASS |

Output Summary: On the rebased tree (HEAD 2637e4c1), all six production split files plus QfcRemainingQueueAdmission and EmailSorter are <=500. The four split test files are <=500. Only QfcHomeControllerTests.cs FAILS at 1370 lines (matches plan anchor) — the expected pre-trim state; Phase 2 trims it. All anchors match the plan exactly.
