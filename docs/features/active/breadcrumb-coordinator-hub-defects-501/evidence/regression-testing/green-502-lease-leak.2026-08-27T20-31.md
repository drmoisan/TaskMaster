# GREEN AFTER THE FIX — #502 Companion Lease Leak (P4-T4)

Timestamp: 2026-08-27T20-31

Command:

```
& $vstest 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' /InIsolation
    '/Settings:scripts\vscode\TaskMaster.cli.runsettings'
    '/TestCaseFilter:FullyQualifiedName~RunSynchronous_SupersededLeaseSettlesAndDisposesItsSource'
    '/Logger:trx;LogFileName=p4-t4.trx'
    '/ResultsDirectory:docs/features/active/breadcrumb-coordinator-hub-defects-501/evidence/regression-testing/trx/p4-t4'
```

EXIT_CODE: 0

Output Summary:

```
  Passed RunSynchronous_SupersededLeaseSettlesAndDisposesItsSource [31 ms]
Test Run Successful.
Total tests: 1
     Passed: 1
```

| Metric | Value |
| --- | ---: |
| Tests run | 1 |
| Passed | 1 |
| Failed | 0 |

## Red-to-green transition

| Run | Outcome | Evidence |
| --- | --- | --- |
| P4-T2, before the fix | FAILED — `Expected lease.Settled to be True ... but found False` | `FF/evidence/regression-testing/red-502-lease-leak.2026-08-27T20-29.md` |
| P4-T4, after the fix | PASSED | this artifact |

The companion leak is closed. `RunSynchronous` now calls `Abandon(lease)` on its own skip path (ruling
PD-1), which reaches `Complete(lease)`, sets `Settled = true`, and lets `CancelLease`'s disposal
condition `lease.Settled && !lease.SourceDisposed` hold, so the lease's `CancellationTokenSource` is
disposed rather than leaked. All three assertions now hold: the guarded action did not run, `Settled` is
`true`, and `SourceDisposed` is `true`.

This artifact supports AC-15 (I-502.3) and is the pass-after half of AC-19.

TRX artifact: `FF/evidence/regression-testing/trx/p4-t4/p4-t4.trx`, post-processed so it carries no
absolute host path, no account name and no machine name.

Acceptance: `EXIT_CODE: 0` with 1 passed, 0 failed. PASS.
