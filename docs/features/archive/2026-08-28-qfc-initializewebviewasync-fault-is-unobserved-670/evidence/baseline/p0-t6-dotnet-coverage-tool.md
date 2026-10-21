# P0-T6 — dotnet-coverage global tool availability

Timestamp: 2026-09-01T19-43
Command: `if (-not (Get-Command dotnet-coverage -ErrorAction SilentlyContinue)) { dotnet tool install --global dotnet-coverage }`, then `Get-Command dotnet-coverage`
EXIT_CODE: 0

Output Summary:

`Get-Command dotnet-coverage` resolved. The resolved location is recorded in the placeholder form the plan's section 0 prescribes:

    <user-profile>\.dotnet\tools\dotnet-coverage.exe

The tool is a global tool rather than a manifest tool, so `dotnet tool restore` does not supply it: it is absent from `dotnet-tools.json`. `scripts/vscode/Invoke-MSTestWithCoverage.ps1` throws before running any test when the tool cannot be resolved, so P0-T12 could record no coverage figure without this task.

Capture-time sanitisation gate: a case-insensitive fixed-string sweep of this artifact for the drive-qualified user-profile root and for the drive-qualified Program Files root, in each of the two separator spellings, returns zero.
