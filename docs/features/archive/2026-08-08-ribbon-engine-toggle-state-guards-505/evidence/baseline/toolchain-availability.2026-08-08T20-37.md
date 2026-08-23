# P0-T1 — Toolchain Availability

Timestamp: 2026-08-08T20-37

Command:

```
pwsh -NoProfile -Command "Set-Location 'C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a406ae4b7a2ce151f'; Test-Path 'C:\Users\DanMoisan\.dotnet\tools\csharpier.exe'; Test-Path 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe'; Test-Path 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe'; (Get-Command nuget -ErrorAction SilentlyContinue) -ne $null; (Get-Command dotnet-coverage -ErrorAction SilentlyContinue) -ne $null"
```

EXIT_CODE: 0

(`$LASTEXITCODE` was unset because the command invokes only cmdlets, no external
process. The `pwsh` process itself exited 0.)

Output Summary:

| Tool | Path / probe | Resolves |
|---|---|---|
| csharpier | `C:\Users\DanMoisan\.dotnet\tools\csharpier.exe` | True |
| msbuild | `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe` | True |
| vstest.console | `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe` | True |
| nuget | `Get-Command nuget` | True |
| dotnet-coverage | `Get-Command dotnet-coverage` | True |

All five tools resolve `True` in the final recorded state. No install step was
required; `dotnet-coverage` was already present as a global tool.

Binary outcome: PASS.
