# Bootstrap: repository .NET SDK (P0-T6)

Timestamp: 2026-10-03T07-36
Task: P0-T6
Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath "WORKTREE"; [Environment]::CurrentDirectory = (Get-Location).Path; if (-not (Test-Path -LiteralPath ".dotnet-sdk\sdk\8.0.205")) { & (Join-Path (Get-Location).Path "scripts\vscode\Install-RepoDotNetSdk.ps1") }; "SDK_MARKER=$(Test-Path -LiteralPath ".dotnet-sdk\sdk\8.0.205")"; dotnet --version'
EXIT_CODE: 0

Output Summary:
- The marker was absent, so the installer ran and installed SDK 8.0.205.
- SDK_MARKER=True
- dotnet --version: 8.0.205 (a version string, not the global.json error message).
- Verdict: PASS.

Details:
```
Downloading .NET SDK 8.0.205 from https://builds.dotnet.microsoft.com/dotnet/Sdk/8.0.205/dotnet-sdk-8.0.205-win-x64.zip...
Installed repo-local .NET SDK 8.0.205 to REDACTED-PATH.
SDK_MARKER=True
8.0.205
EXIT: 0
```
