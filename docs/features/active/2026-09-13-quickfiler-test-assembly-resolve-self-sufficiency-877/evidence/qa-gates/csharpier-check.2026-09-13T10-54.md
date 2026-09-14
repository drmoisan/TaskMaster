# Final QC step 2: csharpier check — issue #877

Timestamp: 2026-09-13T10-54
Command: `pwsh -NoProfile -Command 'Set-Location -LiteralPath "C:/Users/DanMoisan/repos/TaskMaster-wt/bug-877-test-isolation"; dotnet tool run csharpier check . 2>&1 | Tee-Object -Variable out | Out-Null; Write-Host "EXIT_CODE=$LASTEXITCODE"; Write-Host "NOT_FORMATTED_LINES=$(($out | Where-Object { $_.ToString() -match [regex]::Escape("Was not formatted") }).Count)"; $out | Select-Object -Last 10'`
EXIT_CODE: 0
Output Summary: Exit code 0. Lines of the captured output containing the token `Was not formatted`: 0. Tool summary line: `Checked 1627 files in 5047ms.` The read-only check confirms the tree is formatted under the manifest-pinned CSharpier 1.2.6, which is the same version `.github/workflows/_format-check.yml` runs. Run under an acquired build lock, released immediately after the command returned.
