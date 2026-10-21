# P0-T4 Command Channel Probe and C# Toolchain Bootstrap

Timestamp: 2026-10-03T08-26
Command: Part 1 probe: pwsh -NoProfile -Command 'Set-Location -LiteralPath "WORKTREE"; Write-Output ("PROBE-OK " + (Test-Path -LiteralPath "TaskMaster.sln"))'. Part 2 payload: Get-FileHash -Algorithm SHA256 over the fifteen files; runsettings lines 5 and 6 trimmed; & pwsh -NoProfile -File scripts\vscode\Install-RepoDotNetSdk.ps1 (absolute path resolved at run time); Test-Path .dotnet-sdk\sdk\8.0.205; dotnet --version; dotnet tool restore; dotnet tool list --local; vswhere resolution of MSBuild.exe and vstest.console.exe; dotnet-coverage --version
EXIT_CODE: 0 (the printed TOOL-RESTORE-EXIT)
Output Summary: channel available; fifteen pre-edit hashes recorded; runsettings Workers 0 and Scope ClassLevel; repo-local SDK 8.0.205 installed; csharpier 1.2.6 restored; MSBuild and vstest resolved; dotnet-coverage 18.10.0 present.

- CHANNEL: COMMAND (PROBE-OK True)
- PRE-EDIT-HASH-A: A619C1A7C1B98F50B39DB066CA8C4F081410AFB2CB587AA9894C919F02D8305B
- PRE-EDIT-HASH-T: B1E3570AF37D4EBCB0118C39DE283F485D4AFF6401F04BFDDC2A50CBD798719E
- PRE-EDIT-HASH-U: E67C57F42FC7CFB8E72CCFE5BBE3E896A46628123DA6F3214612BA4D86F3B634
- PRE-EDIT-HASH-S: D81EF7DA1573DAC2F6D6392F7BB07D46FCC930D83B53832291D54A48659BBA61
- PRE-EDIT-HASH-M: 94F7FADEF4160906B86F566F83011E5313E22F308BBA37103D21F2F48A3CCF8C
- PRE-EDIT-HASH-L: 11AB72C2F5F2C5BBA3D4E128ED9FDEC602109FD9634056AC482DBB670541CAF0
- PRE-EDIT-HASH-E: E40AB978F8E0C7242873F2120F0B27EE1D2F998472C48CE814D6BD6CA571437A
- PRE-EDIT-HASH-TD: A2F40F61F322AB89788E27D258C5D142E2C42204F12EEBD497043B161743D02F
- PRE-EDIT-HASH-TST1: 791E2B9E00C565C428939F0A3A26B8DE4A613AC492B5D16F80A2CD9627F2957E
- PRE-EDIT-HASH-TST2: BFACBB41EEEC99935D7FB00B4AC94BFD2C810BAFE4C27F9D4B40DFE85F9B0645
- PRE-EDIT-HASH-UCS: CACDCBA7D5434211051070F8979E773B2FF299A7731BDCAB01A627A14740A356
- PRE-EDIT-HASH-UCT: 15642247EC8CC12A19943269707A527E7D8B0DC5C905F27E99A0CEF0E2C7BE8E
- PRE-EDIT-HASH-QFT: 5722B9EB448B5ACAE5D3D590B2E18DC880014634219E3557FB21C45DAB89338D
- PRE-EDIT-HASH-SPEC956: 70AC27CB445EABE59FF501DEEB778441BBFB42C9A5B90BAB8A5AFA4F0C5EC36C
- RUNSETTINGS-HASH: 98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57
- RUNSETTINGS-WORKERS-LINE: <Workers>0</Workers>
- RUNSETTINGS-SCOPE-LINE: <Scope>ClassLevel</Scope>
- Installer output: Downloading .NET SDK 8.0.205 from https://builds.dotnet.microsoft.com/dotnet/Sdk/8.0.205/dotnet-sdk-8.0.205-win-x64.zip... / Installed repo-local .NET SDK 8.0.205 to <repo-root>\.dotnet-sdk.
- SDK-INSTALL-EXIT: 0 (observation)
- SDK-MARKER: True
- dotnet --version: 8.0.205
- DOTNET-VERSION-EXIT: 0
- dotnet tool restore: Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier / Restore was successful.
- TOOL-RESTORE-EXIT: 0
- dotnet tool list --local: csharpier 1.2.6 csharpier (manifest <repo-root>\dotnet-tools.json)
- TOOL-LIST-EXIT: 0
- MSBUILD-RESOLVED: YES
- VSTEST-RESOLVED: YES
- dotnet-coverage --version: 18.10.0+f4cc39224845ffa74bf246c9da2399d50e5d6342
- DOTNET-COVERAGE-EXIT: 0 (no install was needed; the version line is the discriminating half)

Acceptance check: CHANNEL COMMAND; fifteen 64-character hexadecimal hashes; runsettings Workers and Scope lines as required; SDK-MARKER True; DOTNET-VERSION-EXIT 0 and TOOL-RESTORE-EXIT 0; csharpier 1.2.6; MSBUILD-RESOLVED YES and VSTEST-RESOLVED YES; DOTNET-COVERAGE-EXIT 0 with version recorded. All eight hold.
