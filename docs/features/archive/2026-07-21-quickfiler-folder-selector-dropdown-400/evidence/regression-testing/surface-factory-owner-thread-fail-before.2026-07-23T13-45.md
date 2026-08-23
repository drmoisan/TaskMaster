# Surface factory owner-thread failure-first run

- Timestamp: `2026-07-23T13-45Z`
- Command: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /Tests:SurfaceFactory_WorkerCompletion_DispatchesEveryStageAndCleanup,Readiness_DisposeFromAmbientNullWorker_DispatchesHandlerDetachment /Logger:console;Verbosity=detailed`
- EXIT_CODE: `1`
- Output Summary: `Exactly two tests were discovered; zero passed, two failed for the intended creator-thread assertions, and zero were skipped.`

## Intended failures

| Test | Result | Exact assertion evidence |
|---|---|---|
| `SurfaceFactory_WorkerCompletion_DispatchesEveryStageAndCleanup` | Failed | `Expected fixture.Log.OffBoundary to be empty, but found at least one item {"create"}.` |
| `Readiness_DisposeFromAmbientNullWorker_DispatchesHandlerDetachment` | Failed | `Expected log.OffBoundary to be empty, but found at least one item {"detach"}.` |

The strengthened assertion seam changed `OperationRecorder` from ambient
`SynchronizationContext` reference comparison to actual creator-thread identity.
`RecordingSynchronizationContext.Post` remained unchanged for this run. Both existing
tests therefore exposed that posted callbacks execute on worker threads under the current
inline fake.

The run completed in `1.0023` seconds. There was no unrelated failure, build failure,
crash, timeout, hang, missing test, or skipped test. This is the required deterministic
failure-first result for P8-T29.
