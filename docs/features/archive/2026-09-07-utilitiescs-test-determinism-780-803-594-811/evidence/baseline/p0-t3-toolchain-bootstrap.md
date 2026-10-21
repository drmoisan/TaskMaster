# P0-T3 — Repo-local SDK bootstrap and tool-manifest restore

Timestamp: 2026-09-08T09-18
Task: [P0-T3]
Command: pwsh -NoProfile -File coverage/plan811-helper.ps1 wrapping `pwsh -NoProfile -File scripts/vscode/Install-RepoDotNetSdk.ps1`, then `.dotnet-sdk\dotnet.exe --version`, then `.dotnet-sdk\dotnet.exe tool restore`
EXIT_CODE: 0

BRANCH APPLIED: INSTALL

`Test-Path .dotnet-sdk\dotnet.exe` returned `False` before the task, so the SDK acquisition branch
applied. This is a fresh agent worktree that had never been bootstrapped for C# builds.

## Observations

| Observation | Value |
|---|---|
| `Test-Path .dotnet-sdk\dotnet.exe` before | `False` |
| `Install-RepoDotNetSdk.ps1` exit code | `0` |
| `Test-Path .dotnet-sdk\dotnet.exe` after | `True` |
| `dotnet --version` | `8.0.205` |
| `dotnet --version` exit code | `0` |
| `dotnet tool restore` exit code | `0` |

Installer output (host path redacted to `<worktree>`):

```
Downloading .NET SDK 8.0.205 from https://builds.dotnet.microsoft.com/dotnet/Sdk/8.0.205/dotnet-sdk-8.0.205-win-x64.zip...
Installed repo-local .NET SDK 8.0.205 to <worktree>\.dotnet-sdk.
```

Tool-restore output, verbatim:

```
Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier

Restore was successful.
```

## Acceptance evaluation

- `dotnet --version` prints `8.0.205`, matching the `global.json` pin. PASS
- Tool-restore output contains the line
  `Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier`. PASS
- Both commands exited 0. PASS
- The artifact records which branch applied (INSTALL). PASS

## Output Summary

Repo-local .NET SDK 8.0.205 downloaded and installed under `.dotnet-sdk` on the INSTALL branch.
Manifest-pinned CSharpier 1.2.6 restored. Both steps exited 0, so the format gate can now be
invoked through `dotnet tool run`.
