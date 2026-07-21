# Baseline — Analyzer Build (P0-T8)

Timestamp: 2026-07-09T21-56

Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
(executed via VS18 MSBuild 18.7.8 with dash-switch form and MSYS_NO_PATHCONV=1 under git-bash)
EXIT_CODE: 0

Output Summary: `Build succeeded. 75 Warning(s) 0 Error(s)`. The 75 warnings are pre-existing
(CS8632 nullable-annotation-context and CS0067 unused-event warnings in UtilitiesCS.Test and
other projects); none in `Tags` or `Tags.Test`. Baseline analyzer gate is green.
