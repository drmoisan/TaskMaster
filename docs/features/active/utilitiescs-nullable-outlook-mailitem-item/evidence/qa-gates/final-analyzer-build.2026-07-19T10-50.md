# Final QC — Analyzer / Codestyle Build Gate (P10-T2)

- Timestamp: 2026-07-19T10-50
- Task: [P10-T2]
- Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
- EXIT_CODE: 0

## Output Summary

- Result: Build succeeded. 0 errors.
- Warnings: 16 (all pre-existing, all in TEST/other-project files — `ApplicationGlobalsStartupTimingTests.cs`, `AppToDoObjectsTests.cs`, `EngineInitTimingProbeTests.cs`, `PeopleScoDictionaryNewTests.cs`, `QfcFormControllerTests.cs`, `StoreRehookCoordinatorTests.cs`, `StoresWrapperTests.cs`, `TestableApplicationGlobals.cs`). Codes: CS8632 (12), CS0169 (3), MSTEST0032 (1) — all in the P0-T3 baseline set.
- **Zero warnings in the 30 in-scope `UtilitiesCS/OutlookObjects/{MailItem,Item,Conversation,Attachment,Table}/` files** — no new analyzer diagnostics introduced by this feature. (The overall count is lower than the P0-T3 baseline of 78 because this incremental `/t:Build` recompiled only the changed assemblies, and because adding `#nullable enable` to the 30 files resolved several CS8632 diagnostics those files previously emitted.)
- No new analyzer diagnostics vs the P0-T3 baseline. No files changed by this step.
