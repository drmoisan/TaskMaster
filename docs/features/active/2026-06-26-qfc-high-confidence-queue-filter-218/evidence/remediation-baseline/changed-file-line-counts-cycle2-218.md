# Changed-File Line-Count Baseline (Cycle 2) — Issue #218

Timestamp: 2026-06-28T15-34

Command: `$files=@('QuickFiler/Controllers/QfcDatamodel.cs','QuickFiler/Controllers/QfcHomeController.cs','QuickFiler/Controllers/QfcRemainingQueueAdmission.cs','QuickFiler.Test/Controllers/QfcHomeControllerTests.cs','QuickFiler.Test/Controllers/QfcDatamodelTests.cs'); $(foreach($f in $files){[pscustomobject]@{File=$f;Lines=(Get-Content -LiteralPath $f).Count}}) | Format-Table -AutoSize`

Note: the plan's literal `foreach(...){...} | Format-Table` form is not pipeable as a PowerShell statement; the foreach was wrapped in a `$(...)` subexpression to produce the equivalent deterministic line count. Result values are unchanged.

EXIT_CODE: 0

Output Summary:

| File | Lines | Limit | Result |
|---|---:|---:|---|
| QuickFiler/Controllers/QfcDatamodel.cs | 790 | 500 | FAIL |
| QuickFiler/Controllers/QfcHomeController.cs | 739 | 500 | FAIL |
| QuickFiler/Controllers/QfcRemainingQueueAdmission.cs | 58 | 500 | PASS |
| QuickFiler.Test/Controllers/QfcHomeControllerTests.cs | 1370 | 500 | FAIL |
| QuickFiler.Test/Controllers/QfcDatamodelTests.cs | 148 | 500 | PASS |

- Three files exceed the 500-line limit (QfcDatamodel.cs, QfcHomeController.cs, QfcHomeControllerTests.cs) and are the targets of cycle-2 extraction.
- QfcRemainingQueueAdmission.cs (cycle 1 extraction) and QfcDatamodelTests.cs are compliant.

## Phase 3 Test-Preservation Baseline

Command: `(Select-String -Path 'QuickFiler.Test/Controllers/QfcHomeControllerTests.cs' -Pattern '^\s*\[TestMethod\]').Count`

EXIT_CODE: 0

Pre-split active `[TestMethod]` count in `QuickFiler.Test/Controllers/QfcHomeControllerTests.cs`: **30** (matches the plan anchor value 30). Counted active (non-commented) `[TestMethod]` attributes only. This is the conservation target for Phase 3: the sum of `[TestMethod]` attributes across `QfcHomeControllerTests.cs` and the four new split files must equal 30 after the split.
