Timestamp: 2026-09-09T10-22
Command: pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs/features/active/2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read-813/evidence/qa-gates/coverage-post-change.cobertura.xml
EXIT_CODE: 0
Output Summary: Test Run Successful. Total tests: 7196. Passed: 7196. Failed: 0. Total time:
39.4826 seconds. The 7196 total (baseline was 7195) includes the new
AssignFolderComboBox_WhenArchiveRootPathThrows_DegradesToIndexFallbackWithoutThrowing test, which
passed. Repo-wide Cobertura coverage from produced XML: line-rate = 0.856063 (85.6063%),
branch-rate = 0.797939 (79.7939%), lines-covered = 55966, lines-valid = 65376, branches-covered =
13474, branches-valid = 16886.
