Timestamp: 2026-07-06T18-22-04:00
Command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage
EXIT_CODE: 0
Issue: #248
Output Summary:
- The baseline MSTest coverage command completed successfully after repository SDK/package restoration and build reruns.
- Test result: Test Run Successful.
- Total tests: 472.
- Passed tests: 472.
- Failed tests: 0.
- Numeric coverage headline values from converted coverage XML: overall line coverage 18.54%; overall block coverage 18.66%.
- Coverage counts: 21,970 lines covered; 837 lines partially covered; 95,707 lines not covered; 32,148 blocks covered; 140,123 blocks not covered.
- Module headline values: QuickFiler.dll line coverage 63.17%, block coverage 64.18%; QuickFiler.Test.dll line coverage 93.56%, block coverage 95.92%.
- Coverage attachment: TestResults\c35a5cc4-2a98-43ea-a968-1d31d8000114\DanMoisan_MEGALODON4_2026-07-06.18_20_57.coverage.
- Converted coverage XML: docs\features\active\2026-07-06-bayesian-email-sorter-unit-tests-248\evidence\baseline\csharp-vstest-coverage-baseline.2026-07-06T18-07.coveragexml.

Output Excerpt:
- Attachments: TestResults\c35a5cc4-2a98-43ea-a968-1d31d8000114\DanMoisan_MEGALODON4_2026-07-06.18_20_57.coverage.
- Test Run Successful.
- Total tests: 472.
- Passed: 472.
- Total time: 7.3857 Seconds.

Additional Environment Checks:
- The test command was run with the Visual Studio TestPlatform directory added to the process PATH so `vstest.console.exe` resolved.
- Test-Path QuickFiler.Test\bin\Debug\QuickFiler.Test.dll: True.
- Coverage conversion command: dotnet-coverage merge <latest .coverage attachment> --output docs\features\active\2026-07-06-bayesian-email-sorter-unit-tests-248\evidence\baseline\csharp-vstest-coverage-baseline.2026-07-06T18-07.coveragexml --output-format xml --nologo.
- Coverage conversion EXIT_CODE: 0.
