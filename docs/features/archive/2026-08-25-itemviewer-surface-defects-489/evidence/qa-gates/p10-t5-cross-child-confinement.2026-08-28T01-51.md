# P10-T5 — Cross-child confinement of the sibling-owned production diffs

Timestamp: 2026-08-28T01-51
Command: git diff -U0 cecd78130a489fcfdc2ddac7970f344256f4a75a -- QuickFiler/Controllers/QfcItemController.EventWiring.cs QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs QuickFiler/Controllers/QfcItemController.MailActions.cs QuickFiler/Controllers/QfcItemController.FolderHandling.cs
EXIT_CODE: 0

`BASELINE_SHA` is `cecd78130a489fcfdc2ddac7970f344256f4a75a`.

## The sibling-owned production files in the diff

The P10-T2 scope-lock list contains exactly five `QuickFiler/Controllers/` paths. One,
`QfcItemController.EventHandlers.cs`, is **489-owned** by elimination
(`quickfiler-bug-family-446/issue.md:63-64`), so it is not a cross-child edit. The remaining four are
sibling-owned and are exactly the four the task names:

| File | Owner |
|---|---|
| `QuickFiler/Controllers/QfcItemController.EventWiring.cs` | 484 |
| `QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs` | 484 |
| `QuickFiler/Controllers/QfcItemController.MailActions.cs` | 484 |
| `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` | 446 |

There is no fifth. No other sibling-owned production file appears anywhere in the 25-path scope-lock
list.

## Absence assertions

`git diff --name-only <BASELINE_SHA> -- QuickFiler/Controllers/QfcItemController.ViewerSetup.cs QuickFiler/Controllers/QfcItemController.Navigation.cs`
produces **zero output lines**.

| File | Owner | In diff |
|---|---|---|
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` | 484 | **Absent** |
| `QuickFiler/Controllers/QfcItemController.Navigation.cs` | 444 | **Absent** |

Both files are tracked and present on this branch, so these are genuine no-change observations.

## Per-file member-confinement table

Line numbers below are **execution-time** numbers read from the working tree. Both upstreams 484 and
444 have landed (P0-T17), so every line number printed in the plan, the spec, the research document
or either upstream contract table for these files is a pre-upstream number and is stale; the anchors
below are the enclosing member declarations, re-derived here.

### `QfcItemController.EventWiring.cs` — confined to `WireIntentEvents`

`git diff --numstat` reports `1` added, `0` deleted.

| Hunk | Added line | Enclosing member | Permitted |
|---|---|---|---|
| `@@ -93,0 +94 @@` | `_itemViewer.PicturesChanged += this.CbxPictures_CheckedChanged;` | `WireIntentEvents()` | Yes — disposition 1 |

One line, one member. `WireEvents()`, `WireControlTreeEvents()`, `UnwireEvents()`,
`UnwireControlTreeEvents()` and `UnwireIntentEvents()` are all untouched. The handler itself,
`CbxPictures_CheckedChanged`, lands in the 489-owned `QfcItemController.EventHandlers.cs`, so only the
single wire statement is cross-child, exactly as the disposition states.

### `QfcItemController.FocusAndTheme.cs` — confined to `HtmlDarkConverter`

`git diff --numstat` reports `25` added, `5` deleted.

| Hunk | Change | Enclosing member | Permitted |
|---|---|---|---|
| `@@ -292,2 +292,5 @@` | `#489 D2` comment plus the `if (_itemViewer.InvokeRequired)` test | `HtmlDarkConverter(Enums.ToggleState desiredState)` | Yes — disposition 2 |
| `@@ -295,3 +298,20 @@` | The `_itemViewer.Invoke(...)` marshalled branch and the `else` direct branch | `HtmlDarkConverter(Enums.ToggleState desiredState)` | Yes — disposition 2 |

`HtmlDarkConverter` is declared at `:288` and the next member declaration in the file is
`SetThemeLight(bool async)` at `:322`. Both hunks lie between `:292` and `:317`, entirely inside
`HtmlDarkConverter`. The two members 484 changes — `ToggleNavigation` and `ApplyReadEmailFormat`
(declared at `:337`) — carry no hunk, which is the textual disjointness the disposition relies on.

### `QfcItemController.MailActions.cs` — confined to the `RightKeyActions["&Expand"]` lambda, `FlagAsTask`, `FlagAsTaskAsync` and the two `AddFolderItems` invocations

`git diff --numstat` reports `9` added, `7` deleted.

