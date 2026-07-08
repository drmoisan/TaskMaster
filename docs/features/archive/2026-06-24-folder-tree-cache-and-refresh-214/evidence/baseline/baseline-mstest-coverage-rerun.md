Timestamp: 2026-06-24T20-32
Command: C:\Users\DanMoisan\.dotnet\tools\dotnet-coverage.exe collect --output docs\features\active\2026-06-24-folder-tree-cache-and-refresh-214\evidence\baseline\baseline-coverage.xml --output-format xml -- $vstest TaskMaster.Test\bin\Debug\TaskMaster.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /TestCaseFilter:"TestCategory!=LiveOutlook" /ResultsDirectory:docs\features\active\2026-06-24-folder-tree-cache-and-refresh-214\evidence\baseline\coverage-results
EXIT_CODE: 0
Output Summary:
MSTest total tests: 4033
MSTest passed: 4033
MSTest failed: 0
Resolved VSTest: C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe
baseline-coverage.xml exists: True
Coverage conversion: dotnet-coverage merge converted the VSTest .coverage attachment to baseline-coverage.xml because the collect XML initially contained only skipped modules.
Repository line coverage: 82.54% (96,077 covered-or-partial lines / 116,403 repository lines across repository modules in the coverage report)
