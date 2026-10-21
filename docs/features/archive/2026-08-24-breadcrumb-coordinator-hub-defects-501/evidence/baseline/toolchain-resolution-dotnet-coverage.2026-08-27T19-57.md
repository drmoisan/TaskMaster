# Toolchain Resolution — dotnet-coverage (P0-T10)

Timestamp: 2026-08-27T19-57

Command: `dotnet-coverage --version`

EXIT_CODE: 0

Output Summary: `18.5.2+6e39b75eaf98f2691cf62dbf259669cc13851fd3` — a non-empty version string.

`scripts/vscode/Invoke-MSTestWithCoverage.ps1` throws when `dotnet-coverage` is absent from `PATH`,
which would make every coverage acceptance in this plan (P0-T14, P0-T15, P7-T5, P7-T6) unreachable.
The collector is present and resolvable from `WS` under `pwsh -NoProfile`.

Acceptance: `EXIT_CODE: 0` and a non-empty version string. PASS.
