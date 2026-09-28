# [P0-T6] dotnet-coverage global tool presence

- Issue: #792
- Timestamp: 2026-09-17T18-38
- Command: `if (-not (Get-Command dotnet-coverage -ErrorAction SilentlyContinue)) { dotnet tool install --global dotnet-coverage }` then `Get-Command dotnet-coverage` and `dotnet-coverage --version` (run under `pwsh -NoProfile` with the item worktree as the working directory)
- EXIT_CODE: 0
- Output Summary: `Get-Command dotnet-coverage` succeeded (the tool was already installed; no install ran). `dotnet-coverage --version` printed `18.10.0+f4cc39224845ffa74bf246c9da2399d50e5d6342`.

## Observations

- DOTNET-COVERAGE-PRESENT: true
- DOTNET-COVERAGE-VERSION: 18.10.0+f4cc39224845ffa74bf246c9da2399d50e5d6342
- The resolved tool path is not recorded because it carries the account name.
