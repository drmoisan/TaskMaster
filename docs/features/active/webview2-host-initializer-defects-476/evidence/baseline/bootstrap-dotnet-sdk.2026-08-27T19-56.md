# Bootstrap — Repository-Pinned .NET SDK ([P0-T3])

Timestamp: 2026-08-27T19-56

Command:
```
pwsh -NoProfile -File ./scripts/vscode/Install-RepoDotNetSdk.ps1
dotnet --version
```

EXIT_CODE: 0 (both commands)

## Output Summary

- `Install-RepoDotNetSdk.ps1` is idempotent without `-Force`: it tests for the version marker
  (`Install-RepoDotNetSdk.ps1:58`) and returns early when the SDK is present. It reported verbatim:

  ```
  Repo-local .NET SDK 8.0.205 is already installed at <repo-root>/.dotnet-sdk.
  ```

  No download, extraction, or installation was performed by this task; the repo-local SDK was
  already provisioned in this worktree before execution began.
- `dotnet --version`, executed from the workspace root, printed verbatim:

  ```
  8.0.205
  ```

  This is a `8.0.` version resolved through `global.json`, not the `global.json` `errorMessage`
  string. `global.json` pins `sdk.version` `8.0.205` with `rollForward: latestFeature` and
  `paths: [".dotnet-sdk", "$host$"]`; the resolved value is inside the permitted `8.0.x` feature
  band, so the acceptance holds. Per the task text no equality test against `8.0.205` is applied.
- The same version is reported by the repo-local host directly
  (`./.dotnet-sdk/dotnet.exe --version` -> `8.0.205`), confirming the resolution came from
  `.dotnet-sdk` rather than a machine-wide SDK.
