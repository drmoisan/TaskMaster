# Final MSTest Coverage

Timestamp: 2026-06-24T19:13:08-04:00
Command: C:\Users\DanMoisan\.dotnet\tools\dotnet-coverage.exe collect --output docs\features\active\2026-06-24-folder-tree-cache-and-refresh-214\evidence\qa-gates\final-coverage-repository.xml --output-format xml -- C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /TestCaseFilter:"TestCategory!=LiveOutlook" /ResultsDirectory:docs\features\active\2026-06-24-folder-tree-cache-and-refresh-214\evidence\qa-gates\coverage-results-repository
EXIT_CODE: 0
Output Summary: PASS. MSTest coverage completed. Total tests: 4167; Passed: 4167; Failed: 0. Repository line coverage 82.91% (99030/119447 covered or partial lines) across 10 modules using module-level line attributes. final-coverage.runsettings was not used.

VSTest Path: C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe
Coverage Attachment: C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\docs\features\active\2026-06-24-folder-tree-cache-and-refresh-214\evidence\qa-gates\coverage-results-repository\6cf41eab-ed21-4750-bd52-894e809827ba\DanMoisan_MEGALODON4_2026-06-24.19_11_08.coverage
Merge Command: C:\Users\DanMoisan\.dotnet\tools\dotnet-coverage.exe merge --output docs\features\active\2026-06-24-folder-tree-cache-and-refresh-214\evidence\qa-gates\final-coverage-repository.xml --output-format xml <latest .coverage attachment>
Merge Exit Code: 0
Coverage XML: docs\features\active\2026-06-24-folder-tree-cache-and-refresh-214\evidence\qa-gates\final-coverage-repository.xml
Runsettings: final-coverage.runsettings was not used.

## Repository Coverage Modules

| Module | Covered lines | Partially covered lines | Not covered lines | Total lines | Line coverage |
| --- | ---: | ---: | ---: | ---: | ---: |
| Swordfish.NET.General.dll | 826 | 6 | 969 | 1801 | 46.2% |
| Tags.dll | 0 | 0 | 787 | 787 | 0% |
| TaskVisualization.dll | 13 | 0 | 74 | 87 | 14.94% |
| SVGControl.dll | 263 | 20 | 1446 | 1729 | 16.37% |
| TaskMaster.dll | 985 | 64 | 1051 | 2100 | 49.95% |
| QuickFiler.dll | 0 | 0 | 7344 | 7344 | 0% |
| TaskMaster.Test.dll | 3185 | 66 | 292 | 3543 | 91.76% |
| ToDoModel.dll | 41 | 1 | 1994 | 2036 | 2.06% |
| UtilitiesCS.dll | 36452 | 968 | 5192 | 42612 | 87.82% |
| UtilitiesCS.Test.dll | 54943 | 1197 | 1268 | 57408 | 97.79% |

## MSTest Result

- Test Run Successful.
- Total tests: 4167
- Passed: 4167
- Failed: 0
- Repository coverage extraction method: sum selected module `lines_covered`, `lines_partially_covered`, and `lines_not_covered` attributes, matching `baseline-coverage-summary.md`.
- `dotnet-coverage collect` reported `No code coverage data available. Profiler was not initialized.` for its direct XML output, so the newest VSTest `.coverage` attachment was merged into `final-coverage-repository.xml`, matching the baseline conversion method.
