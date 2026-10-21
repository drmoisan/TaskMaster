Timestamp: 2026-09-09T09-54
Command: dotnet tool restore
EXIT_CODE: 0
Output Summary: Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier. Restore was successful.

Note: the repo-local .NET SDK (global.json 8.0.205) was not yet present in this freshly-created
execution worktree. Ran `./scripts/vscode/Install-RepoDotNetSdk.ps1` first (under pwsh 7), which
downloaded and installed .NET SDK 8.0.205 to `.dotnet-sdk/` in this worktree before `dotnet tool
restore` succeeded.
