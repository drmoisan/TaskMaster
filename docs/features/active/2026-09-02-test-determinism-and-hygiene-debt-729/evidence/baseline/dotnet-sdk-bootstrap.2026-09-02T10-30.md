# .NET SDK bootstrap (P0-T3)

Timestamp: 2026-09-03T01-07

Command: `pwsh -NoProfile -File .\scripts\vscode\Install-RepoDotNetSdk.ps1`

EXIT_CODE: 0

RepoLocalSdkPresent: True

## Derivation of RepoLocalSdkPresent

`dotnet --list-sdks` was executed from the workspace root and each reported entry's installation
path was tested for the suffix `.dotnet-sdk\sdk`. Exactly 1 of the 2 reported entries carries that
suffix, so `RepoLocalSdkPresent` is `True`. The `dotnet --list-sdks` output itself is not pasted,
because it prints an absolute host path.

Output Summary:

```
Downloading .NET SDK 8.0.205 from https://builds.dotnet.microsoft.com/dotnet/Sdk/8.0.205/dotnet-sdk-8.0.205-win-x64.zip...
Installed repo-local .NET SDK 8.0.205 to <repo-root>\.dotnet-sdk.
```

`dotnet --version` executed from the workspace root prints:

```
8.0.205
```

`global.json` at the workspace root pins `sdk.version` to `8.0.205` with `paths` of
`.dotnet-sdk` and `$host$`, which is why the on-PATH `dotnet` muxer resolves to the
repo-local SDK. Entries counted from `dotnet --list-sdks`: 2 total, 1 with a
`.dotnet-sdk\sdk` suffix.
