# [P0-T6] dotnet tool restore (Issue 638)

Timestamp: 2026-08-29T12-17

Command: `dotnet tool restore` (run from the worktree root)

EXIT_CODE: 0

Output Summary:

Two invocations were required, in the order the task text authorizes.

1. First invocation of `dotnet tool restore` failed. The repo-local .NET SDK directory
   named in `global.json:7` was absent in this fresh worktree, and the muxer reported:

   ```
   The command could not be loaded, possibly because:
     * You intended to execute a .NET application:
         The application 'tool' does not exist or is not a managed .dll or .exe.
     * You intended to execute a .NET SDK command:
         The repo-local .NET SDK is missing. Run ./scripts/vscode/Install-RepoDotNetSdk.ps1 from the repository root, then retry dotnet format TaskMaster.sln.
   ```

2. `pwsh -NoProfile -File scripts/vscode/Install-RepoDotNetSdk.ps1` was run and exited 0:

   ```
   Downloading .NET SDK 8.0.205 from https://builds.dotnet.microsoft.com/dotnet/Sdk/8.0.205/dotnet-sdk-8.0.205-win-x64.zip...
   Installed repo-local .NET SDK 8.0.205 to <worktree-root>\.dotnet-sdk.
   ```

3. `dotnet tool restore` was retried and exited 0:

   ```
   Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier

   Restore was successful.
   ```

CSharpier version restored: **1.2.6**, matching the `dotnet-tools.json:6` pin.
