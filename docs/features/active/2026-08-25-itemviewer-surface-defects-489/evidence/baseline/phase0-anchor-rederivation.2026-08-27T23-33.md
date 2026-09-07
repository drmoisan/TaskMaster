# Phase 0 — Anchor Re-derivation Against the Actual Branch Head (P0-T18)

Timestamp: 2026-08-27T23-33
Command: git grep -n on each named member across the six sibling-owned QfcItemController partials
EXIT_CODE: 0

This task exists because P0-T17 measured `Upstream484Landed: true` and `Upstream444Landed: true`.
Every line number printed for these six files in `plan.2026-08-25T01-04.md`, in `spec.md`, in the
research document, or in either upstream contract table is a **pre-upstream** number and is stale.
The rows below are the current values, read on this branch head.

## Anchors — `member = file:line`

### QuickFiler/Controllers/QfcItemController.EventWiring.cs (484-owned; 482 lines)

```
WireIntentEvents = QuickFiler/Controllers/QfcItemController.EventWiring.cs:66
NavigateToString guarded pair (InvokeRequired branch) = QuickFiler/Controllers/QfcItemController.EventWiring.cs:141
NavigateToString guarded pair (else branch) = QuickFiler/Controllers/QfcItemController.EventWiring.cs:145
```

`internal void WireIntentEvents()` opens at `:66` and closes at `:94`. Its body carries exactly
**16** intent subscriptions, matching the count 484's contract documents. The guarded
`NavigateToString` pair — the shape P6 mirrors verbatim into `HtmlDarkConverter` — is the
`if (_itemViewer.InvokeRequired)` block spanning `:139-146`, with
`_itemViewer.Invoke(() => _itemViewer.NavigateToString(ItemHelper.Html));` at `:141` and the direct
`_itemViewer.NavigateToString(ItemHelper.Html);` at `:145`. The plan and research cite this pair as
`:139-146`, which is unchanged.

`internal void UnwireIntentEvents()` is at `:445` and its body contains exactly **16** `-=`
detachments, confirming the symmetry 484's contract asserts. This is the count that becomes an
obligation on 484 when this feature adds a 17th wire.

### QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs (484-owned; 338 lines)

```
HtmlDarkConverter = QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs:288
```

Cited as `:289` pre-upstream; it is now at `:288`.

### QuickFiler/Controllers/QfcItemController.MailActions.cs (484-owned; 257 lines)

```
FlagAsTask = QuickFiler/Controllers/QfcItemController.MailActions.cs:199
FlagAsTaskAsync = QuickFiler/Controllers/QfcItemController.MailActions.cs:215
RightKeyActions["&Expand"] lambda = QuickFiler/Controllers/QfcItemController.MailActions.cs:82
FocusSubject call site (inside that lambda) = QuickFiler/Controllers/QfcItemController.MailActions.cs:86
FolderContains("Trash to Delete") guard, sync = QuickFiler/Controllers/QfcItemController.MailActions.cs:237
SetFolderItems call site, sync = QuickFiler/Controllers/QfcItemController.MailActions.cs:239
FolderContains("Trash to Delete") guard, async = QuickFiler/Controllers/QfcItemController.MailActions.cs:249
SetFolderItems call site, async = QuickFiler/Controllers/QfcItemController.MailActions.cs:251
```

The `RightKeyActions["&Expand"]` entry spans `:82-89`; its lambda body is `:84-88` and contains
exactly two statements, `_itemViewer.FocusSubject();` at `:86` and `this.EnumerateConversation();` at
`:87`. The plan cites the lambda as `:60-67` and the call site as `:64` pre-upstream.

The `FlagTaskDialogResult` read-back pairs P8 removes are at `:208`/`:209` and `:227`/`:228`; the plan
cites them as `:176`/`:177` and `:194`/`:195` pre-upstream. In each pair the write
`_itemViewer.FlagTaskDialogResult = flagTask.Run(modal: true);` is immediately followed by
`if (_itemViewer.FlagTaskDialogResult == DialogResult.OK)`, exactly the shape the spec describes.

The two `SetFolderItems` call sites remain guarded and idempotent by construction: `:239` sits under
`if (!_itemViewer.FolderContains("Trash to Delete"))` at `:237`, and `:251` under the same guard at
`:249`. The plan cites these as `:204`/`:206` and `:216`/`:218` pre-upstream.

### QuickFiler/Controllers/QfcItemController.FolderHandling.cs (446-owned; 235 lines)

```
AssignFolderComboBox = QuickFiler/Controllers/QfcItemController.FolderHandling.cs:161
SetFolderItems call site inside AssignFolderComboBox = QuickFiler/Controllers/QfcItemController.FolderHandling.cs:182
```

Both are **unchanged** from the numbers the plan and spec print, because 446 has not landed and
neither 484 nor 444 edits this file. `public void AssignFolderComboBox()` opens at `:161` and the
unguarded `_itemViewer.SetFolderItems(_folderHandler.FolderArray);` is at `:182`. The three
dispatch entry points remain at `:141` (`_itemViewer.Invoke`), `:145` (direct) and `:158`
(`_itemViewer.UiDispatcher.InvokeAsync`), and `:166` is the method's own re-entry guard.

### QuickFiler/Controllers/QfcItemController.Navigation.cs (444-owned; READ ONLY)

```
MenuDropDown = QuickFiler/Controllers/QfcItemController.Navigation.cs:81
JumpToSearchTextbox = QuickFiler/Controllers/QfcItemController.Navigation.cs:51
```

444 added `SyncExpandedRegistrations` at `:186`, below both anchors, so neither moved: `MenuDropDown`
is still at `:81` and `JumpToSearchTextbox` still at `:51`, matching the pre-upstream citations. Both
are in 444's UNCHANGED list. This file is read-only for this feature and P10-T5 asserts it is absent
from the diff.

### QuickFiler/Controllers/QfcItemController.ViewerSetup.cs (484-owned; READ ONLY; 499 lines)

```
AssignControls = QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:409
```

Cited as `:358-394` pre-upstream. `internal void AssignControls(MailItemHelper itemInfo, int viewerPosition)`
is now at `:409`. This file is read-only for this feature and P10-T5 asserts it is absent from the
diff. Note its line count is **499**, one line below the 500-line ceiling.

## Every named member has a row

The task names twelve anchors across six files. All twelve are recorded above, together with six
supporting locations (the `FocusSubject` call site, the four `FlagTaskDialogResult` read-back lines,
and `UnwireIntentEvents`) that later phases depend on.

Output Summary: Every anchor this feature edits or reads in the six sibling-owned
`QfcItemController` partials was re-derived against the actual branch head and recorded as a
`member = file:line` row. **All subsequent edits into these files are anchored on the member name, not
on any line number recorded here or printed in the plan.** The material movements are
`HtmlDarkConverter` `:289` to `:288`, `FlagAsTask` `:176` region to `:199`, `FlagAsTaskAsync` to
`:215`, the `RightKeyActions["&Expand"]` lambda `:60-67` to `:82-89` with the `FocusSubject` call at
`:86`, the two guarded `SetFolderItems` sites to `:239` and `:251`, and `AssignControls` to `:409`.
`WireIntentEvents` remains at `:66` with 16 subscriptions and `UnwireIntentEvents` at `:445` with 16
detachments, confirming the 16-to-17 obligation this feature places on 484.
`FolderHandling.cs`'s `AssignFolderComboBox` (`:161`) and its `SetFolderItems` call site (`:182`),
and `Navigation.cs`'s `MenuDropDown` (`:81`) and `JumpToSearchTextbox` (`:51`), are unchanged.
