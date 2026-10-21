# Phase 0 — Toolchain Path Resolution

Timestamp: 2026-08-26T08-27
Task: [P0-T4]

Command:
`pwsh -NoProfile -Command '$vsw = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; & $vsw -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe"; & $vsw -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe"'`

EXIT_CODE: 0

## Resolved paths

| Tool | Resolved path | Exists |
|---|---|---|
| `vswhere.exe` | `C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe` | True |
| `MSBuild.exe` | `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe` | True |
| `vstest.console.exe` | `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe` | True |

Existence was confirmed with `Test-Path` on each resolved path; all three returned `True`.

## Path-hygiene note

The three paths above are machine-wide Visual Studio installation paths under `C:\Program Files` and
`C:\Program Files (x86)`. None contains a user account name, a user profile directory, or a machine name.
No other absolute path is recorded in this artifact.

## Invocation convention for the rest of this plan

Every `msbuild` and `vstest.console.exe` invocation runs under `pwsh -NoProfile` with the absolute paths
above (decision D4). A bash shell mangles MSBuild's `/m` switch into a drive-letter path and the build
fails with MSB1008. Neither `msbuild`, `vstest.console.exe`, nor `vswhere.exe` is on `PATH` in this
environment.

Output Summary: MSBuild 18 (Community) and its bundled `vstest.console.exe` both resolved through
`vswhere.exe` and both exist on disk.
