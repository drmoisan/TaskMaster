# P0-T7 — dotnet-coverage global tool

Timestamp: 2026-09-30T09-18
Command: pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; if (-not (Get-Command dotnet-coverage -ErrorAction SilentlyContinue)) { dotnet tool install --global dotnet-coverage }; (Get-Command dotnet-coverage).Source'
EXIT_CODE: 0
Output Summary:
- Get-Command resolved dotnet-coverage to <user-profile>\.dotnet\tools\dotnet-coverage.exe (already installed; no install was run).
- The coverage runner's absent-tool guard (Invoke-MSTestWithCoverage.ps1 lines 344 to 346) will not throw.
