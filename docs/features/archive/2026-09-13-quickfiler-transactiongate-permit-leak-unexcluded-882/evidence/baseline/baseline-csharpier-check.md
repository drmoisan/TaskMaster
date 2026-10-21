# Baseline CSharpier Check (P0-T11)

Timestamp: 2026-09-29T09-01
Command: pwsh -NoProfile -Command 'Set-Location "<repo-root>"; dotnet tool run csharpier check . 2>&1 | Tee-Object -FilePath coverage/logs/baseline-csharpier-check.log | Out-Null; "EXIT=$LASTEXITCODE"; Get-Content coverage/logs/baseline-csharpier-check.log | Select-String -Pattern "^Checked |^Error |^Warning " | ForEach-Object { $_.Line }'
EXIT_CODE: 0
Output Summary:
- CHECKED-LINE: Checked 1623 files in 5864ms.
- BASELINE-DRIFT-FILES: NONE
- No `Error ` or `Warning ` line was printed; neither Write Set C# file is unformatted at baseline.
