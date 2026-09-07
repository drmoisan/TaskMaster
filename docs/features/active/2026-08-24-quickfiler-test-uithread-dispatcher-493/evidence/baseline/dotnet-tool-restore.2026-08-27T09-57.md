# CSharpier Manifest Restore (P0-T4)

Timestamp: 2026-08-27T09-57
Task: [P0-T4]
Command: `dotnet tool restore` then `dotnet tool run csharpier --version` (both run from `<repo-root>`)
EXIT_CODE: 0
Output Summary: `dotnet tool restore` exited 0 and restored csharpier 1.2.6.
`dotnet tool run csharpier --version` exited 0 and printed `1.2.6`, matching the pin in
`dotnet-tools.json` at the repository root.

## `dotnet tool restore`

Command: `dotnet tool restore`
EXIT_CODE: 0

```
Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier

Restore was successful.
```

## `dotnet tool run csharpier --version`

Command: `dotnet tool run csharpier --version`
EXIT_CODE: 0

```
1.2.6
```

The recorded output begins with `1.2.6`.

## Pinned version source

`dotnet-tools.json` at the repository root (note: at the repo root, not under `.config/`) declares:

```json
{
  "version": 1,
  "isRoot": true,
  "tools": {
    "csharpier": {
      "version": "1.2.6",
      "commands": [ "csharpier" ],
      "rollForward": false
    }
  }
}
```

`rollForward` is `false`, so the manifest pin is exact. Every formatter invocation in this plan is
made through `dotnet tool run` so this pinned version is the one used, matching the CI format-check
workflow.
