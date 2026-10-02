# Bootstrap: dotnet-coverage global tool (P0-T8)

Timestamp: 2026-10-02T00-49
Command: pwsh -NoProfile -Command 'if (-not (Get-Command dotnet-coverage -ErrorAction SilentlyContinue)) { dotnet tool install --global dotnet-coverage }; "DOTNET_COVERAGE_RESOLVED=$($null -ne (Get-Command dotnet-coverage -ErrorAction SilentlyContinue))"; dotnet-coverage --version; "EXIT=$LASTEXITCODE"'
(The trailing EXIT echo was added to observe the exit value; the rest is the plan P0-T8 command verbatim.)
WORKTREE-LEAF: not applicable (the command touches no repository path)
EXIT_CODE: 0

Output Summary:
DOTNET_COVERAGE_RESOLVED=True
18.10.0+f4cc39224845ffa74bf246c9da2399d50e5d6342
The tool was already installed; no install ran.
