# P2-T3 pass: publish after disposal

Timestamp: 2026-08-04T20:20:00-04:00

Command: `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /TestCaseFilter:'FullyQualifiedName~Dispose_DuringBuild_LeavesDisposedWithoutPublicationOrNotification'`

EXIT_CODE: 0

Output Summary: With `ThrowIfDisposed()` restored in the publication lock, the deterministic controlled incomplete-builder regression passed. The test verifies terminal `Disposed` state, no snapshot publication or notification, and no retained notification handler.
