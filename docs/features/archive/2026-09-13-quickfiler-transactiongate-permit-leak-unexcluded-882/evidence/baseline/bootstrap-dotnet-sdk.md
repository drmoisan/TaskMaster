# Repository-Pinned .NET SDK Provisioning (P0-T4)

Timestamp: 2026-09-29T08-52
Command: pwsh -NoProfile -WorkingDirectory "<repo-root>" -File "<repo-root>/scripts/vscode/Install-RepoDotNetSdk.ps1" ; then pwsh -NoProfile -Command 'Set-Location "<repo-root>"; dotnet --version; dotnet --list-sdks; "SDK-MARKER=" + (Test-Path -LiteralPath .dotnet-sdk/sdk/8.0.205)'
EXIT_CODE: 0
Output Summary:
- Install script EXIT_CODE: 0; console line: "Installed repo-local .NET SDK 8.0.205 to <repo-root>\.dotnet-sdk."
- SDK-STATE: INSTALLED
- SDK-MARKER=True
- dotnet --version: 8.0.205
- dotnet --list-sdks line for the repository SDK: 8.0.205 [<repo-root>\.dotnet-sdk\sdk]
- dotnet --list-sdks also lists the machine-wide 10.0.401 SDK under the Program Files dotnet directory; global.json selects 8.0.205.
- Note: the script path was passed as an absolute worktree path (rendered here as <repo-root>) because the script resolves its install directory from its own location, and pwsh resolves a -File path against the caller's directory rather than -WorkingDirectory.
