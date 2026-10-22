# Bootstrap: dotnet-coverage Global Tool (P0-T8)

Timestamp: 2026-10-01T17-37
Task: P0-T8
Command: pwsh -NoProfile -Command (guarded) dotnet tool install --global dotnet-coverage when unresolved; dotnet-coverage --version
EXIT_CODE: 0

Output Summary:
- INSTALL_RAN=False (the tool was already resolvable on the host)
- DOTNET_COVERAGE_RESOLVED=True
- dotnet-coverage --version: 18.10.0+f4cc39224845ffa74bf246c9da2399d50e5d6342
- VERSION_EXIT=0
- Result: P0-T8 acceptance holds.
