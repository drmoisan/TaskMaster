# Baseline — `dotnet tool restore` (CSharpier manifest pin) ([P0-T5])

Timestamp: 2026-08-10T22-40
Command: `./.dotnet-sdk/dotnet.exe tool restore`
EXIT_CODE: 0

## Console output

```
Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier

Restore was successful.
```

## Manifest verification

The repo-root manifest is `./dotnet-tools.json` (there is no `.config/dotnet-tools.json` in this
checkout; `ls .config/dotnet-tools.json` returns `No such file or directory`).

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

## Output Summary

`dotnet tool restore` succeeded with `EXIT_CODE: 0`. The restored CSharpier version is **1.2.6**,
which equals the version pinned in the repo-root manifest `./dotnet-tools.json` (expected 1.2.6).
`rollForward` is `false`, so the pin is exact. This confirms the premise of Defect B: the pinned tool
is CSharpier v1.x, which requires a subcommand, and the documented bare-path form is v0 syntax.
