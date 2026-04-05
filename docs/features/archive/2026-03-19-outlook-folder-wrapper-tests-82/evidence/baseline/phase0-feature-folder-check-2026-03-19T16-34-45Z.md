Timestamp: 2026-03-19T16:34:45.0479797Z
Command: pwsh -NoProfile -Command "$feature = 'docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82'; $required = @('issue.md','user-story.md','spec.md','plan.2026-03-19T09-43.md'); $missing = $required | Where-Object { -not (Test-Path (Join-Path $feature $_)) }; Write-Output ('MissingRequired=' + ($(if ($missing.Count -gt 0) { $missing -join ',' } else { 'none' }))); if ($missing.Count -gt 0) { exit 1 }"
EXIT_CODE: 0
Output Summary: MissingRequired=none
