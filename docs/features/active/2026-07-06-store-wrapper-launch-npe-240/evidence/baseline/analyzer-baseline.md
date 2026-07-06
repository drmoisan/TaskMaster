# Analyzer Build Baseline (Issue #240)

Timestamp: 2026-07-06T07-10

Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
(Invoked as: `MSBuild.exe TaskMaster.sln -t:Build -p:Configuration=Debug -p:Platform="Any CPU" -p:EnableNETAnalyzers=true -p:EnforceCodeStyleInBuild=true -nologo -clp:Summary`, dash-switch form required by the git-bash shell.)

EXIT_CODE: 0

Output Summary: Build succeeded. 72 warning(s), 0 error(s). Pre-existing warnings include MSTEST0032 (QuickFiler.Test) and multiple CS8632 nullable-annotation-context warnings in TaskMaster.Test. No errors in `UtilitiesCS` or `UtilitiesCS.Test`. A NuGet restore (`scripts/vscode/Invoke-Restore.ps1`, 169 packages) was required first because the repo `packages/` folder was absent.
