---
name: fresh-worktree-nuget-restore-and-csharpier-v1
description: A fresh worktree has no packages/ and msbuild will NOT restore packages.config projects, so every build/test task dies at PrepareForBuild unless the plan runs Invoke-Restore.ps1; also CLAUDE.md's `csharpier .` is v0 syntax that fails against the pinned 1.2.6
metadata:
  type: reference
---

Two toolchain facts that break C# plans in a newly created worktree. Both were caught by
`atomic-executor` preflight on epic child F9 (#452), not by planning.

**1. NuGet restore is mandatory and is not implied by anything else.**
`packages/` is gitignored and absent from a new worktree. The legacy `packages.config` projects
declare `EnsureNuGetPackageBuildImports` with `BeforeTargets="PrepareForBuild"` and hard `<Error>`
elements (see `QuickFiler.Test/QuickFiler.Test.csproj:439-447`). `msbuild /t:Build` does **not**
restore `packages.config` projects, and `dotnet tool restore` restores only the csharpier manifest.
Without an explicit restore, every `msbuild` and `vstest` task fails before compiling.

Bootstrap order that works:
1. `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\vscode\Install-RepoDotNetSdk.ps1`
2. `dotnet tool restore` — the manifest is `dotnet-tools.json` at the **repo root**, not `.config/`
3. `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\vscode\Invoke-Restore.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU"`
   (this wraps `msbuild /t:Restore /p:RestorePackagesConfig=true`)

**2. `csharpier .` is v0 syntax and fails against the pinned 1.2.6.**
`CLAUDE.md` and the C# policy still say `csharpier .` / `dotnet tool run csharpier .`. The pinned
version is 1.2.6 (`dotnet-tools.json`, `rollForward: false`). The working forms, verified in
`.vscode/tasks.json:54-66`, are `dotnet tool run csharpier format .` and
`dotnet tool run csharpier check .`.

**How to apply:** Put all three bootstrap commands in Phase 0 of any C# plan that will run in a fresh
or agent worktree, and use the v1 csharpier syntax. Also note `msbuild` and `vstest.console.exe`
resolve via `vswhere`, not PATH, so plans should resolve and record their absolute paths in Phase 0.
Record the csharpier deviation as a documented correction — do not silently follow CLAUDE.md's literal
text, and do not "fix" CLAUDE.md as a side effect.
