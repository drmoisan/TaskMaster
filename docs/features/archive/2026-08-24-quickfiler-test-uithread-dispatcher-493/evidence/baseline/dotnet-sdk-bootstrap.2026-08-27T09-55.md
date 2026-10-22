# Repo-Local .NET SDK Bootstrap (P0-T3)

Timestamp: 2026-08-27T09-55
Task: [P0-T3]
Command: `pwsh -NoProfile -File scripts/vscode/Install-RepoDotNetSdk.ps1` (run from `<repo-root>`)
EXIT_CODE: 0
Output Summary: The fresh agent worktree carried no `.dotnet-sdk` tree, so `dotnet --version`
failed with the `global.json` "repo-local .NET SDK is missing" error before this task ran. The
install script downloaded and installed SDK 8.0.205. Post-install, `dotnet --version` exits 0 and
prints `8.0.205`, and the marker path `.dotnet-sdk/sdk/8.0.205` exists.

## Install output (redacted)

```
Downloading .NET SDK 8.0.205 from https://builds.dotnet.microsoft.com/dotnet/Sdk/8.0.205/dotnet-sdk-8.0.205-win-x64.zip...
Installed repo-local .NET SDK 8.0.205 to <repo-root>/.dotnet-sdk.
```

## Acceptance verification

| Check | Command | Result |
| --- | --- | --- |
| `dotnet --version` exit code | `dotnet --version` run from `<repo-root>` | `0` |
| `dotnet --version` output | same | `8.0.205` — begins with `8.0.` |
| Marker path present | `Test-Path (Join-Path $WS '.dotnet-sdk/sdk/8.0.205')` | `True` |

`8.0.205` is the version pinned by `global.json` at the repository root and is the default
`-Version` of `scripts/vscode/Install-RepoDotNetSdk.ps1`; it is also the exact marker path that
script checks before deciding the SDK is already installed.

Per the task text, no assertion is made about `dotnet --list-sdks`: that command does not consult
`global.json` and enumerates only the host root of the muxer on `PATH`, so it would print the
machine-wide SDK list and would never name the repo-local install directory.
