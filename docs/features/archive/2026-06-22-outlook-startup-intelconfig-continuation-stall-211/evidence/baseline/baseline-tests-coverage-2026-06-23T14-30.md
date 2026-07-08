# Baseline — Tests + Coverage (#211 Phase 3, LiveOutlook excluded)

Timestamp: 2026-06-23T14-30
Command: `vstest.console.exe TaskMaster.Test/bin/Debug/TaskMaster.Test.dll UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll QuickFiler.Test/bin/Debug/QuickFiler.Test.dll Tags.Test/bin/Debug/Tags.Test.dll TaskVisualization.Test/bin/Debug/TaskVisualization.Test.dll ToDoModel.Test/bin/Debug/ToDoModel.Test.dll VBFunctions.Test/bin/Debug/VBFunctions.Test.dll /EnableCodeCoverage /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook"`
(vstest path: `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`; coverage converted to Cobertura via `dotnet-coverage merge -f cobertura`)
EXIT_CODE: 0

Output Summary:
- `Test Run Successful.` Total tests: 4312. Passed: 4312. Failed: 0. (LiveOutlook category filtered out by `/TestCaseFilter:"TestCategory!=LiveOutlook"`.)
- Repo-wide line coverage (raw `/EnableCodeCoverage` aggregate across all loaded assemblies, including vendored Swordfish/SVGControl): `line-rate=0.6404305705059203` => 64.04% (lines-covered=104118, lines-valid=162575).
- `/InIsolation` is required for these Moq-based assemblies (otherwise vstest STTE setup fails with FileNotFound).
- `MSYS_NO_PATHCONV=1` set for the `/TestCaseFilter` argument and the dotnet-coverage Windows output path.
- Pre-existing flake note: the `UtilitiesCS TimedAsyncTask_Tests` real-interval timer flake is a known intermittent failure under load; it did NOT fail in this baseline run (all 4312 passed). If it surfaces in the Phase 5 final-QC run it is recorded as the pre-existing flake and is not a regression introduced by this change.

This is the deterministic baseline for the P5-T5 coverage delta. The identical command is re-run at P5-T4 against the post-change build for the no-regression comparison.
