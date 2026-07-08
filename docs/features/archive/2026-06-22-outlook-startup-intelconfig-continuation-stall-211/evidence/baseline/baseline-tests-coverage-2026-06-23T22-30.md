# Baseline — Tests + Coverage (#211 Phase 3.2, LiveOutlook excluded)

Timestamp: 2026-06-23T22-30
Command: `vstest.console.exe TaskMaster.Test/bin/Debug/TaskMaster.Test.dll UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll QuickFiler.Test/bin/Debug/QuickFiler.Test.dll Tags.Test/bin/Debug/Tags.Test.dll TaskVisualization.Test/bin/Debug/TaskVisualization.Test.dll ToDoModel.Test/bin/Debug/ToDoModel.Test.dll VBFunctions.Test/bin/Debug/VBFunctions.Test.dll /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook"`
(coverage collected via `dotnet-coverage collect --output-format cobertura --output <path> -- <vstest...>`; vstest path: `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`; `MSYS_NO_PATHCONV=1` set; Windows-style output path required for dotnet-coverage)
EXIT_CODE: 1 (due to the pre-existing cross-assembly Deedle/DataFrame multi-assembly flake; NOT a regression — no source modified at baseline)

Output Summary:
- Total tests: 4324. Passed: 4307. Failed: 17. (LiveOutlook filtered out.)
- Repo-wide line coverage (raw cobertura aggregate across all loaded assemblies, including vendored Swordfish/SVGControl): `line-rate=0.627741214606714` => 62.77% (lines-covered=102249, lines-valid=162884).
- Production `TaskMaster.StartupDiagnosticsProbe`: 24/24 lines covered => 100.00% at baseline.
- `/InIsolation` is required for these Moq-based assemblies (otherwise vstest STTE setup fails with FileNotFound).

Pre-existing flake (recorded explicitly):
- 17 failures, ALL in the UtilitiesCS.Test Deedle/DataFrame area, identical to the 2026-06-23T18-40 baseline. Verified pre-existing and environmental (load/ordering-dependent multi-assembly flake; the run also logs `Failed loading language 'eng'`, a shared native-interop resource). NOT in this change's scope (`ApplicationGlobals`, `StartupDiagnosticsProbe`). No source modified at baseline.
- Recorded so the Phase 5 final-QC comparison treats identical Deedle/DataFrame failures as the pre-existing flake, not a regression.

Coverage cobertura file: `evidence/baseline/coverage-baseline-2026-06-23T22-30.cobertura.xml`.
This is the deterministic-denominator baseline for the P5-T5 coverage delta. The identical command is re-run at P5-T4 against the post-change build.
