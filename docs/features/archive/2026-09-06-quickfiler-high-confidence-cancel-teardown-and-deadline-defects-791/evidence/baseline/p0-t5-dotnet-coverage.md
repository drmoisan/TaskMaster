# [P0-T5] dotnet-coverage resolution

Timestamp: 2026-09-06T14-26

Command: `dotnet-coverage --version`

EXIT_CODE: 0

BRANCH-TAKEN: probe-only. The first `dotnet-coverage --version` probe exited 0, so the conditional
`dotnet tool install --global dotnet-coverage` branch was not taken and no re-probe was required.
The tool was already installed globally before this plan began.

DOTNET-COVERAGE-VERSION: 18.10.0+f4cc39224845ffa74bf246c9da2399d50e5d6342

Output Summary: The probe printed the single version line recorded above and exited 0. This is the
collector D13 pins for the two coverage runs ([P0-T11] and [P3-T5]), which use
`dotnet-coverage collect --output-format cobertura` rather than `vstest /EnableCodeCoverage`,
because `/EnableCodeCoverage` writes a binary `.coverage` file and the two collectors conflict.
