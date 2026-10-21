# P0-T8 dotnet-coverage global tool

Timestamp: 2026-09-29T08-54
Command: pwsh -NoProfile -Command 'if (-not (Get-Command dotnet-coverage -ErrorAction SilentlyContinue)) { dotnet tool install --global dotnet-coverage }; "DOTNET-COVERAGE=" + [bool](Get-Command dotnet-coverage -ErrorAction SilentlyContinue)'
EXIT_CODE: 0
Output Summary:
- DOTNET-COVERAGE=True (the tool was already resolvable; no install output was printed).
