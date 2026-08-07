# P5-T28 red evidence

Timestamp: 2026-08-05T04:13:00.0000000Z

Command: `& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe' 'TaskMaster.Test\bin\Debug\TaskMaster.Test.dll' /Tests:RibbonFolderFilterCallback_ContainsThrowingFailureReporter`

EXIT_CODE: 0

Output Summary: VSTest reported the selected test body passed 1/1, then emitted an unhandled `InvalidOperationException: reporter failure` from `RibbonViewer.RunFolderFilterCallbackAsync`; the post-run exception is the expected red result.

- The command invoked the reporter with the original initialization exception, then emitted an unhandled `InvalidOperationException: reporter failure` from `RibbonViewer.RunFolderFilterCallbackAsync`.
- The test runner reported the test body as passed before the asynchronous unhandled exception was emitted. The post-run unhandled exception demonstrates that the callback boundary did not contain reporter failures.
- The red test suite also includes an incomplete `TaskCompletionSource` barrier that requires zero reports while initialization is suspended, exactly one report of the original fault after completion, delayed-success silence, and legacy-wrapper exception identity.
