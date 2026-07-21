# Batch 4 Regression Tests (P4-T7)

- Timestamp: 2026-07-19T08-48
- Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~KnownTypesBinder|FullyQualifiedName~AppGlobalsConverter|FullyQualifiedName~PeopleScoRemainingObjectConverter|FullyQualifiedName~NonRecursiveConverter"`
- EXIT_CODE: 0
- Output Summary: `Total tests: 29, Passed: 29, Failed: 0`. All Batch 4 tests green and behavior-identical (including `Threading/AppGlobalsConverterTests.cs`, `KnownTypesBinder_Tests.cs`, `PeopleScoRemainingObjectConverter_Tests.cs`, `NonRecursiveConverter_Tests.cs`). `UtilitiesCS.Test.dll` rebuilt clean against the freshly compiled `UtilitiesCS.dll`.
