# Baseline — Nullable / TreatWarningsAsErrors Build (P0-T4)

Timestamp: 2026-06-29T10-52
Command: msbuild TaskMaster.sln -t:Build -p:Configuration=Debug -p:Platform="Any CPU" -p:Nullable=enable -p:TreatWarningsAsErrors=true
EXIT_CODE: 0
Output Summary: Build succeeded. 0 Warning(s), 0 Error(s). Run as an incremental -t:Build immediately after the analyzer build (per the documented toolchain order), so first-party assemblies were already compiled under their real settings and the nullable build found them up-to-date. Baseline nullable warning headline: 0.
