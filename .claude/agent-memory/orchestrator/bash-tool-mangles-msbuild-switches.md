---
name: bash-tool-mangles-msbuild-switches
description: The Bash tool is Git Bash and rewrites MSBuild /switch args into paths (/m becomes M:/, causing MSB1008). Run every C# tool through pwsh -NoProfile, with absolute tool paths, after a nuget restore.
metadata:
  type: project
---

Running msbuild directly through the Bash tool fails with `MSBUILD : error MSB1008: Only one project can be specified` even though the command line looks correct. MSYS path translation rewrites `/t:Build` and `/m` into filesystem paths — the error text shows `M:/` where `/m` was passed.

**Always invoke C# tooling as:** `pwsh -NoProfile -Command "& '<abs-tool-path>' ..."`.

**Verified tool locations in this environment (2026-08-08, VS 18 Community) — none on PATH except `nuget` and `dotnet`:**

- msbuild: `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`
- vstest: `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe`
- csharpier: `<user-profile>\.dotnet\tools\csharpier.exe` (global tool; there is no `.config/dotnet-tools.json`, so `dotnet tool run csharpier` does not work)
- test + coverage: `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -Configuration Debug`

**A fresh worktree needs `nuget restore TaskMaster.sln` first.** Without it the build fails with CS0246 on `log4net` and `SvgDocument` — misleading errors that look like broken source rather than a missing restore.

**PowerShell heredoc caution:** complex `pwsh -NoProfile -Command "..."` strings with nested quotes get mangled by Bash-tool quoting. For anything with embedded XML, escaped quotes, or string concatenation, write a `.ps1` to the scratchpad and run `pwsh -NoProfile -File`.

**How to apply:** put these paths verbatim into the delegation prompt for `atomic-planner`, `atomic-executor`, and preflight, along with measured baseline exit codes. Making the planner encode them into the command tasks stops each downstream agent from rediscovering the MSB1008 failure.
