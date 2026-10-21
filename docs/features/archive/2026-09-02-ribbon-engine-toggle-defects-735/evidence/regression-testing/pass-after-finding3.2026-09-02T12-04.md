# Finding 3 — Pass-After Run (P3-T11)

Timestamp: 2026-09-03T02-43
Task: [P3-T11]
Command: the pinned scoped form with the same six-clause filter P3-T5 used,
`"/Logger:trx;LogFileName=p3-t11.trx"` and
`/ResultsDirectory:docs\features\active\2026-09-02-ribbon-engine-toggle-defects-735\evidence\regression-testing\p3-t11`.
EXIT_CODE: 0

## Results directory contents

Exactly one TRX file and no other entry:

```
p3-t11.trx
```

## Counts read from the TRX `ResultSummary/Counters` element

| Counter | Value | Required |
|---|---|---|
| total | 6 | 6 |
| executed | 6 | — |
| passed | 6 | 6 |
| failed | 0 | 0 |

## Per-test outcomes read from the TRX

| Test | P3-T5 (pre-fix) | This run (post-fix) |
|---|---|---|
| `ApplyPrimeAsync_WhenPrimeResolvesAfterToggle_DoesNotOverwriteToggleResult` | Failed | **Passed** |
| `ExecuteToggleAsync_WhenOlderObservationCompletesLast_DoesNotOverwriteNewerResult` | Failed | **Passed** |
| `GetPressed_WhenPrimeIsCanceled_LogsErrorAndClearsPrimeMarker` | Failed | **Passed** |
| `ExecuteToggleAsync_WithNoCompetingWriter_CachesValueAndInvalidatesExactlyOnce` | Passed | Passed |
| `ExecuteToggleAsync_WithNullEngines_ThrowsInvalidOperationExceptionWithoutTogglingEngine` | Passed | Passed |
| `GetPressed_WhenPrimeIsCanceled_LeavesToggleReportingUnchecked` | Passed | Passed |

All three defect reproductions flipped from Failed to Passed, and the three that already held
pre-fix still hold. The uncontended case is the one that matters for the second direction: it proves
the new conditional invalidation did not over-suppress, because an uncontended write still applies
and still invalidates exactly once.

## What each newly passing test now demonstrates

- **Prime after toggle.** A prime whose activation read began before a toggle's now loses the
  compare-and-apply, so the cached read is `true` (the toggle's value), exactly one invalidation was
  issued, and no error was logged. This is the #525 reproduction.
- **Toggle versus toggle.** The later-started toggle holds the higher ticket and wins regardless of
  completion order, and the rejected write issues no second invalidation.
- **Canceled prime.** A canceled prime is now treated as a failure: exactly one error is logged, its
  message names the engine key, the logged exception is an `OperationCanceledException` synthesized
  because a canceled task carries none to unwrap, and the in-flight marker is cleared so a later
  read starts a genuinely new prime — proven by the second prime handle not being the same instance
  as the first.

Output Summary: All six race tests pass after the fix. EXIT_CODE 0, TRX counters total 6, passed 6,
failed 0. The three that failed in P3-T5 now pass and the three that already passed still pass.
