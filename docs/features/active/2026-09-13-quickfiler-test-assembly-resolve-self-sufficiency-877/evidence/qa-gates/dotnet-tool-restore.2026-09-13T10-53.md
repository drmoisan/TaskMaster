# Final QC step 0: dotnet tool restore — issue #877

Timestamp: 2026-09-13T10-53
Command: `pwsh -NoProfile -Command 'Set-Location -LiteralPath "C:/Users/DanMoisan/repos/TaskMaster-wt/bug-877-test-isolation"; dotnet tool restore; Write-Host "EXIT_CODE=$LASTEXITCODE"'`
EXIT_CODE: 0
Output Summary: `Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier` followed by `Restore was successful.` The manifest-pinned CSharpier version 1.2.6 is available for the format and check steps that follow. Run under an acquired build lock, released immediately after the command returned.
