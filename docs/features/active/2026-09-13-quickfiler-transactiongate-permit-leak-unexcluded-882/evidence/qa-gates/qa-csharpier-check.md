# QA CSharpier Check, Repository-Wide (P4-T2)

Timestamp: 2026-09-29T09-13
Command: pwsh -NoProfile -Command 'Set-Location "<repo-root>"; dotnet tool run csharpier check . 2>&1 | Tee-Object -FilePath coverage/logs/qa-csharpier-check.log | Out-Null; "EXIT=$LASTEXITCODE"; Get-Content coverage/logs/qa-csharpier-check.log | Select-String -Pattern "^Checked |^Error |^Warning " | ForEach-Object { $_.Line }'
EXIT_CODE: 0
ITERATION: 1
Output Summary:
- CHECKED-LINE: Checked 1623 files in 6099ms.
- DRIFT-FILES: NONE
- No `Error ` or `Warning ` line was printed; the file count equals the P0-T11 baseline (1623).
