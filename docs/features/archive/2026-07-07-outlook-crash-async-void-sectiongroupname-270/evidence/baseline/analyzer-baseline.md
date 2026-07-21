# Analyzer Baseline (Issue #270)

Timestamp: 2026-07-07T22-05

Command: `MSBuild.exe TaskMaster.sln -t:Build -p:Configuration=Debug -p:Platform="Any CPU" -p:EnableNETAnalyzers=true -p:EnforceCodeStyleInBuild=true` (VS18 Community MSBuild 18.7.8; dash-switch form required under git-bash to avoid MSYS path conversion)

EXIT_CODE: 0

Output Summary: Build succeeded with 0 errors. Pre-existing (baseline) warnings are confined to test projects and are informational in analyzer mode (not treated as errors): CS8632 (nullable annotation outside #nullable context) in `UtilitiesCS.Test`, `TaskMaster.Test`; CS0067 (event never used) in `UtilitiesCS.Test`; MSTEST0032 (assertion always true) in `QuickFiler.Test`. None originate from `TaskMaster/AppGlobals/AppEvents.ReadinessHookup.cs` or `TaskMaster.Test/AppGlobals/AppEventsTests.cs`. These warnings are the reference point for the P3-T2 "no new warnings" check.
