Timestamp: 2026-06-24T16-25
Command: pwsh -File scripts/vscode/Install-RepoDotNetSdk.ps1; pwsh -File scripts/vscode/Invoke-Restore.ps1; resolve vstest.console.exe through vswhere; verify C:\Users\DanMoisan\.dotnet\tools\dotnet-coverage.exe
EXIT_CODE: 0
Output Summary:
- Repo-local .NET SDK verification passed; active SDK: 8.0.205.
- Restore command completed successfully with 0 warnings and 0 errors.
- VSTest resolved to C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe.
- dotnet-coverage resolved to C:\Users\DanMoisan\.dotnet\tools\dotnet-coverage.exe and exists.
