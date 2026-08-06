# P1-T2 fail-before: publish after disposal

Timestamp: 2026-08-04T20:19:00-04:00

Command: `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /TestCaseFilter:'FullyQualifiedName~Dispose_DuringBuild_LeavesDisposedWithoutPublicationOrNotification'`

EXIT_CODE: 1

Output Summary: After temporarily removing only `ThrowIfDisposed()` from the publication lock in `OutlookFolderTreeService.BuildAndPublishAsync`, the deterministic controlled incomplete-builder test failed because no `ObjectDisposedException` was thrown after disposal. The terminal-state guard was immediately restored before any further work.
