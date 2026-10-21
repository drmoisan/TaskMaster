# Final 02: CSharpier check, read-only ([P2-T2])

Timestamp: 2026-09-29T09-18
Command: pwsh -NoProfile -Command 'dotnet tool run csharpier check . 2>&1 | Tee-Object -FilePath coverage/930-format-check.log; "CHECK_EXIT=$LASTEXITCODE"'
EXIT_CODE: 0
Iteration: 1
Output Summary:
- Summary line: Checked 1623 files in 5899ms.
- CHECK_EXIT=0
- No unformatted path reported (the baseline [P0-T9] drift list was NONE, and none is reported now).
