# P6-T5 restart coverage and quality delta

The P0 baseline, P5-T46 focused run, and restarted P6-T4 run use the repository coverage wrapper with `SearchRoot .`, Debug configuration, eight test assemblies, `/InIsolation`, `TestCategory!=LiveOutlook`, one Cobertura source root, and the nine-package inventory: QuickFiler, UtilitiesCS, TaskVisualization, SVGControl, ToDoModel, Tags, TaskMaster, TaskTree, and VBFunctions.

| Measure | P0 baseline | P5-T46 | restarted P6-T4 | Reconciliation |
| --- | ---: | ---: | ---: | --- |
| Repository lines | 109,324 valid | 93,666/110,478 (84.7825%) | 93,687/110,478 (84.8015%) | P5 and P6 denominators are identical. The 1,154-line denominator increase from P0 is reconciled to the reviewed production source additions in `git diff origin/main -- '*.cs'`; it is not coverage-scope drift. |
| Repository branches | 27,320 valid | 21,453/27,698 | 21,459/27,698 (77.4749%) | P5 and P6 denominators are identical. The 378-branch denominator increase from P0 is reconciled to the reviewed production source additions; it is not coverage-scope drift. |
| Changed executable production lines | n/a | 890/892 (99.7758%) method-context inventory | 879/881 (99.7730%) direct final diff-line scan | The P5 method-context inventory includes eleven additional executable lines beyond the raw added-hunk set. The final scan uses `git diff --unified=0 origin/main -- '*.cs'`, excludes test paths, and unions Cobertura hits by source filename and line. |

The two uncovered raw changed executable lines are `TaskMaster/AppGlobals/AppOlObjects.FolderTreeService.cs:289` and `UtilitiesCS/EmailIntelligence/OlFolderTools/FilterOlFolders/FilterOlFoldersController.Lifecycle.cs:81`. They remain within their respective class/module totals of 291/292 and 334/335, and no changed method falls below the required 90% threshold. The P5 method-context and P6 direct-hunk inventories use different, documented executable-line selection rules; neither changes the coverage scope. No changed-line regression is present.

Target class/module coverage is: `AppOlObjects.FolderTreeService.cs` 291/292, `FilterOlFoldersController.cs` 101/102, `FilterOlFoldersController.Lifecycle.cs` 334/335, `OutlookFolderTreeService.cs` 359/359, and `WpfUiDispatcher.cs` 12/12. This includes `CreateFolderTreeServiceDispatcher`, the disposed and composition-failure paths, both `CloseViewerAfterInitializationFailure` `InvokeRequired` branches, Outlook-folder authorization/cleanup-observer behavior, and `WpfUiDispatcher.InvokeAsync(Action)`. The XML method scan found 0 required target methods below 95% coverage.

Result: pass. Repository line coverage is 84.8015% (minimum 80%), changed production coverage is 99.7730% (minimum 90%), and every required changed target method/class/module is at least 90%. No waiver, exception, policy relaxation, or coverage-scope change was used.
