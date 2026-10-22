# RED — #500 Hub Half: Lock Scope and Re-Entrancy (P5-T5) [expect-fail]

Timestamp: 2026-08-27T20-45

ExpectedExitCode: 1

Command:

```
& $vstest 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' /InIsolation
    '/Settings:scripts\vscode\TaskMaster.cli.runsettings'
    '/TestCaseFilter:FullyQualifiedName~PostJson_SurfaceInvocationRunsAfterHubLockIsReleased|FullyQualifiedName~PostJson_ReentrantAttachFromSurfaceDoesNotThrowCollectionModified'
    '/Logger:trx;LogFileName=p5-t5.trx'
    '/ResultsDirectory:docs/features/active/breadcrumb-coordinator-hub-defects-501/evidence/regression-testing/trx/p5-t5'
```

The test project was rebuilt (`/t:Rebuild`, `BUILD_EXIT=0`, zero compiler errors) immediately before
this run.

EXIT_CODE: 1

Output Summary:

```
  Failed PostJson_SurfaceInvocationRunsAfterHubLockIsReleased [251 ms]
  Failed PostJson_ReentrantAttachFromSurfaceDoesNotThrowCollectionModified [9 ms]
Total tests: 2
     Failed: 2
Test Run Failed.
```

| Metric | Value |
| --- | ---: |
| Tests run | 2 |
| Failed | 2 |
| Passed | 0 |

The observed exit code equals the declared `ExpectedExitCode`, so this gate is a PASS: failing tests
are the intended outcome of this task, and only of this task.

## Verbatim failure text 1 — the lock probe (I-500.2)

```
Expected held to be False because no surface call may run under the hub's _sync (I-500.2), but found True.
```

The lock probe observed **`True`**: `QuickFiler/Viewers/BreadcrumbMessengerHub.cs:126` takes
`lock (_sync)` and `:133` calls `PostToSurface` — which reaches
`attachment.Messenger.PostJson(json)` at `:206` — while still inside that lock.

## Verbatim failure text 2 — the re-entrancy test (I-500.4)

```
Did not expect System.InvalidOperationException because a re-entrant Attach must not invalidate the broadcast enumeration, but found System.InvalidOperationException: Collection was modified; enumeration operation may not execute.
```

The exception is **`InvalidOperationException`** with the message
`Collection was modified; enumeration operation may not execute.` The `foreach` at
`QuickFiler/Viewers/BreadcrumbMessengerHub.cs:131` enumerates the LIVE `_attachments` dictionary, so the
re-entrant `Attach` performed from inside the surface callback invalidates the in-progress enumerator.

Both failure forms are exactly the two the task's acceptance names: one failure showing the lock probe
observed `True`, and the other showing `InvalidOperationException`.

## Note on test-file budget

Both tests live in `QuickFiler.Test/Viewers/BreadcrumbSelectorCoordinatorTests.cs`, the sanctioned
overflow home for the hub-side probes (it owns the `Monitor.IsEntered` template). The two tests plus
one reflection helper were authored, then compacted, to fit the file's exact stated headroom: 66 added
lines against 66 available, bringing the file to **exactly 500** lines. CSharpier reports the file
already formatted, so the Phase 7 formatting pass will not change that count. No test method was
relocated and no new file was created.

TRX artifact: `FF/evidence/regression-testing/trx/p5-t5/p5-t5.trx`, post-processed so it carries no
absolute host path, no account name and no machine name.

Acceptance: 2 run, 2 failed, 0 passed, one failure showing the lock probe observed `True` and the other
showing `InvalidOperationException`. PASS.
