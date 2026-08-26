# [P0-T8] Cobertura Collector Availability

Timestamp: 2026-08-26T08-44

Task: [P0-T8]
Feature: docs/features/active/quickfiler-bug-family-446

## Invocation 1 — availability probe

Command: `pwsh -NoProfile -Command 'dotnet-coverage --version; exit $LASTEXITCODE'`
EXIT_CODE: 0
Output: `18.5.2+6e39b75eaf98f2691cf62dbf259669cc13851fd3`

## Invocation 2 — install

Not required. The first invocation resolved the tool and exited 0, so the conditional
`dotnet tool install --global dotnet-coverage` branch of this task was not taken and no
second invocation was made.

## Output Summary

`dotnet-coverage` is already installed and on `PATH` for this session, reporting version
`18.5.2+6e39b75eaf98f2691cf62dbf259669cc13851fd3` with exit code 0.
`scripts/vscode/Invoke-MSTestWithCoverage.ps1`, used by `[P0-T12]` and `[P5-T6]`, requires this
tool and its requirement is satisfied.