| Hunk | Change | Enclosing member | Permitted |
|---|---|---|---|
| `@@ -86 +86 @@` | `_itemViewer.FocusSubject();` becomes `_ = _itemViewer.FocusSubject();` | `RightKeyActions` (declared `:76`), inside the `"&Expand"` lambda | Yes — `RightKeyActions["&Expand"]` lambda, disposition 3 |
| `@@ -208,2 +208,3 @@` | `flagTask.Run(modal: true)` held in the local `flagTaskResult`, property assigned once, branch on the local | `FlagAsTask()` (declared `:199`) | Yes — `FlagAsTask` |
| `@@ -227,2 +228,3 @@` | Same three-line shape | `FlagAsTaskAsync()` (declared `:216`) | Yes — `FlagAsTaskAsync` |
| `@@ -239 +241 @@` | `SetFolderItems` invocation renamed to `AddFolderItems` | `MarkItemForDeletion()` (declared `:237`) | Yes — first of the two `AddFolderItems` invocations |
| `@@ -251 +253 @@` | `SetFolderItems` invocation renamed to `AddFolderItems` | `MarkItemForDeletionAsync()` (declared `:246`) | Yes — second of the two `AddFolderItems` invocations |

Five hunks across four members, and the permitted set the plan names is exactly the
`RightKeyActions["&Expand"]` lambda, `FlagAsTask`, `FlagAsTaskAsync` and the two `AddFolderItems`
invocations. The two rename hunks are one token each and are compiler-forced: a rename that skips
them does not compile. `MoveFailureNotifier`, `NotifyMoveFailure`, `MoveMailAsync`,
`CollapseConversation`, `EnumerateConversation`, `EnumerateConversationAsync`, `PackageItems` and
`RightKeyActionsAsync` are all untouched.

### `QfcItemController.FolderHandling.cs` — confined to the single `AddFolderItems` invocation

`git diff --numstat` reports `1` added, `1` deleted.

| Hunk | Change | Enclosing member | Permitted |
|---|---|---|---|
| `@@ -182 +182 @@` | `_itemViewer.SetFolderItems(_folderHandler.FolderArray);` becomes `_itemViewer.AddFolderItems(_folderHandler.FolderArray);` | `AssignFolderComboBox()` (declared `:161`) | Yes — the single `AddFolderItems` invocation, disposition 5 |

One hunk, one token, line-neutral. This is disposition 5 in the spec's § Sibling-collision resolution
table: the rename is compiler-forced and is textually disjoint from disposition 4, the deferred
`ClearFolderItems()` insertion, which it does not pre-empt. P10-T6 asserts that deferral separately.

## Acceptance

| P10-T5 condition | Result |
|---|---|
| `QfcItemController.ViewerSetup.cs` absent from the diff | Met |
| `QfcItemController.Navigation.cs` absent from the diff | Met |
| `EventWiring.cs` confined to `WireIntentEvents` | Met — 1 hunk, 1 line |
| `FocusAndTheme.cs` confined to `HtmlDarkConverter` | Met — 2 hunks, both inside `:288`–`:321` |
| `MailActions.cs` confined to the `RightKeyActions["&Expand"]` lambda plus `FlagAsTask`, `FlagAsTaskAsync` and the two `AddFolderItems` invocations | Met — 5 hunks, all in that set |
| `FolderHandling.cs` confined to the single `AddFolderItems` invocation | Met — 1 hunk |
| The only sibling-owned production files in the diff are the four named | Met |

Output Summary: Cross-child confinement **holds**. The only sibling-owned production files in the
P10-T2 diff are the four the plan names — `QfcItemController.EventWiring.cs`, `.FocusAndTheme.cs` and
`.MailActions.cs` (484) and `.FolderHandling.cs` (446) — and `QfcItemController.ViewerSetup.cs` (484)
and `.Navigation.cs` (444) are both absent. Each diff is confined to the members the spec's
§ Sibling-collision resolution names: `EventWiring.cs` to a single added line inside
`WireIntentEvents` (1/0); `FocusAndTheme.cs` to two hunks inside `HtmlDarkConverter`, whose declared
range `:288`–`:321` contains both, leaving 484's `ToggleNavigation` and `ApplyReadEmailFormat`
untouched (25/5); `MailActions.cs` to five hunks in the `RightKeyActions["&Expand"]` lambda,
`FlagAsTask`, `FlagAsTaskAsync`, `MarkItemForDeletion` and `MarkItemForDeletionAsync` — the last two
being the two `AddFolderItems` invocations (9/7); and `FolderHandling.cs` to the single line-neutral
`AddFolderItems` rename inside `AssignFolderComboBox` (1/1).
