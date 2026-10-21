# Baseline — Repository-pinned .NET SDK provisioning ([P0-T2])

- Issue: #644
- Task: `[P0-T2]`
- Timestamp: 2026-08-29T08-15

## Skip-branch evaluation

The task authorizes skipping the install when `dotnet --version` already exits 0 and prints a
version line. That branch **did not apply**. The pre-install probe printed the `global.json`
repo-local-SDK error message and returned a non-zero exit code:

```
The command could not be loaded, possibly because:
  * You intended to execute a .NET application:
      The application '--version' does not exist or is not a managed .dll or .exe.
  * You intended to execute a .NET SDK command:
      The repo-local .NET SDK is missing. Run ./scripts/vscode/Install-RepoDotNetSdk.ps1 from the repository root, then retry dotnet format TaskMaster.sln.
```

Pre-install `EXIT_CODE: -2147450725`. The install was therefore performed as the task's primary
branch requires.

## Install

Command: `pwsh -NoProfile -File .\scripts\vscode\Install-RepoDotNetSdk.ps1`
EXIT_CODE: 0

Output (host path redacted):

```
Downloading .NET SDK 8.0.205 from https://builds.dotnet.microsoft.com/dotnet/Sdk/8.0.205/dotnet-sdk-8.0.205-win-x64.zip...
Installed repo-local .NET SDK 8.0.205 to <repo-root>\.dotnet-sdk.
```

## Acceptance verification

Command: `dotnet --version`
EXIT_CODE: 0

Output:

```
8.0.205
```

Output Summary: The worktree was cold — no `.dotnet-sdk` directory existed and the skip branch
did not apply. `Install-RepoDotNetSdk.ps1` exited 0 and installed the `global.json`-pinned SDK
`8.0.205` into `<repo-root>\.dotnet-sdk`. The acceptance probe `dotnet --version` then exited 0
and printed the version string `8.0.205`. `.dotnet*/` is matched by `.gitignore` line 350, so
the installed SDK does not dirty the tree.
