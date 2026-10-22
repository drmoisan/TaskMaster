# Bootstrap: dotnet-coverage global tool (issue 942)

Timestamp: 2026-09-30T07-26
Task: P0-T7
Command: pwsh -NoProfile -Command 'if (-not (Get-Command dotnet-coverage -ErrorAction SilentlyContinue)) { dotnet tool install --global dotnet-coverage }; "DOTNET_COVERAGE_RESOLVED=$($null -ne (Get-Command dotnet-coverage -ErrorAction SilentlyContinue))"; dotnet-coverage --version'
EXIT_CODE: 0

Output Summary:
- The tool was already resolvable; the guarded install did not run.
- DOTNET_COVERAGE_RESOLVED=True
- Version line: 18.10.0+f4cc39224845ffa74bf246c9da2399d50e5d6342
