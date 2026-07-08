# Final QC — Tests + Coverage (#211 Phase 3.2, LiveOutlook excluded)

Timestamp: 2026-06-23T22-30
Command: `vstest.console.exe TaskMaster.Test/bin/Debug/TaskMaster.Test.dll UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll QuickFiler.Test/bin/Debug/QuickFiler.Test.dll Tags.Test/bin/Debug/Tags.Test.dll TaskVisualization.Test/bin/Debug/TaskVisualization.Test.dll ToDoModel.Test/bin/Debug/ToDoModel.Test.dll VBFunctions.Test/bin/Debug/VBFunctions.Test.dll /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook"`
(coverage via `dotnet-coverage collect --output-format cobertura --output <path> -- <vstest...>`; `MSYS_NO_PATHCONV=1`; Windows-style output path)
EXIT_CODE: 1 (due to the pre-existing cross-assembly Deedle/DataFrame multi-assembly flake; NOT a regression — see below)

Output Summary:
- Total tests: 4327. Passed: 4310. Failed: 17. (Baseline was 4324/4307/17; +3 tests, +3 passed, same 17 failures.)
- The +3 net tests are the new phase-annotated probe tests in `StartupDiagnosticsProbeTests.cs` (P3-T1..P3-T3).
- Repo-wide line coverage (raw cobertura aggregate, including vendored Swordfish/SVGControl): `line-rate=0.6279915827704464` => 62.80% (lines-covered=102362, lines-valid=162999). Baseline was 62.77%; +0.03 pts, NO regression.
- Production `TaskMaster.StartupDiagnosticsProbe`: 46/46 lines covered => 100.00% (was 24/24 at baseline; the two new phase-annotated overloads added 22 lines, ALL covered). New-code coverage for the coverable additions = 100% (>= 90% threshold met).
- `TaskMaster.ApplicationGlobals`: 56.64% overall. The new host-bound seams (`StartStartupUiHeartbeat`, `StopStartupUiHeartbeat`, `BeginPhaseGcCapture`, `EmitPhaseGcDelta`) construct a live `DispatcherTimer` and perform live `GC.*`/`GCSettings.*` reads; they have no injectable seam beyond the override-to-no-op test pattern and fall under the CLAUDE.md COM/host-bound coverage exemption (same structure ratified for the Phase 3.1 Engines-only probe). All coverable formatting/aggregation was deliberately placed in `StartupDiagnosticsProbe`.

Regression check (no-regression vs baseline):
- The 17 failures are the identical pre-existing UtilitiesCS.Test Deedle/DataFrame multi-assembly flake recorded in `baseline-tests-coverage-2026-06-23T22-30.md` (the run logs `Failed loading language 'eng'`, a shared native-interop resource). Verified NOT a regression:
  - `TaskMaster.Test` (the only assembly modified by this change) run ALONE: 134/134 passed, 0 failed.
  - The new probe tests (8 total in `StartupDiagnosticsProbeTests`) and all AppGlobals seam tests pass in isolation (22/22 in the AppGlobals|ContinuationProbe|StartupDiagnosticsProbe filter).
- No test in the touched scope (`ApplicationGlobals`, `StartupDiagnosticsProbe`, the three subclass test files) fails.

Coverage cobertura file: `evidence/qa-gates/coverage-postchange-2026-06-23T22-30.cobertura.xml`.
