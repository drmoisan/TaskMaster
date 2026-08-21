# itemviewer-move-option-menu-defects (Issue #486)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/itemviewer-move-option-menu-defects/ (Issue #486)
- Discovered during: preparation research for issue #456 (epic #136, child F14)

- Issue: #486
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/486
- Last Updated: 2026-08-08
## Summary

The QuickFiler item-viewer move-option menu is defective in three independent ways. All three were
found while researching coverage for the `ItemViewer` family and are out of scope to fix under the
epic's no-behavior-change NFR.

## Defect 1 — the menu check image is cleared immediately after being set

`QuickFiler/Viewers/ToolStripMenuItemCb.cs:32-58` shadows `Checked` and `CheckedChanged` with `new`
and never assigns `base.Checked`. The consuming handlers at
`QuickFiler/Viewers/ItemViewerExpanded.cs:169-179` and `QuickFiler/Viewers/ItemViewer.cs:177-187`
receive the parameter as the base `ToolStripMenuItem`, so the write lands on the base property and
the shadowed value is never reflected. The user-visible result is that the four move-option menu
items never display a check mark.

Candidate fix: assign `base.Checked = value;` at `ToolStripMenuItemCb.cs:37`.

## Defect 2 — `ItemViewer` and `ItemViewerExpanded` have silently divergent menu behavior

`ItemViewer.cs:171-175`, `:177-187`, and `:205` have no caller and no designer wiring anywhere in the
solution. `ItemViewer.Designer.cs` wires exactly one handler, at `:256`, and it is not one of these.
The same three members in `ItemViewerExpanded.cs:163-179` **are** wired four times
(`ItemViewerExpanded.Designer.cs:171,180,189,198`) and called four times from its constructor
(`ItemViewerExpanded.cs:24-27`).

Combined with Defect 1, the wired path is the defective one: it clears the check image the setter
just applied. The two twins therefore behave differently and the divergence appears accidental
rather than designed.

Candidate disposition: delete the three dead members from `ItemViewer.cs` (behavior-neutral) and fix
or document the `ItemViewerExpanded` path.

## Defect 3 — `PicturesChanged` has no production subscriber

Toggling "Save Pictures" in QuickFiler is silently discarded. `QfcItemController.EventWiring.cs:66-94`
wires the other three move-option events but not `PicturesChanged`. `EfcFormController.cs:389` does
wire it, so the omission is specific to the QuickFiler path rather than a design decision.

## Impact

Defects 1 and 3 are user-visible: the move-option menu gives no check-mark feedback, and one of the
four options has no effect at all in QuickFiler. Defect 2 is a latent correctness and maintenance
hazard rather than a live failure.

## Acceptance Criteria (early draft)

- [ ] `ToolStripMenuItemCb.Checked` round-trips through the base property and the check image renders.
- [ ] The `ItemViewer` / `ItemViewerExpanded` menu-handler divergence is resolved or documented.
- [ ] `PicturesChanged` is wired in the QuickFiler path, or its absence is documented as intended.
- [ ] Regression tests cover each fixed behavior.

## Constraints & Risks

- `ToolStripMenuItemCb.cs` is assigned to epic child F15; `ItemViewer*.cs` to F14; the wiring file
  `QfcItemController.EventWiring.cs` to F10. Scheduling this fix against an in-flight child will
  produce a semantic conflict, so reconcile against those children's plans first.

## Next Step

- [ ] Promote to GitHub issue (bug template)
