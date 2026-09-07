# [P0-T7] Pinned Local Tool Restore

Timestamp: 2026-08-26T08-43

Task: [P0-T7]
Feature: docs/features/active/quickfiler-bug-family-446

## Manifest Observed

`dotnet-tools.json` sits at the repository root (not under `.config/`) and pins:

```
"csharpier": { "version": "1.2.6", "commands": [ "csharpier" ], "rollForward": false }
```

CSharpier v1 requires a subcommand (`format`, `check`, `pipe-files`, `server`); the bare
`csharpier .` form of v0 does not run.

## Invocation 1 — restore

Command: `pwsh -NoProfile -Command 'dotnet tool restore'`
EXIT_CODE: 0
Output Summary: "Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier"
followed by "Restore was successful."

## Invocation 2 — version check

Command: `pwsh -NoProfile -Command 'dotnet tool run csharpier --version; exit $LASTEXITCODE'`
EXIT_CODE: 0
Output: `1.2.6`

## Output Summary

The manifest-pinned CSharpier 1.2.6 is restored and reachable through `dotnet tool run`.
The recorded version output is exactly `1.2.6` with exit code 0, satisfying the acceptance
condition. All formatting steps in this plan invoke CSharpier through `dotnet tool run` so the
manifest-pinned version is used, never a global install.
