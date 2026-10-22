# Baseline — dotnet-coverage Availability (Issue #656)

Timestamp: 2026-09-01T14-37
Task: [P0-T6]

Command:
```
dotnet-coverage --version
```

EXIT_CODE: 0

Results:

- `dotnet-coverage` resolved on the first attempt; no install was required, so the conditional
  `dotnet tool install --global dotnet-coverage` branch of this task was not taken.
- Reported version: `18.10.0+f4cc39224845ffa74bf246c9da2399d50e5d6342`.

This check is required because `scripts/vscode/Invoke-MSTestWithCoverage.ps1` throws when
`dotnet-coverage` is absent, which would surface as a test-gate failure rather than as the missing
prerequisite it actually is.

Output Summary: Bootstrap satisfied. The coverage collector is present and reports version 18.10.0.
This is a bootstrap step, not a toolchain gate.
