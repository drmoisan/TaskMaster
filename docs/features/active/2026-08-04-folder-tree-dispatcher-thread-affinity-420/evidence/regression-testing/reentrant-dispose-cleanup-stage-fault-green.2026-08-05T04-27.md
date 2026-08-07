# P5-T33 green evidence

Timestamp: 2026-08-05T04:27:00-04:00

Command: `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~OutlookFolderTreeServiceTraversalCancellationTests"`

EXIT_CODE: 0

Output Summary: Four traversal cancellation tests passed.

- The reentrant hierarchy read calls `Dispose` twice before the queued dispatcher is drained and records exactly one queued cleanup action.
- A retained notification raised after terminal state and before drain neither schedules work nor reattaches subscriptions; publication count remains zero.
- The controlled `FolderAdded` unsubscribe failure does not prevent the later unsubscribe stages or sink disposal. The observer receives that exact stage exception once.
- Cancellation callback failure is unwrapped to the exact original exception while every cleanup stage still runs.
- No post-cleanup reader-adapter access occurs, and the original traversal completes with `ObjectDisposedException`.
