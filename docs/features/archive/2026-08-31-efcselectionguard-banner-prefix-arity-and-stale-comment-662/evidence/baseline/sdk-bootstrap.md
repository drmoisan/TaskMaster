# Repo-local .NET SDK Bootstrap (P0-T3)

Timestamp: 2026-09-01T15-40

Command: `pwsh -NoProfile -File ./scripts/vscode/Install-RepoDotNetSdk.ps1`

EXIT_CODE: 0

Output Summary:

Script output, transcribed:

```
Downloading .NET SDK 8.0.205 from https://builds.dotnet.microsoft.com/dotnet/Sdk/8.0.205/dotnet-sdk-8.0.205-win-x64.zip...
Installed repo-local .NET SDK 8.0.205 to <repo-root>\.dotnet-sdk.
```

Acceptance checks, both run from the worktree root:

- `Test-Path .dotnet-sdk\sdk\8.0.205` printed `True`.
- `dotnet --version` printed `8.0.205`, which is in the `8.0.2` feature band
  that `global.json` pins with `rollForward` `latestFeature`.

No assertion is made against `dotnet --list-sdks`: that command enumerates SDKs
under the host root and does not consult the `global.json` `paths` array, so it
is not expected to name the repo-local install.

The download from `builds.dotnet.microsoft.com` succeeded, so no BLOCKED
condition arises. The `.dotnet-sdk` directory is git-ignored by `.gitignore:350`,
so this step does not dirty the tree.
