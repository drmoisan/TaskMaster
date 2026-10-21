# Finding 3 — Fail-Before Run (P3-T5) [expect-fail]

Timestamp: 2026-09-03T02-33
Task: [P3-T5]
ExpectedExitCode: 1
EXIT_CODE: 1

A non-zero exit is the required outcome of this task. Three of the six new tests assert behavior the
pre-fix coordinator does not have; P3-T11 re-runs all six after the fix and requires all six to pass.

Command:

```
& $vstest TaskMaster.Test\bin\Debug\TaskMaster.Test.dll `
  /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation `
  "/TestCaseFilter:FullyQualifiedName~ApplyPrimeAsync_WhenPrimeResolvesAfterToggle_DoesNotOverwriteToggleResult|FullyQualifiedName~ExecuteToggleAsync_WhenOlderObservationCompletesLast_DoesNotOverwriteNewerResult|FullyQualifiedName~ExecuteToggleAsync_WithNoCompetingWriter_CachesValueAndInvalidatesExactlyOnce|FullyQualifiedName~ExecuteToggleAsync_WithNullEngines_ThrowsInvalidOperationExceptionWithoutTogglingEngine|FullyQualifiedName~GetPressed_WhenPrimeIsCanceled_LogsErrorAndClearsPrimeMarker|FullyQualifiedName~GetPressed_WhenPrimeIsCanceled_LeavesToggleReportingUnchecked" `
  "/Logger:trx;LogFileName=p3-t5.trx" `
  /ResultsDirectory:docs\features\active\2026-09-02-ribbon-engine-toggle-defects-735\evidence\regression-testing\p3-t5
```

## Counts read from the TRX `ResultSummary/Counters` element

| Counter | Value | Required |
|---|---|---|
| total | 6 | 6 |
| executed | 6 | — |
| passed | 3 | 3 |
| failed | 3 | 3 |
| notExecuted | 0 | — |

## Results directory contents

Exactly one TRX file and no other entry:

```
p3-t5.trx
```

Cleanup micro-action recorded for audit: as in P1-T2, the failing run left an empty MSTest
deployment scratch directory whose generated directory names are derived from the local account name
and the machine name. It contained no files and no evidence and was removed with
`[System.IO.Directory]::Delete(path, true)` immediately after the run. The token values are not
written here; they are derived at run time from `Split-Path -Leaf $env:USERPROFILE` and
`$env:COMPUTERNAME`.

## The three failures are exactly the three the plan named

### 1. `ApplyPrimeAsync_WhenPrimeResolvesAfterToggle_DoesNotOverwriteToggleResult` — FAILED

```
Expected harness.Coordinator.GetPressed(SpamEngine) to be True because the prime's observation
began before the toggle's, so its stale value must not overwrite the newer one, but found False.
```

This is the #525 reproduction. The prime's activation read began first but resolved last, and the
pre-fix prime writer stored its stale `false` unconditionally over the toggle's `true`.

### 2. `ExecuteToggleAsync_WhenOlderObservationCompletesLast_DoesNotOverwriteNewerResult` — FAILED

```
Expected harness.Coordinator.GetPressed(SpamEngine) to be True because the newer observation must
survive regardless of completion order, but found False.
```

Toggle versus toggle. Completion order does not track observation order, and the pre-fix toggle
writer also stores unconditionally, so the older observation won.

### 3. `GetPressed_WhenPrimeIsCanceled_LogsErrorAndClearsPrimeMarker` — FAILED

```
Expected harness.Errors to contain a single item because a canceled prime is a failure and must be
reported, not silently ignored, but the collection is empty.
```

CR-2. The pre-fix completion handler tests only the completed task's `Exception`, which is null for a
canceled task, so it returned early: nothing was logged and the in-flight marker was never cleared.

## Why the other three already pass before the fix

### `ExecuteToggleAsync_WithNoCompetingWriter_CachesValueAndInvalidatesExactlyOnce` — PASSED

The uncontended path. With no competing writer, the pre-fix unconditional write stores the same
value the post-fix compare-and-apply write stores, and invalidates once either way. This test exists
to guard against over-suppression by the NEW conditional invalidation, so it is expected to pass on
both sides of the fix; a post-fix failure here would mean the guard had degenerated into "never
invalidate".

### `ExecuteToggleAsync_WithNullEngines_ThrowsInvalidOperationExceptionWithoutTogglingEngine` — PASSED

CR-3. This is zero production change. The `InvalidOperationException` guard on the toggle path
already exists and already behaves correctly; the test closes an untested branch rather than
reproducing a defect, so it passes before and after.

### `GetPressed_WhenPrimeIsCanceled_LeavesToggleReportingUnchecked` — PASSED

The pre-fix canceled prime already leaves the cache unset, because it returns before writing
anything. What it fails to do is log and clear the marker, and that is what its companion test 3
catches. This test's role is to ensure the CR-2 fix clears the marker without also inventing a
cached value.

Output Summary: EXPECTED FAILURE achieved. EXIT_CODE 1 with TRX counters total 6, passed 3, failed 3.
The three failures are exactly the prime-after-toggle reproduction, the toggle-versus-toggle case
and the canceled-prime logging case; the uncontended case, the CR-3 guard case and the
cache-stays-unset companion already hold on the pre-fix code and are recorded above with the reason
for each.
