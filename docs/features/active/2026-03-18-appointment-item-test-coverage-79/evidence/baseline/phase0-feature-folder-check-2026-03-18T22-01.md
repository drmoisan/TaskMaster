Timestamp: 2026-03-18T22:21:20-04:00
Command: pwsh -NoProfile -Command "$feature = 'docs/features/active/2026-03-18-appointment-item-test-coverage-79'; $files = Get-ChildItem $feature -File | Select-Object -ExpandProperty Name | Sort-Object; $blocked = @('spec.md','user-story.md','research.md') | Where-Object { Test-Path (Join-Path $feature $_) }; Write-Output ('Files=' + ($files -join ',')); Write-Output ('BlockedFiles=' + ($(if ($blocked.Count -gt 0) { $blocked -join ',' } else { 'none' }))) ; if ($blocked.Count -gt 0) { exit 1 }"
EXIT_CODE: 0
Output Summary: Files=issue.md,plan.2026-03-18T22-01.md; BlockedFiles=none
