# Baseline Test Coverage (Remediation: issue-96 2026-03-26T15-25)

Timestamp: 2026-03-26T15:42:00Z

Command: pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug

EXIT_CODE: 0

## Output Summary

Test Run Successful. Total tests: 3409, Passed: 3407, Skipped: 2. Full repository coverage run completed.

### QuickFiler Package Coverage (Baseline)

- **Package line-rate: 21.54%** (0.21542270958613371)
- **Package branch-rate: 8.08%** (0.08082497212931995)

### Issue #96 Touched Files Coverage

| File | Line Rate |
|------|-----------|
| QuickFiler\Controllers\QfcItemController.cs | 8.22% (0.082208) |
| QuickFiler\Controllers\KbdActions.cs | 26.42% (0.264151) |
| QuickFiler\Controllers\KeyboardHandler.cs | 0.00% (0) |

### Context: Other QuickFiler Files

| File | Line Rate |
|------|-----------|
| QuickFiler\Controllers\QfcHomeController.cs | 60.60% (0.605965) |
| QuickFiler\Controllers\QfcCollectionController.cs | 3.33% (0.033292) |
| QuickFiler\Controllers\QfcFormController.cs | 40.67% (0.406707) |
| QuickFiler\Controllers\EfcHomeController.cs | 5.84% (0.058355) |
