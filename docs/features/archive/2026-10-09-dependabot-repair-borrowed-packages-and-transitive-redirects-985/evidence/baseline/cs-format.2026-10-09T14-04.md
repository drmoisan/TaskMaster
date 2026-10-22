# C# Baseline Format (P0-T9)

Timestamp: 2026-10-09T14-04
Command: pwsh -NoProfile -File WORKSPACE-ROOT\scripts\vscode\Install-RepoDotNetSdk.ps1; pwsh -NoProfile -File CMDDIR\985-csharpier.ps1 -WorkspaceRoot WORKSPACE-ROOT -Mode restore; pwsh -NoProfile -File CMDDIR\985-csharpier.ps1 -WorkspaceRoot WORKSPACE-ROOT -Mode check
EXIT_CODE: 0
Output Summary:
- SDK-INSTALL-EXIT_CODE: 0 (Installed repo-local .NET SDK 8.0.205 to WORKSPACE-ROOT\.dotnet-sdk, ignored by .gitignore `.dotnet*/`)
- restore: COMMAND dotnet tool restore; EXIT_CODE 0; Tool 'csharpier' (version '1.2.6') was restored; Restore was successful.
- check: COMMAND dotnet tool run csharpier check .; EXIT_CODE 0; Checked 1650 files in 4288ms.
- Result: C# format baseline green (read-only check); no CS-BASELINE-RED.
