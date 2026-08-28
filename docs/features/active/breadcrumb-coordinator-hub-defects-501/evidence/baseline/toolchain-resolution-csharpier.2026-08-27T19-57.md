# Toolchain Resolution — CSharpier (P0-T9)

Timestamp: 2026-08-27T19-57

## Manifest location

The manifest is at the repository root as `dotnet-tools.json`; `.config/dotnet-tools.json` does not
exist. `dotnet tool restore` nonetheless read the root manifest successfully, so the plan's contingency
remedy (relocate the manifest to `.config/dotnet-tools.json` and re-run) was NOT needed and was NOT
performed. No global-tool fallback was used at any point; `CLAUDE.md` C#1.1 forbids one.

Manifest contents:

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

## Step 1 — restore

Command: `dotnet tool restore`
EXIT_CODE: 0
Output Summary:

```
Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier
Restore was successful.
```

## Step 2 — version probe

Command: `dotnet tool run csharpier --version`
EXIT_CODE: 0
Output Summary: `1.2.6` — matches the version pinned by the root `dotnet-tools.json` manifest.

## Resolved invocation

CSHARPIER_INVOCATION: dotnet tool run csharpier

Acceptance: exactly one `CSHARPIER_INVOCATION:` line whose value is exactly `dotnet tool run csharpier`;
the `--version` run for that invocation recorded `EXIT_CODE: 0` and reported `1.2.6`. PASS.
