# Phase 0 — Repository-local .NET SDK bootstrap ([P0-T3])

Timestamp: 2026-09-01T21-48

Command: `pwsh -NoProfile -File scripts/vscode/Install-RepoDotNetSdk.ps1`

EXIT_CODE: 0

Installer output, verbatim, with the worktree root rendered as `<repo-root>`:

```
Downloading .NET SDK 8.0.205 from https://builds.dotnet.microsoft.com/dotnet/Sdk/8.0.205/dotnet-sdk-8.0.205-win-x64.zip...
Installed repo-local .NET SDK 8.0.205 to <repo-root>\.dotnet-sdk.
```

## Acceptance readings

Command: `dotnet --version`

EXIT_CODE: 0

Output, verbatim:

```
8.0.205
```

Command: `pwsh -NoProfile -Command 'Test-Path .dotnet-sdk/sdk/8.0.205'`

EXIT_CODE: 0

Output, verbatim:

```
True
```

`dotnet --list-sdks` was deliberately not used as an acceptance reading. That command enumerates the
muxer's own root and does not consult `global.json`, so it prints the host line whether or not the
repo-local SDK was installed.

Output Summary: The repository-local SDK was provisioned. `dotnet --version` prints `8.0.205`, which
`global.json` pins and which can only be resolved through the `.dotnet-sdk` entry in `global.json`'s
`sdk.paths`, and the marker path `.dotnet-sdk/sdk/8.0.205` exists. Both acceptance clauses of `[P0-T3]`
hold.
