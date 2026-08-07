# P4-T2 regression pass: WPF dispatcher async overload

Timestamp: 2026-08-04T20:24:00-04:00

Command: `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /TestCaseFilter:'FullyQualifiedName~WpfUiDispatcherTests'`

EXIT_CODE: 0

Output Summary: Passed 3 of 3 tests: `InvokeAsync_AsyncFunction_ReturnsResultFromCapturedDispatcher`, `InvokeAsync_AsyncFunction_PropagatesOriginalFault`, and `InvokeAsync_CanceledBeforeDispatch_DoesNotExecuteAction`. The dedicated STA dispatcher host verified captured-dispatcher execution, preservation of the original inner exception, and cancellation before dispatch.
