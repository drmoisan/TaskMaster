# Phase 0 — `dotnet-coverage` global tool presence ([P0-T7])

Timestamp: 2026-09-01T21-56

`scripts/vscode/Invoke-MSTestWithCoverage.ps1` lines 292 and 293 throw before running anything when
`dotnet-coverage` is absent, and it is a global tool that `dotnet tool restore` does not supply.

Command: `pwsh -NoProfile -Command 'if (-not (Get-Command dotnet-coverage -ErrorAction SilentlyContinue)) { dotnet tool install --global dotnet-coverage }'`

EXIT_CODE: 0

Output: none. The command printed nothing, which means the `Get-Command` probe resolved `dotnet-coverage`
and the guarded `dotnet tool install --global dotnet-coverage` branch did not execute. The tool was
already present on this host.

## Acceptance reading

Command: `dotnet-coverage --version`

EXIT_CODE: 0

Output, verbatim:

```
18.10.0+f4cc39224845ffa74bf246c9da2399d50e5d6342
```

Output Summary: `dotnet-coverage` is present and resolvable. The guarded install was a no-op because the
tool was already installed. `dotnet-coverage --version` printed the version string
`18.10.0+f4cc39224845ffa74bf246c9da2399d50e5d6342` and exited 0, so the acceptance condition of `[P0-T7]`
holds and `Invoke-MSTestWithCoverage.ps1` will not throw at its lines 292-293 presence check.
