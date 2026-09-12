---
name: efc-item-controller-452
description: "#452/epic #136 F9: EfcItemController seam findings — IItemViewer covers ~70% of the viewer surface via verified 1:1 forwards; WpfUiDispatcher(Dispatcher) ctor is internal to UtilitiesCS; F8's dependency graph stops at EfcFormController"
metadata:
  type: project
---

Research for issue #452 (epic #136 child F9, `QuickFiler/Controllers/EfcItemController.cs`,
1,170 lines, `[ExcludeFromCodeCoverage]` at line 25) established four things that were not obvious
from the file itself.

**Why:** F9 is the heaviest seam-extraction child in the epic and a planner working from the file
alone would either over-build seams that already exist or under-build the one that does not.

**How to apply:** when planning any EFC/QFC viewer-bound controller work, check these first.

1. **`QuickFiler/Viewers/IItemViewer.cs` already abstracts most of what a viewer-bound controller
   needs**, and `ItemViewer.DisplayState.cs` / `.Commands.cs` / `.WebViewThread.cs` are verifiable
   1:1 forwards (`SenderText` -> `LblSender.Text`, `AttachmentsChecked` ->
   `SaveAttachmentsMenuItem.Checked`, `SortConversationByDate` -> `TopicThread.Sort(SentDate, ...)`,
   etc.). Retyping a `ItemViewer` field to `IItemViewer` is a behaviour-preserving substitution, not
   a new abstraction. `QfcItemController.cs:51` is the precedent. The residual that `IItemViewer`
   does NOT cover is the raw-TLP/column-style/WebView2-control cluster.

2. **`UtilitiesCS.Threading.WpfUiDispatcher(Dispatcher)` is `internal` to `UtilitiesCS`** — only the
   public parameterless ctor (which binds the static `UiThread.Dispatcher`) is reachable from
   QuickFiler. A controller that needs to route through `IItemViewer.UiDispatcher` (a sealed WPF
   `Dispatcher`) must author its own local `IUiDispatcher` adapter; substituting the static
   `UiThread.Dispatcher` for the viewer's dispatcher is a behaviour change, not a refactor.

3. **F8's `EfcHomeControllerDependencies` factory graph stops at `EfcFormController`** — it never
   mentions `EfcItemController`, which `EfcFormController` constructs itself. F9's own dependency
   bundle is therefore additive and non-overlapping; the "do not edit F8's files" constraint costs
   nothing.

4. **`EfcItemController.cs` and `EfcFormController.cs` are absent from the committed #424 Cobertura**
   while `EfcHomeController.cs` is present (positive control) — proving zero coverage rather than
   merely unmeasured. Use that absence/presence pairing as the standard evidence pattern for any
   `[ExcludeFromCodeCoverage]` file in this epic.

Latent defects found and queued for promotion (12, IDs D-1..D-12 in the research artifact); the two
most transferable are the `"–incognito "` EN-DASH browser argument (present in BOTH
`EfcItemController.cs:184/:217` and `QfcItemController.ViewerSetup.cs:52`) and the
`KbdActions<>` indexer setter being a silent no-op for unregistered keys
(`KbdActions.cs:38-47`), which makes `RegisterActions` register nothing.

See also [[quickfiler-percoverage-epic-136]], [[efc-home-controller-deps-437]],
[[qfc-helper-classes-f4-434]].
