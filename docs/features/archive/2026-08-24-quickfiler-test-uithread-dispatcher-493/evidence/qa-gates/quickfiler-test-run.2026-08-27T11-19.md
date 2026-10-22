# CI-Parity Test Gate (P3-T5)

Timestamp: 2026-08-27T11-19
Task: [P3-T5]
Command: `& $VSTEST QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook" /Logger:"trx;LogFileName=quickfiler-test-final.trx" /ResultsDirectory:TestResults\plan-logs\p3-t5`
EXIT_CODE: 0
Output Summary: `Test Run Successful.` Total tests 1072, Passed 1072, Failed 0, Skipped 0. The
failed-test set is empty and is therefore a subset of the empty `BaselineFailedTests` set. All six
R1-R6 names and both named theme tests appear in the passed-test list.

## Run summary

| Metric | Value | Phase 0 baseline (`P0-T12`) |
| --- | --- | --- |
| Verdict line | `Test Run Successful.` | `Test Run Successful.` |
| Total tests | 1072 | 1066 |
| Passed | 1072 | 1066 |
| Failed | 0 | 0 |
| Skipped | 0 | 0 |

The total rose by exactly 6, which is the six new R1-R6 regression tests. No pre-existing test was
lost, renamed away, or filtered out.

No `/Settings:` argument was supplied, matching `.github/workflows/_mstest-coverage.yml`. This run is
therefore the sequential CI-parity gate; `P3-T6` supplies the parallelized supplementary run.

## Subset comparison against BaselineFailedTests

Cited artifact, resolved per § Conventions from the stem `quickfiler-test-run-baseline`:
`<FEATURE>/evidence/baseline/quickfiler-test-run-baseline.2026-08-27T10-22.md`.

| Set | Contents |
| --- | --- |
| `BaselineFailedTests` recorded by `P0-T12` | (empty) |
| This run's failed fully-qualified test names | (empty) |
| Is this run's set a subset of the baseline set? | **yes** |

No test failed that was not already failing at the Phase 0 baseline, so the
`BLOCKED: post-change test regression blocks AC-9` branch of `P5-T9` is not taken.

## Required names present in the passed-test list

| Name | Result | Duration |
| --- | --- | --- |
| `EnsureDispatcher_WhileATransactionHoldsALiveDispatcher_DoesNotReplaceIt` (R1) | Passed | 7 ms |
| `EnsureDispatcher_WhenTheFieldIsNull_InstallsAndRestoresOnDispose` (R2) | Passed | 1 ms |
| `EnsureDispatcher_ScopeDisposedTwice_IsIdempotent` (R3) | Passed | 1 ms |
| `Transaction_SecondCallerCannotInstallUntilTheFirstRestores` (R4) | Passed | 6 ms |
| `Transaction_DisposedTwice_DoesNotOverReleaseTheGate` (R5) | Passed | 3 ms |
| `Install_CalledTwiceOnTheSameTransaction_ThrowsInvalidOperationException` (R6) | Passed | 4 ms |
| `SetThemeDark_FromNormal_SelectsDarkNormalTheme` | Passed | < 1 ms |
| `SetThemeLight_FromNormal_SelectsLightNormalTheme` | Passed | < 1 ms |

The two theme tests are this plan's only absolute pass assertions over a file it does not own; spec
AC-6 requires precisely that of those two by name. Both were already passing in the `P0-T12`
baseline, so the `BLOCKED: pre-existing failure in a sibling-owned test blocks AC-6` branch is not
taken either.

## Artifact hygiene

The TRX name is controlled by `LogFileName=quickfiler-test-final.trx`, so it carries no account or
host name. `/EnableCodeCoverage` also produced a `.coverage` attachment whose default filename embeds
the account and machine name; that file sits in the git-ignored `TestResults/plan-logs/p3-t5/` tree
and its path is deliberately not quoted here. Console log:
`TestResults/plan-logs/p3-t5/vstest.log` (git-ignored).
