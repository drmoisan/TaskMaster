# Final QC — Tests + Coverage (#211 Phase 3.1, LiveOutlook excluded)

Timestamp: 2026-06-23T18-40
Command: `vstest.console.exe TaskMaster.Test/bin/Debug/TaskMaster.Test.dll UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll QuickFiler.Test/bin/Debug/QuickFiler.Test.dll Tags.Test/bin/Debug/Tags.Test.dll TaskVisualization.Test/bin/Debug/TaskVisualization.Test.dll ToDoModel.Test/bin/Debug/ToDoModel.Test.dll VBFunctions.Test/bin/Debug/VBFunctions.Test.dll /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook"`
(coverage collected via `dotnet-coverage collect --output-format cobertura -- <vstest...>`; vstest path: `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`; `MSYS_NO_PATHCONV=1` set for the `/TestCaseFilter` argument)
EXIT_CODE: 1 (due solely to the pre-existing cross-assembly Deedle/DataFrame flake; see below)

Output Summary:
- Total tests: 4324 (was 4318 at baseline; +6 = 5 new StartupDiagnosticsProbe tests + 1 new EngineInitTimingProbe worker-thread-context test). Passed: 4307. Failed: 17.
- Repo-wide line coverage (raw cobertura aggregate across all loaded assemblies, including vendored Swordfish/SVGControl): `line-rate=0.6277350752682891` => 62.77% (lines-covered=102248, lines-valid=162884). Baseline was 62.73% (102052/162684); post-change is NOT a regression (slightly higher, reflecting the new probe code being exercised).
- All 111 TaskMaster.Test AppGlobals tests pass, including the 7 ApplicationGlobals startup-timing/continuation tests that drive the real `LoadSequentialAsync` through the phase-wrapper seam, and the new StartupDiagnosticsProbe/EngineInitTimingProbe tests.

Pre-existing flake (the only failures; matches the Phase 0 baseline flake exactly):
- 17 failures, ALL in the UtilitiesCS.Test Deedle/DataFrame area (DeedleDoodles, FromArray2D_*, FromDefaultFolder_* (5), Email2dArrayToDf_*, GetColumnEid_*, GetEmailDataFromTable_*, GetEmailDataInView_*, GetEmailDataInViewAsync_*, GetDuplicateEntriesByColumn_*, DropFirstN_*, Exclude_* (2), PrintToLog_*), accompanied by the native-resource marker `Failed loading language 'eng'`.
- Verified pre-existing and environmental in Phase 0: UtilitiesCS.Test alone passes 3916/3916; the Deedle filter alone passes 42/42. The failures appear only when all 7 test assemblies execute in one vstest invocation (shared native-interop/OCR-resource contention across assemblies). This is NOT in this change's scope (`ApplicationGlobals`, `EngineInitTimingProbe`, `StartupDiagnosticsProbe`) and the failing set is identical to baseline. Treated as the recorded pre-existing flake, not a regression.

Loop status: formatting (clean), analyzers (clean), nullable/TWAE (clean), and tests (all in-scope tests pass; only the recorded pre-existing Deedle flake remains) all passed in a single final pass after the seam refactor. Coverage numbers captured. See `final-qc-coverage-delta-2026-06-23T18-40.md` for the delta and new-code determination.
