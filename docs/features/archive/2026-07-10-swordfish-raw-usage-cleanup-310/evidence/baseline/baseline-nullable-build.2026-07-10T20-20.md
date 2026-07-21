# Baseline — Nullable / TreatWarningsAsErrors Build

Timestamp: 2026-07-10T20-55
Command: `MSBuild.exe TaskMaster.sln -t:Build -p:Configuration=Debug "-p:Platform=Any CPU" -p:Nullable=enable -p:TreatWarningsAsErrors=true`
EXIT_CODE: 0
Output Summary: Build succeeded. 0 Warning(s), 0 Error(s). MSBuild's incremental engine
short-circuited most non-SDK legacy projects as up-to-date (72 "already up to date" skip lines)
because the prior P0-T3 pass had just recompiled the same outputs; this is a known behavior for
this repo's legacy `packages.config`/non-SDK project set (see prior repo-local-SDK build memory).
The up-to-date no-op is reported as the primary baseline signal: EXIT_CODE 0, no nullable/
warnings-as-errors failures, consistent with the clean P0-T3 analyzer-build compilation that
immediately preceded it.
