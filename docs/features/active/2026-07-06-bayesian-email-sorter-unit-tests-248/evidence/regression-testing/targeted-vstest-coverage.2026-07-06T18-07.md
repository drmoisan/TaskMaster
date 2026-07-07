Timestamp: 2026-07-06T18:28:05-04:00
Command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /TestCaseFilter:"FullyQualifiedName~EmailSorterTests|FullyQualifiedName~BayesianPerformanceControllerTests"
EXIT_CODE: 0
Issue: #248
Output Summary:
- The targeted MSTest coverage command completed successfully after resolving vstest.console.exe through a process-local alias to C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe.
- Test result: Test Run Successful.
- Total targeted tests: 14.
- Passed targeted tests: 14.
- Failed targeted tests: 0.
- Numeric coverage headline values from converted coverage XML: overall line coverage 3.52%; overall block coverage 3.60%.
- Coverage counts: 2,554 lines covered; 155 lines partially covered; 74,359 lines not covered; 3,640 blocks covered; 97,529 blocks not covered.
- Module headline values: QuickFiler.dll line coverage 6.29%, block coverage 7.62%; QuickFiler.Test.dll line coverage 3.80%, block coverage 1.59%; UtilitiesCS.dll line coverage 0.09%, block coverage 0.10%.
- Coverage attachment: TestResults\81decd9c-34c8-4b1a-9cc2-aa1fbea241f2\DanMoisan_MEGALODON4_2026-07-06.18_28_05.coverage.
- Converted coverage XML: docs\features\active\2026-07-06-bayesian-email-sorter-unit-tests-248\evidence\regression-testing\targeted-vstest-coverage.2026-07-06T18-07.coveragexml.

Output Excerpt:
- Test Run Successful.
- Total tests: 14.
- Passed: 14.
- Total time: 3.2344 Seconds.

Additional Environment Checks:
- The initial direct shell invocation of vstest.console.exe returned EXIT_CODE: 1 because vstest.console.exe was not on PATH.
- The successful run used Set-Alias in the current PowerShell process only; no repository or user profile PATH changes were made.
- Test-Path QuickFiler.Test\bin\Debug\QuickFiler.Test.dll: True.
- Coverage conversion command: dotnet-coverage merge TestResults\81decd9c-34c8-4b1a-9cc2-aa1fbea241f2\DanMoisan_MEGALODON4_2026-07-06.18_28_05.coverage --output docs\features\active\2026-07-06-bayesian-email-sorter-unit-tests-248\evidence\regression-testing\targeted-vstest-coverage.2026-07-06T18-07.coveragexml --output-format xml --nologo.
- Coverage conversion EXIT_CODE: 0.
