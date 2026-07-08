# Nullable / TreatWarningsAsErrors Baseline (Issue #211 PostLoad/LoadInboxes attribution probe)

Timestamp: 2026-06-24T18-30

Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
(Executed via git-bash with dash-switches against MSBuild 18. Per policy, the gate uses `-t:Build` — a forced `-t:Rebuild` surfaces ~84 pre-existing errors confined to the vendored/exempt projects SVGControl and UtilitiesSwordfish, which are outside this plan's scope.)

EXIT_CODE: 0

Output Summary:
- Build succeeded. 0 Warning(s), 0 Error(s). No nullable warnings promoted to errors under TreatWarningsAsErrors for first-party code at baseline.
