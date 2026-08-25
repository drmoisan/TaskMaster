Timestamp: 2026-08-25T12-29
Command: C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe .\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /TestCaseFilter:"FullyQualifiedName~DequeueAsync_SubsequentScreenNonEmptyAcceptedPrefixPastDeadline_ReturnsEightInQueueOrder"
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary: The deterministic deadline-crossing subsequent-screen regression failed before the production change. Expected eight ordered qualifiers but the result contained only high-1, seven items short. The three-second FakeTimeProvider deadline expires after the pre-deadline accepted prefix while remaining qualifiers are available.
