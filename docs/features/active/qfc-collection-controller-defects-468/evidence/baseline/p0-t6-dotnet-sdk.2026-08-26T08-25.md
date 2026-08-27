# [P0-T6] Repo-pinned .NET SDK provisioning

Timestamp: 2026-08-26T08-25

Command: `pwsh -NoProfile -File scripts/vscode/Install-RepoDotNetSdk.ps1`

EXIT_CODE: 0

ExpectedExitCode: 0

## Output Summary

The worktree had no `.dotnet-sdk` directory before this task ran (`ls -d .dotnet-sdk` returned
`No such file or directory`), so every `dotnet` SDK command would have failed against `global.json`,
which pins `sdk.version 8.0.205` under `rollForward: latestFeature` with
`paths: [".dotnet-sdk", "$host$"]`.

Installer output, verbatim (host path replaced with `<WS>`):

```
Downloading .NET SDK 8.0.205 from https://builds.dotnet.microsoft.com/dotnet/Sdk/8.0.205/dotnet-sdk-8.0.205-win-x64.zip...
Installed repo-local .NET SDK 8.0.205 to <WS>\.dotnet-sdk.
```

### Acceptance verification

Command: `pwsh -NoProfile -Command "Set-Location '<WS>'; dotnet --version"`

```
8.0.205
```

Exit code: 0. The value printed is exactly `8.0.205`, matching the plan's stated expectation.

Command: `pwsh -NoProfile -Command "Set-Location '<WS>'; dotnet --list-sdks"`

```
8.0.205 [<WS>\.dotnet-sdk\sdk]
10.0.302 [C:\Program Files\dotnet\sdk]
```

Exit code: 0. The list includes a path ending `.dotnet-sdk\sdk`, as required. The machine-wide
10.0.302 SDK is also visible but is not selected, because `global.json` pins the 8.0 feature band and
lists `.dotnet-sdk` ahead of `$host$`.

Result: PASS. Both acceptance conditions are met.
