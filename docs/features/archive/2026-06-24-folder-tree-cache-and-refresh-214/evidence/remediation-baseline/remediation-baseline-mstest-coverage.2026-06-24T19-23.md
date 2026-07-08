Timestamp: 2026-06-24T19-30-04:00
Command: C:\Users\DanMoisan\.dotnet\tools\dotnet-coverage.exe collect --output docs\features\active\2026-06-24-folder-tree-cache-and-refresh-214\evidence\remediation-baseline\remediation-baseline-coverage.2026-06-24T19-23.xml --output-format xml -- C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /TestCaseFilter:"TestCategory!=LiveOutlook" /ResultsDirectory:docs\features\active\2026-06-24-folder-tree-cache-and-refresh-214\evidence\remediation-baseline\coverage-results
EXIT_CODE: 0
Output Summary:
- MSTest run succeeded with `TestCategory!=LiveOutlook`.
- Total tests: 4167.
- Passed: 4167.
- Failed: 0.
- Repository line coverage headline: 82.91% (99029 covered-or-partial lines / 119447 total lines) across the repository's 10 counted modules.
- `dotnet-coverage collect` direct XML output reported no module data, matching the existing repository evidence behavior; merged the newest VSTest `.coverage` attachment into the XML evidence.

Coverage Merge:
- Command: C:\Users\DanMoisan\.dotnet\tools\dotnet-coverage.exe merge --output docs\features\active\2026-06-24-folder-tree-cache-and-refresh-214\evidence\remediation-baseline\remediation-baseline-coverage.2026-06-24T19-23.xml --output-format xml docs\features\active\2026-06-24-folder-tree-cache-and-refresh-214\evidence\remediation-baseline\coverage-results\2c09065b-928d-41a0-85ba-c005aa31d34f\DanMoisan_MEGALODON4_2026-06-24.19_30_43.coverage
- Merge Exit Code: 0.
- Coverage XML: docs\features\active\2026-06-24-folder-tree-cache-and-refresh-214\evidence\remediation-baseline\remediation-baseline-coverage.2026-06-24T19-23.xml
