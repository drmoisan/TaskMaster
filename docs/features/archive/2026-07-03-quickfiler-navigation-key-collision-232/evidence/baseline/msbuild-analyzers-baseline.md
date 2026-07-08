# MSBuild Analyzer Baseline (Issue #232)

Timestamp: 2026-07-03T11-30

Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
(invoked from git-bash as `MSBuild.exe TaskMaster.sln -t:Build -p:Configuration=Debug -p:Platform="Any CPU" -p:EnableNETAnalyzers=true -p:EnforceCodeStyleInBuild=true -m -clp:Summary`; MSBuild.exe from Visual Studio 18 Community.)

Pre-step note: A first invocation failed at 1.59s with 35 CS0246 missing-type errors (Fizzler, Svg,
log4net) because the fresh worktree had no restored `packages/` directory. `nuget restore TaskMaster.sln`
was run (exit 0; 169 packages installed to `packages.config` projects), then the build was re-run.

EXIT_CODE: 0

Output Summary: `Build succeeded. 72 Warning(s) 0 Error(s)`. Time Elapsed 00:00:12.64. The full
solution builds clean under .NET analyzers with zero errors. 72 pre-existing analyzer warnings
are the baseline (not promoted to errors in this configuration).
