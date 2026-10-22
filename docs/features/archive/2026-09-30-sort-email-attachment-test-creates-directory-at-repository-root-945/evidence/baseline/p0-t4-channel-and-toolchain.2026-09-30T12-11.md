# P0-T4 command channel probe and C# toolchain bootstrap

Timestamp: 2026-09-30T12-11
Command: pwsh -NoProfile -Command (Set-Location to the worktree root; PROBE-OK probe); Get-FileHash -Algorithm SHA256 on three files; pwsh -NoProfile -File scripts\vscode\Install-RepoDotNetSdk.ps1; dotnet --version; dotnet tool restore; dotnet tool list --local; vswhere resolution of MSBuild.exe and vstest.console.exe; dotnet-coverage --version
EXIT_CODE: 0

Output Summary:
CHANNEL: COMMAND (line `PROBE-OK True` observed)
RUNSETTINGS-HASH: 98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57
PRE-EDIT-HASH-SORTEMAIL: FBB07E251FAC8C3BA488FADBD9A8DD65B7C100ECE26F3231B24A44014D1B1448
PRE-EDIT-HASH-TST: 440D5A99678FE7B8093BE56388364448A0A9B813C47E2DF79E7391C32E37087C
Install-RepoDotNetSdk.ps1: exit 0 (installed repo-local .NET SDK 8.0.205 into .dotnet-sdk)
SDK-MARKER: YES (.dotnet-sdk\sdk\8.0.205 exists)
dotnet --version: 8.0.205 (exit 0)
dotnet tool restore: exit 0 (csharpier 1.2.6 restored)
dotnet tool list --local: csharpier row reads 1.2.6
MSBUILD-RESOLVED: YES (18\Community\MSBuild\Current\Bin\MSBuild.exe)
VSTEST-RESOLVED: YES (18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe)
dotnet-coverage --version: exit 0, 18.10.0+f4cc39224845ffa74bf246c9da2399d50e5d6342 (already installed; no install performed)
