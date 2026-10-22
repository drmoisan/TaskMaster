# Bootstrap: dotnet-coverage global tool (P0-T9)

Timestamp: 2026-10-03T07-36
Task: P0-T9
Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath "WORKTREE"; [Environment]::CurrentDirectory = (Get-Location).Path; if (-not (Get-Command dotnet-coverage -ErrorAction SilentlyContinue)) { dotnet tool install --global dotnet-coverage }; "DOTNET_COVERAGE_RESOLVED=$($null -ne (Get-Command dotnet-coverage -ErrorAction SilentlyContinue))"; dotnet-coverage --version'
EXIT_CODE: 0

Output Summary:
- dotnet-coverage was already on PATH, so the guarded install did not run.
- DOTNET_COVERAGE_RESOLVED=True
- Version line: 18.10.0+f4cc39224845ffa74bf246c9da2399d50e5d6342
- Verdict: PASS.
