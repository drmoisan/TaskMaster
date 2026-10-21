# Command Channel and C# Toolchain Bootstrap (P0-T4)

Timestamp: 2026-09-30T07-14
Task: P0-T4
Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath "WORKTREE"; Write-Output ("PROBE-OK " + (Test-Path -LiteralPath "TaskMaster.sln"))'; then, in payloads: Get-FileHash -Algorithm SHA256 -LiteralPath <six paths>; & pwsh -NoProfile -File scripts\vscode\Install-RepoDotNetSdk.ps1 (absolute script path resolved at run time); Test-Path .dotnet-sdk\sdk\8.0.205; dotnet --version; dotnet tool restore; dotnet tool list --local; vswhere resolution of MSBuild.exe and vstest.console.exe; dotnet-coverage --version
EXIT_CODE: 0
Output Summary: channel available (PROBE-OK True); six hashes recorded; SDK 8.0.205 installed; csharpier 1.2.6 restored; MSBuild and vstest resolved; dotnet-coverage 18.10.0 present (no install needed).

## Part 1: channel

- PROBE-OUTPUT: PROBE-OK True
- CHANNEL: COMMAND

## Part 2: hashes

- RUNSETTINGS-HASH: 98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57
- PRE-EDIT-HASH-PFS: FD50F380509E730337CD342FC9C56989C41802378D2F773A221E59DBA7B90CF2
- PRE-EDIT-HASH-DIW: DF1DFBBB4CD8806B029A99B41FDBCDEAFF7AD46D54177D4FAC081ACE728EB4B7
- PRE-EDIT-HASH-PDA: FBAC7002BE4DF5979624F8AE649D52F1044BC40D54EBDA4618C6374EAB9CB242
- PRE-EDIT-HASH-PFA: 0337E5C1FF7A2E3FF5E5D67D383E68583838AFB5E5BA999A30502CF24333D948
- PRE-EDIT-HASH-DIWP: F77616271ABB36C9133ADE496550703B391B077AB68C8F5DC652AB73FD177DC3

## Part 2: toolchain

- INSTALL-SDK-EXIT: 0 (output: `Installed repo-local .NET SDK 8.0.205 to <repo-root>\.dotnet-sdk.`)
- SDK-MARKER: YES
- DOTNET-VERSION: 8.0.205 (exit 0)
- DOTNET-TOOL-RESTORE-EXIT: 0 (`Tool 'csharpier' (version '1.2.6') was restored.`, `Restore was successful.`)
- DOTNET-TOOL-LIST-CSHARPIER-ROW: csharpier 1.2.6 csharpier <repo-root>\dotnet-tools.json
- MSBUILD-RESOLVED: YES (18\Community\MSBuild\Current\Bin\MSBuild.exe)
- VSTEST-RESOLVED: YES (18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe)
- DOTNET-COVERAGE-VERSION: 18.10.0+f4cc39224845ffa74bf246c9da2399d50e5d6342 (exit 0; already installed, no global install run)
