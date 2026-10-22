# AC1 — The Scan-Cap Bound Log Assertion

Timestamp: 2026-09-13T15-46
Task: [P2-T15]

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
| DequeueAsync_ZeroAcceptedAndCapReached_LogsScanCapBoundAndStopDecision | 1 | Passed |

The name matched exactly one unit test result.

## Source Reading

Read from `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part4.cs`, lines 178 to
219.

**The debugLog delegate is injected.** The arrangement declares `var logs = new List<string>();` and
passes `debugLog: logs.Add` to the gate factory, alongside a counting take delegate over a ten-item
candidate queue, a score loader returning 100, a threshold of 0.90, a fake time provider that is never
advanced, a source-active delegate returning true, and `maxScanWithoutAcceptance: 4`.

**The filter is applied before any field assertion.** The assertion block selects
`logs.Where(log => log.Contains("Zero-acceptance scan bound reached"))` into a list and asserts
`ContainSingle` on it before reaching any field. The full four-word opening phrase is used rather than
its first word alone, because the sibling checkpoint message opens with the same first word and a
one-word filter would not discriminate between them.

Filtering before asserting is load-bearing rather than cosmetic. The launch line that this arrangement
does emit carries `Cutoff=900` and a scan-cap bound field spelled `ScanCap`, so an unfiltered field
assertion could have been satisfied by the launch line rather than by the scan-bound line under test.

**The five field tokens are asserted.** On the single matching line the test asserts `Accepted=0`,
`Scanned=4`, `Cutoff=900`, `Bound=scan-cap` and `Decision=stop`. All five are asserted against the
captured message string and none against the repository tree.

**No total-count assertion.** The test asserts no exact total of captured log lines, because that total
is not established. In this arrangement the fake clock is never advanced, so the checkpoint interval
never elapses and no checkpoint line is emitted; the filter is a guard that keeps the single-match
assertion correct under a future arrangement in which both lines are emitted.

## Fail-Before Position

Defect A carries no fail-before obligation, per the P1-T17 exception dossier. The gate's production
source already emits every field asserted here, so the defect was a missing assertion rather than a
behavioural fault, and this test was expected to pass immediately. The production source of the
high-confidence dequeue gate is in the Scope Boundary and was read but never edited.
