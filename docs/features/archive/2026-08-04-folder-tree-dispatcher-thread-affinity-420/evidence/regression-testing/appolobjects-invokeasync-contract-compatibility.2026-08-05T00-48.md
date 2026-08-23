Timestamp: 2026-08-05T00-48

Command: Multiple recorded commands — `dotnet tool run csharpier format 'TaskMaster/AppGlobals/AppOlObjects.FolderTreeService.cs'`; `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`; and the individually serialized VSTest cases listed under Verification.
EXIT_CODE: Mixed — formatter and analyzer build exited 0; the two P5-T20 disposal cases are explicitly recorded expected-red results.
Output Summary: P5-T19 compatibility behavior was verified; the artifact retains the two intentionally unresolved P5-T20 expected-red cases and makes no acceptance-criterion completion claim.

P5-T19 compatibility assessment:

- `TaskMaster/AppGlobals/AppOlObjects.FolderTreeService.cs` now consumes the existing `IUiDispatcher.InvokeAsync(Action): Task` member for worker-first composition. It does not alter the `IUiDispatcher` interface, `WpfUiDispatcher`, any fake dispatcher, or QuickFiler seam signature.
- The dispatcher task is observed before the synchronous public getter waits. The observer uses `TaskContinuationOptions.ExecuteSynchronously` and `TaskScheduler.Default`; already-completed tasks are also classified immediately so terminal cancellation or fault cannot leave the getter waiting for scheduler availability.
- Canceled dispatch tasks preserve the observed `OperationCanceledException.CancellationToken` via `TrySetCanceled`. Faulted tasks, including an `OperationCanceledException` fault, remain faulted through `TrySetException` with the exception returned by `GetAwaiter().GetResult()`.
- A null dispatcher task becomes `InvalidOperationException("Folder tree service dispatcher returned a null task.")` through the same reset path. Terminal detachment clears only matching ownership; stale callbacks cannot publish, clear, or overwrite newer ownership.
- `OnFolderTreeServiceInitializationTerminal(Task<IOutlookFolderTreeService>)` is protected/internal, instance-scoped, and invoked through a nonthrowing wrapper after terminal completion. It does not alter public production behavior.

Verification:

- `dotnet tool run csharpier format 'TaskMaster/AppGlobals/AppOlObjects.FolderTreeService.cs'` exited 0.
- `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` exited 0 with seven existing repository warnings and no errors.
- Individually serialized tests ran after verifying no active `vstest` or `testhost` process before each run:
  - `WorkerFirst_CanceledInvokeAsyncTask_RequiresTerminalCancellation`: passed in 0.4376 seconds.
  - `WorkerFirst_FaultedInvokeAsyncTask_RequiresOriginalFault`: passed in 0.4379 seconds.
  - `DisposeBeforeQueuedCallback_LeavesWorkerIncompleteBeforeCleanup`: expected red failure in 0.4656 seconds because P5-T20 has not yet detached pending ownership on `Dispose`.
  - `LateCallbackAfterDispose_DoesNotLoadFolderTreeService`: expected red failure in 0.4630 seconds with `LoadCount=1`, also reserved for P5-T20.
- Two superseded VSTest processes terminated during diagnosis are excluded from evidence. The final individual runs completed naturally; the post-run process check found no `vstest` or `testhost` process.
- `git diff --check -- 'TaskMaster/AppGlobals/AppOlObjects.FolderTreeService.cs' 'TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceLifecycleTests.cs' 'TaskMaster.Test/TaskMaster.Test.csproj'` exited 0, with only the repository LF-to-CRLF advisory for the project file.

Result: P5-T19 implementation verified. The P5-T20 disposal-specific red cases remain intentionally unresolved. No acceptance criterion is marked complete.
