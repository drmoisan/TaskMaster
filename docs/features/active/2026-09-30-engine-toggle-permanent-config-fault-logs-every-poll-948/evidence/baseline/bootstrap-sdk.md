# Bootstrap SDK (P0-T8)

Timestamp: 2026-10-01T23-03
Command: pwsh -NoProfile -Command (guarded) scripts/vscode/Install-RepoDotNetSdk.ps1 when .dotnet-sdk\sdk\8.0.205 is absent; then Test-Path of the marker; then dotnet --version
EXIT_CODE: 0
Output Summary: the marker was absent, so the installer ran and installed SDK 8.0.205; SDK_MARKER=True; dotnet --version printed 8.0.205 (a version string, not the global.json error message).

Transcript (absolute path replaced):

```
Downloading .NET SDK 8.0.205 from https://builds.dotnet.microsoft.com/dotnet/Sdk/8.0.205/dotnet-sdk-8.0.205-win-x64.zip...
Installed repo-local .NET SDK 8.0.205 to REDACTED-PATH\.dotnet-sdk.
SDK_MARKER=True
8.0.205
EXIT=0
```
