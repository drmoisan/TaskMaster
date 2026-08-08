# itemviewer-ui-thread-marshalling-divergence (Issue #489)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/itemviewer-ui-thread-marshalling-divergence/ (Issue #489)
- Discovered during: preparation research for issue #456 (epic #136, child F14)

- Issue: #489
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/489
- Last Updated: 2026-08-08
## Summary

The `ItemViewer` type exposes three different UI-thread seams and the codebase uses all three, with
one operation marshalled onto a WPF `Dispatcher` that may never be pumped and another marshalled not
at all. Out of scope under epic #136's no-behavior-change NFR.

## Defect 1 — `ShowMoveOptionsMenu` is marshalled onto a WPF `Dispatcher`

`QuickFiler/Controllers/QfcItemController.Navigation.cs:83` marshals with
`await _uiDispatcher.InvokeAsync(() => _itemViewer.ShowMoveOptionsMenu())`, where `_uiDispatcher`
originates from `System.Windows.Threading.Dispatcher.CurrentDispatcher` captured in the `ItemViewer`
constructor (`QuickFiler/Viewers/ItemViewer.cs:13,28,71-75`). Every other forwarder in
`ItemViewer.WebViewThread.cs` is marshalled — where it is marshalled at all — with WinForms
`Control.InvokeRequired` / `Control.Invoke` (`QfcItemController.EventWiring.cs:139-146`,
`QfcItemController.Conversation.cs:224-228`).

A WPF `Dispatcher` and a `WindowsFormsSynchronizationContext` are different queues.
`Dispatcher.CurrentDispatcher` on the Outlook UI thread creates a WPF dispatcher whose message loop is
pumped only if a WPF component pumps it. In a VSTO add-in with no WPF root, work queued to that
dispatcher can be delayed until a nested WPF pump runs, or never run at all. The move-options menu
therefore has a different and weaker delivery guarantee than every other UI operation on the same
control.

## Defect 2 — `NavigateToString` is called unguarded from the theme path

`QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs:293` calls
`_itemViewer.NavigateToString(ItemHelper.ToggleDark(desiredState))` with no `InvokeRequired` guard.
The same operation at `QfcItemController.EventWiring.cs:139-146` is explicitly guarded, and the
comparable topic-thread pair at `QfcItemController.Conversation.cs:224-228` is guarded.
`ItemViewer.WebViewThread.cs:15` performs no marshalling of its own, so the unguarded site is
protected only by the assumption that theme toggling always originates on the UI thread. Theme
toggling is precisely the family that produced issues #254 and #269, so the assumption is not
obviously safe.

Failure mode: `InvalidOperationException: Cross-thread operation not valid: Control 'L0v2h2_WebView2'
accessed from a thread other than the thread it was created on` — the exact shape recorded for the
sibling control in issue #400's runtime evidence
(`docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/regression-testing/runtime-selector-toggle-thread-affinity.2026-07-22T01-29.md:25`).

## Defect 3 — `SetConversationItems` and `SortConversationByDate` must be an atomic pair, unenforced

`ItemViewer.WebViewThread.cs:23` and `:25` are independent `IItemViewer` members
(`QuickFiler/Viewers/IItemViewer.cs:109-110`). The only production caller,
`QfcItemController.Conversation.cs:231-232`, issues them back to back inside a single `Invoke`d
`SetTopicThread` call, so the pair is atomic in that path. Nothing in the interface, the
implementation, or the XML documentation records that atomicity as a requirement. Any future caller
that marshals the two separately, or that calls `SetConversationItems` without a following sort,
leaves the conversation list in source order rather than descending sent-date order, with no error.

## Defect 4 — three concurrent marshalling contracts on one control

`QuickFiler/Viewers/ItemViewer.cs` exposes `UiSyncContext` (`:59-63`), `UiScheduler` (`:65-69`), and
`UiDispatcher` (`:71-75`), all captured in the constructor (`:26-28`). Consumers have diverged onto
three strategies: `Control.Invoke` (`EventWiring.cs:141`, `Conversation.cs:226`), WPF
`Dispatcher.InvokeAsync` (`Navigation.cs:83`), and unguarded (`EventHandlers.cs:196,200`,
`FocusAndTheme.cs:293`). This makes every thread-affinity review of the area expensive and makes
Defects 1 and 2 easy to reintroduce.

## Acceptance Criteria (early draft)

- [ ] `ShowMoveOptionsMenu` uses the same UI-thread queue as the other forwarders.
- [ ] The theme-path `NavigateToString` call is guarded consistently with its siblings.
- [ ] The conversation set-then-sort pair is atomic by construction, or the contract is documented.
- [ ] `ItemViewer` consolidates onto a single documented UI-thread seam.
- [ ] Regression tests are deterministic and use no real wall-clock waits.

## Constraints & Risks

- `ItemViewer.cs` and `ItemViewer.WebViewThread.cs` are assigned to epic child F14;
  `QfcItemController.*` to F10. Consolidating the seam is a cross-child contract change; reconcile
  against both plans before scheduling.

## Next Step

- [ ] Promote to GitHub issue (bug template)
