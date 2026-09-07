# GREEN BEFORE THE FIX — #500 Cross-Cutting NFR Guard (P3-T4)

Timestamp: 2026-08-27T20-25

Command:

```
& $vstest 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' /InIsolation
    '/Settings:scripts\vscode\TaskMaster.cli.runsettings'
    '/TestCaseFilter:FullyQualifiedName~TryRunCurrent_ReentrantInvalidateStillReportsEntryTimeInvocation'
    '/Logger:trx;LogFileName=p3-t4.trx'
    '/ResultsDirectory:docs/features/active/breadcrumb-coordinator-hub-defects-501/evidence/regression-testing/trx/p3-t4'
```

The test project was rebuilt (`/t:Rebuild`, zero compiler errors) immediately before this run.

EXIT_CODE: 0

Output Summary:

```
  Passed TryRunCurrent_ReentrantInvalidateStillReportsEntryTimeInvocation [36 ms]

Test Run Successful.
Total tests: 1
     Passed: 1
 Total time: 1.4025 Seconds
```

| Metric | Value |
| --- | ---: |
| Tests run | 1 |
| Passed | 1 |
| Failed | 0 |

## Why establishing green BEFORE the fix matters

This guard already holds on HEAD. Running it now, before `TryRunCurrent` is restructured, is what makes
it a regression detector rather than a restatement of the fix. If it were only ever run after the
change, a later reader could not distinguish "the fix preserved the entry-time-verdict contract" from
"the fix established it", and the guard would carry no information about a future regression.

The regression it guards is specific: research section 6.2 option B, which folds a post-action currency
re-check into `TryRunCurrent`'s return value. Under option B this test fails, because an action that
invalidates its own lease would make the method return `false` even though the action ran. Per the
cross-cutting NFR that would turn the #502 remedy into a fresh instance of the #502 defect —
`SetSuggestions` would overwrite a live `SuggestionsUpgrade` handle with `Task.CompletedTask` after the
guarded lambda had already assigned the real one.

P3-T6 re-runs this test with the P3-T1 lock probe after the fix; both must be green there.

TRX artifact: `FF/evidence/regression-testing/trx/p3-t4/p3-t4.trx`, post-processed so it carries no
absolute host path, no account name and no machine name.

Acceptance: `EXIT_CODE: 0` with 1 passed, 0 failed, establishing that the guard already holds before
the fix and therefore detects a regression rather than the fix itself. PASS.
