# itemviewer-display-and-folder-contract-defects (Issue #490)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/itemviewer-display-and-folder-contract-defects/ (Issue #490)
- Discovered during: preparation research for issue #456 (epic #136, child F14)

- Issue: #490
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/490
- Last Updated: 2026-08-08
## Summary

Five contract and state-handling defects across `ItemViewer.FolderSearch.cs`,
`ItemViewer.DisplayState.cs`, and `ItemViewer.Commands.cs`. All are out of scope to fix under epic
#136's no-behavior-change NFR.

## Defect 1 — `SetFolderItems` appends rather than sets

`SetFolderItems` calls `AddItems` rather than replacing the collection, contradicting both its own
name and its `IItemViewer` contract. The defect is masked in production only because the caller
issues a preceding `ClearFolderItems()`. Any caller that omits the clear gets silently duplicated
folder entries.

## Defect 2 — `FocusSearch` and `FocusSubject` use incompatible threading discipline on one type

`FocusSearch` marshals through `Control.Invoke` while `FocusSubject` calls `.Focus()` bare. Two
different threading contracts on the same control creates a blocking-marshal deadlock risk if
`FocusSearch` is reached beneath a modal dialog, and makes the type's thread-affinity contract
unstateable. Related to the broader marshalling divergence tracked separately for
`ItemViewer.WebViewThread.cs`.

## Defect 3 — `FocusSubject()` targets a non-selectable control and discards its result

`FocusSubject()` calls `Focus()` on a `Label`. `Label` is not selectable, so the call is a no-op, and
the returned `bool` — the only signal that the focus request failed — is discarded. The intended
focus target is never reached and nothing reports it.

## Defect 4 — `FlagTaskDialogResult` uses a WinForms control property as cross-call scratch state

`FlagTaskDialogResult` stores intermediate state in `Button.DialogResult` between calls, using a
presentation property as mutable application state. This couples the result protocol to control
lifetime and to any designer regeneration of the button, and it is not discoverable from the member's
signature.

## Defect 5 — ten mutable display projections with no transactional grouping

`ItemViewer` exposes ten independently settable display projections with no consistency guarantee. A
caller that applies a subset — or that is interrupted partway during pooled viewer reuse — renders a
viewer showing fields from two different mail items simultaneously. There is no grouping construct
and no assertion that a render is complete.

## Related nullability observation

`GetSelectedFolder()` erases a `string?` annotation to `string` at the `ItemViewer` boundary. The
downstream impact was not traced during research; it is recorded here so a fix can evaluate it.

## Acceptance Criteria (early draft)

- [ ] `SetFolderItems` replaces rather than appends, or is renamed to match its behavior.
- [ ] `FocusSearch` and `FocusSubject` share one documented threading contract.
- [ ] `FocusSubject` targets a selectable control and its failure is observable.
- [ ] `FlagTaskDialogResult` holds its state in a field rather than a control property.
- [ ] Display projections are applied as one grouped operation, or the partial-render risk is
      documented and guarded.
- [ ] Regression tests cover each fixed behavior deterministically.

## Constraints & Risks

- All three files are assigned to epic child F14 (issue #456). Scheduling this fix while F14 is in
  flight will produce a semantic conflict; reconcile against F14's plan first.
- Defect 2 overlaps the separately tracked `ItemViewer` UI-thread marshalling divergence; fix them
  together or sequence them deliberately.

## Next Step

- [ ] Promote to GitHub issue (bug template)
