# Bootstrap: Repository .NET SDK (P0-T5)

Timestamp: 2026-10-01T17-37
Task: P0-T5
Command: pwsh -NoProfile -Command (guarded) scripts\vscode\Install-RepoDotNetSdk.ps1 when .dotnet-sdk\sdk\8.0.205 is absent; then dotnet --version
EXIT_CODE: 0

Output Summary:
- INSTALLER_RAN=True (the marker was absent in this fresh worktree)
- Installer output: "Downloading .NET SDK 8.0.205 ..." then "Installed repo-local .NET SDK 8.0.205 to <WORKTREE>\.dotnet-sdk."
- SDK_MARKER=True
- dotnet --version: 8.0.205 (a version string, not the global.json error message)
- DOTNET_VERSION_EXIT=0
- Result: P0-T5 acceptance holds.
