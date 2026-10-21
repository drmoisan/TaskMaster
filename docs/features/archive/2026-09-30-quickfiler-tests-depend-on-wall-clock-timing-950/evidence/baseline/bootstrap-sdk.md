# Bootstrap: repository .NET SDK (P0-T4)

Timestamp: 2026-10-02T00-48
Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath "WORKTREE"; [System.IO.Directory]::SetCurrentDirectory((Get-Location).Path); Write-Output ("WORKTREE-LEAF: " + (Split-Path -Leaf (Get-Location).Path)); if (-not (Test-Path -LiteralPath ".dotnet-sdk\sdk\8.0.205")) { & (Join-Path (Get-Location).Path "scripts\vscode\Install-RepoDotNetSdk.ps1") }; "SDK_MARKER=$(Test-Path -LiteralPath ".dotnet-sdk\sdk\8.0.205")"; dotnet --version; "DOTNET_EXIT=$LASTEXITCODE"'
Canonical command: scripts\vscode\Install-RepoDotNetSdk.ps1 (guarded on the .dotnet-sdk\sdk\8.0.205 marker)
EXIT_CODE: 0

Output Summary:
WORKTREE-LEAF: agent-a7805823735145ca4
Downloading .NET SDK 8.0.205 from https://builds.dotnet.microsoft.com/dotnet/Sdk/8.0.205/dotnet-sdk-8.0.205-win-x64.zip...
Installed repo-local .NET SDK 8.0.205 to REDACTED-PATH.
SDK_MARKER=True
8.0.205
DOTNET_EXIT=0

The marker was absent, so the installer ran; the version line is a version string, not the global.json errorMessage text.
