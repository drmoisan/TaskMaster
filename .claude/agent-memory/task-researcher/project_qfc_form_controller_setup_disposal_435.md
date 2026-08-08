---
name: qfc-form-controller-setup-disposal-435
description: "Issue #435 (epic #136 child F6) research decisions for QfcFormController.cs + .SetupDisposal.cs: no new seams, 827-line test file split deferred, Cleanup() proven idempotent"
metadata:
  type: project
---

Issue #435 is child F6 of epic #136 (QuickFiler per-file 80% coverage), wave 1, band C3. Research on
`QuickFiler/Controllers/QfcFormController.cs` (196 lines) and
`QuickFiler/Controllers/QfcFormController.SetupDisposal.cs` (232 lines) completed 2026-08-07.
Four researchers ran concurrently across the F6 file set; the others own `EventHandlers.cs`,
`Actions.cs`, `QfcExplorerController.cs`, and the five interface files.

Decisions recorded that a later cycle should not relitigate without new evidence:

1. **Neither of these two partials needs a new production seam.** The `IQfcFormViewer` interface
   seam from issue #223 (Seams B/C/D) already covers every boundary they touch. No `IQfcFormViewer`
   growth, so no `QfcFormViewer.cs` (F15) edit and no cross-child contract note.
2. **`Cleanup()` called twice does NOT throw** — the orchestrator's double-dispose hypothesis was
   wrong. Idempotence follows from the null-conditional operators plus the field nulling, so there
   is no behavior fix to weigh against the "no behavior change" acceptance criterion.
3. **Do not split `QuickFiler.Test/Controllers/QfcFormControllerTests.cs` (827 lines) inside F6.**
   It mixes tests for all four partials, so a correct split needs a four-way concurrent edit by four
   F6 plan phases. Promote the split to its own issue; it is a natural F16 capstone item.
4. Latent defects deliberately left unfixed and queued for promotion: non-idempotent `Init()`
   (double event subscription on a second call), `_undoQueue` disposed but not nulled, and the
   host-dependent `SpaceForEmail_ShouldReturnCorrectValue` assertion.

**Why:** epic #136 mandates per-file research and per-file atomic planning, and every wave-1 child
merges into one integration branch, so any edit to a file a sibling also touches becomes a merge
conflict the child's own remediation loop must absorb.

**How to apply:** if asked to re-audit F6 or widen its scope, check these four decisions first.
Prefer new test files named after the production partial over growing either existing test file.

Related: [[qfc-form-controller-coverage-435]], [[qfc-explorer-controller-435]],
[[qfc-item-controller-227-r2-denial]], [[feedback-exemption-audit-check-proven-techniques]]
