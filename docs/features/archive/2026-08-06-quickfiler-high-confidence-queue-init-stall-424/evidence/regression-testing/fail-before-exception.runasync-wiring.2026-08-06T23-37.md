# [P4-T1] Fail-Before Exception Dossier — Overload Plumbing, Band Mapping, and O1 Poll

- **Issue:** #424
- **Task:** [P4-T1]
- **Changes covered:** the new `IQfcDatamodel.DequeueNextItemGroupAsync` overload ([P4-T4]), the `QfcScanProgressBandMapper` module and `RunAsync` band mapping ([P4-T2], [P4-T5]), and the O1 poll reduction 1000 -> 200 ms at the pre-UI call site ([P4-T5])
- **Acceptance criteria served:** AC 6 (and the mapping half of AC 2)

Timestamp: 2026-08-06T23-37

WhyFailingRunImpossible: A compiling pre-change test cannot capture a progress sink, because the `IQfcDatamodel` overload that carries one does not exist. There is no parameter to pass a sink to and no member to observe, so a test referencing the seam fails to **compile** rather than failing an assertion, and a non-compiling test produces no auditable failing test run. The same holds for the band mapper: `QfcScanProgressBandMapper` is a new type, so a test naming it cannot compile until `[P4-T2]` creates it.

## Absence-of-seam proof

State of `QuickFiler/Interfaces/IQfcDatamodel.cs` immediately before `[P4-T4]` (40 lines). The interface declares exactly one asynchronous dequeue member:

```csharp
public interface IQfcDatamodel                                    // IQfcDatamodel.cs:24
{
    Task<IList<MailItem>> DequeueNextItemGroupAsync(int quantity, int timeOut);   // :26
    IList<MailItem> DequeueNextItemGroup(int quantity);                           // :27
    void UndoMove();                                                              // :28
    SloStack<IMovedMailInfo> MovedItems { get; }                                  // :29
    IList<MailItem> InitEmailQueue(int batchSize, BackgroundWorker worker);       // :30
    Task<IList<MailItem>> InitEmailQueueAsync(...);                               // :31-36
    bool Complete { get; set; }                                                   // :37
    void Cleanup();                                                               // :38
}
```

`DequeueNextItemGroupAsync` at `:26` takes `(int quantity, int timeOut)` only — **no deadline parameter and no progress-sink parameter**. A Moq setup such as `mock.Setup(m => m.DequeueNextItemGroupAsync(itemsPerIteration, 200, deadline, sink))` does not compile against this interface (CS1501, wrong argument count).

Correspondingly, `QuickFiler/Controllers/QfcScanProgressBandMapper.cs` does not exist at this point, so no test can reference the mapper type.

### Search performed for an existing failing run

SearchScope: `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/regression-testing/`
SearchPatterns: `*wiring*`, `*runasync*`, `fail-before-exception.*.md`
SearchResult: no wiring failing-run artifact exists (none is possible, per the proof above); this dossier is the substitute.

## Why no behavioral fail-before exists for this change either

Unlike the Phase 1 deadline and the Phase 3 liveness flag — both of which had an observable pre-change behavior that could be asserted to fail — this phase adds **new reporting surface** rather than correcting existing behavior:

- Before the change, `RunAsync` emits **zero** progress reports between `progress.Report(0, "Initializing Email Queue")` (`QfcHomeController.cs:277`) and `progress.Report(30, "Initializing Qfc Items")` (`:297`). That silence is the symptom, but "no report was emitted" is the *absence* of a signal; a test asserting reports land in [0, 30] has nothing to bind to and no seam through which to observe.
- The O1 poll change (1000 -> 200 ms) is a latency constant at one call site (`QfcHomeController.cs:294`), not a behavioral contract. Its effect is covered by the exact-argument verification updated in `[P4-T6]`.

## Authoritative fail-before evidence for this bug

The Phase 1 deadline regression test is the authoritative fail-before/pass-after evidence for issue #424 and AC 11:

- `evidence/regression-testing/deadline-fail-before.2026-08-06T22-41.md` — EXIT_CODE 1, 51 `tryTakeNext` invocations against the `<= 13` bound.
- `evidence/regression-testing/deadline-pass-after.2026-08-06T22-48.md` — EXIT_CODE 0.

A second genuine fail-before/pass-after pair exists for the latent producer-liveness defect this fix also repairs:

- `evidence/regression-testing/liveness-fail-before.2026-08-06T23-20.md` — EXIT_CODE 1.
- `evidence/regression-testing/liveness-pass-after.2026-08-06T23-26.md` — EXIT_CODE 0.

The wiring in this phase is what makes the bounded scan visible to the user. Its correctness is proven by the pass-after tests added in `[P4-T3]`, `[P4-T6]`, and `[P4-T7]` — mapper unit coverage, the updated exact-argument overload verification, and the RunAsync band-mapping and empty-batch tests — all recorded in `evidence/regression-testing/wiring-suite.<ts>.md`.
