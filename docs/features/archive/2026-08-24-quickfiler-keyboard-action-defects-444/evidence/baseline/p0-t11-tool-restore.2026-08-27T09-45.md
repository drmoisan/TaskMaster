# [P0-T11] Local tool manifest restore and coverage collector provisioning

Timestamp: 2026-08-27T09-45
Command: `dotnet tool restore`
EXIT_CODE: 0

## Manifest

The manifest is the repository-root `dotnet-tools.json` (not `.config/dotnet-tools.json`). Its
contents:

```json
{
  "version": 1,
  "isRoot": true,
  "tools": {
    "csharpier": {
      "version": "1.2.6",
      "commands": [
        "csharpier"
      ],
      "rollForward": false
    }
  }
}
```

`rollForward` is `false`, so the version comparison below is an exact-equality comparison against a
hard pin, not against a rolling range.

## Restore result (verbatim)

```
Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier

Restore was successful.
```

## Version check

`dotnet tool run csharpier --version` (verbatim):

```
1.2.6
```

## Coverage collector

`Get-Command dotnet-coverage` resolved before any install was attempted, so
`dotnet tool install --global dotnet-coverage` was not run:

```
dotnet-coverage: ALREADY-PRESENT <user-home>\.dotnet\tools\dotnet-coverage.exe
```

`scripts/vscode/Invoke-MSTestWithCoverage.ps1` terminates with
`dotnet-coverage not found. Install it with: dotnet tool install --global dotnet-coverage` when the
global tool is absent; that failure mode is not reachable here.

## Acceptance evaluation

- `EXIT_CODE: 0`. PASS.
- `dotnet tool run csharpier --version` prints `1.2.6`. PASS.
- `Get-Command dotnet-coverage` resolves to an executable path, recorded above with the install root
  written as `<user-home>`. PASS.

Output Summary: csharpier 1.2.6 restored from the root manifest; `dotnet-coverage` already present at
`<user-home>\.dotnet\tools\dotnet-coverage.exe`; all three acceptance conditions met.
