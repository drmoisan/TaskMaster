# C# Toolchain Bootstrap (issue #781)

Timestamp: 2026-09-05T16-16

Task: [P0-T4]

All three invocations were issued from inside a single `pwsh -NoProfile -Command` process whose
working directory is the repository root.

## Invocation 1 — restore the manifest-pinned tools

Command: `dotnet tool restore`

EXIT_CODE: 0

## Invocation 2 — resolve MSBuild and vstest.console.exe through vswhere

Command: `& "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -find 'MSBuild\**\Bin\MSBuild.exe'`
and `& "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe'`

EXIT_CODE: 0 (both `-find` queries)

## Invocation 3 — prove the coverage collector

Command: `dotnet-coverage --version`

EXIT_CODE: 0

Output Summary: All three invocations exited 0.

- `dotnet tool restore` restored CSharpier version 1.2.6, which is the version
  `dotnet-tools.json` pins and the version `.github/workflows/_format-check.yml` runs. Reported
  result: "Tool 'csharpier' (version '1.2.6') was restored." followed by "Restore was
  successful."
- Resolved MSBuild path:
  `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`
  (Visual Studio 18 Community; the file exists).
- Resolved `vstest.console.exe` path:
  `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`
  (the file exists). This is the executable [P1-T4], [P1-T9], and [P2-T5] invoke.
- `dotnet-coverage` version string: `18.10.0+f4cc39224845ffa74bf246c9da2399d50e5d6342`.

Both resolved paths lie under the machine-independent `Program Files` install root and carry no
operating-system account name or machine name, so recording them does not breach the repository
rule against embedding host-identifying paths in a tracked artifact.
