# Batch 6 Regression Tests (P6-T7)

- Timestamp: 2026-07-19T08-48
- Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~WrapperScoDictionary|FullyQualifiedName~WrapperScDictionary|FullyQualifiedName~WrapperPeopleScoDictionaryNew"`
- EXIT_CODE: 0
- Output Summary: `Total tests: 43, Passed: 43, Failed: 0`. All Batch 6 wrapper tests green and behavior-identical (`WrapperScDictionaryTest.cs`, `WrapperScoDictionaryTest.cs`, `WrapperPeopleScoDictionaryNew_Tests.cs`). `UtilitiesCS.Test.dll` rebuilt clean against the freshly compiled `UtilitiesCS.dll`.
