Timestamp: 2026-06-24T18-23
Task: P9-T21

# Coverage Denominator Audit

## Inputs

- Baseline summary: `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/evidence/baseline/baseline-coverage-summary.md`
- Baseline XML: `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/evidence/baseline/baseline-coverage.xml`
- Final XML: `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/evidence/qa-gates/final-coverage.xml`
- Final runsettings: `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/evidence/qa-gates/final-coverage.runsettings`
- Final comparison: `docs/features/active/2026-06-24-folder-tree-cache-and-refresh-214/evidence/qa-gates/final-coverage-comparison.md`

## Extraction And Filter Settings

- Baseline extraction method: raw dotnet-coverage XML from `baseline-coverage.xml`, counting every `<range covered="...">` entry across all reported modules.
- Baseline filter settings: no matching runsettings file was used by the Phase 0 baseline command or recorded in `baseline-coverage-summary.md`.
- Final extraction method: dotnet-coverage XML from `final-coverage.xml`, counting every `<range covered="...">` entry across modules admitted by `final-coverage.runsettings`.
- Final filter settings: `final-coverage.runsettings` excludes test assemblies and selected third-party assemblies, and includes production modules matching `TaskMaster.dll`, `UtilitiesCS.dll`, `QuickFiler.dll`, `ToDoModel.dll`, `TaskTree.dll`, `TaskVisualization.dll`, `Tags.dll`, `VBFunctions.dll`, and `Swordfish.NET.General.dll`.

## Baseline Module List

| Module | Covered | Partial | Not covered | Total lines |
| --- | ---: | ---: | ---: | ---: |
| Mono.Reflection.dll | 155 | 0 | 221 | 376 |
| Swordfish.NET.General.dll | 709 | 3 | 819 | 1531 |
| SVGControl.dll | 225 | 8 | 1215 | 1448 |
| System.Interactive.dll | 27 | 0 | 797 | 824 |
| log4net.dll | 2007 | 68 | 4919 | 6994 |
| QuickFiler.dll | 0 | 0 | 5029 | 5029 |
| System.Linq.Async.dll | 348 | 5 | 7891 | 8244 |
| FluentAssertions.dll | 1814 | 84 | 3720 | 5618 |
| TaskMaster.Test.dll | 1826 | 21 | 181 | 2028 |
| Tags.dll | 0 | 0 | 701 | 701 |
| ToDoModel.dll | 44 | 1 | 1595 | 1640 |
| TaskMaster.dll | 659 | 26 | 805 | 1490 |
| TaskVisualization.dll | 13 | 0 | 44 | 57 |
| Deedle.dll | 521 | 17 | 8188 | 8726 |
| UtilitiesCS.dll | 28539 | 517 | 4251 | 33307 |
| FSharp.Core.dll | 1368 | 2 | 15993 | 17363 |
| UtilitiesCS.Test.dll | 38145 | 599 | 1217 | 39961 |

- Baseline total covered-or-partial lines: 96077
- Baseline total lines: 116403
- Baseline repository line coverage: 82.54%

## Final Module List

| Module | Covered | Partial | Not covered | Total lines |
| --- | ---: | ---: | ---: | ---: |
| UtilitiesCS.dll | 29094 | 567 | 4400 | 34061 |
| TaskMaster.dll | 660 | 26 | 810 | 1496 |
| Swordfish.NET.General.dll | 709 | 3 | 819 | 1531 |
| Tags.dll | 0 | 0 | 701 | 701 |
| ToDoModel.dll | 44 | 1 | 1591 | 1636 |
| TaskVisualization.dll | 13 | 0 | 44 | 57 |
| QuickFiler.dll | 0 | 0 | 5025 | 5025 |

- Final total covered-or-partial lines: 31117
- Final total lines: 44507
- Final line coverage from `final-coverage.xml`: 69.91%

## Comparability Finding

- DENOMINATOR_COMPARABLE: no
- REPOSITORY_THRESHOLD_NOT_EVALUATED: mismatched denominator

The Phase 0 baseline denominator includes test assemblies and third-party assemblies such as `UtilitiesCS.Test.dll`, `TaskMaster.Test.dll`, `FSharp.Core.dll`, `FluentAssertions.dll`, `Deedle.dll`, `log4net.dll`, `System.Linq.Async.dll`, `System.Interactive.dll`, `SVGControl.dll`, and `Mono.Reflection.dll`. The final denominator was filtered by `final-coverage.runsettings` and excludes those modules.

Because the baseline total is 116403 covered ranges and the final total is 44507 covered ranges, the repository-wide threshold cannot be evaluated from these two files. The plan therefore requires the later `P9-T33` repository-wide final coverage run and `P9-T34` comparison using the same repository-wide extraction method as the Phase 0 baseline.
