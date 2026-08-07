# [P2-T1] Fail-Before Exception Dossier — Gate Progress Callback

- **Issue:** #424
- **Task:** [P2-T1]
- **Change covered:** the incremental progress callback added to `QfcStreamingDequeueConfidenceGate` by `[P2-T2]`
- **Acceptance criterion served:** AC 5

Timestamp: 2026-08-06T23-06

WhyFailingRunImpossible: The progress-callback seam does not exist on the pre-change constructor, so a test that captures progress reports cannot be written against the current surface — there is no parameter to pass a sink to and no member to observe. A test referencing the seam would fail to **compile**, not fail an assertion, and a non-compiling test cannot produce an auditable failing test run. Unlike the Phase 1 deadline change, there is no pre-existing observable whose behavior can be asserted to fail: the pre-change gate emits no progress signal of any kind.

## Absence-of-seam proof

State of `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs` immediately before `[P2-T2]` (157 lines).

Command: `grep -n "internal QfcStreamingDequeueConfidenceGate\|Action<string> debugLog\|Func<bool> sourceActive\|TimeSpan? firstBatchDeadline\|Action<int" QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs`
EXIT_CODE: 0

Output Summary:

```
32:        internal QfcStreamingDequeueConfidenceGate(
37:            Action<string> debugLog = null
47:        internal QfcStreamingDequeueConfidenceGate(
52:            Action<string> debugLog,
53:            Func<bool> sourceActive,
54:            TimeSpan? firstBatchDeadline = null
```

Command: `grep -c "progressCallback\|Action<int, int, int>" QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs`
EXIT_CODE: 1
Output Summary: `0` — **zero** occurrences. No progress-callback parameter, field, or invocation exists anywhere in the gate.

### Complete pre-change constructor parameter list

The widest constructor (`QfcStreamingDequeueConfidenceGate.cs:47-55`) takes exactly seven parameters, none of which is a progress sink:

| # | Parameter | Type |
|---|---|---|
| 1 | `tryTakeNext` | `Func<MailItem>` |
| 2 | `scoreLoader` | `Func<MailItem, CancellationToken, Task<long>>` |
| 3 | `threshold` | `double` |
| 4 | `timeProvider` | `TimeProvider` |
| 5 | `debugLog` | `Action<string>` |
| 6 | `sourceActive` | `Func<bool>` |
| 7 | `firstBatchDeadline` | `TimeSpan?` (added by `[P1-T3]`) |

The convenience overload at `:32-38` takes five of these. Neither accepts an `Action<int, int, int>`. The only per-candidate observable the pre-change gate exposes is the `_debugLog` string seam, which reports scores rather than `(scanned, accepted, quantity)` progress and is already pinned by the existing `DequeueAsync_UsesDequeueTimeScoreSelection_AndLogsScoreContext` test.

### Search performed for an existing failing run

SearchScope: `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/regression-testing/`
SearchPatterns: `*progress*`, `fail-before-exception.*.md`
SearchResult: no progress-callback failing-run artifact exists (none is possible, per the proof above); this dossier is the substitute.

## Authoritative fail-before evidence for this bug

The Phase 1 deadline regression test is the authoritative fail-before/pass-after evidence for issue #424 and for AC 11:

- `evidence/regression-testing/deadline-fail-before.2026-08-06T22-41.md` — EXIT_CODE 1, 51 `tryTakeNext` invocations against the `<= 13` bound.
- `evidence/regression-testing/deadline-pass-after.2026-08-06T22-48.md` — EXIT_CODE 0.

The progress callback is a **user-visible-feedback** improvement layered on that fix: it does not change what the gate returns, only that the ProgressViewer advances while scanning. Its correctness is proven by the pass-after tests added in `[P2-T3]` and `[P2-T4]` (cadence, monotonicity, no invocation after return, and exception propagation), all recorded in `evidence/regression-testing/gate-progress-suite.<ts>.md`.
