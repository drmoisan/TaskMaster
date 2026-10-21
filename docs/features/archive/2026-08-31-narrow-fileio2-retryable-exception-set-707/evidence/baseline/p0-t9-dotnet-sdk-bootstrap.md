Timestamp: 2026-09-03T11-59
Command: Test-Path .dotnet-sdk/dotnet.exe ; pwsh -File scripts/vscode/Install-RepoDotNetSdk.ps1 ; dotnet --version
EXIT_CODE: 0

OBSERVED_DOTNET_SDK_PRESENT: False
BOOTSTRAP_EXIT_CODE: 0
Bootstrap output: "Downloading .NET SDK 8.0.205 ... Installed repo-local .NET SDK 8.0.205 to <worktree>\.dotnet-sdk."

dotnet --version EXIT_CODE: 0
dotnet --version output: 8.0.205

Output Summary: Repo-local SDK was absent, bootstrapped successfully (exit 0), dotnet --version now reports 8.0.205 matching global.json.
