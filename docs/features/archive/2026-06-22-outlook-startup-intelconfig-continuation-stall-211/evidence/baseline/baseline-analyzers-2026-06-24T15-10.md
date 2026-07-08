# Baseline — Analyzer Build (issue #211)

Timestamp: 2026-06-24T15-10

Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
(executed via git-bash with dash-switches: `MSBuild.exe TaskMaster.sln -t:Build -p:Configuration=Debug -p:Platform="Any CPU" -p:EnableNETAnalyzers=true -p:EnforceCodeStyleInBuild=true`)

EXIT_CODE: 0

Output Summary:
- Result: `Build succeeded. 0 Warning(s) 0 Error(s)`.
- Baseline analyzer build is clean prior to any edit.
