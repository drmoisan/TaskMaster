# C# Analyzer Build Baseline (Issue #283)

Timestamp: 2026-07-08T17-56
Command: `msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
EXIT_CODE: 0

Output Summary:
- Build succeeded (exit 0). 0 errors.
- Prerequisite: a fresh-worktree NuGet restore was required first (`scripts/vscode/Invoke-Restore.ps1`, 169 packages installed, exit 0). Before restore the build failed with CS0006 metadata-file-not-found for Roslynator/AsyncFixer/BannedApiAnalyzers DLLs and CS0246 for vendored Svg/Fizzler/log4net. After restore the analyzer build succeeds.
- Pre-existing warnings (NOT errors; not promoted to errors under this gate because `TreatWarningsAsErrors` is not set here): numerous `CS8632` (nullable annotation outside `#nullable` context) in existing `TaskMaster.Test` and `UtilitiesCS.Test` files, plus a few `CS0067` (event never used) in `UtilitiesCS.Test`. None of these files are touched by this fix. This is the pre-existing baseline warning set.
- The new seam file (P1-T1) uses `#nullable enable` at file top, which avoids introducing new CS8632 warnings.
