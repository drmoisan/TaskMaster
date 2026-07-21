# Baseline — Analyzer Build (P0-T3)

Timestamp: 2026-07-09T16-33
Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
(git-bash invocation uses dash-form switches: -t:Build -p:... ; MSYS mangles /-switches)
EXIT_CODE: 0
Output Summary: Build succeeded. 0 Error(s), 75 Warning(s). All 75 warnings are pre-existing and reside in files this feature does not touch (UtilitiesCS.Test CS8632 nullable-annotation-context and CS0067 unused-event warnings). No TaskTree production warnings at baseline. Analyzer gate is green.
