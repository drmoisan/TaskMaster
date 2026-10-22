# P0-T11 repo-local .NET SDK (issue #973)

Timestamp: 2026-10-03T10-48
Command: pwsh -NoProfile -File <execution-worktree-root>/scripts/vscode/Install-RepoDotNetSdk.ps1; pwsh -NoProfile -Command 'Set-Location -LiteralPath "<execution-worktree-root>"; dotnet --version; dotnet --list-sdks'; pwsh -NoProfile -Command 'Test-Path -LiteralPath "<execution-worktree-root>/.dotnet-sdk/sdk/8.0.205"'; git -C <execution-worktree-root> status --porcelain --untracked-files=all -- .dotnet-sdk
EXIT_CODE: 0
Output Summary: SDK 8.0.205 downloaded and installed under the git-ignored .dotnet-sdk folder; dotnet --version prints 8.0.205; the marker folder exists; porcelain over .dotnet-sdk is empty.

Install output (C4):
Downloading .NET SDK 8.0.205 from https://builds.dotnet.microsoft.com/dotnet/Sdk/8.0.205/dotnet-sdk-8.0.205-win-x64.zip...
Installed repo-local .NET SDK 8.0.205 to <execution-worktree-root>\.dotnet-sdk.

DOTNET-VERSION: 8.0.205
LIST-SDKS (recorded, not gated):
8.0.205 [<execution-worktree-root>\.dotnet-sdk\sdk]
10.0.401 [machine-wide SDK folder]
TEST-PATH .dotnet-sdk/sdk/8.0.205: True
PORCELAIN .dotnet-sdk: (empty)
