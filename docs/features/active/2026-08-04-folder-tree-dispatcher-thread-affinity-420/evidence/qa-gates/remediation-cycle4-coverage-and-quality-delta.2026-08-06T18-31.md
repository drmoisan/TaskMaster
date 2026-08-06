# P6-T5 coverage and quality delta

Timestamp: 2026-08-06T18-31

The P5-T46 and P6-T4 commands use the same search root (`.`), Debug configuration, eight discovered test assemblies, `/InIsolation`, `TestCategory!=LiveOutlook`, one Cobertura source root, and nine-package inventory. Both use the repository coverage wrapper and coverage settings.

| Measure | P5-T46 | P6-T4 | Delta |
| --- | ---: | ---: | ---: |
| Repository covered lines | 93,666/110,478 | 93,674/110,478 | +8 covered, denominator unchanged |
| Repository branches | 21,453/27,698 | 21,453/27,698 | unchanged |
| Raw changed executable diff lines | 890/892 reviewed P5 method inventory | 879/881 direct final diff-line scan | no uncovered-line regression |

The P5 method inventory included eleven additional executable lines in changed-method context beyond the raw added-hunk line set. The final raw-diff calculation follows `git diff --unified=0 origin/main -- '*.cs'`, excludes tests, and unions Cobertura hits by source filename/line. Its two uncovered changed executable lines are `AppOlObjects.FolderTreeService.cs:289` and `FilterOlFoldersController.Lifecycle.cs:81`; both were already identified as unchanged source lines in the P5 method/class inventory, so the direct final hunk scan’s denominator treatment is the reviewed reconciliation rather than coverage-scope drift.

Final changed production coverage is 879/881 (99.773%). Target class coverage is: `AppOlObjects.FolderTreeService` 291/292, controller 101/102, controller lifecycle 334/335, Outlook service 359/359, and WPF dispatcher 12/12. Every emitted target method is at least 95% covered. Repository line coverage is 84.7897%, above the 80% policy floor. No changed-line regression is observed.

The required P6-T5 thresholds pass: repository lines >=80%, changed production >=90%, each changed target class/module/method >=90%, and no changed-line regression. No waiver or coverage-scope change is used.
