# Phase 0 — Resolved Toolchain Paths ([P0-T4])

Timestamp: 2026-08-28T05-10

Command: `vswhere.exe -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe"`
and `vswhere.exe -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe"`,
both invoked under `pwsh -NoProfile` with `vswhere.exe` located under the Visual Studio Installer
directory beneath the 32-bit Program Files root.
EXIT_CODE: 0

## Resolved paths

| Tool | Resolved path | Exists | Version |
| --- | --- | --- | --- |
| MSBuild | `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe` | True | 18.9.1.35102 |
| vstest.console | `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe` | True | VSTest version 18.9.0 (x64) |

Both resolved files were confirmed present with `Test-Path`, which returned `True` for each, and both
responded to a version probe, which establishes that they are executable rather than merely present.

## Notes carried into the remaining tasks

- These are the Visual Studio **18** full-framework tools. The repository's projects are legacy
  non-SDK `packages.config` VSTO / .NET Framework projects, so the full-framework `MSBuild.exe`
  resolved here is the correct driver; a .NET SDK `dotnet build` is not a substitute for it.
- A second `vstest.console.exe` also exists on this machine at
  `Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe` and was confirmed present.
  The plan's stated `vswhere` `-find` argument selects the `Extensions\TestPlatform` path, and that is
  the path recorded above and used by every test task in this plan. Recording the second path here is
  a disambiguation note, not a second resolution.
- Neither `msbuild`, nor `vstest.console.exe`, nor `vswhere.exe` is on `PATH` in this environment, and
  every invocation in this plan supplies the absolute path resolved above, under `pwsh -NoProfile`,
  per decision D-4.

## Host-path hygiene

Neither resolved path contains a user account name, a home directory, or a machine name. Both sit
under the machine-independent `C:\Program Files\Microsoft Visual Studio\18\Community\` installation
root. No other absolute path is recorded in this artifact.

Output Summary: Both tools resolved and both exist. MSBuild 18.9.1.35102 at
`C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`;
vstest.console 18.9.0 at
`C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`.
