# Phase 0 — .NET SDK Resolution

Timestamp: 2026-08-26T08-29
Task: [P0-T5]

## Probe 1 (before bootstrap)

Command: `dotnet --version`
EXIT_CODE: 155

```
The command could not be loaded, possibly because:
  * You intended to execute a .NET application:
      The application '--version' does not exist or is not a managed .dll or .exe.
  * You intended to execute a .NET SDK command:
      The repo-local .NET SDK is missing. Run ./scripts/vscode/Install-RepoDotNetSdk.ps1 from the repository root, then retry dotnet format TaskMaster.sln.
```

The probe printed the `global.json` `errorMessage` rather than a version, confirming that `.dotnet-sdk/`
was absent from this fresh agent worktree. `global.json` declares
`"version": "8.0.205"`, `"rollForward": "latestFeature"`, and `"paths": [".dotnet-sdk", "$host$"]`.

## Bootstrap

Command: `pwsh -NoProfile -File ./scripts/vscode/Install-RepoDotNetSdk.ps1`
EXIT_CODE: 0

```
Downloading .NET SDK 8.0.205 from https://builds.dotnet.microsoft.com/dotnet/Sdk/8.0.205/dotnet-sdk-8.0.205-win-x64.zip...
Installed repo-local .NET SDK 8.0.205 to <repo-root>\.dotnet-sdk.
```

## Probe 2 (after bootstrap)

Command: `dotnet --version`
EXIT_CODE: 0

```
8.0.205
```

Command: `dotnet --list-sdks`
EXIT_CODE: 0

```
8.0.205 [<repo-root>\.dotnet-sdk\sdk]
10.0.302 [<program-files>\dotnet\sdk]
```

## Acceptance

`dotnet --version` exits 0 and prints `8.0.205`. `global.json` pins `8.0.205` under
`rollForward: latestFeature`, and `8.0.205` satisfies that pin exactly (a `latestFeature` roll-forward
accepts the pinned feature band `8.0.2xx`; the resolved version is the pinned version itself). The
machine-wide `10.0.302` SDK is not selected because `global.json` lists `.dotnet-sdk` ahead of `$host$`
in `paths` and the repo-local SDK satisfies the pin.

Output Summary: The repo-local SDK was absent and was installed by
`scripts/vscode/Install-RepoDotNetSdk.ps1`. `dotnet --version` now exits 0 and prints `8.0.205`,
satisfying `global.json`.
