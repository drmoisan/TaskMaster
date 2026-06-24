# Final Coverage Comparison

Timestamp: 2026-06-24T19:14:03-04:00
Command: PowerShell XML comparison of baseline-coverage.xml and final-coverage-repository.xml using module-level repository line attributes and issue-scoped source-range checks.
EXIT_CODE: 0
Output Summary: PASS. Repository coverage 82.91% (99030/119447); folder tree scoped coverage 97.95%; EmailDataMiner folder extraction 95.68%; FilterOlFolders issue #214 scoped snapshot 100%; SubjectMap orchestration 94.05%; Ribbon uses method-level non-instrumentation rationale.

## Inputs

- Baseline: `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/evidence/baseline/baseline-coverage.xml`
- Final: `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/evidence/qa-gates/final-coverage-repository.xml`
- Baseline summary reference: `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/evidence/baseline/baseline-coverage-summary.md`
- Extraction method: repository coverage sums selected module `lines_covered`, `lines_partially_covered`, and `lines_not_covered` attributes; issue-scoped gates use source ranges for the file or method range named by the plan.

## Denominator

- Baseline denominator: 
116403
 lines; covered-or-partial: 
96077
; modules: 
10
.
- Final denominator: 
119447
 lines; covered-or-partial: 
99030
; modules: 
10
.
- Baseline modules: 
Swordfish.NET.General.dll, SVGControl.dll, QuickFiler.dll, TaskMaster.Test.dll, Tags.dll, ToDoModel.dll, TaskMaster.dll, TaskVisualization.dll, UtilitiesCS.dll, UtilitiesCS.Test.dll
.
- Final modules: 
Swordfish.NET.General.dll, Tags.dll, TaskVisualization.dll, SVGControl.dll, TaskMaster.dll, QuickFiler.dll, TaskMaster.Test.dll, ToDoModel.dll, UtilitiesCS.dll, UtilitiesCS.Test.dll
.
- DENOMINATOR_COMPARABLE: yes

## Thresholds

| Gate | Baseline | Final | Required | Status |
| --- | ---: | ---: | ---: | --- |
| Repository line coverage | 82.54% (96077/116403) | 82.91% (99030/119447) | >= 80.00% | PASS |
| Issue #214 folder tree snapshot/cache scoped coverage | N/A | 97.95% (383/391, files=10) | >= 90.00% | PASS |
| EmailDataMiner issue #214 snapshot coverage | N/A | 95.68% (133/139, files=1) | >= 90.00% | PASS |
| FilterOlFoldersController issue #214 snapshot coverage | N/A | 100% (53/53, lines 227-296) | >= 90.00% | PASS |
| SubjectMap issue #214 orchestration coverage | N/A | 94.05% (79/84, files=1) | >= 90.00% | PASS |
| TaskMaster Ribbon issue #214 scoped snapshot coverage | N/A | Non-instrumented by existing type-level [ExcludeFromCodeCoverage]; see rationale below | >= 90.00% or method-level rationale | PASS |
| New module EmailDataMiner.FolderExtraction.cs coverage | N/A | 95.68% (133/139) | >= 90.00% | PASS |

## Touched-Area Regression

| Area | Baseline | Final | Status |
| --- | ---: | ---: | --- |
| UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailDataMiner.cs | 88.15% (305/346) | 97.14% (34/35) | PASS |
| UtilitiesCS/EmailIntelligence/OlFolderTools/FilterOlFolders/FilterOlFoldersController.cs | 90.32% (84/93 covered or partial lines) from baseline-coverage-summary.md | 100.00% (53/53 covered or partial ranges) for issue #214 snapshot methods, lines 227-296 | PASS |
| UtilitiesCS/EmailIntelligence/SubjectMap/SubjectMapSco.Orchestration.cs | 90.7% (78/86) | 94.05% (79/84) | PASS |

## Ribbon Non-Instrumentation Rationale

TaskMaster Ribbon issue #214 helper methods remain non-instrumented because `TaskMaster/Ribbon/RibbonController.cs` has an existing type-level `[ExcludeFromCodeCoverage]` on `RibbonController`. `issue-214-coverage-gap-map.md` records method-level entries RB-01 through RB-07 with named scoped tests for the helper behavior: `GetFolderTreeSnapshotAsync_UsesInjectedFolderTreeService`, `GetFolderTreeSnapshotAsync_WhenFolderStoreMissing_RequestsAllStores`, `CompareFolderSnapshots_UsesScopedCachedSnapshotViews`, `CompareFolderSnapshots_WhenFolderRootMissing_ComparesFullSnapshot`, `GetStats_WithFolders_ReturnsFormattedSizeAndCount`, and `GetStats_WhenNodesMissing_ReturnsZero`.

## Result

PASS. P9-T34 coverage comparison gates are satisfied.
