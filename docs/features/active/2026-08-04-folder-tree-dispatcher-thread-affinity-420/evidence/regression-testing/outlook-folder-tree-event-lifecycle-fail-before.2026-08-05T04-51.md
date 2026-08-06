# P5-T34 fail-before evidence

Timestamp: 2026-08-05T04:51:00-04:00

Command: `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~Dispose_SuppressesRetainedNotificationsAndInFlightRefreshFaults|FullyQualifiedName~SnapshotChanged_DisposingSubscriberSuppressesLaterSubscriber"`

EXIT_CODE: 1

Output Summary: The two intended M2 tests failed. A disposed in-flight scheduled refresh delivered `ObjectDisposedException` to `ScheduledRefreshFaulted`, and the first `SnapshotChanged` subscriber observed `_gate` held. No unrelated test was included in this expect-fail command.
