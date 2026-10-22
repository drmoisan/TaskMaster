# Phase 0 — Toolchain Probe

Timestamp: 2026-08-26T10-42
Task: [P0-T3]
Command: `pwsh -NoProfile -Command '$vsw = "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe"; $mb = & $vsw -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; $vt = & $vsw -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; foreach ($n in @("dotnet","dotnet-coverage","nuget","gh")) { Get-Command $n -ErrorAction SilentlyContinue }'`
EXIT_CODE: 0

MSBUILD: C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe
VSTEST: C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe
DOTNET: C:\Program Files\dotnet\dotnet.exe
DOTNET_COVERAGE: <user-profile>\.dotnet\tools\dotnet-coverage.exe
NUGET: <user-profile>\AppData\Local\Microsoft\WinGet\Packages\Microsoft.NuGet_Microsoft.Winget.Source_8wekyb3d8bbwe\nuget.exe
GH: C:\Program Files\GitHub CLI\gh.exe

## Output Summary

All six required tools resolved. No `NOT_FOUND` value was recorded, so no downstream task is
blocked by a missing tool.

- `vswhere.exe` was confirmed present at
  `C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe` and returned Visual
  Studio 18 Community for both the MSBuild and the test-platform lookups.
- The two `C:\Program Files` and `C:\Program Files (x86)` paths are machine paths and are
  recorded verbatim.
- `DOTNET_COVERAGE` and `NUGET` resolve under the host account profile, so their profile prefix
  is replaced with the literal token `<user-profile>`.
- `GH` resolving to a real path means the [P7-T4] CFN-4 promotion path is available and the
  `PROMOTION BLOCKED` fallback branch of that task is not expected to be taken.
- The exit code recorded above is the native exit status of the `vswhere.exe` invocation,
  captured by a separate `exit $LASTEXITCODE` probe. The combined probe command ends in
  PowerShell cmdlets rather than a native process, so `$LASTEXITCODE` renders empty at the end
  of that pipeline; that is a property of the shell, not a failure signal.
