Timestamp: 2026-08-05T00-36
Command: `& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe' 'UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll' /TestCaseFilter:"FullyQualifiedName~DisposeDuringCandidateViewCommit_DoesNotRetainViewOrSubscription"`
EXIT_CODE: 1
Output Summary: Expected red result; 1 test ran and failed naturally in 0.637 seconds. After the blocking `SetController` signal, disposal, release, readiness completion, and retained `SnapshotChanged` invocation, the controller retained a `FolderTreeCompatibilityView` and one service handler rather than zero.

Command: `& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe' 'UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll' /TestCaseFilter:"FullyQualifiedName~DisposeDuringSnapshotSubscription_DoesNotRetainViewOrSubscription"`
EXIT_CODE: 1
Output Summary: Expected red result; 1 test ran and failed naturally in 0.648 seconds. After the add accessor stored the delegate, disposal occurred before subscription-state commit, the retained callback was invoked, and release completed readiness; the controller retained a view and one service handler rather than zero.

Command: `& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe' 'UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll' /TestCaseFilter:"FullyQualifiedName~DisposeDuringCandidateViewCommit_DoesNotRetainViewOrSubscription|FullyQualifiedName~DisposeDuringSnapshotSubscription_DoesNotRetainViewOrSubscription"`
EXIT_CODE: 1
Output Summary: Fresh combined serialized confirmation: 2 expected-red tests executed in the same class and both failed naturally in 0.650 seconds. Each recorded both retained-view and retained-handler assertions; no hang occurred.

Preconditions and diagnostics:
- Before every run, no `vstest` or `testhost` process was active.
- After the combined run, no `vstest` or `testhost` process remained.
- The superseded killed runs are not used as evidence.
- `dotnet tool run csharpier format 'UtilitiesCS.Test/EmailIntelligence/FilterOlFoldersControllerRefreshDisposalTests.LifecycleRaces.cs'` exited 0.
- `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` exited 0. Existing repository warnings remain, including the P5-T37 CS8632 diagnostic.
- `git diff --check -- 'UtilitiesCS.Test/EmailIntelligence/FilterOlFoldersControllerRefreshDisposalTests.cs' 'UtilitiesCS.Test/EmailIntelligence/FilterOlFoldersControllerRefreshDisposalTests.LifecycleRaces.cs' 'UtilitiesCS.Test/UtilitiesCS.Test.csproj'` exited 0 with only the repository LF-to-CRLF advisory.
- Original controller partial: 498 lines; lifecycle-races partial: 235 lines (limit: 300).

Candidate-view interleaving:
- Construction runs on `Task.Run` while the snapshot is incomplete, preventing the production continuation from capturing the MSTest synchronization context.
- The blocking recording viewer signals inside `SetController` after candidate-view assignment, then waits for a `RunContinuationsAsynchronously` release signal.
- The test awaits barriers, readiness, and the retained-notification operation with `ConfigureAwait(false)`; it calls `Dispose` while `SetController` is paused and releases unconditionally in `finally`.
- The test invokes `RaiseSnapshotChanged` after release and awaits `LastAsyncOperation`, so the assertion covers post-dispose notification work rather than a vacuous refresh count.
- `AssertionScope` reports the red retained view and `SnapshotChangedHandlerCount=1` together. `RefreshViewAppliedCount=0` confirms no post-dispose refresh application occurred.

Subscription interleaving:
- The fake `SnapshotChanged` add accessor stores the delegate, signals storage, then waits for a `RunContinuationsAsynchronously` release signal before returning.
- The test disposes after storage and before `_snapshotSubscriptionAttached` can commit, invokes `RaiseSnapshotChanged` against the retained delegate, releases in `finally`, and awaits readiness without capturing the test context.
- `AssertionScope` reports the red retained view and `SnapshotChangedHandlerCount=1` together. `RefreshViewAppliedCount=0` confirms no post-dispose refresh application occurred.

Result: EXPECTED FAIL. These replacement red tests close the prior audit gap by proving both precise lifecycle windows with task signals, exercising the candidate retained callback, and completing without a runner hang. No acceptance criterion is marked complete.
