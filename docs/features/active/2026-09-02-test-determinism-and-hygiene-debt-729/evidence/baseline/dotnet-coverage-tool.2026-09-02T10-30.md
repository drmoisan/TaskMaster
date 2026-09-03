# dotnet-coverage global tool (P0-T7)

Timestamp: 2026-09-03T01-12

Command: `if (-not (Get-Command dotnet-coverage -ErrorAction SilentlyContinue)) { dotnet tool install --global dotnet-coverage }`

EXIT_CODE: 0

Output Summary:

`Get-Command dotnet-coverage` resolved, so the conditional install branch was not taken and no
package was downloaded. The tool reports:

```
dotnet-coverage --version
18.10.0+f4cc39224845ffa74bf246c9da2399d50e5d6342
```

The `dotnet-coverage` global tool is present at version 18.10.0 and is available to
`scripts/vscode/Invoke-MSTestWithCoverage.ps1`.
