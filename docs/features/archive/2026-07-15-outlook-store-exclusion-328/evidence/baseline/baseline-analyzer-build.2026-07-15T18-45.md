# Baseline — Analyzer Build (Issue #328)

Timestamp: 2026-07-15T18-45
Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
EXIT_CODE: 0
Output Summary: Build succeeded. 76 Warning(s), 0 Error(s). Warnings are pre-existing
(predominantly CS8632 nullable-annotation-outside-#nullable-context and CS0067
never-used-event, concentrated in UtilitiesCS.Test). No analyzer errors at baseline.

Note: Executed with git-bash dash-switch form of the same command
(-t:Build -p:Configuration=Debug -p:Platform="Any CPU" -p:EnableNETAnalyzers=true
-p:EnforceCodeStyleInBuild=true) per repo git-bash MSBuild convention.
