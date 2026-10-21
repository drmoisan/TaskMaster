# Bootstrap SDK (P0-T9)

Timestamp: 2026-09-30T13-21
Command: pwsh -NoProfile -Command (if the .dotnet-sdk\sdk\8.0.205 marker is absent, run scripts\vscode\Install-RepoDotNetSdk.ps1; print SDK_MARKER; dotnet --version)
EXIT_CODE: 0
Output Summary: The marker was absent, so the installer ran and installed the repo-local .NET SDK 8.0.205 into the worktree .dotnet-sdk folder (installer line printed an absolute path; recorded here as REDACTED-PATH). SDK_MARKER=True. dotnet --version printed 8.0.205 (exit 0), not the global.json error message.

## Observed

- Installer ran: yes (marker absent before the task)
- Installer output: Installed repo-local .NET SDK 8.0.205 to REDACTED-PATH\.dotnet-sdk.
- SDK_MARKER=True
- dotnet --version: 8.0.205
- DOTNET_VERSION_EXIT=0
