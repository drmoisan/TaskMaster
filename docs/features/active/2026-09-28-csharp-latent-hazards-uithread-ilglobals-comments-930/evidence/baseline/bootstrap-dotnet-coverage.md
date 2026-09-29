# Bootstrap: dotnet-coverage global tool ([P0-T7])

Timestamp: 2026-09-29T08-55
Command: (1) pwsh -NoProfile -Command 'if (-not (Get-Command dotnet-coverage -ErrorAction SilentlyContinue)) { dotnet tool install --global dotnet-coverage }; "INSTALL_STEP_DONE=True"'
Command: (2, separate invocation; its exit is the EXIT_CODE row) pwsh -NoProfile -Command '"DOTNET_COVERAGE_RESOLVED=$([bool](Get-Command dotnet-coverage -ErrorAction SilentlyContinue))"; dotnet-coverage --version'
EXIT_CODE: 0
Output Summary:
- Invocation 1: INSTALL_STEP_DONE=True (the tool was already resolvable, so no install was performed).
- Invocation 2: DOTNET_COVERAGE_RESOLVED=True; version line 18.10.0+f4cc39224845ffa74bf246c9da2399d50e5d6342; probe exit 0.
