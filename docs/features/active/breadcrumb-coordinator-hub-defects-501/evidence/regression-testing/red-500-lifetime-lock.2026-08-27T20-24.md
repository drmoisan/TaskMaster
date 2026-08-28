# RED — #500 Lifetime Lock Scope (P3-T2) [expect-fail]

Timestamp: 2026-08-27T20-24

ExpectedExitCode: 1

Command:

```
& $vstest 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' /InIsolation
    '/Settings:scripts\vscode\TaskMaster.cli.runsettings'
    '/TestCaseFilter:FullyQualifiedName~TryRunCurrent_GuardedActionRunsWithoutHoldingLifetimeSync'
    '/Logger:trx;LogFileName=p3-t2.trx'
    '/ResultsDirectory:docs/features/active/breadcrumb-coordinator-hub-defects-501/evidence/regression-testing/trx/p3-t2'
```

The test project was rebuilt (`/t:Rebuild`, EXIT_CODE 0, zero compiler errors) immediately before this
run.

EXIT_CODE: 1

Output Summary:

```
Test Run Failed.
Total tests: 1
     Failed: 1
 Total time: 1.3238 Seconds
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
Expected heldDuringAction to be False because no foreign call may be made while the lifetime's _sync is held (I-500.1), but found True.
```

The observed value was **`True`**. That is the defect exactly as diagnosed:
`QuickFiler/Viewers/BreadcrumbCoordinatorUpgradeLifetime.cs:139` takes `lock (_sync)` and `:145`
invokes `action()` while still inside that lock, so any foreign call the action makes — in production,
the chain that ends at `WebView2Messenger`'s out-of-process `PostWebMessageAsJson` — is made under the
monitor.

Note that the companion assertion in the same test,
`invoked.Should().BeTrue()`, is already satisfied on HEAD: `TryRunCurrent` does return `true` for a
current lease. The test fails only on the lock-scope probe, which isolates the defect precisely.

TRX artifact: `FF/evidence/regression-testing/trx/p3-t2/p3-t2.trx`, post-processed so it carries no
absolute host path, no account name and no machine name.

Acceptance: 1 run, 1 failed, 0 passed, and the failure text shows the observed value was `True`. PASS.
