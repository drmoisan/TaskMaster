# Bootstrap: repo-local .NET SDK (issue #839)

Timestamp: 2026-09-13T02-30
Command: pwsh -NoProfile -Command 'if (-not (Test-Path -LiteralPath .dotnet-sdk/sdk/8.0.205)) { & ./scripts/vscode/Install-RepoDotNetSdk.ps1 }; "SDK_MARKER=$(Test-Path -LiteralPath .dotnet-sdk/sdk/8.0.205)"; dotnet --version'
EXIT_CODE: 0

Output Summary:
- The guard found no repo-local SDK in this fresh worktree, so the installer ran and downloaded .NET SDK 8.0.205, installing it into the worktree-local .dotnet-sdk directory (absolute path replaced with REPO-ROOT: REPO-ROOT/.dotnet-sdk).
- SDK_MARKER=True
- dotnet --version printed 8.0.205, which is a version string and not the global.json errorMessage, so the pinned SDK resolves correctly from the worktree root.
