---
name: qfc-queueprocessing-436
description: "#436/epic #136 F5: QfcDatamodel.QueueProcessing.cs needs zero new seams (no COM deref); two latent defects found (null-return at quantity<=0, rejected items discarded still-hooked); FakeTimeProvider omission fails silently"
metadata:
  type: project
---

Research completed 2026-08-08 for `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs`
(child F5 of epic `quickfiler-per-file-coverage`, issue #436). Artifact:
`docs/features/active/2026-08-07-quickfiler-datamodel-coverage-436/research/2026-08-08T00-43-qfcdatamodel-queueprocessing.md`

**Why:** the file was flagged as the cluster's highest-risk file because it carries the
concurrency/ordering invariants. It turned out to be the *least* blocked file in the cluster.

**Findings worth carrying forward (verified by reading, not inferred):**

1. **Zero new production seams needed.** The file dereferences no Outlook COM member at all —
   `MailItem` appears only as a generic type argument / list element. Its only transitive COM reach
   is the scorer method group handed to the dequeue gate, resolved by the sibling
   `IFolderScoringService` seam on `QfcDatamodel.cs`. Do not propose a gate-factory or scorer-delegate
   seam here; a gate-substituting seam would *weaken* the invariants rather than pin them.
2. **Latent defect A — null vs empty asymmetry.** `quantity <= 0` returns `null` in normal mode
   (`LockingLinkedList.TryTakeFirst(n)` returns null for `n < 1`) but an empty list in
   high-confidence mode. `QfcHomeController.Iteration.cs` dereferences `.Count` with no null guard.
   Promote-to-issue candidate; do not fix inside a coverage feature.
3. **Latent defect B — items leave the master queue still hooked.** The streaming confidence gate
   permanently discards below-threshold candidates without unhooking them from `IEmailMoveMonitor`,
   and a batch that shrinks during `TryUnhookOrReplace` returns its tail nodes unhooked. Also a
   promote-to-issue candidate.
4. **`FakeTimeProvider` omission fails silently.** `QfcDatamodel.TimeProvider` is an auto-property
   with an initializer, so `FormatterServices.GetUninitializedObject` leaves it null; the gate then
   falls back to `TimeProvider.System` instead of throwing. A test that forgets to assign it runs
   against the real 12-second deadline and looks fine. Always route construction through a shared
   `CreateModelWithFakeClock` helper.
5. **Deadline constants are testable without wall-clock waits** by advancing the `FakeTimeProvider`
   from inside the injected scorer callback and asserting the scan count.

**How to apply:** when planning or reviewing F5's QueueProcessing phase, do not accept an
`[ExcludeFromCodeCoverage]` or a new seam for this file. See also
[[qfc-datamodel-coverage-436]] for the type-scoped-attribute sequencing constraint.
