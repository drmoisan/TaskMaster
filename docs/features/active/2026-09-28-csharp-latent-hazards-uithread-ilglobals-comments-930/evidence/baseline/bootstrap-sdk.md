# Bootstrap: repository .NET SDK ([P0-T4])

Timestamp: 2026-09-29T08-52
Command: pwsh -NoProfile -Command 'if (-not (Test-Path -LiteralPath .dotnet-sdk/sdk/8.0.205)) { & ./scripts/vscode/Install-RepoDotNetSdk.ps1 }; "SDK_MARKER=$(Test-Path -LiteralPath .dotnet-sdk/sdk/8.0.205)"; dotnet --version'
EXIT_CODE: 0
Output Summary:
- Attempt 1 (2026-09-29T08-52) failed before completing: the installer's shared temporary download file under USER-PROFILE was locked by a concurrent sibling worktree's download ("being used by another process"); the marker was not created. Once the lock cleared (the file was absent on the next check), the same command was re-run unchanged. This was a shared-resource contention, not a retry of a gate.
- Attempt 2: "Downloading .NET SDK 8.0.205 ..." then "Installed repo-local .NET SDK 8.0.205 to REPO-ROOT\.dotnet-sdk."
- SDK_MARKER=True
- dotnet --version: 8.0.205 (a version string, not the global.json error message)
