---
name: feedback-exemption-audit-check-proven-techniques
description: When re-auditing a coverage-exemption boundary, cross-check each residual against test techniques already proven elsewhere in the same repo before accepting "requires a live host" claims.
metadata:
  type: feedback
---

When asked to independently re-verify a delivered `[ExcludeFromCodeCoverage]` residual boundary
(e.g., issue #227 cycle-2's 41-member boundary vs. the original ~6-8 pre-seam estimate,
2026-07-02), do not accept a residual's stated justification ("requires a live WPF pump", "requires
a live COM MailItem", "requires a live control tree") at face value. Concretely check:

1. **Grep the test suite for a technique that already defeats the claimed barrier.** In #227,
   `WpfUiDispatcher`'s exemption claimed "requires a live WPF message pump", but
   `QfcItemController.TestSupport.cs` already contained a proven `StartRunningDispatcher()` helper
   (a real `Dispatcher.Run()` on a background STA thread, no external process) used elsewhere in the
   same test project — the adapter's own forwarding body was never wired to it. Similarly,
   `MailItemActionsAdapter` claimed a COM barrier but `MailItem` is an interop **interface** and was
   already fully `Mock<MailItem>`-tested in the same file, making the exemption attribute simply
   redundant.
2. **Check sibling methods with identical shape for inconsistent exemption.** `BtnFlagTask_Click` was
   exempted while its structurally identical sibling `BtnDelItem_Click` (same
   `SynchronizationContext` guard + single already-tested delegate call) was not — direct in-file
   proof the exemption was a leftover inconsistency, not a genuine barrier. Same pattern found for
   `RegisterExpandedActions` vs. its already-non-exempt siblings `RegisterFocusActions` /
   `RegisterExpandedAsyncActions`.
3. **Check whether a proven seam pattern from the SAME cycle was applied inconsistently to
   structurally identical collaborators.** Cycle-2 built a factory-delegate seam for
   `EmailFiler`/`FlagTasks`/`ConversationResolver` but left `FolderPredictor` (same shape: no
   existing interface, construct-then-call) exempted as "out of scope" — an inconsistent scoping
   decision, not a technical difference.

**Why:** a pre-seam research estimate (like the original ~6-8 in
`artifacts/research/2026-07-01T00-00-qfc-item-controller-seam-redesign-research.md`) cannot foresee
seam-infrastructure's own residual shape (adapter bodies, thin async-void shells) OR catch
inconsistent application of the seams once they exist. Both directions of error are real: some
residuals grow because seaming itself creates new, legitimately-irreducible shells (e.g. `async
void` WinForms-event-handler signatures — a framework contract, not a barrier); others persist only
because a proven technique/pattern wasn't actually applied everywhere it could be. Rigor requires
separately identifying each, not defaulting to "the delivered count is close enough" or "the
original estimate must have been right."

**How to apply:** for any future coverage-exemption re-audit in this repo, read every residual's
actual source body (not just its justification comment), grep the test project for
already-existing live-host/mocking techniques for the same external type, and check every sibling
method with a similar shape before accepting IRREDUCIBLE. See
[[qfc-item-controller-227-r2-denial]] for the precedent that started this scrutiny.
