# [P0-T5] Manifest-Pinned Local Tool Restore

Timestamp: 2026-09-08T09-15
Command: `dotnet tool restore`
EXIT_CODE: 0
Output Summary: CSharpier 1.2.6 was restored from the repository-root `dotnet-tools.json` manifest and the restore reported success.

CSHARPIER-VERSION: 1.2.6

## Command output

```
Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier

Restore was successful.
```

## Manifest reading

The version above is read with the Read tool from the `tools.csharpier.version` value of the repository-root `dotnet-tools.json`, not from a `--version` probe. The manifest content is:

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

The manifest is the repository-root file `dotnet-tools.json`. `.config/dotnet-tools.json` does not exist in this checkout. The manifest declares a single command name `csharpier`, and CSharpier 1 requires a subcommand, so every invocation in this plan uses `dotnet tool run csharpier format .` or `dotnet tool run csharpier check .`.
