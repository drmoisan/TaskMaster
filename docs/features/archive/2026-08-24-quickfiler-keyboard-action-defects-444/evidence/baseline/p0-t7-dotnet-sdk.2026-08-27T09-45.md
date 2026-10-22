# [P0-T7] Repository-pinned .NET SDK provisioning

Timestamp: 2026-08-27T09-45
Command: `pwsh -NoProfile -File .\scripts\vscode\Install-RepoDotNetSdk.ps1`
EXIT_CODE: 0

## Pre-check (skip branch NOT taken)

`dotnet --version` before the script reported a load failure, not a version:

```
The command could not be loaded, possibly because:
  * You intended to execute a .NET application:
      The application '--version' does not exist or is not a managed .dll or .exe.
  * You intended to execute a .NET SDK command:
      The repo-local .NET SDK is missing. Run ./scripts/vscode/Install-RepoDotNetSdk.ps1 from the repository root, then retry dotnet format TaskMaster.sln.
```

That is `global.json`'s configured `errorMessage`, emitted because the pinned `paths` entry
`.dotnet-sdk` did not exist in this fresh worktree. The explicit `SKIPPED-ALREADY-PROVISIONED` branch
was therefore not available and the script was run.

## Script output (verbatim, workspace root substituted)

```
Downloading .NET SDK 8.0.205 from https://builds.dotnet.microsoft.com/dotnet/Sdk/8.0.205/dotnet-sdk-8.0.205-win-x64.zip...
Installed repo-local .NET SDK 8.0.205 to <repo-root>\.dotnet-sdk.
```

## Post-check

`dotnet --version` (verbatim):

```
8.0.205
```

`dotnet --list-sdks` (verbatim, workspace root and Program Files substituted):

```
8.0.205 [<repo-root>\.dotnet-sdk\sdk]
10.0.400 [<program-files>\dotnet\sdk]
```

## Acceptance evaluation

- `dotnet --version` from `WS` prints `8.0.205`: major `8`, minor `0`, and the full version is at or
  above `8.0.205`. PASS.
- `dotnet --list-sdks` includes a path ending `.dotnet-sdk\sdk`. PASS.

No string-equality assertion against `8.0.205` was required; `global.json` sets `rollForward` to
`latestFeature`, so a higher `8.0.x` feature band would also have satisfied this task. The observed
resolution happens to be the pinned version exactly.

Output Summary: repo-local SDK provisioned from absent; `dotnet --version` = `8.0.205`;
`.dotnet-sdk\sdk` present in `--list-sdks`. Both acceptance conditions met.
