# Batch 7 Regression Tests (P7-T7)

- Timestamp: 2026-07-19T08-48
- Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~ScDictionaryConverter|FullyQualifiedName~ScoDictionaryConverter|FullyQualifiedName~PeopleScoConverter"`
- EXIT_CODE: 0
- Output Summary: `Total tests: 12, Passed: 12, Failed: 0`. All Batch 7 dictionary-converter tests green and behavior-identical (`ScDictionaryConverter_Tests.cs`, `ScoDictionaryConverterTests.cs`, `PeopleScoConverter_Tests.cs`). `UtilitiesCS.Test.dll` rebuilt clean against the freshly compiled `UtilitiesCS.dll`.
