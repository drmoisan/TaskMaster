# [P0-T6] dotnet-coverage Global Tool Verification

Timestamp: 2026-09-08T09-16
Command: `dotnet-coverage --version`
EXIT_CODE: 0
Output Summary: The `dotnet-coverage` global tool is already installed and resolvable from this worktree. The probe printed a single version line and exited 0, so no installation was required.

DOTNET-COVERAGE-VERSION: 18.10.0+f4cc39224845ffa74bf246c9da2399d50e5d6342
BRANCH-TAKEN: probe-only

## Note

`dotnet-coverage` is a global tool and is not listed in the repository-root `dotnet-tools.json` manifest, so `dotnet tool restore` at [P0-T5] did not supply it. The probe succeeded on this workstation, so `dotnet tool install --global dotnet-coverage` was NOT run.

This is the collector D11 pins for both sides of the coverage comparison: `dotnet-coverage collect --output-format cobertura` wrapping `vstest.console.exe`, rather than `vstest /EnableCodeCoverage`.
