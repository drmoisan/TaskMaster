# dotnet-coverage and Visual Studio Tool Resolution (P0-T8)

Timestamp: 2026-09-29T08-54
Command: pwsh -NoProfile -Command 'Set-Location "<repo-root>"; if (-not (Get-Command dotnet-coverage -ErrorAction SilentlyContinue)) { $env:Path = (Join-Path $env:USERPROFILE ".dotnet/tools") + ";" + $env:Path }; if (-not (Get-Command dotnet-coverage -ErrorAction SilentlyContinue)) { dotnet tool install --global dotnet-coverage }; "DOTNET-COVERAGE=" + [bool](Get-Command dotnet-coverage -ErrorAction SilentlyContinue); dotnet-coverage --version; $m = & (Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio/Installer/vswhere.exe") -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; "MSBUILD-TAIL=" + $m.Substring($m.IndexOf("Microsoft Visual Studio")); $v = & (Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio/Installer/vswhere.exe") -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; "VSTEST-TAIL=" + $v.Substring($v.IndexOf("Microsoft Visual Studio"))'
EXIT_CODE: 0
Output Summary:
- DOTNET-COVERAGE=True (already resolvable; no install was performed)
- dotnet-coverage --version: 18.10.0+f4cc39224845ffa74bf246c9da2399d50e5d6342
- MSBUILD-TAIL=Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe
- VSTEST-TAIL=Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe
- Both tails are non-empty and end with MSBuild.exe and vstest.console.exe respectively; the installation root is omitted.
