# Remediation QA loop: tests with coverage (P4-T5, #1002 cycle 1)

Timestamp: 2026-10-22T13-32
Command: pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot .
EXIT_CODE: 0
Output Summary:
- Test run outcome Completed: total 7597, executed 7597, passed 7597, failed 0 (error, timeout, aborted, notExecuted 0).
- Baseline (P0-T3) passed 7579; delta +18 = 8 (EfcFormControllerTests) + 7 (BreadcrumbMessagesIssue1002Tests) + 3 (BreadcrumbHtmlRendererIssue1002Tests). The plan's "19 (9 + 7 + 3)" figure counted 9 for Phase 1; the Phase 1 task list defines 8 tests.
- First-party coverage: lines 57541/66677 (86.30%), branches 14036/17419 (80.58%). Floors 80% line, 75% branch.
- No raw trx or Cobertura committed.
