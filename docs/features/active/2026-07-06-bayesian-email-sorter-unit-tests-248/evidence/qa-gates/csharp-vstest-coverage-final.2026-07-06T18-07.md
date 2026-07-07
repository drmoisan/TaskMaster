Timestamp: 2026-07-06T18:42:00-04:00
Command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage
EXIT_CODE: 0
Issue: #248
Output Summary:
- The final MSTest coverage command completed successfully through the resolved Visual Studio test runner.
- Resolved executable: C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe.
- Test result: Test Run Successful.
- Total tests: 486.
- Passed tests: 486.
- Failed tests: 0.
- Numeric coverage headline values from converted coverage XML: overall line coverage 20.21%; overall block coverage 19.48%.
- Coverage counts: 23,153 lines covered; 890 lines partially covered; 94,914 lines not covered; 33,610 blocks covered; 138,963 blocks not covered.
- Module headline values: QuickFiler.dll line coverage 69.36%, block coverage 71.80%; QuickFiler.Test.dll line coverage 93.73%, block coverage 95.96%; UtilitiesCS.dll line coverage 4.65%, block coverage 5.05%.
- Coverage attachment: TestResults\6b3c3023-2992-478a-9d3b-21f6150fbad1\DanMoisan_MEGALODON4_2026-07-06.18_34_15.coverage.
- Converted coverage XML: docs\features\active\2026-07-06-bayesian-email-sorter-unit-tests-248\evidence\qa-gates\csharp-vstest-coverage-final.2026-07-06T18-07.coveragexml.

Output Excerpt:
- Test Run Successful.
- Total tests: 486.
- Passed: 486.
- Total time: 10.0698 Seconds.

Additional Environment Checks:
- The bare vstest.console.exe command was not on PATH in this session.
- The successful run used the resolved Visual Studio executable path listed above.
- Test-Path QuickFiler.Test\bin\Debug\QuickFiler.Test.dll: True.
- Coverage conversion command: dotnet-coverage merge TestResults\6b3c3023-2992-478a-9d3b-21f6150fbad1\DanMoisan_MEGALODON4_2026-07-06.18_34_15.coverage --output docs\features\active\2026-07-06-bayesian-email-sorter-unit-tests-248\evidence\qa-gates\csharp-vstest-coverage-final.2026-07-06T18-07.coveragexml --output-format xml --nologo.
- Coverage conversion EXIT_CODE: 0.
