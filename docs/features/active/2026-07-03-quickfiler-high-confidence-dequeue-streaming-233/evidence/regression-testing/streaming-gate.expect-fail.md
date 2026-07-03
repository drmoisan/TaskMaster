Timestamp: 2026-07-03T17:33:27-04:00

Command: `& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe' QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Tests:QfcStreamingDequeueConfidenceGateTests`

EXIT_CODE: 1

Output Summary:

- VSTest version 18.7.0 (x64).
- Test discovery matched `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`.
- Total tests: 7.
- Failed: 7.
- Each failure reported `Expected gateType not to be <null> because the dequeue-layer confidence gate must exist`.
- Result: expected failing regression established before implementing `QfcStreamingDequeueConfidenceGate`.
