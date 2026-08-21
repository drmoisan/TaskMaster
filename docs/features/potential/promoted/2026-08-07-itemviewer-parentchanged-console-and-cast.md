# itemviewer-parentchanged-console-and-cast (Issue #487)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/itemviewer-parentchanged-console-and-cast/ (Issue #487)
- Discovered during: preparation research for issue #456 (epic #136, child F14)

- Issue: #487
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/487
- Last Updated: 2026-08-08
## Summary

Two small policy violations in the `ItemViewer` / `ItemViewerExpanded` twins, both out of scope to fix
under epic #136's no-behavior-change NFR because the least-bad disposition for each is a deletion
rather than a rewrite.

## Defect 1 — production `Console.WriteLine` in a designer-wired WinForms event handler

`QuickFiler/Viewers/ItemViewer.cs:168` is `Console.WriteLine("Parent Changed");` — the entire body of
`L0v2h2_WebView2_ParentChanged`, wired at `ItemViewer.Designer.cs:256`.
`QuickFiler/Viewers/ItemViewerExpanded.cs:160` is identical, wired at
`ItemViewerExpanded.Designer.cs:274`.

This violates the General Code Change Policy § 3, which requires the project's logging pattern rather
than ad-hoc console output. Because the handler is otherwise a no-op, an alternative disposition is
deleting both the handler and its designer wiring — a behavior change, hence promotion rather than an
in-scope fix.

## Defect 2 — unguarded downcast in an event handler

`QuickFiler/Viewers/ItemViewer.cs:173` and `QuickFiler/Viewers/ItemViewerExpanded.cs:165` are
`var menuItem = (ToolStripMenuItem)sender;` with no `is` / `as` guard. A non-`ToolStripMenuItem`
sender raises `InvalidCastException` on the UI thread with no diagnostic context.

Severity is low today — all four current wirings pass a `ToolStripMenuItemCb`, and in `ItemViewer` the
member is dead code — but it is a fail-fast-without-context path that becomes live the moment anyone
wires it.

## Acceptance Criteria (early draft)

- [ ] No production `Console.WriteLine` remains in either handler.
- [ ] The downcast is guarded, or the handler is removed together with its designer wiring.
- [ ] Designer wiring and handler bodies stay consistent between the two twins.

## Constraints & Risks

- Editing `*.Designer.cs` files means touching generated code; confirm the designer round-trips.
- `ItemViewer*.cs` is assigned to epic child F14 and `ToolStripMenuItemCb.cs` to F15; reconcile
  against those children before scheduling.

## Next Step

- [ ] Promote to GitHub issue (bug template)
