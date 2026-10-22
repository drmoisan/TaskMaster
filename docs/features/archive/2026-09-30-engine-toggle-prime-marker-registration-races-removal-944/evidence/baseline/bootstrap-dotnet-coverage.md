# Bootstrap dotnet-coverage (P0-T12)

Timestamp: 2026-09-30T13-21
Command: pwsh -NoProfile -Command (if dotnet-coverage does not resolve, dotnet tool install --global dotnet-coverage; print DOTNET_COVERAGE_RESOLVED; dotnet-coverage --version)
EXIT_CODE: 0
Output Summary: dotnet-coverage already resolved (no install performed). DOTNET_COVERAGE_RESOLVED=True. Version line: 18.10.0+f4cc39224845ffa74bf246c9da2399d50e5d6342 (exit 0).

## Observed

- Install performed: no (already on PATH)
- DOTNET_COVERAGE_RESOLVED=True
- dotnet-coverage --version: 18.10.0+f4cc39224845ffa74bf246c9da2399d50e5d6342
- VERSION_EXIT=0
