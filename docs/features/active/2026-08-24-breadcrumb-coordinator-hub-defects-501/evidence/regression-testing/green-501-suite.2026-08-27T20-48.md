# GREEN — #501 Hub Regression Surface (P5-T9)

Timestamp: 2026-08-27T20-48

Command:

```
& $vstest 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' /InIsolation
    '/Settings:scripts\vscode\TaskMaster.cli.runsettings'
    '/TestCaseFilter:FullyQualifiedName~BreadcrumbMessengerHubTests|FullyQualifiedName~BreadcrumbMessengerHubCoverageTests|FullyQualifiedName~BreadcrumbSelectorCoordinatorTests|FullyQualifiedName~BreadcrumbDuplicateIdentityIntegrationTests'
    '/Logger:trx;LogFileName=p5-t9.trx'
    '/ResultsDirectory:docs/features/active/breadcrumb-coordinator-hub-defects-501/evidence/regression-testing/trx/p5-t9'
```

EXIT_CODE: 0

Output Summary:

```
  Passed Attach_ReplayFailureRollsBackSubscriptionAndAllowsRetry [< 1 ms]
Test Run Successful.
Total tests: 41
     Passed: 41
```

| Metric | Value |
| --- | ---: |
| Total | 41 |
| Passed | 41 |
| Failed | 0 |
| Skipped | 0 |

`vstest.console.exe` prints a `Failed:` line and a `Skipped:` line only when the respective count is
non-zero; neither line appears in the output, so both are 0.

## Named test confirmation

`Attach_ReplayFailureRollsBackSubscriptionAndAllowsRetry` is confirmed **PASSED** in this run (its
`Passed` line is quoted verbatim above). That test is I-501.5 and AC-22: `Attach`'s existing
transactional rollback must not be weakened by the `PostJson` rewrite.

It is the sharpest available check that the rewrite did not over-reach. The test relies on `Attach`
still PROPAGATING a replay failure and rolling back the subscription, even though `PostJson` now
SWALLOWS a broadcast failure. Those two behaviours had to diverge: SR-3 contains the broadcast throw,
while `Attach`'s own replay path keeps its `try`/`catch`-and-rethrow. The rewrite touched only the
broadcast, so `Attach` is unchanged and the test passes with no edit to its file.

The 41-test surface also covers `BreadcrumbMessengerHubCoverageTests`, whose 478 lines of hub coverage
tests exercise attachment, detachment, disposal and replay paths, and
`BreadcrumbDuplicateIdentityIntegrationTests`.

TRX artifact: `FF/evidence/regression-testing/trx/p5-t9/p5-t9.trx`, post-processed so it carries no
absolute host path, no account name and no machine name.

Acceptance: `EXIT_CODE: 0`, 0 failed, 0 skipped, and
`Attach_ReplayFailureRollsBackSubscriptionAndAllowsRetry` named as passed. PASS.
