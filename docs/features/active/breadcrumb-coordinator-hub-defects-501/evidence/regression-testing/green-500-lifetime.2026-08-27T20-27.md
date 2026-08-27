# GREEN AFTER THE FIX — #500 Lifetime Half (P3-T6)

Timestamp: 2026-08-27T20-27

Command:

```
& $vstest 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' /InIsolation
    '/Settings:scripts\vscode\TaskMaster.cli.runsettings'
    '/TestCaseFilter:FullyQualifiedName~TryRunCurrent_GuardedActionRunsWithoutHoldingLifetimeSync|FullyQualifiedName~TryRunCurrent_ReentrantInvalidateStillReportsEntryTimeInvocation'
    '/Logger:trx;LogFileName=p3-t6.trx'
    '/ResultsDirectory:docs/features/active/breadcrumb-coordinator-hub-defects-501/evidence/regression-testing/trx/p3-t6'
```

EXIT_CODE: 0

Output Summary:

```
  Passed TryRunCurrent_GuardedActionRunsWithoutHoldingLifetimeSync [34 ms]
  Passed TryRunCurrent_ReentrantInvalidateStillReportsEntryTimeInvocation [< 1 ms]

Test Run Successful.
Total tests: 2
     Passed: 2
 Total time: 1.2889 Seconds
```

| Metric | Value |
| --- | ---: |
| Total | 2 |
| Passed | 2 |
| Failed | 0 |
| Skipped | 0 |

## Red-to-green transition

| Test | Before the fix | After the fix |
| --- | --- | --- |
| `TryRunCurrent_GuardedActionRunsWithoutHoldingLifetimeSync` (I-500.1) | FAILED — `Monitor.IsEntered` observed `True` (P3-T2) | PASSED |
| `TryRunCurrent_ReentrantInvalidateStillReportsEntryTimeInvocation` (I-500.3) | PASSED (P3-T4) | PASSED |

The lock probe moved from RED to GREEN: the guarded action now runs with `_sync` released, so no
foreign or out-of-process call is made under the lifetime's monitor.

The NFR guard held across the change, which is the discriminating result. It confirms the fix is
research section 6.2 **option A** and not option B: `TryRunCurrent` still returns `true` for an action
that re-entrantly invalidates its own lease, so the `bool` remains the entry-time verdict that the #502
call sites in Phase 4 will branch on. Had option B been implemented, this test would have failed here
while the lock probe passed.

This artifact supports AC-04 (I-500.1), AC-06 (I-500.3) and AC-28 (the cross-cutting NFR).

TRX artifact: `FF/evidence/regression-testing/trx/p3-t6/p3-t6.trx`, post-processed so it carries no
absolute host path, no account name and no machine name.

Acceptance: `EXIT_CODE: 0` with 2 passed, 0 failed, 0 skipped. PASS.
