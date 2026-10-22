Timestamp: 2026-08-25T12-29
Command: C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe .\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /TestCaseFilter:"FullyQualifiedName~DequeueAsync_InitialScreenNonEmptyAcceptedPrefixPastDeadline_ReturnsSevenInQueueOrder"
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary: The deterministic deadline-crossing regression failed before the production change. Expected seven ordered qualifiers but the result contained only high-1, six items short. The score loader advances FakeTimeProvider one second per candidate and the three-second deadline expires after the initial accepted prefix while 40 rejected candidates and later qualifiers remain queued.
