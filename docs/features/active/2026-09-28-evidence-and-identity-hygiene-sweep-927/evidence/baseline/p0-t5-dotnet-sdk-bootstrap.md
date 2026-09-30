# P0-T5 Repo-local .NET SDK bootstrap

Timestamp: 2026-09-29T08-53
Command: pwsh -NoProfile -Command 'if (-not (Test-Path ".dotnet-sdk/sdk")) { & ./scripts/vscode/Install-RepoDotNetSdk.ps1 }; "DOTNET-VERSION=" + (dotnet --version); "SDK-DIR=" + (Test-Path ".dotnet-sdk/sdk")'
EXIT_CODE: 0
Output Summary:
- The SDK directory was absent, so the installer ran: "Downloading .NET SDK 8.0.205 ..." then "Installed repo-local .NET SDK 8.0.205 to <repo-root>\.dotnet-sdk."
- DOTNET-VERSION=8.0.205
- SDK-DIR=True
- The .dotnet-sdk tree is ignored and is not staged.
