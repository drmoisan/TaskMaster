# Remediation Inputs — itemviewer-surface-defects (Issue #489)

- Timestamp: 2026-08-28T03-13 (UTC)
- Source review artifacts: `policy-audit.2026-08-28T03-13.md` (§ 8), `code-review.2026-08-28T03-13.md` (RC-1), `feature-audit.2026-08-28T03-13.md`
- Branch under review: `bug/itemviewer-surface-defects-489` at `74d02ad2`

## Remediation-required finding

### RC-1 (Blocking) — add the 17th intent detachment for `PicturesChanged`

**Invariant to restore (state the invariant, not the symptom):** every event `WireIntentEvents()` subscribes must be detached by `UnwireIntentEvents()`; after `Cleanup()`, a controller holds zero live subscriptions on its viewer.

**Current state (measured on HEAD):** `WireIntentEvents()` performs 17 subscriptions (`QfcItemController.EventWiring.cs:94` adds `_itemViewer.PicturesChanged += this.CbxPictures_CheckedChanged;`); `UnwireIntentEvents()` (`:446` ff.) performs 16 detachments with no `PicturesChanged` line. The AC11 handoff record (`evidence/other/wireintentevents-16-to-17-handoff.2026-08-28T01-55.md`) hands the detachment to upstream 484 while itself recording `Upstream484Landed: true`; 484 is merged, so the obligation has no live owner.

## Directed changes

1. **Production (one line):** in `QuickFiler/Controllers/QfcItemController.EventWiring.cs`, `UnwireIntentEvents()`, append `_itemViewer.PicturesChanged -= this.CbxPictures_CheckedChanged;` after the existing `AttachmentsChanged` detachment, mirroring the 16 lines already there. Safe on every teardown path: the member's `_itemViewer is null` guard runs first, and `-=` on a never-attached handler is a no-op. File is at 483 lines (17 spare).
2. **Regression test (one test, RED-first):** add `UnwireIntentEvents_DetachesPicturesChanged` to `QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.Part2.cs` (81 lines; 419 spare; this feature's own file), mirroring the existing `UnwireIntentEvents_DetachesAllSixteenIntentSubscriptions` fixture shape: wire, unwire, then `viewer.VerifyRemove(v => v.PicturesChanged -= It.IsAny<EventHandler>(), Times.Once())`. It fails before change 1 and passes after — a true RED per the Bugfix Workflow.
3. **Spec amendment (dated, in place, per the existing 2026-08-27 precedent at `spec.md:751`):** extend § Sibling-collision resolution disposition 1 and the scope-discipline criterion that confines the `EventWiring.cs` diff to `WireIntentEvents`, so the diff is confined to `WireIntentEvents` **and the single matching `UnwireIntentEvents` detachment**; quote the original wording and the reason (484 already landed, so the planned handoff has no recipient — recorded in the handoff artifact itself). Do not weaken any other criterion; the criterion count stays 62.
4. **Plan amendment:** append the remediation tasks to `plan.2026-08-25T01-04.md` under a dated remediation phase (or route through atomic-planner per the remediation-handoff skill), including the gate refreshes in item 5.
5. **Gate refresh (evidence, not assumptions):** rerun and re-record under `FEATURE/evidence/qa-gates/` — csharpier check; analyzer rebuild (warnings must stay <= 5, zero CoreCompile skips); nullable rebuild; `QuickFiler.Test` vstest (expect 1122/0/0); repo-wide run + coverage (line rate must not drop below the 0.851567 now recorded; the new test executes existing production lines, so no drop is expected); refresh `p11-t11` line counts for the two grown files (`EventWiring.cs` 483->484, `EventWiringTests.Part2.cs` 81->~96).
6. **Handoff record addendum:** append a dated section to `evidence/other/wireintentevents-16-to-17-handoff.2026-08-28T01-55.md` recording that the obligation was discharged in-branch after review, so the artifact does not read as a live obligation on 484 at fan-in.

## Constraints verified for the remediation executor

- The 484-owned test `UnwireIntentEvents_DetachesAllSixteenIntentSubscriptions` (`EventWiringTests.cs:377-419`) asserts sixteen individual `VerifyRemove ... Times.Once()` calls and pins no total; it stays green unmodified. Do not rename or edit it.
- Do not edit `EventWiringTests.cs` (499/500 lines) beyond nothing at all — the new test belongs in `Part2.cs`.
- 484's spec text documenting "16 detachments" is a merged sibling's historical record; do not edit sibling feature folders. The addendum in item 6 is the reconciliation record.
- All other review findings (CR-1..CR-5, PA-1, PA-2, NB-4) are non-blocking and require no code change in this cycle; the out-of-scope-findings promotions (O1–O8, E1–E4, reframed O3, #489 D4 residual, #490 D5, #490 D1 second half) remain an orchestrator obligation after merge per the feature-promotion lifecycle.
