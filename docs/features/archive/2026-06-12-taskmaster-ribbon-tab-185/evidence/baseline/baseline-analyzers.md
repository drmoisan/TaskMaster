# Baseline — Analyzer Build (Issue #185)

Timestamp: 2026-06-12T10-38

Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true

Environment note: This is a fresh worktree. The first build attempt failed with 35 errors
(CS0006/missing-package + missing analyzer DLLs) because packages.config NuGet packages were
not restored. `nuget restore TaskMaster.sln` installed 168 packages (environment setup,
mechanically necessary to run the baseline; not a scope change). The build was then re-run.

EXIT_CODE: 0

Output Summary: Build succeeded. 0 Error(s), 62 Warning(s). Warnings are pre-existing
CS8632 (nullable annotation outside #nullable context) and CS0067 (event never used) in
test projects (TaskMaster.Test, UtilitiesCS.Test), unrelated to the in-scope ribbon files.
Baseline analyzer gate passes.
