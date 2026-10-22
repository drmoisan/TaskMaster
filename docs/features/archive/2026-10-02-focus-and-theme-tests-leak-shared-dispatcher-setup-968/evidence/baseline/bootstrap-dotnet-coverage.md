# Bootstrap: dotnet-coverage global tool (issue #968, task P0-T8)

Timestamp: 2026-10-03T02-43
Command: pwsh -NoProfile -Command 'if (-not (Get-Command dotnet-coverage -ErrorAction SilentlyContinue)) { dotnet tool install --global dotnet-coverage }; "DOTNET_COVERAGE_RESOLVED=$($null -ne (Get-Command dotnet-coverage -ErrorAction SilentlyContinue))"; dotnet-coverage --version'
Canonical command: dotnet tool install --global dotnet-coverage (guarded), then dotnet-coverage --version
EXIT_CODE: 0
Output Summary:
- WORKTREE-LEAF: not applicable (the command touches no repository path; no PREFIX)
- DOTNET_COVERAGE_RESOLVED=True (already installed; the guarded install did not run)
- 18.10.0+f4cc39224845ffa74bf246c9da2399d50e5d6342
