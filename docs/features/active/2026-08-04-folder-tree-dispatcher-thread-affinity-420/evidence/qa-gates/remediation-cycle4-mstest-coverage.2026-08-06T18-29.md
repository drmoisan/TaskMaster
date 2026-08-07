# P6-T4 full MSTest coverage result

Timestamp: 2026-08-06T18-29

Command:

`pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs/features/active/2026-08-04-folder-tree-dispatcher-thread-affinity-420/evidence/qa-gates/remediation-cycle4-coverage-final.cobertura.xml`

Result: exit code 0. The command discovered eight `*.Test.dll` assemblies, used `/InIsolation` and `TestCategory!=LiveOutlook`, and passed 6,166/6,166 tests in 1.0115 minutes.

Cobertura inventory: one source root (`.`); nine packages (`QuickFiler`, `SVGControl`, `Tags`, `TaskMaster`, `TaskTree`, `TaskVisualization`, `ToDoModel`, `UtilitiesCS`, and `VBFunctions`); 93,674/110,478 lines covered (84.7897%); and 21,453/27,698 branches covered (77.4532%). The report was post-processed by the wrapper for Koverage compatibility and was written to the stated final coverage path.
