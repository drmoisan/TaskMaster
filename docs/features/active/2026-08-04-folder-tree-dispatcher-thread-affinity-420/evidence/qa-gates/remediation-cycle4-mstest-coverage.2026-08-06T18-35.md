# Cycle 4 final MSTest coverage restart result

- Task: `[P6-T4]` restart after the P6-T6 whitespace correction.
- Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs/features/active/2026-08-04-folder-tree-dispatcher-thread-affinity-420/evidence/qa-gates/remediation-cycle4-coverage-final.cobertura.xml`
- Test isolation: the wrapper used `vstest.console.exe` with `/InIsolation` and the `TestCategory!=LiveOutlook` filter.
- Discovered test assemblies: 8: QuickFiler.Test, Tags.Test, TaskMaster.Test, TaskTree.Test, TaskVisualization.Test, ToDoModel.Test, UtilitiesCS.Test, and VBFunctions.Test.
- Result: 6,166 total tests; 6,166 passed; 0 failed. Exit status: 0.
- Cobertura source-root count: 1 (`.`).
- Package inventory: QuickFiler, UtilitiesCS, TaskVisualization, SVGControl, ToDoModel, Tags, TaskMaster, TaskTree, and VBFunctions.
- Repository coverage: 93,687 / 110,478 lines (84.8015%); 21,459 / 27,698 branches (77.4749%).
- `lines-valid`: 110,478. `branches-valid`: 27,698.
- Result: pass; repository line coverage exceeds the 80% threshold.
