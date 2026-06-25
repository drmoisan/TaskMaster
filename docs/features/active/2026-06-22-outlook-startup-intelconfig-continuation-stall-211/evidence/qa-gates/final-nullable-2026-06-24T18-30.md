# Final QA — Nullable / TreatWarningsAsErrors (Issue #211 PostLoad/LoadInboxes attribution probe)

Timestamp: 2026-06-24T18-30

Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`

EXIT_CODE: 0

Output Summary:
- Build succeeded. 0 Warning(s), 0 Error(s). Zero warnings-as-errors. The new code (StartupInboxAttributionProbe, the AppEvents probe markers, the AppOlObjects.EmitPerStoreInboxAttribution extraction) compiles clean under nullable analysis with TreatWarningsAsErrors. Per policy the gate uses `-t:Build` (a forced `-t:Rebuild` surfaces only pre-existing vendored-project errors outside this plan's scope).
