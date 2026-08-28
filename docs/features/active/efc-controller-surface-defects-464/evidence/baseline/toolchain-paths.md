# Phase 0 — resolved toolchain paths

Timestamp: 2026-08-27T23-18
Task: [P0-T4]
Command: `& "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe"` and `& vswhere.exe -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe"`, both under `pwsh -NoProfile`
EXIT_CODE: 0

## Resolved paths

| Tool | Resolved absolute path | `Test-Path` |
|---|---|---|
| MSBuild | `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe` | True |
| vstest.console | `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe` | True |

Both are the full-framework Visual Studio 18 Community tools. Each `vswhere` query returned exactly one
path; neither result was ambiguous.

Neither resolved path contains a user account name or a machine name, so recording them verbatim
introduces no host-identifying string. No other absolute path is written into this artifact.

Every later task in this plan that invokes MSBuild or `vstest.console.exe` uses these two paths, invoked
from `pwsh -NoProfile` with the worktree root as the working directory.

Output Summary: Both tools resolved through vswhere and both files exist. MSBuild and vstest.console.exe
come from the Visual Studio 18 Community installation.
