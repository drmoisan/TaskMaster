# RED — #502 Companion Lease Leak (P4-T2) [expect-fail]

Timestamp: 2026-08-27T20-29

ExpectedExitCode: 1

Command:

```
& $vstest 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' /InIsolation
    '/Settings:scripts\vscode\TaskMaster.cli.runsettings'
    '/TestCaseFilter:FullyQualifiedName~RunSynchronous_SupersededLeaseSettlesAndDisposesItsSource'
    '/Logger:trx;LogFileName=p4-t2.trx'
    '/ResultsDirectory:docs/features/active/breadcrumb-coordinator-hub-defects-501/evidence/regression-testing/trx/p4-t2'
```

The test project was rebuilt (`/t:Rebuild`, `BUILD_EXIT=0`, zero compiler errors) immediately before
this run. That zero-error build is itself significant: it confirms the test **compiles against the
current signature**, which is what makes this a genuine failing-first test rather than a compile error.
`lifetime.RunSynchronous(lease, () => ran = true);` is written as a statement, so it is valid against
both the pre-change `void` return and the post-change `bool` return.

EXIT_CODE: 1

Output Summary:

```
  Failed RunSynchronous_SupersededLeaseSettlesAndDisposesItsSource [135 ms]
Total tests: 1
     Failed: 1
Test Run Failed.
```

| Metric | Value |
| --- | ---: |
| Tests run | 1 |
| Failed | 1 |
| Passed | 0 |

The observed exit code equals the declared `ExpectedExitCode`, so this gate is a PASS: a failing test
is the intended outcome of this task, and only of this task.

## Verbatim failure text

```
Expected lease.Settled to be True because a skipped lease must still be settled (I-502.3), but found False.
```

The failure names **`Settled`** with an observed value of **`False`**, which is the companion defect
exactly. `Complete(lease)` — the only thing that sets `Settled = true` — is reached only from
`RunAsync`'s `finally` and from `Abandon`. A skipped `RunSynchronous` calls neither, so
`CancelLease`'s disposal condition `lease.Settled && !lease.SourceDisposed` never holds and the lease's
`CancellationTokenSource` is leaked once per superseded population.

The test's first assertion, `ran.Should().BeFalse()`, is already satisfied on HEAD — the guarded action
genuinely does not run for a superseded lease. The test fails only on the settlement assertion, which
isolates the leak precisely.

This is the failing-first test AC-19 requires: it lives in
`QuickFiler.Test/Viewers/BreadcrumbCoordinatorUpgradeLifetimeTests.cs`, compiles against HEAD, and is
demonstrated RED there.

TRX artifact: `FF/evidence/regression-testing/trx/p4-t2/p4-t2.trx`, post-processed so it carries no
absolute host path, no account name and no machine name.

Acceptance: 1 run, 1 failed, 0 passed, and the failure text names `Settled` with an observed value of
`False`. PASS.
