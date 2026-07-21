# Baseline Analyzer / Code-Style Build

Timestamp: 2026-07-19T00-25

Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` (VS18 amd64 MSBuild 18.8.2; dash-switch form under MSYS_NO_PATHCONV=1)

EXIT_CODE: 0

Output Summary: Build succeeded. 75 Warning(s), 0 Error(s). Warnings are all pre-existing and unrelated to this feature (CS8632 "nullable annotation outside #nullable context" in UtilitiesCS.Test test files, CS0067 "event never used" in test doubles, NU1902 AngleSharp advisory). No errors.

## Pre-existing environment note (not a source change)

A clean-worktree restore installs only the analyzer versions named in each project's `packages.config` (which was bumped to Meziantou.Analyzer 3.0.123, BannedApiAnalyzers 5.6.0, SonarAnalyzer.CSharp 10.29.0.143774 by main commit 097f0ba2 "Bump the analyzers-dev-deps group"). However, the committed `<Analyzer Include>` DLL paths in 16 first-party csproj files still reference the OLD versions (Meziantou 3.0.101, BannedApiAnalyzers 3.3.4, SonarAnalyzer 10.27.0.140913). This csproj/packages.config version skew is pre-existing on both `origin/main` and the integration branch. On developer/CI machines the old analyzer package folders linger in `packages/`; on a clean worktree the initial build failed with CS0006 (missing analyzer DLLs).

Resolution (no tracked-file change; annotation-only scope preserved): the three missing old-version analyzer packages were installed into the gitignored `packages/` folder via `nuget install <id> -Version <v> -OutputDirectory packages`:
- Meziantou.Analyzer 3.0.101
- Microsoft.CodeAnalysis.BannedApiAnalyzers 3.3.4
- SonarAnalyzer.CSharp 10.27.0.140913

No csproj, packages.config, or source file was modified. This is environment repair for a pre-existing repo-wide condition, flagged for the maintainer.
