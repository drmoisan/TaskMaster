# Bootstrap: repository .NET SDK (issue 942)

Timestamp: 2026-09-30T07-24
Task: P0-T3
Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath "WORKTREE"; if (-not (Test-Path -LiteralPath ".dotnet-sdk\sdk\8.0.205")) { & .\scripts\vscode\Install-RepoDotNetSdk.ps1 }; "SDK_MARKER=$(Test-Path -LiteralPath ".dotnet-sdk\sdk\8.0.205")"; dotnet --version'
EXIT_CODE: 0

Output Summary:
- The guard found no .dotnet-sdk\sdk\8.0.205 marker, so scripts/vscode/Install-RepoDotNetSdk.ps1 ran and installed the repo-local SDK 8.0.205 into the ignored .dotnet-sdk directory (installer path line omitted: it carries an absolute host path).
- SDK_MARKER=True
- dotnet --version printed: 8.0.205 (a version string, not the global.json error message)
