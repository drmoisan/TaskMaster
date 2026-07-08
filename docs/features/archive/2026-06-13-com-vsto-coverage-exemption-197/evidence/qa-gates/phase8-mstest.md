# Phase 8 — MSTest with coverage gate (P8-T4)

Timestamp: 2026-06-13T13-46
Command: pwsh ./scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput 'docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/evidence/qa-gates/coverage-firstparty.phase8.cobertura.xml'
EXIT_CODE: 0
Output Summary:
- Test Run Successful. Total tests: 4068; Passed: 4068; Failed: 0. (The 2 pre-existing flaky timing tests noted in roadmap §0.1 passed in this run; no new failures.)
- Coverage pipeline: dotnet-coverage collect with coverage.config (TaskVisualization exclude REMOVED in P8-T1) + inner vstest /Settings:TaskMaster.cli.runsettings /InIsolation, then Koverage post-processing (vendored/third-party stripped, .Test stripped per #193).
- TaskVisualization package RETURNED to the first-party denominator (confirmed present with line-rate 0.0037; classes TaskController, TaskViewer, FlagTasks, AutoAssignContext, AutoAssignPeople, AutoCreateProject, EditFilterController, EditFilterViewer, ManageFilters, FlagChangeGroup, FlagChangeItem all at 0; FlagChangeTrainingQueue at 0.3467). This confirms the assembly-level exclude is no longer in effect.
- Production-only first-party deduped (excludes vendored SVGControl + Swordfish.NET.General; .Test stripped): lines-valid 51,871; lines-covered 36,045; rate 69.49% (per-line count method, matching the prior P7-T8 method). This is the pre-Phase-9 figure: TaskVisualization is fully back in the denominator but not yet class-level annotated, so the rate is below the assembly-exclude variant (71.73%). Phase 9 annotation removes the COM/WinForms-bound TaskVisualization classes again at class level.
- Raw Cobertura overall (incl. vendored + UtilitiesCS/VBFunctions reference packages): lines-valid 99,196; lines-covered 67,141; line-rate 0.6769.

Artifact: docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/evidence/qa-gates/coverage-firstparty.phase8.cobertura.xml
