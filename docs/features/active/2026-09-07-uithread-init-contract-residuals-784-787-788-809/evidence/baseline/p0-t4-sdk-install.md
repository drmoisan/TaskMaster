# [P0-T4] Repository-pinned .NET SDK install

Timestamp: 2026-09-08T00-18

Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File ./scripts/vscode/Install-RepoDotNetSdk.ps1`

EXIT_CODE: 0

Output Summary:

The installer printed:

```
Downloading .NET SDK 8.0.205 from https://builds.dotnet.microsoft.com/dotnet/Sdk/8.0.205/dotnet-sdk-8.0.205-win-x64.zip...
Installed repo-local .NET SDK 8.0.205 to <worktree>\.dotnet-sdk.
```

After the SDK preamble, `dotnet --version` printed the single verbatim line:

```
8.0.205
```

`.dotnet-sdk/dotnet.exe` exists (`Test-Path` returned `True`).

`git status --porcelain --untracked-files=all` does not list `.dotnet-sdk/`, because `.gitignore:350` carries `.dotnet*/`. The only paths listed are this plan file and the three Phase 0 evidence artifacts written so far.

The host was `pwsh` 7, not Windows PowerShell 5.1. This task ran before any `dotnet` or `msbuild` command in the plan; the worktree contained neither `.dotnet-sdk/` nor `packages/` on entry.
