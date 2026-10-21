# Baseline 01: CSharpier check, read-only ([P0-T9])

Timestamp: 2026-09-29T08-53
Command: pwsh -NoProfile -Command 'dotnet tool run csharpier check . 2>&1 | Tee-Object -FilePath coverage/930-format-check.log; "CHECK_EXIT=$LASTEXITCODE"'
EXIT_CODE: 0
Output Summary:
- Summary line: Checked 1623 files in 6594ms.
- CHECK_EXIT=0
- BASELINE-DRIFT: NONE
