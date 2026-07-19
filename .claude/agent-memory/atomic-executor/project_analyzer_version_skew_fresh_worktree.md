---
name: analyzer-version-skew-fresh-worktree
description: Fresh TaskMaster worktree fails first analyzer build with CS0006 (missing Meziantou/Sonar/BannedApi analyzer DLLs) — csproj Analyzer Include paths lag packages.config; install the old versions into gitignored packages/
metadata:
  type: project
---

On a clean TaskMaster worktree, the first `msbuild TaskMaster.sln ... /p:EnableNETAnalyzers=true` fails with `error CS0006: Metadata file '..\packages\Meziantou.Analyzer.3.0.101\...\Meziantou.Analyzer.dll' could not be found` (also SonarAnalyzer.CSharp.10.27.0.140913 and Microsoft.CodeAnalysis.BannedApiAnalyzers.3.3.4).

**Why:** This is a pre-existing repo-wide skew on `origin/main` (commits like 097f0ba2 "Bump the analyzers-dev-deps group"): `packages.config` was bumped to newer analyzer versions (Meziantou 3.0.123, BannedApi 5.6.0, Sonar 10.29.0.143774) but the hand-written `<Analyzer Include>` DLL paths in 16 first-party csproj files still reference the OLD versions. On dev/CI machines the old analyzer package folders linger in `packages/`; a clean-worktree restore installs only the current `packages.config` versions, so the old folders are absent -> CS0006.

**How to apply:** Do NOT edit the 16 csproj files (out of scope, huge churn). Install the missing OLD analyzer versions into the gitignored `packages/` folder with the winget nuget.exe: `nuget install Meziantou.Analyzer -Version 3.0.101 -OutputDirectory packages` (repeat for `Microsoft.CodeAnalysis.BannedApiAnalyzers 3.3.4` and `SonarAnalyzer.CSharp 10.27.0.140913`). This touches no tracked files. Also required on a fresh worktree before any build: `pwsh ./scripts/vscode/Install-RepoDotNetSdk.ps1` then `./.dotnet-sdk/dotnet.exe tool restore` (for csharpier 1.2.6) and `pwsh ./scripts/vscode/Invoke-Restore.ps1` (nuget restore). csharpier via repo SDK: `./.dotnet-sdk/dotnet.exe tool run csharpier format|check .`.
