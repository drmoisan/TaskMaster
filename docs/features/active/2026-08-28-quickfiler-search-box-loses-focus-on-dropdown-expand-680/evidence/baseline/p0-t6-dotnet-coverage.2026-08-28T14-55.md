# P0-T6 — dotnet-coverage Global Tool Provisioning (Issue #680)

Timestamp: 2026-08-28T14-57

Command: `if (-not (Get-Command dotnet-coverage -ErrorAction SilentlyContinue)) { dotnet tool install --global dotnet-coverage }`
followed by `Get-Command dotnet-coverage` and `dotnet-coverage --version`

EXIT_CODE: 0

Output Summary:

- The guard found `dotnet-coverage` already on PATH, so no install was performed.
- `Get-Command dotnet-coverage` succeeded (`FOUND`).
- `dotnet-coverage --version` printed `18.5.2+6e39b75eaf98f2691cf62dbf259669cc13851fd3`.
- `scripts/vscode/Invoke-MSTestWithCoverage.ps1` throws before running anything when this tool is
  absent; the precondition for P0-T10 and P6-T4 is therefore satisfied.

Acceptance: satisfied — `Get-Command dotnet-coverage` succeeds.
