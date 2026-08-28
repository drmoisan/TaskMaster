# Phase 0 — CSharpier Manifest Tool Restore

Timestamp: 2026-08-26T10-42
Task: [P0-T4]
Command: `pwsh -NoProfile -Command 'dotnet tool restore; Write-Host "EXIT_CODE=$LASTEXITCODE"'`
EXIT_CODE: 0

## Output Summary

```
Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier

Restore was successful.
EXIT_CODE=0
```

Version verification:

```
> dotnet tool run csharpier --version
1.2.6
EXIT_CODE=0
```

The manifest is the repository-root file `dotnet-tools.json`; there is no `.config/` directory
in this worktree. It pins `csharpier` at `1.2.6` with `rollForward: false`.

## Prerequisite resolved during this task

The first `dotnet tool restore` attempt failed with EXIT_CODE `-2147450725` and the repository's
own custom `global.json` error message stating that the repo-local .NET SDK is missing.

`global.json` pins `sdk.version` to `8.0.205` with `rollForward: latestFeature` and
`paths: [".dotnet-sdk", "$host$"]`. The only SDK installed at the machine-wide `$host$` root is
`10.0.302`, which `latestFeature` cannot roll forward to because it is a different major version.
This worktree had no `.dotnet-sdk` directory at all.

Resolution: `pwsh -NoProfile -ExecutionPolicy Bypass -File ./scripts/vscode/Install-RepoDotNetSdk.ps1`
was run from the worktree root. It downloaded and extracted SDK `8.0.205` into the worktree's own
`.dotnet-sdk` directory, after which `dotnet --version` reports `8.0.205` and the tool restore
succeeds.

`.dotnet-sdk/` is git-ignored (`.gitignore` line 350 matches it), so this bootstrap step writes
nothing that any ownership or changed-file gate can observe. It is confined to this worktree and
does not modify any shared or sibling-worktree state.
