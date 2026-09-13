# P0-T4 — Repository-Local .NET SDK Bootstrap

Timestamp: 2026-09-13T04-53
Task: [P0-T4]

Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath <worktree>; & <worktree>/scripts/vscode/Install-RepoDotNetSdk.ps1'
EXIT_CODE: 0

The installer was run with its default version. Its `-Version` parameter default is `8.0.205`
(`scripts/vscode/Install-RepoDotNetSdk.ps1` line 3) and its install directory default resolves to
the repository-local `.dotnet-sdk` directory (line 36). The marker path the installer itself
asserts is the install directory joined with `sdk` and the version (line 56, asserted at lines
102 through 103).

Build lock: acquired for item 873 before the command and released immediately after it returned.

## Output

```
Downloading .NET SDK 8.0.205 from https://builds.dotnet.microsoft.com/dotnet/Sdk/8.0.205/dotnet-sdk-8.0.205-win-x64.zip...
Installed repo-local .NET SDK 8.0.205 to <worktree>\.dotnet-sdk.
```

`$LASTEXITCODE` was unset after the invocation because the script runs no native executable; the
script completed without a terminating error, so EXIT_CODE is recorded as 0.

## Marker Directory Observation

Command: Test-Path -LiteralPath <worktree>/.dotnet-sdk/sdk/8.0.205 -PathType Container

SDK_MARKER_DIRECTORY: .dotnet-sdk/sdk/8.0.205
SDK_MARKER_DIRECTORY_EXISTS: True

## Output Summary

The installer's own filesystem marker directory, that is the version-named subdirectory
`.dotnet-sdk/sdk/8.0.205` beneath the repository-local SDK directory for the installer's default
version 8.0.205, exists after the run. The recorded directory-existence result is `True`. No
runtime version listing was used as the gate. `.dotnet-sdk/` is ignored by the repository ignore
file, so this bootstrap adds no tracked path and does not widen this delivery's footprint.

EXIT_CODE: 0
