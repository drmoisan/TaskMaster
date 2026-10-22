# P4-T9 — No test regressed

Timestamp: 2026-09-01T20-19
Command: enumeration of the per-test result lines in the captured output of the P4-T5 run
EXIT_CODE: 0

## The post-change failure set

    POSTCHANGE_FAILURE_SET: NONE

A count of result lines beginning `Failed `, `Skipped ` or `NotRunnable ` returns **0**. The run summary independently reports `Total tests: 6938` and `Passed: 6938`, with no `Failed:` or `Skipped:` line.

## Subset relation against the baseline

    BASELINE_FAILURE_SET   = NONE   (recorded in evidence/baseline/p0-t14-baseline-failure-set.md)
    POSTCHANGE_FAILURE_SET = NONE

The empty set is a subset of the empty set, so **the subset relation holds**. Every name in the post-change failure set also appears in the baseline set, vacuously.

The subset framing exists in the plan because a pre-existing red suite cannot be cleared by restarting the Phase 4 toolchain pass. On this tree that allowance turned out not to be needed: the baseline suite was already fully green at 6934 passed, so the gate reduces to requiring the post-change failure set to be empty, and it is. Any post-change failure would therefore have been a genuine regression attributable to this delivery run, with no pre-existing-failure defence available.

## All seven enumerated tests are reported passed

| Test | Passed | Failed |
| --- | --- | --- |
| `InitializeWebViewGuardedAsync_WhenTheWebViewSeamFaults_ReportsToTheSinkAndDoesNotFault` | 1 | 0 |
| `WebViewInitializationErrorSink_DefaultDelegate_InvokesWithoutThrowing` | 1 | 0 |
| `InitializeBool_WhenTheWebViewSeamFaults_ObservesTheFaultThroughTheSink` | 1 | 0 |
| `InitializeWebViewGuardedAsync_WhenTheTokenIsAlreadyCanceled_DoesNotInvokeTheSink` | 1 | 0 |
| `InitializeSequentialAsync_ThroughThePumpHost_CompletesAndInitializesState` | 1 | 0 |
| `InitializeGraphicsAsync_ThroughThePumpHost_CompletesAndAppliesDarkTheme` | 1 | 0 |
| `InitializeAsync_ThroughThePumpHost_RunsToTheMockedWebViewSeamAndFaults` | 1 | 0 |

The first four are the tests this change adds. The last three are the pre-existing tests AC9 pins, all passing here in the **full-suite** run — not only in the isolated P3-T11 run — which is the stronger observation, because it shows they pass alongside the whole suite and under the parallelism the runsettings apply.

`InitializeAsync_ThroughThePumpHost_RunsToTheMockedWebViewSeamAndFaults` is the substantive pin among the three. It asserts that `InitializeAsync` **throws** `WebViewSentinelException`. Had line 256 been routed through the guard, the guard's `catch (Exception ex)` arm would have contained that fault and the test would have failed. Its passing is behavioural confirmation that the fix was not applied over-broadly.

## The enumeration discriminates

Each row above was produced by an anchored match on the result keyword followed by the exact test name. A control query using the identical form against a test name that does not exist in the run returned **0**:

    CONTROL | passed=0 | ThisTestNameDoesNotExist

So a value of 1 in the table is evidence the named test genuinely ran and passed, rather than an artifact of a match that always succeeds. The complementary count — the same extraction with the `Failed` keyword — returned 0 for all seven, and the two keywords were counted separately rather than inferred from one another.

## Test count movement

    Baseline:    6934 total, 6934 passed
    Post-change: 6938 total, 6938 passed

An increase of exactly 4, matching the four tests added and no more. No test disappeared from the suite, which a count that had risen by fewer than 4, or fallen anywhere, would have indicated.

Base-ref note: this task states no `git` command. The re-anchored base used throughout this delivery run is `988d35a8f8eb7436cc46a9f6424db917ed93807a`, replacing the plan-pinned `2b85134b42872e405602e6064e02dc9cda6c319b`, which is a stale ancestor rather than the current merge base.
