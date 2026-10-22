# P3-T1 — Targeted Pass-After Verification (Issue #751)

Timestamp: 2026-09-03T14-36

Command (executed from `coverage\logs\P3-T1.ps1`):

```
& $vstest 'TaskMaster.Test\bin\Debug\TaskMaster.Test.dll' /EnableCodeCoverage /InIsolation "/Logger:trx;LogFileName=P3-T1.trx" "/TestCaseFilter:FullyQualifiedName~AppOlObjectsFolderTreeServiceLifecycleTests" "/ResultsDirectory:coverage\trx\P3-T1"
```

EXIT_CODE: 0

The filter selects the partial class `TaskMaster.Test.AppGlobals.AppOlObjectsFolderTreeServiceLifecycleTests`,
which is where the target method is declared even though the method's source text lives in
`AppOlObjectsFolderTreeServiceTests.cs`. This narrowed `/TestCaseFilter` is a targeted diagnostic; the
CI-shaped filter `TestCategory!=LiveOutlook` is restored for P3-T2 and P4-T5.

## Counters

| Counter | Value |
|---|---|
| Total | 20 |
| Executed | 20 |
| Passed | 20 |
| Failed | **0** |
| NotExecuted (Skipped) | 0 |
| ResultSummary outcome | Completed |

## Per-method outcome of the target test

| Test | Outcome | Duration | Assembly |
|---|---|---|---|
| `TerminalNotificationHookFailure_DoesNotReplaceDispatchFault` | **Passed** | 00:00:00.0016867 | `taskmaster.test.dll` |

## Acceptance — the two sets, printed so the subset relation is checkable

**Set A — failed fully qualified names in this run:**

```
(empty)
```

**Set B — the `TaskMaster.Test.dll` members of `BASELINE_FAILURE_SET` as recorded by P0-T14:**

```
(empty)
```

Subset relation: Set A is a subset of Set B. Both are empty, so the relation holds.

| Required | Observed | Result |
|---|---|---|
| The set of failed fully qualified names in this run is a subset of the `TaskMaster.Test.dll` members of `BASELINE_FAILURE_SET` | empty subset of empty | PASS |
| `TerminalNotificationHookFailure_DoesNotReplaceDispatchFault` is recorded with outcome `Passed` | Passed | PASS |

The acceptance is stated relative to the recorded baseline rather than as an absolute zero. In this
execution the recorded baseline happens to be empty (P0-T14 recorded `BASELINE_FAILURE_SET: none`), so the
subset condition reduces to requiring an empty failed set. That reduction is a consequence of the measured
baseline, not a substituted absolute-zero demand.

## Interpretation

The repaired test passes under the barrier assertion. The added statement
`(await GetExceptionAsync(await run.Terminal)).Should().BeSameAs(fault);` completed without hanging, which
confirms the deadlock-freedom property research §4.1 predicted: the fixture completes the terminal signal at
`AppOlObjectsFolderTreeServiceLifecycleTests.cs:202`, after the increment at `:200` and before the hook
throws at `:204`, so awaiting the captured `run.Terminal` is both a valid barrier and deadlock-free even
with `throwFromTerminalHook: true`.
