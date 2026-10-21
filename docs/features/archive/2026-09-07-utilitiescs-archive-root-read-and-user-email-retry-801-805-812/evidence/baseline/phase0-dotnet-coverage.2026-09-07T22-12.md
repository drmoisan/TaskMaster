# Phase 0 — dotnet-coverage Global Tool Availability (P0-T6)

Timestamp: 2026-09-08T06-37

Command: `pwsh -NoProfile -Command 'if (-not (Get-Command dotnet-coverage -ErrorAction SilentlyContinue)) { dotnet tool install --global dotnet-coverage }'`

EXIT_CODE: 0

Output Summary: The guard found `dotnet-coverage` already resolvable on this machine, so the install branch did not execute and the guarded command produced no output. Because the guarded branch did not run, `$LASTEXITCODE` was left unset by that invocation; availability was therefore confirmed directly by a separate read-only invocation.

- `dotnet-coverage --version` prints `18.10.0+f4cc39224845ffa74bf246c9da2399d50e5d6342` and exits 0.

`dotnet-coverage` is a global tool and is not listed in `dotnet-tools.json`, so the P0-T5 `dotnet tool restore` does not supply it. It is required by the Cobertura conversions in P0-T12 and P6-T6.
