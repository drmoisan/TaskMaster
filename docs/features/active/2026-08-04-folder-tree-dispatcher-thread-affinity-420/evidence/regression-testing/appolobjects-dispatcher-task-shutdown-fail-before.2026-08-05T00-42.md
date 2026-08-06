Timestamp: 2026-08-05T00-42
Command: `& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe' 'TaskMaster.Test\bin\Debug\TaskMaster.Test.dll' /TestCaseFilter:"FullyQualifiedName~AppOlObjectsFolderTreeServiceLifecycleTests"`
EXIT_CODE: 1
Output Summary: Expected red result. Four class-level serialized lifecycle tests ran in 0.4845 seconds and failed naturally. No test runner was terminated.

Expected-red assertions:

- Both worker-first terminal-task tests observed the current implementation call `BeginInvoke` once rather than `InvokeAsync`: `BeginInvokeCallCount=1`, expected `0`. The controlled cancellation token and exact `InvalidOperationException` object therefore remain unobserved by production until the worker-first implementation is added.
- Disposal before the queued callback left the worker incomplete before test cleanup, rather than reaching the required terminal state.
- Releasing the late queued callback after disposal produced `LoadCount=1`, rather than the required zero.

Test design and cleanup:

- `ControlledUiDispatcher` implements `IUiDispatcher`, captures one `Action` per invocation, returns a controlled `Task` as `IAsyncResult` for legacy `BeginInvoke`, and exposes independent `Interlocked`/`Volatile` counters.
- Controlled callback-captured and operation tasks use `TaskCreationOptions.RunContinuationsAsynchronously`.
- Each worker starts with `Task.Run`; `Task.WhenAny(callbackCaptured, worker)` proves callback capture without timing, polling, retries, sleeps, timers, reflection, static UI dispatcher mutation, a live UI control, or temporary files.
- Each `finally` disposes the SUT, releases only the captured legacy callback, and observes the worker and every task created by the controlled dispatcher. The natural expected-red run completed with no remaining `vstest` or `testhost` process.

Supporting checks:

- Before the VSTest invocation, no `vstest` or `testhost` process was active. The same post-run check found none.
- `dotnet tool run csharpier format 'TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceLifecycleTests.cs'` exited 0.
- `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` exited 0 with five existing System.Reactive packages.config warnings and no errors.
- `git diff --check -- 'TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceLifecycleTests.cs' 'TaskMaster.Test/TaskMaster.Test.csproj'` exited 0; Git emitted only its LF-to-CRLF advisory for the project file.
- The lifecycle test partial is 308 lines, below the 500-line repository limit. `TaskMaster.Test.csproj` contains exactly one compile entry for it.

Result: EXPECTED FAIL. The tests establish the required red behavior before the P5-T19 production implementation. No acceptance criterion is marked complete.
