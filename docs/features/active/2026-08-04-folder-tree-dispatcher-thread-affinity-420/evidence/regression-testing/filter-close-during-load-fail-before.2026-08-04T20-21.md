# P1-T4 fail-before: close during cold initialization

Timestamp: 2026-08-04T20:21:00-04:00

Command: `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /TestCaseFilter:'FullyQualifiedName~CreateAsync_ClosedBeforeSnapshotCompletes_DoesNotWireViewerOrRetainHandler'`

EXIT_CODE: 1

Output Summary: After temporarily moving only the `FormClosed` subscription from constructor time to after the initial snapshot await, the controlled close-before-completion regression failed because the viewer was wired to a `FilterOlFoldersController`. The early constructor subscription was restored immediately.
