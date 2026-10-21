# [P0-T3] Repository-Local .NET SDK Resolution

Timestamp: 2026-09-08T09-12
Command: `dotnet --version` (probe); `pwsh -NoProfile -File ./scripts/vscode/Install-RepoDotNetSdk.ps1`; `dotnet --version` (re-probe)
EXIT_CODE: 0
Output Summary: The first probe failed with the `global.json` `errorMessage` rather than printing a version, confirming the worktree had no `.dotnet-sdk` directory. The repository installer downloaded and installed SDK 8.0.205 into the worktree, and the re-probe printed the version and exited 0.

SDK-VERSION: 8.0.205
BRANCH-TAKEN: installed

## First probe (before install)

Exit code: -2147450725 (non-zero). Output was the `global.json` `errorMessage`:

```
The command could not be loaded, possibly because:
  * You intended to execute a .NET application:
      The application '--version' does not exist or is not a managed .dll or .exe.
  * You intended to execute a .NET SDK command:
      The repo-local .NET SDK is missing. Run ./scripts/vscode/Install-RepoDotNetSdk.ps1 from the repository root, then retry dotnet format TaskMaster.sln.
```

## Install

```
Downloading .NET SDK 8.0.205 from https://builds.dotnet.microsoft.com/dotnet/Sdk/8.0.205/dotnet-sdk-8.0.205-win-x64.zip...
Installed repo-local .NET SDK 8.0.205 to <worktree>\.dotnet-sdk.
```

The absolute destination path printed by the installer is reduced to `<worktree>` per D14.

## Re-probe (after install)

Exit code: 0. Output: `8.0.205`.

The version matches the `sdk.version` value `8.0.205` that `global.json` pins with `rollForward: latestFeature`.
