# Phase 0 — .NET SDK resolution

Timestamp: 2026-08-27T23-19
Task: [P0-T5]
Command: `dotnet --version` and `dotnet --list-sdks` from the worktree root, under `pwsh -NoProfile`; then, after the first probe printed the `global.json` error message, `pwsh -NoProfile -File ./scripts/vscode/Install-RepoDotNetSdk.ps1`, then both probes re-run
EXIT_CODE: 0

## First probe — failed, triggering the installer branch

`dotnet --version` did not print a version. It printed the `global.json` `errorMessage` value
("The repo-local .NET SDK is missing. Run ./scripts/vscode/Install-RepoDotNetSdk.ps1 from the repository
root, ...") and exited non-zero. This is the exact condition `[P0-T5]` names as the installer branch.

## Installer run

`pwsh -NoProfile -File ./scripts/vscode/Install-RepoDotNetSdk.ps1` — EXIT_CODE: 0. It downloaded .NET SDK
8.0.205 and installed it into the worktree-local `.dotnet-sdk` directory. `.dotnet-sdk` is a repo-local
tool directory, not feature output.

## Second probe — passed

- `dotnet --version` → `8.0.205`, EXIT_CODE: 0
- `dotnet --list-sdks` → EXIT_CODE: 0, two SDKs visible:
  - `8.0.205` from the worktree-local `.dotnet-sdk\sdk` directory
  - `10.0.400` from the machine-wide install

## `global.json` values

| Key | Value |
|---|---|
| `sdk.version` | `8.0.205` |
| `sdk.rollForward` | `latestFeature` |
| `sdk.allowPrerelease` | `false` |
| `sdk.paths` | `.dotnet-sdk`, `$host$` |

The printed version `8.0.205` is accepted by `global.json` under `rollForward: latestFeature`, which is
what the successful exit code demonstrates: the SDK resolver reports the error message rather than a
version whenever no acceptable SDK is found. **No equality between the printed version and the pinned
version is asserted here**, because `latestFeature` admits a range of 8.0.x feature bands.

Output Summary: `dotnet --version` exits 0 and prints 8.0.205, accepted by global.json under
rollForward latestFeature. The repo-local SDK was absent on first probe and was installed by
Install-RepoDotNetSdk.ps1 (exit 0) before the passing re-probe.
