# GREEN AFTER THE FIX — #502 All Three Assertions (P4-T10)

Timestamp: 2026-08-27T20-36

## Step 1 — analyzer build

Command: `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
EXIT_CODE: 0
Output Summary: `5 Warning(s)`, `0 Error(s)`, `Time Elapsed 00:00:19.71`.
Count of lines matching `Skipping target "CoreCompile"`: **0** — the gate is non-vacuous.

This build is the first to compile the new test file
`QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorSupersessionTests.cs` through the single
`<Compile Include>` line P4-T9 added, and it also confirms the `SetSuggestionsCore` internal seam is
reachable from `QuickFiler.Test` via `[assembly: InternalsVisibleTo("QuickFiler.Test")]`.

## Step 2 — scoped test run

Command:

```
& $vstest 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' /InIsolation
    '/Settings:scripts\vscode\TaskMaster.cli.runsettings'
    '/TestCaseFilter:FullyQualifiedName~RunSynchronous_SupersededLeaseReportsSkipToCaller|FullyQualifiedName~SetSuggestionsCore_SupersededLeaseReplacesStaleSuggestionsUpgrade|FullyQualifiedName~RunSynchronous_SupersededLeaseSettlesAndDisposesItsSource'
    '/Logger:trx;LogFileName=p4-t10.trx'
    '/ResultsDirectory:docs/features/active/breadcrumb-coordinator-hub-defects-501/evidence/regression-testing/trx/p4-t10'
```

EXIT_CODE: 0

Output Summary:

```
  Passed RunSynchronous_SupersededLeaseSettlesAndDisposesItsSource [32 ms]
  Passed RunSynchronous_SupersededLeaseReportsSkipToCaller [< 1 ms]
  Passed SetSuggestionsCore_SupersededLeaseReplacesStaleSuggestionsUpgrade [233 ms]

Test Run Successful.
Total tests: 3
     Passed: 3
```

| Metric | Value |
| --- | ---: |
| Total | 3 |
| Passed | 3 |
| Failed | 0 |
| Skipped | 0 |

## Invariant coverage

| Test | Invariant | AC |
| --- | --- | --- |
| `RunSynchronous_SupersededLeaseReportsSkipToCaller` | I-502.1, both directions ("when and only when") | AC-12 |
| `SetSuggestionsCore_SupersededLeaseReplacesStaleSuggestionsUpgrade` | I-502.2, the stale handle is replaced by a completed task | AC-13 |
| `RunSynchronous_SupersededLeaseSettlesAndDisposesItsSource` | I-502.3, no lease leak | AC-15, AC-19 |

The supersession test's 233 ms duration reflects the real coordinator construction plus the gated
population it arranges; its gating `TaskCompletionSource` is never completed, so no wait and no timer is
involved. Its arrange section asserts the captured handle's `IsCompleted` is `false` BEFORE the act
step, which is what prevents the reference-inequality assertion from being trivially satisfiable
against the `Task.CompletedTask` singleton.

TRX artifact: `FF/evidence/regression-testing/trx/p4-t10/p4-t10.trx`, post-processed so it carries no
absolute host path, no account name and no machine name.

Acceptance: `EXIT_CODE: 0` with 3 passed, 0 failed, 0 skipped. PASS.
