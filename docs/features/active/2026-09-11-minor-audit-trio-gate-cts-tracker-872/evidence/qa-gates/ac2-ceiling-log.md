# AC2 — The Time-Ceiling Bound Log Assertion

Timestamp: 2026-09-13T15-46
Task: [P2-T16]

Verdict: PASS

TrxCount: 1
OperativeTrx: p2-t6-final-quickfiler.trx

## Outcome Transcribed From The TRX

The TRX read is `TestResults/vstest/p2-t6/p2-t6-final-quickfiler.trx`, produced by the P2-T6 run. The
results directory holds exactly one TRX file, so the most-recent-last-write selection rule is inert;
the count and the file name are recorded above so that a third party re-running the selection obtains
the same file.

| Test name | Matching results | Outcome |
|---|---|---|
| DequeueAsync_ZeroAcceptedAndCeilingReached_LogsCeilingBoundNotScanCapBound | 1 | Passed |

The name matched exactly one unit test result.

## Source Reading

Read from `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part4.cs`, lines 267 to
310.

**The arrangement drives the time-ceiling bound.** A take delegate returning null, a score loader
returning 950 that is never invoked, a threshold of 0.90, a fake time provider, `debugLog: logs.Add`,
a source-active delegate returning true, and `zeroAcceptanceCeiling: TimeSpan.FromSeconds(120)` with
the maximum scan without acceptance left at its default. The test starts the dequeue without awaiting,
asserts `pending.IsCompleted` is false, advances the fake clock by 121 seconds, then awaits. Advancing
the fake clock is the only thing that releases the injected empty-source delay, so the test carries no
wall-clock wait and no sleep.

**The filter is applied on the full four-word phrase.** The assertion block selects
`logs.Where(log => log.Contains("Zero-acceptance scan bound reached"))` into a list and asserts
`ContainSingle` on it. The full phrase is required rather than its first word alone, because the
checkpoint message opens with the same first word.

In this arrangement the checkpoint line is not emitted: the loop iteration that follows the clock
advance evaluates the two bounds before the checkpoint interval and returns at the bound, so the
checkpoint branch is never reached. The filter is therefore a guard that keeps the single-match
assertion correct under a future arrangement in which both lines are emitted, rather than a filter
against a line present today.

**Presence and absence are both asserted.** On the single matching line the test asserts
`Bound=zero-acceptance-ceiling` is present and asserts `Bound=scan-cap` is absent.

The absence assertion is the discriminating one. A regression that collapsed the two bounds to a single
value would emit the item-cap token and would still satisfy a presence-only assertion on the other
token, so without the `NotContain` clause the test could not detect that regression. With it, the
collapse is detectable.

## Fail-Before Position

Defect A, of which this is the second half, carries no fail-before obligation per the P1-T17 exception
dossier: the gate's production source already emits both bound values correctly and the defect was a
missing assertion rather than a behavioural fault. The gate's production source is in the Scope
Boundary and was read but never edited.
