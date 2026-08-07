Timestamp: 2026-08-06T17-03
Task: [P5-T43] deterministic lifecycle coverage-test synchronization correction.

## Failure record and root cause

The fresh combined P5-T46 diagnostic coverage run failed `DispatchAndCandidateDisposalFailures_PreserveTerminalBehavior` with 6148/6149 tests passing. The failing assertion was `Task.WhenAny(callbackCaptured, worker).Should().BeSameAs(callbackCaptured)` in `StartWorkerAsync`.

The first correction accepted a completed callback when the worker completed first. A rebuilt coverage-context rerun still failed because `callbackCaptured` was not completed. The controlled dispatcher test double permits inline composition when its captured thread id matches the thread-pool worker. Under coverage instrumentation, the test method and `Task.Run` worker can use that same pool thread. The helper therefore did not guarantee the queued path that its returned `ControlledDispatchOperation` requires.

`StartWorkerAsync` now sets its existing instance-local `dispatcher.ForceQueue` seam before it starts the worker. It retains the causal completion guard for a worker-first observation. This forces only the test double's queued path; it introduces no timer, polling, reflection, global state, or production-code change.

## Verification

Commands run in order:

1. `dotnet tool run csharpier format TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceTests.cs TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceLifecycleTests.Coverage.cs`
2. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /verbosity:minimal`
3. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true /verbosity:minimal`
4. Five repeated `dotnet-coverage collect` runs using the P5-T46 settings, `/InIsolation`, test-assembly exclusion, and filter `TestCategory!=LiveOutlook&(FullyQualifiedName~DispatchAndCandidateDisposalFailures_PreserveTerminalBehavior|FullyQualifiedName~DiscardCandidate_ServiceDisposeFailureStillDisposesSink)`.

Formatting passed. Analyzer and nullable builds passed with the repository's existing five packages.config warnings and zero errors. Each of the five coverage runs passed both affected tests (10/10 total): `DispatchAndCandidateDisposalFailures_PreserveTerminalBehavior` and `DiscardCandidate_ServiceDisposeFailureStillDisposesSink`.

The lifecycle coverage partial remains 102 lines (<=500), the shared lifecycle helper file is 456 lines (<=500), and `TaskMaster.Test.csproj` has exactly one `Compile` entry for the partial. The repeated and combined diagnostic Cobertura/TRX/effective-settings artifacts are transient diagnostics and are removed after this record.
