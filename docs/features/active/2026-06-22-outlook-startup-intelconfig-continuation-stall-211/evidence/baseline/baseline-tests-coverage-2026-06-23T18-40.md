# Baseline — Tests + Coverage (#211 Phase 3.1, LiveOutlook excluded)

Timestamp: 2026-06-23T18-40
Command: `vstest.console.exe TaskMaster.Test/bin/Debug/TaskMaster.Test.dll UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll QuickFiler.Test/bin/Debug/QuickFiler.Test.dll Tags.Test/bin/Debug/Tags.Test.dll TaskVisualization.Test/bin/Debug/TaskVisualization.Test.dll ToDoModel.Test/bin/Debug/ToDoModel.Test.dll VBFunctions.Test/bin/Debug/VBFunctions.Test.dll /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook"`
(coverage collected via `dotnet-coverage collect --output-format cobertura -- <vstest...>`; vstest path: `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`; `MSYS_NO_PATHCONV=1` set for the `/TestCaseFilter` argument)
EXIT_CODE: 1 (due to the pre-existing cross-assembly Deedle/DataFrame flake described below; NOT a regression from this change, which has not yet modified any source)

Output Summary:
- Total tests: 4318. Passed: 4301. Failed: 17. (LiveOutlook filtered out by `/TestCaseFilter:"TestCategory!=LiveOutlook"`.)
- Repo-wide line coverage (raw cobertura aggregate across all loaded assemblies, including vendored Swordfish/SVGControl): `line-rate=0.6273020088023408` => 62.73% (lines-covered=102052, lines-valid=162684).
- `/InIsolation` is required for these Moq-based assemblies (otherwise vstest STTE setup fails with FileNotFound).

Pre-existing flake (recorded explicitly):
- 17 failures, ALL in the UtilitiesCS.Test Deedle/DataFrame area: `DeedleDoodles`, `DropFirstN_DropsFirstNRows`, `Email2dArrayToDf_ViaReflection_ValidData_ReturnsFrame`, `Exclude_EmptyOtherFrame_ReturnsSameRowCount`, `Exclude_NonEmptyOtherFrame_RemovesMatchingRows`, `FromArray2D_EmailLikeArray_...`, `FromArray2D_EmptyData_...`, `FromDefaultFolder_*` (5), `GetColumnEid_WithStringValues_...`, `GetDuplicateEntriesByColumn_...`, `GetEmailDataFromTable_OneRow_...`, `GetEmailDataInViewAsync_...`, `GetEmailDataInView_WithInjectedEtlResult_...`, `PrintToLog_WithPopulatedFrame_...`.
- Verified pre-existing and environmental: running `UtilitiesCS.Test` alone (`/TestCaseFilter:"TestCategory!=LiveOutlook"`) passes 3916/3916 with 0 failures; running just the Deedle filter passes 42/42. The failures appear ONLY when all 7 test assemblies execute in one vstest invocation, caused by shared native-resource contention across assemblies (the run also logs `Failed loading language 'eng'`, an OCR/native-interop resource shared across assemblies in the batch). This is a known load/ordering-dependent multi-assembly flake, NOT in this change's scope (`ApplicationGlobals`, `EngineInitTimingProbe`, `StartupDiagnosticsProbe`), and no source has been modified at baseline.
- The earlier 2026-06-23T14-30 baseline (0 failures) did not trigger this load-dependent flake; this run did. The flake is recorded here so the Phase 5 final-QC comparison treats any identical Deedle/DataFrame failures as the pre-existing flake rather than a regression.

This is the deterministic-denominator baseline for the P5-T5 coverage delta. The identical command is re-run at P5-T4 against the post-change build for the no-regression comparison.
