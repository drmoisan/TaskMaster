# P0-T3 — Repository-pinned .NET SDK bootstrap

Timestamp: 2026-09-30T09-14
Command: pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; if (-not (Test-Path ".dotnet-sdk\sdk")) { & ".\scripts\vscode\Install-RepoDotNetSdk.ps1" }; dotnet --version; dotnet --list-sdks'
EXIT_CODE: 0
Output Summary:
- .dotnet-sdk\sdk was absent before the run (fresh worktree), so Install-RepoDotNetSdk.ps1 ran and printed "Installed repo-local .NET SDK 8.0.205 to <execution-worktree-root>\.dotnet-sdk."
- dotnet --version: 8.0.205
- dotnet --list-sdks:
  - 8.0.205 [<execution-worktree-root>\.dotnet-sdk\sdk]
  - 10.0.401 [<program-files>\dotnet\sdk] (machine-wide install)
- The 8.0.205 line names the sdk folder under the repository-root .dotnet-sdk folder (Install-RepoDotNetSdk.ps1 line 36, global.json line 7).
