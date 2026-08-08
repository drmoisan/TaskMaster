# Research: `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs`

- Parent epic: #136 (`quickfiler-per-file-coverage`)
- Child feature: #431 F2 (`quickfiler-queue-admission-coverage`)
- File under research: `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs` (171 lines, verified by direct read)
- Evidence basis: direct read of the file on disk in this worktree; direct read of
  `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs` (base file) plus
  `.Part2.cs` and `.Part3.cs`; direct read of issue #424's `spec.md` and `feature-audit.2026-08-06T23-40.md`.

## Issue #424 disposition (read this first)

**The on-disk version of this file already reflects issue #424's changes.** Verified directly by
reading the production source: it declares `internal static readonly TimeSpan DefaultFirstBatchDeadline
= TimeSpan.FromSeconds(12);` (line 22), a `firstBatchDeadline`/`progressCallback` constructor overload
(lines 55-85), a deadline-exit check inside `DequeueAsync`'s loop (`if (deadlineEnabled &&
_timeProvider.GetElapsedTime(start) >= _firstBatchDeadline) { LogDeadlineExpiry(...); return accepted; }`,
lines 110-114), and a `_progressCallback?.Invoke(scanned, accepted.Count, quantity)` call after every
accept/reject decision (line 145). The file is exactly 171 lines, matching the epic's line-count table.
This is the full post-#424 design described in `spec.md`, not the pre-#424 unbounded-scan version
described in the original bug report. Do not re-implement or re-test the deadline/progress/liveness
design as if it were new; only close genuine gaps in the existing test suite (see below).

## Current structure

- `internal sealed class QfcStreamingDequeueConfidenceGate` — no public surface; reached only via
  reflection in tests (`CreateGate` helper) because the type and its constructors are internal, and via
  `QfcDatamodel.QueueProcessing.DequeueWithHighConfidenceGateAsync` in production (F5's file, not F2's).
- Constructor-injected: `Func<MailItem> tryTakeNext`, `Func<MailItem, CancellationToken, Task<long>>
  scoreLoader`, `double threshold`, `TimeProvider timeProvider` (defaults to `TimeProvider.System`),
  `Action<string> debugLog`, `Func<bool> sourceActive`, `TimeSpan? firstBatchDeadline`,
  `Action<int, int, int> progressCallback`. Every collaborator is an injected delegate or the
  already-standard `TimeProvider` abstraction — this file has no interface-seam gap.
- No direct construction of `Microsoft.Office.Interop.Outlook.Application/Store/MAPIFolder`. `MailItem`
  appears only as the type flowing through `tryTakeNext`/`scoreLoader`, already mockable.
- Concurrency/ordering: single `async Task<IList<MailItem>>` method with a `while` loop; uses
  `_timeProvider.GetTimestamp()`/`GetElapsedTime` for the deadline and `_timeProvider.Delay(...)` for the
  empty-queue poll — fully injectable-clock, no real wall-clock reads.
- No RNG usage.
- `log4net.ILog logger` static field — standard repo logging pattern, plus the injected `_debugLog`
  seam used in parallel (both fire on every log line).

## Existing test coverage

Three files, all `[TestClass] public partial class QfcStreamingDequeueConfidenceGateTests`:

- Base `QfcStreamingDequeueConfidenceGateTests.cs`: `DequeueAsync_UsesDequeueTimeScoreSelection_AndLogsScoreContext`, `DequeueAsync_ScansManyToYieldFew_BackfillsUntilQuantityMet`, `DequeueAsync_SourceExhaustion_ReturnsEmptyAndPartialResults`, `DequeueAsync_ThresholdComparisonIsInclusive`, `DequeueAsync_PropagatesCancellationBeforeTakingSourceItem`, `DequeueAsync_BelowThresholdItemsAreDiscarded`, `DequeueAsync_WhenSourceInitiallyEmpty_WaitsWithTimeProviderBeforeRetry`, `DequeueAsync_SourceActiveAfterRepeatedEmptyReads_ContinuesPollingUntilCandidateArrives`.
- `.Part2.cs` (issue #424 deadline suite): `DequeueAsync_LowYieldStream_StopsScanningAtDefaultFirstBatchDeadline`, `DequeueAsync_DeadlineExpiresWithZeroAccepted_ReturnsEmptyListAtTheBound`, `DequeueAsync_DeadlineExpiresDuringInFlightScore_IncludesFinalAcceptedItem`, `DequeueAsync_AfterDeadlineReturn_StopsTakingAndLeavesUnscannedCandidates`, `DequeueAsync_QuantitySatisfiedBeforeExpiry_ReturnsUnchangedBatchAndOrder`, `DequeueAsync_DisabledSentinel_ReproducesUnboundedPreChangeBehavior`, `Constructor_NonPositiveNonSentinelDeadline_IsRejectedByGuardClause`, `DequeueAsync_DeadlineExpiry_EmitsOneExpiryLineAndKeepsPerCandidateLogging`, `DequeueAsync_CancelledDuringEmptyQueueWait_ThrowsOperationCanceled`, `DequeueAsync_CancelledDuringScoring_ThrowsOperationCanceled`.
- `.Part3.cs` (issue #424 progress-callback suite): `DequeueAsync_ProgressCallback_FiresOncePerScannedCandidateMonotonically`, `DequeueAsync_ProgressCallback_StopsReportingOnceTheMethodReturns`, `DequeueAsync_ThrowingProgressCallback_PropagatesAndLeavesSourceUsable`.

This is an unusually thorough suite (21 tests) already covering: score-selection cadence and logging;
scan/backfill until quantity met; source exhaustion partial return; inclusive threshold boundary;
pre-take cancellation; below-threshold discard; empty-source wait via `TimeProvider`; continued polling
while the (now honest, per #424) `sourceActive` signal remains true; the full deadline lifecycle (default
budget, zero-accepted-at-deadline, in-flight-score-completes-before-return, post-deadline no-further-takes,
fast-path-unaffected-by-deadline, disabled sentinel reproducing pre-#424 behavior, constructor guard for
a non-positive non-sentinel deadline, the deadline-expiry debug log line); cancellation during the
empty-queue wait and during scoring under both deadline configurations; and the full progress-callback
contract (once-per-candidate monotonicity, no invocation after return, propagation of a throwing
callback without corrupting the source).

## Coverage gap

Two genuine gaps remain, both simple guard/early-return paths that the #424 work did not need to touch
and that the pre-existing suite never happened to cover:

- **`quantity <= 0` early return (`DequeueAsync`, lines 96-99: `if (quantity <= 0) { return accepted; }`).**
  No test calls `DequeueAsync` with `quantity == 0` or a negative quantity. This is a real branch: it
  returns an empty list without ever calling `_tryTakeNext` or `_scoreLoader`.
- **Constructor null-guards for `tryTakeNext` and `scoreLoader`** (`ArgumentNullException` at
  `QfcStreamingDequeueConfidenceGate.cs:66-67`). No test constructs the gate with either delegate null;
  every existing test supplies both. `threshold`'s conversion to `_cutoff` has no guard (any `double` is
  accepted, including negative or `NaN`) — not a gap, since there is no branch to exercise, but worth
  noting for the atomic-planner as intentional unvalidated input.

No other branch, including every #424-introduced path, is missing coverage based on the direct read of
all three test files above; the #424 feature audit's independently-reverified coverage figures
(96.63% line / 92.11% branch, coverage-final.cobertura.xml) are consistent with these two remaining gaps.

## Seam requirements

None. All collaborators are already injected delegates or `TimeProvider`; the two gaps above are
pure test-writing against the existing constructor and public (internal) `DequeueAsync` surface.

## Candidate test cases

| # | Case | Type | Notes |
|---|---|---|---|
| 1 | `DequeueAsync(0, timeOut, token)` returns an empty list without calling `tryTakeNext` or `scoreLoader` | Boundary | Use a `tryTakeNext`/`scoreLoader` that throws `AssertFailedException` if invoked, matching the existing style in this suite |
| 2 | `DequeueAsync(-1, timeOut, token)` returns an empty list without calling `tryTakeNext` or `scoreLoader` | Negative/boundary | Same technique as case 1; confirms the guard is `<= 0`, not `== 0` |
| 3 | Constructor (6-arg or 8-arg overload) with `tryTakeNext == null` throws `ArgumentNullException` naming `tryTakeNext` | Negative | |
| 4 | Constructor (6-arg or 8-arg overload) with `scoreLoader == null` throws `ArgumentNullException` naming `scoreLoader` | Negative | |

## Determinism constraints

Already fully satisfied by the existing suite's conventions and must be preserved by any new tests: use
`FakeTimeProvider` (`Microsoft.Extensions.Time.Testing`) for all timing, never advance real wall-clock
time, never use `Thread.Sleep`/`Task.Delay` directly in test code, and use the existing reflection-based
`CreateGate` helper (base file, lines 26-110) so new tests keep compiling against whichever constructor
shape the gate exposes. No RNG is used in this file.
