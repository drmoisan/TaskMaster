# P0-T4 Command channel probe, toolchain bootstrap and pre-edit hashes

Timestamp: 2026-10-01T20-38
Command: Part 1: pwsh -NoProfile -Command 'Set-Location -LiteralPath "WORKTREE"; Write-Output ("PROBE-OK " + (Test-Path -LiteralPath "TaskMaster.sln"))'
Part 2 (one payload): Get-FileHash -Algorithm SHA256 -LiteralPath on the six files; lines 5 and 6 of scripts\vscode\TaskMaster.cli.runsettings trimmed; & pwsh -NoProfile -File (Join-Path (Get-Location).Path "scripts\vscode\Install-RepoDotNetSdk.ps1"); Test-Path -LiteralPath ".dotnet-sdk\sdk\8.0.205"; dotnet --version; dotnet tool restore; dotnet tool list --local; vswhere resolution of MSBuild.exe and vstest.console.exe. Then, in a separate invocation, dotnet-coverage --version.
EXIT_CODE: 0
Output Summary:
CHANNEL: COMMAND (PROBE-OK True)
PRE-EDIT-HASH-SRC: 195BABDB966DFB24CEF1C8F7681B59B8CDBA84C1C045B57FB8F8060FE048708B
PRE-EDIT-HASH-TST: 791E2B9E00C565C428939F0A3A26B8DE4A613AC492B5D16F80A2CD9627F2957E
PRE-EDIT-HASH-UCS-CSPROJ: 753140B279E84BE107D47B91BB33DF7D582028BC096EC27305BF4E1BFFA533F4
PRE-EDIT-HASH-UCT-CSPROJ: B17BF70F2D104F820424BCE79360F07716C5A3CCF6404FDE88E52B9A051A8ACE
PRE-EDIT-HASH-YESNOTOALL: 86A8A356B4905936F20DEDB7BBF501998E6B92487410CA91FCA0ADBB9DC81C71
RUNSETTINGS-HASH: 98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57
RUNSETTINGS-WORKERS-LINE: <Workers>0</Workers>
RUNSETTINGS-SCOPE-LINE: <Scope>ClassLevel</Scope>
SDK-INSTALL: Downloading .NET SDK 8.0.205 ... Installed repo-local .NET SDK 8.0.205 to <repo-root>\.dotnet-sdk. (exit 0)
SDK-MARKER: True
dotnet --version: 8.0.205 (exit 0)
dotnet tool restore: Tool 'csharpier' (version '1.2.6') was restored. Restore was successful. (exit 0)
dotnet tool list --local: csharpier 1.2.6 csharpier <repo-root>\dotnet-tools.json
MSBUILD-RESOLVED: YES
VSTEST-RESOLVED: YES
dotnet-coverage --version: 18.10.0+f4cc39224845ffa74bf246c9da2399d50e5d6342 (exit 0; already installed, no install needed)
Acceptance: all eight conditions hold.
