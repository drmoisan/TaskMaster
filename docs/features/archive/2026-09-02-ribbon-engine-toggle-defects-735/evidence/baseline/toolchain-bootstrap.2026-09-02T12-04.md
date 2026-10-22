# Phase 0 — Toolchain Bootstrap (P0-T3)

Timestamp: 2026-09-03T01-16
Task: [P0-T3]
EXIT_CODE: 0

Rationale for this task: `global.json` pins the SDK under a directory that is absent in a fresh
worktree, so every `dotnet` command fails until the bootstrap script has run.

## Invocation 1 — repo-local SDK install

Command: `pwsh -NoProfile -File <worktree>/scripts/vscode/Install-RepoDotNetSdk.ps1`
EXIT_CODE: 0

Output:

```
Downloading .NET SDK 8.0.205 from https://builds.dotnet.microsoft.com/dotnet/Sdk/8.0.205/dotnet-sdk-8.0.205-win-x64.zip...
Installed repo-local .NET SDK 8.0.205 to <worktree>\.dotnet-sdk.
```

## Invocation 2 — dotnet local tool restore

Command: `<worktree>\.dotnet-sdk\dotnet.exe tool restore` (run with the working directory set to the
worktree root so the manifest is discovered)
EXIT_CODE: 0

Output:

```
Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier

Restore was successful.
```

## CSharpier version resolved

Command: `<worktree>\.dotnet-sdk\dotnet.exe tool run csharpier --version`
EXIT_CODE: 0
Reported version: `1.2.6`

This matches the version pinned by `dotnet-tools.json` and required by the C# code-change policy.
Every formatter invocation in this plan goes through `dotnet tool run` so the manifest-pinned
version is used; no globally installed CSharpier is invoked.

## Toolchain paths resolved for the remainder of this plan

- dotnet: `<worktree>\.dotnet-sdk\dotnet.exe`
- vswhere: `${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe`
- msbuild and vstest.console.exe are resolved through vswhere at each command task, as the plan's
  toolchain-resolution block specifies. Neither is on PATH in this environment.

Output Summary: Both bootstrap commands returned EXIT_CODE 0. The repo-local .NET SDK 8.0.205 is
installed under `.dotnet-sdk`, the CSharpier local tool restored successfully, and
`dotnet tool run csharpier --version` reports `1.2.6` as required.
