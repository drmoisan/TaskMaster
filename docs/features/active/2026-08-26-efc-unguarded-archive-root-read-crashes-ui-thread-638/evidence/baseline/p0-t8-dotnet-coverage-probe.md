# [P0-T8] dotnet-coverage probe (Issue 638)

Timestamp: 2026-08-29T12-21

Command: `Get-Command dotnet-coverage -ErrorAction SilentlyContinue`

EXIT_CODE: 0

Resolution: already present

Output Summary:

`Get-Command dotnet-coverage` resolved the global tool, so no
`dotnet tool install --global dotnet-coverage` was required.

`dotnet-coverage --version` reported:

```
18.5.2+6e39b75eaf98f2691cf62dbf259669cc13851fd3
```

Resolved `dotnet-coverage` version string: **18.5.2**
(commit `6e39b75eaf98f2691cf62dbf259669cc13851fd3`).
