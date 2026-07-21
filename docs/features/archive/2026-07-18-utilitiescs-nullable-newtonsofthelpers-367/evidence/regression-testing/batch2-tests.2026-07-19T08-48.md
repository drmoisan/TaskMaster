# Batch 2 Regression Tests (P2-T6)

- Timestamp: 2026-07-19T08-48
- Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~ILGlobals|FullyQualifiedName~ILInstruction|FullyQualifiedName~MethodBodyReader"`
- EXIT_CODE: 0
- Output Summary: `Total tests: 50, Passed: 50, Failed: 0`. All Batch 2 (SDIL Reader) tests green and behavior-identical. `UtilitiesCS.Test.dll` rebuilt clean against the freshly compiled `UtilitiesCS.dll`.
