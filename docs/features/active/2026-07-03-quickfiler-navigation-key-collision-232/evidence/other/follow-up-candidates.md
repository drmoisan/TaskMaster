# Follow-up Candidates (Issue #232, out of scope)

Timestamp: 2026-07-03T13-48

The following items were identified during Issue #232 work and are explicitly NOT part of this change,
matching `spec.md` "Rollout & Follow-up" and "Scope & Non-Goals". They are recorded here as candidate
follow-up issues.

1. **Fixed-batch-without-backfill pattern** — `QfcDatamodel.InitEmailQueue`/`InitEmailQueueAsync` (first
   screen) and `QfcDatamodel.DequeueNextItemGroupAsync`/`WaitForQueue` (subsequent screens). This is the
   most likely explanation for the separately-reported "only a subset of items appears in high-confidence
   mode" symptom that motivated the Part B diagnostic logging. Resolving it is a batch-sizing/backfill
   design decision, not a threshold bug. Spun out as candidate Issue #233. Rationale: larger design change
   outside the reported navigation-key defect; the additive logging added in this change is intended to make
   it empirically diagnosable.

2. **Dormant Issue #171 pre-filter pipeline** — `QfcHighConfidencePreFilterLoader` / the
   `QfcHighConfidencePreFilter` pipeline is not wired into the live QuickFiler startup path. Rationale:
   wiring it up is a separate feature-enablement decision; a `logger` field was added to
   `QfcHighConfidencePreFilter.cs` in this change for diagnostics only and must not be mistaken for enabling
   #171.

3. **`removespecificcontrolgroupcounter` reentrancy-counter hygiene** — the counter in
   `QfcCollectionController.RemoveSpecificControlGroupAsync` can leak upward on any exception thrown mid-method
   and is unsynchronized outside the `Interlocked` calls themselves. Rationale: a broader reentrancy-hygiene
   fix beyond what was strictly necessary to remove the reported `ArgumentException`; the Part A guard added
   in this change is scoped only to the double-registration defect and does not alter the counter logic.
