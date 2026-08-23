# P5-T15 corrected H2 contract red evidence

Timestamp: 2026-08-04T23:47:00-04:00
Command: `Get-Process vstest`; `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Tests:UtilitiesCS.Test.EmailIntelligence.FilterOlFoldersControllerRefreshDisposalTests.CreateAsync_NullGlobals_DisposesFactoryViewer,UtilitiesCS.Test.EmailIntelligence.FilterOlFoldersControllerRefreshDisposalTests.CreateAsync_CompositionFault_DisposesFactoryViewerAndRethrowsOriginal`
EXIT_CODE: 1
Output Summary: No active `vstest` process was present before the serialized run. Two targeted tests ran: `CreateAsync_NullGlobals_DisposesFactoryViewer` failed and `CreateAsync_CompositionFault_DisposesFactoryViewerAndRethrowsOriginal` passed. The red failure was `Expected viewer.CloseCount to be 1, but found 0`.

The corrected null-globals test constructs and shows the recording viewer inside the supplied factory, verifies the direct `ArgumentNullException` (`ParamName == appGlobals`, no inner exception), and uses an assertion scope for independent factory/create, show, close, and dispose count observations. The current constructor performs the null-globals guard before assigning the candidate viewer or entering its cleanup region, leaving the factory-created viewer undisposed. The synchronous `FormClosed` add-fault case executes after service acquisition, preserves the original exception, and confirms zero retained `SnapshotChanged` handlers; its existing pass does not remediate the null-globals ownership failure.

An earlier run that asserted `ShowCount` without creating/showing inside the supplied factory was superseded and is not used as evidence.
