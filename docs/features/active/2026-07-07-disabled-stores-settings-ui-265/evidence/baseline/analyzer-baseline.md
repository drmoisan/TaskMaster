# Phase 0 — Analyzer Build Baseline (P0-T9)

Timestamp: 2026-07-08T03-51

Command: `msbuild TaskMaster.sln -t:Build -p:Configuration=Debug -p:Platform="Any CPU" -p:EnableNETAnalyzers=true -p:EnforceCodeStyleInBuild=true`
(Dash-form switches used because git-bash MSYS converts `/t:` style switches into file
paths; dash form is the documented equivalent. MSBuild 18.7.8 for .NET Framework.)

EXIT_CODE: 0

Output Summary:
- Build succeeded.
- Errors: 0
- Warnings: 75 (pre-existing baseline). Predominant categories: CS8632 (nullable
  annotation used outside a `#nullable` context) and CS0067 (unused event) — concentrated
  in existing test code (e.g. ConversationHelper_ExtendedTests.cs, SmartSerializable_Tests.cs,
  StoreWrapperControllerTests.cs). These are the analyzer-build baseline warning counts
  against which the P7-T2 post-change count is compared for no-increase.
