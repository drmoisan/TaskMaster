# ItemViewer surface defects (#486, #487, #489, #490) — implementation research

- Feature: `docs/features/active/itemviewer-surface-defects-489/`
- Epic: `quickfiler-bug-family` (#446), wave 2
- Branch: `worktree-agent-a64aadc99c0b8f362`, merged to epic integration head `2300becf`
- Research date: 2026-08-25
- Mode: preparation / research only. No production file, test file, or project file was modified.
  No build, test, or formatter was run.

---

## 0. How to read this document

Every `file:line` below was opened on this branch on 2026-08-25 and the code at that line is quoted
or paraphrased from what was actually read. The four promoted potential documents were captured on
2026-08-07; their citations are treated as hypotheses and each is adjudicated in §2.

Three findings materially change the shape of this feature and are stated up front:

1. **#489 Defect 1 is already fixed.** `QfcItemController._uiDispatcher` is no longer the
   `ItemViewer`-captured WPF `Dispatcher`. It is `UtilitiesCS.Threading.IUiDispatcher`
   (`QuickFiler/Controllers/QfcItemController.cs:66`), an injectable seam, and
   `MenuDropDown()` is already covered by a passing deterministic test
   (`QuickFiler.Test/Controllers/QfcItemController.SeamDispatcherTests.cs:99`). See §5.3.1.
2. **#490 Defect 1's stated mask no longer exists.** Issue #438 removed the
   `ClearFolderItems() + SetFolderItems(...)` composition entirely. There is now **zero** production
   caller of `ClearFolderItems()`. The append behaviour is real but its masking mechanism and its
   exposure both changed. See §5.4.1.
3. **Both upstream contracts describe code that is not yet on this branch.** `UnwireEvents`,
   `UnwireControlTreeEvents`, `UnwireIntentEvents`, `MoveFailureNotifier`, `TryResolveCidResource`,
   `DetachWebResourceRequestedHandler` (484) and `SyncExpandedRegistrations` (444) return **zero
   matches** across `QuickFiler/`. Both siblings are *prepared*, not *executed*. See §3.

---

## 1. Files in scope — current line counts (all verified 2026-08-25)

### Production (candidate edit targets)

| File | Lines | 500-line status | Coverage status |
|---|---:|---|---|
| `QuickFiler/Viewers/ItemViewer.cs` | 432 | OK (68 spare) | `[ExcludeFromCodeCoverage]` on the type, `:20` |
| `QuickFiler/Viewers/ItemViewer.WebViewThread.cs` | 37 | OK | exempt via the primary partial |
| `QuickFiler/Viewers/ItemViewer.DisplayState.cs` | 81 | OK | exempt via the primary partial |
| `QuickFiler/Viewers/ItemViewer.FolderSearch.cs` | 81 | OK | exempt via the primary partial |
| `QuickFiler/Viewers/ItemViewer.Commands.cs` | 109 | OK | exempt via the primary partial |
| `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` | 319 | OK | **owned by sibling 488** — do not edit |
| `QuickFiler/Viewers/ItemViewerExpanded.cs` | 181 | OK | **no** exemption attribute — measurable |
| `QuickFiler/Viewers/IItemViewer.cs` | 143 | OK | interface-only |
| `QuickFiler/Viewers/ToolStripMenuItemCb.cs` | 87 | OK | **no** exemption attribute — measurable |
| `QuickFiler/Viewers/ToolStripMenuItemCb.Designer.cs` | 41 | OK | generated |
| `QuickFiler/Viewers/ItemViewer.Designer.cs` | 6224 | **pre-existing excess** | generated |
| `QuickFiler/Viewers/ItemViewerExpanded.Designer.cs` | 821 | **pre-existing excess** | generated |
| `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` | 487 (approx., 13 spare) | near ceiling | **owned by sibling 501** — do not edit |

The two `.Designer.cs` files already exceed the 500-line ceiling by a wide margin. That excess is
pre-existing, is not created by this feature, and this feature must not attempt to remediate it.
Record it as a pre-existing finding in `spec.md`, mirroring how 444 and 468 record
`QfcCollectionController.cs`.

### `QfcItemController` partials (context, mostly not owned by this feature — see §8)

| File | Lines |
|---|---:|
| `QfcItemController.cs` | 323 |
| `QfcItemController.Conversation.cs` | 235 |
| `QfcItemController.EventHandlers.cs` | 223 |
| `QfcItemController.EventWiring.cs` | 391 |
| `QfcItemController.FocusAndTheme.cs` | 326 |
| `QfcItemController.FolderHandling.cs` | 235 |
| `QfcItemController.Initialization.cs` | 489 (11 spare) |
| `QfcItemController.MailActions.cs` | 224 |
| `QfcItemController.Navigation.cs` | 228 |
| `QfcItemController.ViewerSetup.cs` | 430 |

---

## 2. Verified citation table

Legend: **EXACT** = same file, same line, same construct. **MOVED** = construct present, different
line. **CHANGED** = construct present but materially different. **GONE** = construct no longer
exists.

### 2.1 From `2026-08-07-itemviewer-move-option-menu-defects.md` (#486)

| Cited | Verdict | Current fact |
|---|---|---|
| `ToolStripMenuItemCb.cs:32-58` shadows `Checked`/`CheckedChanged` with `new`, never assigns `base.Checked` | **EXACT** | `:32` `public new bool Checked`; setter `:36-49` writes `_checked`, `base.Image`, raises the shadowed `CheckedChanged`, calls `base.Invalidate()`. No `base.Checked` write anywhere in the file. `:58` `public new event EventHandler CheckedChanged;` |
| `ItemViewerExpanded.cs:169-179` handler | **EXACT** | `:169` `private void MenuItem_CheckedChanged(ToolStripMenuItem menuItem)`; body `:171-178` reads `menuItem.Checked` and assigns `menuItem.Image` |
| `ItemViewer.cs:177-187` handler | **EXACT** | `:177` `private void MenuItem_CheckedChanged(ToolStripMenuItem menuItem)`, identical body |
| Candidate fix "assign `base.Checked = value;` at `ToolStripMenuItemCb.cs:37`" | **EXACT line, NOT recommended** | `:37` is `{` opening the setter body; `:38` is `_checked = value;`. See §5.2.1 for why the consumer-side fix is preferred |
| `ItemViewer.cs:171-175`, `:177-187`, `:205` have no caller / no designer wiring | **EXACT and CONFIRMED** | `:171` `private void MenuItem_CheckedChanged(object sender, EventArgs e)`; `:205` `private void MoveOptionsMenu_Click(object sender, EventArgs e) { }`. Solution-wide grep for `MenuItem_CheckedChanged` and `MoveOptionsMenu_Click` finds no `ItemViewer` caller and no `ItemViewer.Designer.cs` wiring |
| `ItemViewer.Designer.cs` wires exactly one handler, at `:256`, and it is not one of these | **EXACT and CONFIRMED** | Grep for `+= new System` in that 6224-line file returns **exactly one** hit: `:256 this._l0v2h2_WebView2.ParentChanged += new System.EventHandler(this.L0v2h2_WebView2_ParentChanged);` |
| `ItemViewerExpanded.cs:163-179` members are wired four times at `ItemViewerExpanded.Designer.cs:171,180,189,198` | **EXACT** | Grep for `+= new System` in that file returns exactly five hits: `:171` `ConversationMenuItem.CheckedChanged`, `:180` `SaveAttachmentsMenuItem.CheckedChanged`, `:189` `SaveEmailMenuItem.CheckedChanged`, `:198` `SavePicturesMenuItem.CheckedChanged`, `:274` `L0v2h2_WebView2.ParentChanged` |
| called four times from `ItemViewerExpanded.cs:24-27` | **EXACT** | `:24-27` `MenuItem_CheckedChanged(this.ConversationMenuItem);` … `SavePicturesMenuItem` |
| `QfcItemController.EventWiring.cs:66-94` wires the other three move-option events but not `PicturesChanged` | **EXACT** | `WireIntentEvents()` spans `:66-94`. `:68 ConversationModeChanged`, `:92 EmailCopyChanged`, `:93 AttachmentsChanged`. No `PicturesChanged` |
| `EfcFormController.cs:389` wires it | **MOVED / RECLASSIFIED** | The four EFC wirings are now at `EfcFormController.cs:385-388`; `SavePicturesMenuItem.CheckedChanged += SavePictures_CheckedChanged;` is at **`:387`**, not `:389`. Note it wires the raw `_formViewer.SavePicturesMenuItem.CheckedChanged` on `EfcViewer`, **not** `IItemViewer.PicturesChanged`. See §5.2.3 |

### 2.2 From `2026-08-07-itemviewer-parentchanged-console-and-cast.md` (#487)

| Cited | Verdict | Current fact |
|---|---|---|
| `ItemViewer.cs:168` `Console.WriteLine("Parent Changed");` | **EXACT** | `:166` `private void L0v2h2_WebView2_ParentChanged(object sender, EventArgs e)`, `:168` `Console.WriteLine("Parent Changed");` — the entire body |
| wired at `ItemViewer.Designer.cs:256` | **EXACT** | verified above |
| `ItemViewerExpanded.cs:160` identical | **EXACT** | `:158` handler, `:160` `Console.WriteLine("Parent Changed");` |
| wired at `ItemViewerExpanded.Designer.cs:274` | **EXACT** | verified above |
| `ItemViewer.cs:173` `var menuItem = (ToolStripMenuItem)sender;` unguarded | **EXACT** | `:173` |
| `ItemViewerExpanded.cs:165` identical | **EXACT** | `:165` |
| "all four current wirings pass a `ToolStripMenuItemCb`" | **CONFIRMED** | `ItemViewerExpanded.Designer.cs:36-39` construct all four as `new QuickFiler.Viewers.ToolStripMenuItemCb()`; fields declared `ToolStripMenuItemCb` at `:811-814` |
| "in `ItemViewer` the member is dead code" | **CONFIRMED** | see §2.1 |

### 2.3 From `2026-08-07-itemviewer-ui-thread-marshalling-divergence.md` (#489)

| Cited | Verdict | Current fact |
|---|---|---|
| `QfcItemController.Navigation.cs:83` `await _uiDispatcher.InvokeAsync(() => _itemViewer.ShowMoveOptionsMenu())` | **EXACT line, CHANGED meaning** | `:83` is character-for-character as cited, inside `public async Task MenuDropDown()` at `:81-84`. But `_uiDispatcher` is now `UtilitiesCS.Threading.IUiDispatcher` (`QfcItemController.cs:66`), **not** `_itemViewer.UiDispatcher`. See §5.3.1 |
| "`_uiDispatcher` originates from `Dispatcher.CurrentDispatcher` captured in the `ItemViewer` constructor (`ItemViewer.cs:13,28,71-75`)" | **FALSE on this branch** | `ItemViewer.cs:13` `using System.Windows.Threading;`, `:28` `_uiDispatcher = Dispatcher.CurrentDispatcher;`, `:71-75` `UiDispatcher` property — all **EXACT** — but the controller field is a different object. `QfcItemController.Initialization.cs:383` defaults it to `new UtilitiesCS.Threading.WpfUiDispatcher()` |
| "every other forwarder … is marshalled with `Control.InvokeRequired`/`Control.Invoke` (`EventWiring.cs:139-146`, `Conversation.cs:224-228`)" | **EXACT** | `EventWiring.cs:139-146` is the guarded `NavigateToString` pair; `Conversation.cs:224-228` is the guarded `SetTopicThread` re-entry |
| `QfcItemController.FocusAndTheme.cs:293` `_itemViewer.NavigateToString(ItemHelper.ToggleDark(desiredState));` unguarded | **EXACT** | `:289` `public void HtmlDarkConverter(Enums.ToggleState desiredState)`, `:291` `if (_isWebViewerInitialized)`, `:293` the unguarded call |
| `ItemViewer.WebViewThread.cs:15` performs no marshalling | **EXACT** | `:15` `public void NavigateToString(string html) => L0v2h2_WebView2.NavigateToString(html);` |
| `ItemViewer.WebViewThread.cs:23` and `:25` are the pair | **EXACT** | `:23` `public void SetConversationItems(IList items) => TopicThread.SetObjects(items);`, `:25` `public void SortConversationByDate(SortOrder order) => TopicThread.Sort(SentDate, order);` |
| `IItemViewer.cs:109-110` declares them | **MOVED** | now `IItemViewer.cs:119` `void SetConversationItems(System.Collections.IList items);` and `:120` `void SortConversationByDate(SortOrder order);`. `:109-110` are now `event System.EventHandler SearchTextChanged;` / `event KeyEventHandler SearchKeyDown;` |
| `QfcItemController.Conversation.cs:231-232` is the only production caller | **EXACT and CONFIRMED** | `:231` `_itemViewer.SetConversationItems(conversationInfo);`, `:232` `_itemViewer.SortConversationByDate(SortOrder.Descending);` inside `SetTopicThread` (`:221-233`). Solution-wide grep confirms no other production caller |
| `ItemViewer.cs` exposes `UiSyncContext` (`:59-63`), `UiScheduler` (`:65-69`), `UiDispatcher` (`:71-75`), captured at `:26-28` | **EXACT** | all five spans verified verbatim |
| consumers "unguarded (`EventHandlers.cs:196,200`, `FocusAndTheme.cs:293`)" | **MOVED (EventHandlers)** | `EventHandlers.cs:195-198` is now the `TopicThread_ItemSelectionChanged` signature; `:200` is `var objects = _itemViewer.GetSelectedConversationItems();` and the unguarded `NavigateToString` is at **`:204`**. `FocusAndTheme.cs:293` is EXACT |
| runtime-evidence path `docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/…` | **GONE** | that feature folder is not under `docs/features/active/` on this branch. Do not cite the path; cite the exception shape only |

### 2.4 From `2026-08-07-itemviewer-display-and-folder-contract-defects.md` (#490)

| Cited | Verdict | Current fact |
|---|---|---|
| "`SetFolderItems` calls `AddItems` rather than replacing" | **EXACT** | `ItemViewer.FolderSearch.cs:20` `public void SetFolderItems(string[] items) => BreadcrumbCoordinator?.AddItems(items);`. `BreadcrumbBridgeCoordinator.cs:130` doc: "Appends Path B plain rows verbatim and re-renders (legacy AddRange semantics)"; `:131` `public void AddItems(IReadOnlyList<string> items)` |
| "masked in production only because the caller issues a preceding `ClearFolderItems()`" | **GONE** | **No production caller of `ClearFolderItems()` exists.** The only mentions are the declaration (`ItemViewer.FolderSearch.cs:41`, `IItemViewer.cs:101`) and three doc comments recording that #438 removed the composition (`EventHandlers.cs:165`, `IItemViewer.cs:93`, `BreadcrumbBridgeCoordinator.Search.cs:26`). See §5.4.1 |
| "`FocusSearch` marshals through `Control.Invoke` while `FocusSubject` calls `.Focus()` bare" | **EXACT** | `ItemViewer.FolderSearch.cs:79` `public void FocusSearch() => TxtboxSearch.Invoke(new Action(() => TxtboxSearch.Focus()));`; `ItemViewer.DisplayState.cs:79` `public void FocusSubject() => LblSubject.Focus();` |
| "`FocusSubject()` calls `Focus()` on a `Label` … the returned `bool` is discarded" | **EXACT** | `LblSubject` is declared `public System.Windows.Forms.Label LblSubject` at `ItemViewer.cs:244-248`. `Control.Focus()` returns `bool`; the expression-bodied member discards it |
| "`FlagTaskDialogResult` stores intermediate state in `Button.DialogResult` between calls" | **EXACT** | `ItemViewer.Commands.cs:97-101` `public DialogResult FlagTaskDialogResult { get => BtnFlagTask.DialogResult; set => BtnFlagTask.DialogResult = value; }`. `BtnFlagTask` is `ButtonSVG` (`ItemViewer.cs:354`), and `SVGControl/ButtonSVG.cs:13` is `public partial class ButtonSVG : Button` — so the backing property is `System.Windows.Forms.Button.DialogResult` (`IButtonControl`) |
| "ten independently settable display projections" | **EXACT (count confirmed)** | `IItemViewer.cs:43-52`: `SenderText`, `SubjectText`, `BodyText`, `TriageText`, `SentOnText`, `ActionableText`, `ItemNumberText`, `FolderText`, `ConversationCountText`, `ConversationCountBackColor` — exactly ten. Implemented `ItemViewer.DisplayState.cs:13-71` |
| "`GetSelectedFolder()` erases a `string?` annotation to `string`" | **EXACT** | `ItemViewer.FolderSearch.cs:25` `public string GetSelectedFolder() => BreadcrumbCoordinator?.GetSelectedFolder();`. `BreadcrumbBridgeCoordinator.cs:1` is `#nullable enable` and `:190` declares `public string? GetSelectedFolder()`. `ItemViewer.FolderSearch.cs` carries no `#nullable` directive, so the erasure is silent |

---

## 3. Upstream contract reconciliation — a material disagreement

### 3.1 Neither upstream's code is on this branch

Solution-wide grep across `QuickFiler/` for
`UnwireEvents|UnwireIntentEvents|UnwireControlTreeEvents|MoveFailureNotifier|TryResolveCidResource|SyncExpandedRegistrations`
returns **no matches**. Both sibling `spec.md` files are present and readable; their production code
is not. This is consistent with the delegation wording ("already prepared") and with 444's own
spec, which records at line 808-813 that `QfcCollectionController.TestSupport.cs` "does not exist at
base commit `988e819b`" because #468 had prepared but not executed.

**Consequence for planning.** Author against the documented post-change shape (as instructed), but
every acceptance condition must be anchored on a member name, never on a post-upstream line number.
Phase 0 must re-derive anchors against the actual branch head at execution time. In particular:

- 484's contract cites `EventWiring.cs:28-32`, `:50`, `ViewerSetup.cs:396-425`, `:400`, `:403`,
  `:404`, `:407`, `:420`, `:424`, `FocusAndTheme.cs:168-179`, `:318-324`, `MailActions.cs:83-126`,
  `:183-200`, `:49-52`. These are **pre-change** line numbers and will shift once 484 executes.
- 444's contract cites `QfcItemController.Navigation.cs` members but explicitly (444 spec, line 830)
  forbids transcribing post-#468 line numbers. The same discipline applies here.

### 3.2 `QfcItemController.Navigation.cs:83` — reconciled

The delegation asked which upstream, if either, moves the `ShowMoveOptionsMenu` call site.

- **444 does not move it.** 444's exhaustive table for `QfcItemController.Navigation.cs` (spec lines
  742-784) lists exactly one ADDED member (`SyncExpandedRegistrations`, `private`), zero REMOVED, and
  two CHANGED (`ToggleExpansion(Enums.ToggleState)` and `ToggleExpansionAsync(Enums.ToggleState)`).
  `MenuDropDown` is named in the UNCHANGED list at spec line 776. So `MenuDropDown`'s **body** is
  unchanged.
- **484 does not touch `Navigation.cs` at all.** 484's owned file set is `FocusAndTheme.cs`,
  `EventWiring.cs`, `ViewerSetup.cs`, `MailActions.cs` (spec lines 323-327). Its only mention of
  `Navigation.cs` is a preservation constraint (spec line 440).
- **Line number will shift.** 444 adds `SyncExpandedRegistrations` to `Navigation.cs`. Whether it
  lands before or after `MenuDropDown` is not fixed by 444's spec. `:83` is therefore **not** a
  stable anchor after 444 executes. Anchor on `public async Task MenuDropDown()`.

Current post-read body (pre-444, this branch):

```csharp
public async Task MenuDropDown()
{
    await _uiDispatcher.InvokeAsync(() => _itemViewer.ShowMoveOptionsMenu());
}
```

Current lines: `:81-84`, with the dispatch on `:83`.

### 3.3 The disagreement — report prominently

484's CHANGED table (spec line 359) says of `MoveMailAsync()`:

> "(c) The user-facing message is routed through `MoveFailureNotifier` on `_uiDispatcher` instead of
> a direct `MessageBox.Show`."

and 484's Technical specifications (spec line 516) says:

> "`NotifyMoveFailure` marshals through `_uiDispatcher.Invoke` when the dispatcher is non-null …
> because existing tests (`SeamFactoryTests.cs` `MoveMailAsync_*`) do not set `_uiDispatcher`."

That is consistent with the source: `_uiDispatcher` is `IUiDispatcher`. **484's contract is correct
and current.**

The disagreement is with the **#489 potential document**, not with either sibling spec. The
potential asserts that `_uiDispatcher` "originates from `System.Windows.Threading.Dispatcher.CurrentDispatcher`
captured in the `ItemViewer` constructor". On this branch that is false:

| Claim (potential #489, Defect 1) | Source reality on this branch |
|---|---|
| `_uiDispatcher` is the `ItemViewer`-captured WPF `Dispatcher` | `QuickFiler/Controllers/QfcItemController.cs:66` — `private UtilitiesCS.Threading.IUiDispatcher _uiDispatcher;` |
| it comes from `ItemViewer.cs:28` | `QuickFiler/Controllers/QfcItemController.Initialization.cs:383` — `_uiDispatcher ??= new UtilitiesCS.Threading.WpfUiDispatcher();` |
| `ShowMoveOptionsMenu` has "a different and weaker delivery guarantee than every other UI operation on the same control" | `_uiDispatcher` is the **dominant** pattern in `Navigation.cs`: `:43`, `:59`, `:83`, `:90`, `:96`, `:102`, `:116`, `:128`, `:197`, `:202`. Also `MailActions.cs:51`, `:186`, `:214`; `Conversation.cs:163`, `:207`; `FocusAndTheme.cs:270` |

The `ItemViewer`-captured dispatcher (`_itemViewer.UiDispatcher`) still has four live production
consumers, enumerated in §4.

---

## 4. The three `ItemViewer` marshalling seams — exhaustive consumer enumeration

All three seams still exist on `ItemViewer` and on `IItemViewer`:

- `ItemViewer.cs:59-63` `public SynchronizationContext UiSyncContext` (field `_context`, `:59`;
  captured `:26`)
- `ItemViewer.cs:65-69` `public TaskScheduler UiScheduler` (field `_uiScheduler`, `:65`;
  captured `:27`)
- `ItemViewer.cs:71-75` `public Dispatcher UiDispatcher` (field `_uiDispatcher`, `:71`;
  captured `:28` as `Dispatcher.CurrentDispatcher`)
- Declared on the interface at `IItemViewer.cs:36` (`UiDispatcher`), `:37` (`UiScheduler`),
  `:38` (`UiSyncContext`)

### 4.1 `IItemViewer.UiDispatcher` consumers (live production, 4 sites)

| Site | Statement |
|---|---|
| `QuickFiler/Controllers/QfcItemController.Initialization.cs:192` | `_ = _itemViewer.UiDispatcher.InvokeAsync(InitializeWebViewAsync);` |
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:353` | `await _itemViewer.UiDispatcher.InvokeAsync(() => …` (inside `AssignControlsAsync`) |
| `QuickFiler/Controllers/QfcItemController.FolderHandling.cs:158` | `await _itemViewer.UiDispatcher.InvokeAsync(AssignFolderComboBox);` |
| `QuickFiler/Controllers/EfcItemController.cs:913` and `:922` | `await _itemViewer.UiDispatcher.InvokeAsync(ToggleExpansionOn);` / `…(ToggleExpansionOff);` |

Commented-out (not consumers): `QfcItemController.ViewerSetup.cs:347`,
`EfcItemController.cs:918`/`:927`, `QuickFiler/Helper Classes/ConversationResolver.cs:317`/`:343`,
`QuickFiler/Controllers/KeyboardHandler.cs:328`/`:377`.

### 4.2 `IItemViewer.UiScheduler` consumers (live production)

**Zero.** The only `UiScheduler` references on `ItemViewer` are the declaration
(`ItemViewer.cs:65-69`) and the interface (`IItemViewer.cs:37`). The single call-site-shaped mention,
`QfcItemController.ViewerSetup.cs:346`, is commented out. Other types (`EfcViewer.cs:43`,
`QfcItemViewer.cs:61`, `QfcFormViewer.cs:41`, `ItemViewerExpanded.cs:64`, `QfcHomeController.cs:438`,
`IQfcFormViewer.cs:16`) declare their own unrelated `UiScheduler`.

### 4.3 `IItemViewer.UiSyncContext` consumers (live production, 6 sites in `QfcItemController`)

| Site | Statement |
|---|---|
| `QfcItemController.ViewerSetup.cs:58` | `await _itemViewer.UiSyncContext;` |
| `QfcItemController.ViewerSetup.cs:264` | `_itemViewer.UiSyncContext,` (argument to `QfcTipsDetails.CreateAsync`) |
| `QfcItemController.ViewerSetup.cs:269` | `await itemViewer.UiSyncContext;` |
| `QfcItemController.ViewerSetup.cs:280` | `.SelectAwait(x => QfcTipsDetails.CreateAsync(x, _itemViewer.UiSyncContext, Token))` |
| `QfcItemController.ViewerSetup.cs:285` | same shape |
| `QfcItemController.cs:305` | commented out — **not** a consumer |

Consumers of the *distinct, unrelated* `UiSyncContext` members on other viewer types (out of scope,
listed only so they are not mistaken for `IItemViewer` consumers): `EfcFormController.cs:418`, `:434`,
`:450`, `:466`, `:703`, `:734`, `:742`, `:762`, `:788`, `:860`, `:1033`; `EfcItemController.cs:220`,
`:749`, `:1153`; `EfcHomeController.cs:78`, `:227`; `QfcHomeController.cs:95`, `:135`;
`QfcCollectionController.cs:1028`, `:2082`; `KeyboardHandler.cs:107`, `:136`, `:153`, `:241`;
`QfcFormController.EventHandlers.cs:24`, `:26`, `:74`, `:89`, `:100`, `:250`, `:348`;
`QfcFormViewer.cs:63`.

### 4.4 Consolidation feasibility

Removing `UiScheduler` from `ItemViewer`/`IItemViewer` is a zero-consumer deletion. Removing
`UiDispatcher` or `UiSyncContext` requires rewriting 4 and 5 call sites respectively, all of them in
`QfcItemController.ViewerSetup.cs`, `.Initialization.cs`, `.FolderHandling.cs` and
`EfcItemController.cs`. Ownership of those files is split across siblings 484, 446 and 464 (§8).
This is the basis of the out-of-scope recommendation in §9.1.

---

## 5. Per-defect findings

### 5.1 Cross-cutting: `ItemViewer` is `[ExcludeFromCodeCoverage]`

`QuickFiler/Viewers/ItemViewer.cs:20` carries `[ExcludeFromCodeCoverage]` on the type. Because the
attribute is not `AllowMultiple`, the other five partials cannot repeat it and inherit the exemption
(comment at `ItemViewer.DisplayState.cs:8-10` records this explicitly). Therefore **no fix inside any
`ItemViewer*.cs` partial can be proved by a coverage delta.** Per-defect proof must be carried by a
**named test**, exactly as 444 records for `QfcCollectionController` (444 spec, lines 850-855). Note
`.claude/rules/plan-acceptance-gates.md` rejects acceptance conditions that cannot fail; a coverage
claim on these files would be one.

`ItemViewerExpanded` (no attribute) and `ToolStripMenuItemCb` (no attribute) **are** measurable.

### 5.2 Issue #486 — move-option menu

#### 5.2.1 Defect 1 — the check image is cleared immediately after being set

**Still reproducible: YES, but only on the `ItemViewerExpanded` twin.**

Verified end-to-end chain (all lines read on this branch):

1. `ToolStripMenuItemCb.Designer.cs:34` — `this.Click += new System.EventHandler(this.ToolStripMenuItemCb_Click);`
   (run from the ctor at `ToolStripMenuItemCb.cs:15`).
2. `ItemViewerExpanded.Designer.cs:165`/`:175`/`:184`/`:193` set `CheckOnClick = true`. The shadowed
   setter (`ToolStripMenuItemCb.cs:63-79`) does `base.Click -= …; base.Click += …;` — the `-=`
   removes the designer subscription (same `Method` + same `Target`), so **exactly one** subscription
   survives. There is no double-toggle.
3. A click runs `ToolStripMenuItemCb_Click` (`:53-56`) → `Checked = !Checked`.
4. The shadowed setter (`:32-50`) writes `_checked`, sets `base.Image` correctly (`:40` / `:43`),
   raises the **shadowed** `CheckedChanged` (`:47`), and calls `base.Invalidate()` (`:48`).
   **`base.Checked` is never written.**
5. `ItemViewerExpanded.Designer.cs:171/180/189/198` subscribed the shadowed event to
   `ItemViewerExpanded.MenuItem_CheckedChanged(object, EventArgs)` (`:163-167`), which casts to
   `ToolStripMenuItem` and calls the typed overload (`:169-179`).
6. The typed overload reads `menuItem.Checked` — because the parameter is `ToolStripMenuItem`, this
   binds to `ToolStripMenuItem.Checked`, which is **always false** — and takes the `else` branch,
   `:177` `menuItem.Image = null;`. **The image the setter just applied is cleared.**

**The `ItemViewer` twin is unaffected**, because `ItemViewer.Designer.cs` wires no such handler
(single `+=` in the whole file, at `:256`). `EfcViewer` is also unaffected: its equivalent handler and
constructor calls are commented out at `EfcViewer.cs:109-140`.

**Correction to the potential's severity claim.** "the four move-option menu items never display a
check mark" is true for `ItemViewerExpanded` only, not for the primary QuickFiler `ItemViewer`.
`ItemViewerExpanded` is production-live: it is `_qfcItemViewerExpandedTemplate` in
`QfcFormViewer.Designer.cs:42` and `:256`, consumed at `QfcFormViewer.cs:189`, `:202-210`.

**Minimal fix — recommended: delete the redundant handler, not add `base.Checked`.**

The potential's candidate fix (`base.Checked = value;`) is rejected. Setting
`ToolStripMenuItem.Checked = true` enables WinForms' own check-margin glyph in addition to the
custom `base.Image`, producing a doubled indicator. That is a rendering change beyond the defect.
Also note `ToolStripMenuItemCb.cs:81-85` deliberately hides `Image` as `private new`, showing the
design intent is that the custom image is the *only* indicator.

Recommended minimal fix, which resolves #486 D1, #486 D2 and #487 D2 with one coherent deletion:

- Delete `ItemViewerExpanded.cs:163-167` (`MenuItem_CheckedChanged(object, EventArgs)`) and
  `:169-179` (the typed overload).
- Delete the four constructor calls `ItemViewerExpanded.cs:24-27`. They are redundant: the
  `ToolStripMenuItemCb` constructor already applies the correct initial image at `:16-19`
  (`_checked` defaults to `false`, so `Image` stays null, which is the correct unchecked rendering).
- Delete the four designer wirings `ItemViewerExpanded.Designer.cs:171`, `:180`, `:189`, `:198`.
- Delete the dead `ItemViewer.cs:171-175`, `:177-187` and `:205` (behaviour-neutral — no caller, no
  wiring).

After this, `ToolStripMenuItemCb.Checked`'s setter is the sole owner of the check image, which is
already correct.

**Alternative if deletion is judged too broad:** change the two handler signatures from
`ToolStripMenuItem` to `QuickFiler.Viewers.ToolStripMenuItemCb`. This makes `menuItem.Checked` bind
to the shadowed property and the handler becomes a harmless duplicate of the setter. It is a smaller
diff but leaves the duplication in place. The deletion is preferred.

**Deterministic test (headless, seam already available).** `ToolStripMenuItemCb` derives from
`ToolStripMenuItem` → `ToolStripDropDownItem` → `ToolStripItem` → `Component`. It is **not** a
`Control`, requires no window handle, and does not trip `NoLiveFormInTestAssemblyTests`
(`QuickFiler.Test/NoLiveFormInTestAssemblyTests.cs:16-36`, which scans only for
`System.Windows.Forms.Form`-derived types in the test assembly). `ToolStripItem.Invalidate()` is
null-parent-safe in the framework. A test can therefore:

- construct a `ToolStripMenuItemCb` directly, set `Checked = true`, and assert the base `Image`
  property (readable via `((ToolStripItem)item).Image`) is `Properties.Resources.CheckBoxChecked`;
- subscribe the *current* `ItemViewerExpanded`-shaped handler body to the item's `CheckedChanged` and
  assert the image is null afterwards — this is the RED assertion that fails before the fix;
- after the fix, assert no `CheckedChanged` subscriber exists that nulls the image (a reflection
  assertion on `ItemViewerExpanded` for the absence of `MenuItem_CheckedChanged`).

This defect is fully testable without a form, a pump, or an `ItemViewer` instance. **No
`*.StaTests.cs` file is needed.**

#### 5.2.2 Defect 2 — divergent menu behaviour between the twins

**Still reproducible: YES, exactly as cited.** All six citation points verified EXACT (§2.1).

**Minimal fix.** Covered by the deletion in §5.2.1. Deleting `ItemViewer.cs:171-175`, `:177-187`,
`:205` removes three dead private members; deleting the `ItemViewerExpanded` handler + wirings
removes the divergence in the other direction. After the fix the two twins are consistent: neither
carries a check-image handler, and `ToolStripMenuItemCb` owns the behaviour.

**Deterministic test.** Reflection/metadata only, in the existing
`QuickFiler.Test/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs` idiom (132 lines, 5
`[TestMethod]`, plenty of headroom — see §7): assert that
`typeof(QuickFiler.ItemViewer).GetMethod("MenuItem_CheckedChanged", BindingFlags.Instance | BindingFlags.NonPublic)`
and `GetMethod("MoveOptionsMenu_Click", …)` are null, and the same for `ItemViewerExpanded`. These
tests fail before the fix (methods present) and pass after. No instance construction at all.

#### 5.2.3 Defect 3 — `PicturesChanged` has no production subscriber

**Still reproducible: YES.** Solution-wide grep for `PicturesChanged` returns exactly two production
hits, both declarations: `ItemViewer.Commands.cs:85` (the event, forwarding to
`SavePicturesMenuItem.CheckedChanged`) and `IItemViewer.cs:71`. There is **no `+=` anywhere**.

Supporting facts:

- `WireIntentEvents()` (`EventWiring.cs:66-94`) wires 16 intent events; `PicturesChanged` is not one.
- There is **no `CbxPictures_CheckedChanged` handler** on `QfcItemController` — the sibling handlers
  are `CbxConversation_CheckedChanged` (`EventHandlers.cs:27`), `CbxEmailCopy_CheckedChanged`
  (`:208`), `CbxAttachments_CheckedChanged` (`:218`). The fix must add the handler as well as the
  wiring.
- `_optionsPictures` (`QfcItemController.cs:57`) is written exactly once, at
  `ViewerSetup.cs:392` from `_globals.QfSettings.SavePictures`, and read exactly once, at
  `MailActions.cs:102` (`SavePictures = _optionsPictures,`). It is never refreshed from the menu.
  This is the precise user-visible consequence: **toggling "Save Pictures" in QuickFiler is
  discarded; the setting value at viewer-assign time is what the filer uses.**
- `PicturesChecked` **is** written at `ViewerSetup.cs:393`, so the menu shows the right initial state
  — which makes the failure silent rather than visible.

**Correction to the potential's supporting citation.** `EfcFormController.cs:387` (not `:389`) does
`_formViewer.SavePicturesMenuItem.CheckedChanged += SavePictures_CheckedChanged;` — but that is the
`EfcViewer` raw menu item, not `IItemViewer.PicturesChanged`. The EFC surface wires all four
directly (`:385-388`) with handlers at `:534`, `:539`, `:544`, `:549`. The comparison still supports
the conclusion (the EFC path handles all four; the QFC path handles three), but the mechanism is
different and the spec must say so. `EfcFormController.cs` changed in PR #605 and these line numbers
were re-read on this branch on 2026-08-25.

**Minimal fix.** Two lines: a `CbxPictures_CheckedChanged` handler in
`QfcItemController.EventHandlers.cs` (`_optionsPictures = _itemViewer.PicturesChecked;`) plus
`_itemViewer.PicturesChanged += this.CbxPictures_CheckedChanged;` in `WireIntentEvents()`.

**COLLISION.** `QfcItemController.EventWiring.cs` is owned by sibling 484 (§8), and 484 adds
`UnwireIntentEvents()` to that file with a documented count of **16** intent detachments (484 spec,
line 358 and line 664). Adding a 17th wire obligates a 17th unwire. See §8.2 for the disposition
options.

**Deterministic test.** `Mock<IItemViewer>` + `VerifyAdd(v => v.PicturesChanged += It.IsAny<EventHandler>(), Times.Once())`
against `WireIntentEvents()`. `VerifyAdd` on this exact mock is already a proven technique in this
suite (484 spec, line 666, cites `SeamFactoryTests.cs:250-259`). A second test raises the mock's
`PicturesChanged` and asserts `_optionsPictures` flipped (reflection read via
`QfcItemControllerTestSupport.GetField`). Fully headless.

### 5.3 Issue #487 — `Console.WriteLine` and unguarded cast

#### 5.3.1 Defect 1 — production `Console.WriteLine`

**Still reproducible: YES, both twins, at the exact cited lines.** `ItemViewer.cs:166-169` and
`ItemViewerExpanded.cs:158-161`, wired at `ItemViewer.Designer.cs:256` and
`ItemViewerExpanded.Designer.cs:274`.

**Is a designer edit avoidable?** Yes, but only at the cost of leaving dead code:

| Option | Diff | Residual |
|---|---|---|
| A. Delete handler + delete the one designer wiring line, in both twins | 2 `.cs` deletions + 2 `.Designer.cs` line deletions | none — recommended |
| B. Keep the handler, empty its body | 2 one-line edits, **no designer edit** | a no-op designer-wired handler remains; still a General Code Change Policy §5.2 smell but no `Console` |
| C. Keep the handler, route to a logger | requires introducing a logger into `[ExcludeFromCodeCoverage]` view code | worst — adds an untestable dependency to a view |

**Recommendation: Option A.**

**Does a designer edit round-trip safely?** Yes, with one ordering rule. Removing an event-handler
subscription line from `InitializeComponent()` is exactly what the WinForms designer emits when the
event is cleared in the property grid; on reopen the designer re-parses `InitializeComponent()` and
will not re-add it. The handler method must be deleted in the **same change** as the wiring — deleting
the method while the `+=` remains is a compile error (CS0103). No `.resx` change is needed: the
wiring is a code statement, not a resource.

**One item to verify in Phase 0 before touching either `.Designer.cs`.** `.csharpierignore` (read on
this branch, 15 lines) excludes `**/evidence/**`, `*.cobertura.xml`, `*.coverage`, `*.coveragexml`,
`*.trx`, `*.csproj`, `*.props`, `*.targets`. It does **not** list `*.Designer.cs`. Yet
`ItemViewer.Designer.cs:256` is roughly 107 columns, above CSharpier's default 100-column print width,
and there is no `.csharpierrc` in the repository. The most likely explanation is that CSharpier's
built-in generated-file detection skips `*.designer.cs` by filename, but **this was not verified**
(no tool was run in this preparation session). Phase 0 must run
`dotnet tool run csharpier check .` on the untouched worktree and record the baseline, so that a
whole-file reformat of a 6224-line generated file cannot be attributed to this feature's one-line
edit. Treat this as a hard gate before the first `.Designer.cs` edit.

**Deterministic test.** Metadata-only: assert that `L0v2h2_WebView2_ParentChanged` is absent from
both types via `GetMethod(…, BindingFlags.Instance | BindingFlags.NonPublic)`. Fails before (present),
passes after. A second, stronger structural test is possible without reflection: a source-text
assertion is prohibited by the plan-gate rules if it cannot fail, so prefer the reflection form.
No `Console` capture, no instance construction.

#### 5.3.2 Defect 2 — unguarded downcast

**Still reproducible: YES**, at `ItemViewer.cs:173` and `ItemViewerExpanded.cs:165`, exact.

**Minimal fix.** Subsumed by the §5.2.1 deletion — deleting `MenuItem_CheckedChanged(object, EventArgs)`
in both twins removes both casts. If the deletion is rejected in favour of the retyping alternative,
the cast becomes `if (sender is ToolStripMenuItemCb menuItem) { … }` in both twins.

**Deterministic test.** Same metadata-absence assertion as §5.2.2. If the retyping alternative is
chosen instead, the test is `Action act = () => InvokeNonPublic(viewer, "MenuItem_CheckedChanged", new object(), EventArgs.Empty); act.Should().NotThrow();`
— but that requires an `ItemViewerExpanded` instance (a `UserControl` whose `InitializeComponent`
builds a WebView2), which is the exact construction the policy discourages. This asymmetry is a
further argument for the deletion.

### 5.4 Issue #489 — UI-thread marshalling

#### 5.4.1 Defect 1 — `ShowMoveOptionsMenu` marshalled onto a WPF `Dispatcher`

**Still reproducible: NO. This defect has already been fixed by an intervening change.**

`MenuDropDown()` marshals through `IUiDispatcher`, the injectable seam introduced by the #230
work, not through `ItemViewer.UiDispatcher`. Evidence:

- `QfcItemController.cs:66` — `private UtilitiesCS.Threading.IUiDispatcher _uiDispatcher;`
- `QfcItemController.Initialization.cs:38`, `:57`, `:419`, `:430`, `:461`, `:472` — constructor and
  factory injection points; `:383` — the production default `new WpfUiDispatcher()`.
- `UtilitiesCS/Threading/WpfUiDispatcher.cs:24-25` — the production adapter resolves
  `UiThread.Dispatcher`, which `UtilitiesCS/Threading/UiThread.cs:61` sets from
  `SyncContextForm.UiDispatcher` (`UtilitiesCS/Threading/SyncContextForm.cs:38`,
  `UiDispatcher = Dispatcher.CurrentDispatcher;`) — a dispatcher captured on the *host* UI thread by
  a form that `UiThread.Initialize()` shows and hides (`UiThread.cs:50-78`), not one captured
  per-viewer.
- `MenuDropDown` is not anomalous: **ten** sites in `Navigation.cs` alone use the same seam
  (`:43`, `:59`, `:83`, `:90`, `:96`, `:102`, `:116`, `:128`, `:197`, `:202`), plus
  `MailActions.cs:51`, `:186`, `:214`; `Conversation.cs:163`, `:207`; `FocusAndTheme.cs:270`.
- It is already covered by a passing deterministic test:
  `QuickFiler.Test/Controllers/QfcItemController.SeamDispatcherTests.cs:99-107`
  (`MenuDropDown_ShowsMoveOptionsMenuThroughDispatcher`), which asserts
  `dispatcher.Verify(d => d.InvokeAsync(It.IsAny<Action>()), Times.Once())` and
  `viewer.Verify(v => v.ShowMoveOptionsMenu(), Times.Once())`.

**Recommended disposition.** Record this in `spec.md` as **fixed by an intervening change, no work
item**, name the existing test as the standing regression, and close the corresponding acceptance
criterion by citation rather than by new code. Per the CLAUDE.md Bugfix Workflow a regression test is
required before a fix; there is no fix, so a new test would be a test with nothing to assert against —
`.claude/rules/plan-acceptance-gates.md` would treat a synthesised failing condition here as
unfalsifiable.

The residual concern the potential raised — that `WpfUiDispatcher` still resolves a WPF `Dispatcher`
rather than the WinForms sync context — is a *different* and much broader question that now applies
uniformly to sixteen call sites across four files. It is not a `ShowMoveOptionsMenu` defect. See §9.1.

#### 5.4.2 Defect 2 — `NavigateToString` unguarded on the theme path

**Still reproducible: YES**, at `FocusAndTheme.cs:293`, exact. But the exposure analysis is more
nuanced than the potential recorded, and the plan should say so.

Call graph, verified:

- `HtmlDarkConverter` (`FocusAndTheme.cs:289-301`) is never called directly. It is passed as an
  `Action<Enums.ToggleState>` into `QfcThemeHelper.SetupThemes` at
  `QfcItemController.Initialization.cs:177`, `:213`, `:272`, `:305`
  (`QfcThemeHelper.cs:39` parameter `Action<Enums.ToggleState> htmlConverter`, forwarded at `:53`,
  `:91`, `:347` to `Theme.HtmlConverter`).
- It is invoked from two places in `UtilitiesCS`:
  - `UtilitiesCS/HelperClasses/ThemeHelpers/Theme.Rendering.cs:126` — `HtmlConverter(HtmlDark);`
    inside the `_webView2.CoreWebView2 is not null` guard of `SetQfcTheme()`.
  - `UtilitiesCS/HelperClasses/ThemeHelpers/ThemeControlGroup.cs:294` — `_htmlConverter(_htmlDark);`
    inside `ApplyThemeWebView2()`.
- On the **QuickFiler** path only the first is reachable: `QfcItemController` calls
  `Theme.SetQfcTheme(async)` (`FocusAndTheme.cs:64`, `:120`, `:279`, `:284`, `:307`, `:312`), never
  `Theme.SetTheme(async)`. `SetQfcTheme(bool)` (`Theme.cs:427-445`) marshals in all three branches:
  `async` → `_uiDispatcher.InvokeAsync` (`:431`); else `_lblItemNumber.InvokeRequired` → `Invoke`
  (`:433-436`); else direct.
- **The residual hole.** `Control.InvokeRequired` returns `false` when the control has no created
  handle, even off the UI thread. `SetThemeDark(async: false)` / `SetThemeLight(async: false)` are
  reachable from `QfcCollectionController.cs:818` and `:822`, and `SetQfcTheme(async: false)` from
  `FocusAndTheme.cs:64` and `:120`. If any of those runs on a non-UI thread against a viewer whose
  `_lblItemNumber` handle is not yet created, `SetQfcTheme()` executes inline and
  `NavigateToString` is called cross-thread.
- The EFC path uses `Theme.SetTheme(async)` (`EfcItemController.cs:1087`, `:1092`, `:1114`, `:1119`),
  which reaches `ThemeControlGroup.ApplyTheme(bool)` (`ThemeControlGroup.cs:212-229`). That method
  marshals **only when `_controls is not null`** (`:214`); the WebView2 group's decision path
  (`:201-203` → `ApplyThemeWebView2`, `:289-296`) is reached unmarshalled when `_controls` is null.
  `Theme.cs` and `ThemeControlGroup.cs` are **out of this feature's scope** (`UtilitiesCS`).

**Minimal fix.** Guard `HtmlDarkConverter` in `QfcItemController.FocusAndTheme.cs`, mirroring the
established shape at `EventWiring.cs:139-146` verbatim:

```csharp
public void HtmlDarkConverter(Enums.ToggleState desiredState)
{
    if (_itemViewer.InvokeRequired)
    {
        _itemViewer.Invoke(() => HtmlDarkConverter(desiredState));
        return;
    }
    if (_isWebViewerInitialized) { … existing body … }
}
```

`IItemViewer.cs:135-137` declares `InvokeRequired` and `Invoke(Delegate)` on the interface
specifically so `InvokeRequired`-guarded routing stays mockable (the `#pragma warning disable CS0108`
block at `:134-139` records this rationale). The identical re-entry pattern is used at
`Conversation.cs:181-185`, `:224-228`; `FolderHandling.cs:139-146`, `:164-168`;
`ViewerSetup.cs:361-365`.

**COLLISION.** `QfcItemController.FocusAndTheme.cs` is owned by sibling 484 (§8), which changes
`ToggleNavigation` (`:168-179`) and `ApplyReadEmailFormat` (`:318-324`) in the same file.
`HtmlDarkConverter` (`:289-301`) sits between them and is not in 484's changed set, so the textual
conflict is avoidable but the ownership question is not. See §8.2.

**Deterministic test.** Directly available with no new seam. `HtmlDarkConverter` is `public`, and
`QfcItemController.FocusAndThemeTests.cs:483-491`
(`HtmlDarkConverter_WhenWebViewNotInitialized_DoesNotNavigate`) already calls it on a
`Mock<IItemViewer>`-backed harness. The RED test is:

- Arrange a `Mock<IItemViewer>` with `SetupGet(v => v.InvokeRequired).Returns(true)` and
  `Setup(v => v.Invoke(It.IsAny<Delegate>()))` recording the delegate without running it (or running
  it, per the existing `BuildExecutingViewer` idiom at `FocusAndThemeTests.cs:99-115`).
- Set `_isWebViewerInitialized` to `true` by reflection.
- Act: `controller.HtmlDarkConverter(ToggleState.On)`.
- Assert before the fix: `viewer.Verify(v => v.Invoke(It.IsAny<Delegate>()), Times.Never())` **fails**
  in the tightened form; the direct assertion is
  `viewer.Verify(v => v.NavigateToString(It.IsAny<string>()), Times.Once())` **without** any
  `Invoke` — i.e. assert `Times.Once()` on `Invoke` and observe zero. Fully headless, no pump,
  no wall clock.

#### 5.4.3 Defect 3 — `SetConversationItems` / `SortConversationByDate` atomicity

**Still reproducible: YES.** Both are still separate `IItemViewer` members
(`IItemViewer.cs:119`, `:120`), separately implemented (`ItemViewer.WebViewThread.cs:23`, `:25`), and
neither the interface nor the implementation nor any XML doc records the ordering requirement.

**Callers — exhaustive (solution-wide grep, 2026-08-25):**

| Site | Kind |
|---|---|
| `QuickFiler/Controllers/QfcItemController.Conversation.cs:231` + `:232` | the only production caller, back-to-back inside `SetTopicThread` (`:221-233`), after the `InvokeRequired` re-entry guard at `:224-228` |
| `QuickFiler.Test/Controllers/QfcItemController.ConversationTests.cs:261-262` | `SetTopicThread_WhenNotInvokeRequired_SetsItemsAndSortsDescending` (`:249`) — asserts both, `Times.Once()` each |
| `QuickFiler.Test/Controllers/QfcItemController.ConversationTests.cs:279` | `SetTopicThread_WhenInvokeRequired_MarshalsViaInvoke` (`:266`) |
| `QuickFiler.Test/Controllers/QfcItemController.ConversationTests.cs:341` | a third assertion site |

No other implementation of `IItemViewer` exists in production (`QfcItemViewer.cs` and
`QfcItemViewerExpanded.cs` do not implement it — verified by their lack of the members).

**Minimal fix — two options.**

- **F1 (contract-only, zero behaviour change).** Add XML documentation to `IItemViewer.cs:119-120`
  stating that `SetConversationItems` must be followed by `SortConversationByDate` within the same
  UI-thread turn, and that calling either alone leaves the list in source order. Cheapest, satisfies
  the potential's own acceptance wording ("or the contract is documented"), touches one file that
  this feature owns.
- **F2 (atomic by construction).** Add `void SetConversationItemsSorted(IList items, SortOrder order)`
  to `IItemViewer` implemented in `ItemViewer.WebViewThread.cs` as the two calls, and change
  `Conversation.cs:231-232` to the single call. This is an interface addition — 484's contract
  explicitly guarantees "No interface is modified" (484 spec, line 367) for its own change, so an
  *additive* member here does not contradict it, but it does obligate updating the three test
  assertions above and any `Mock<IItemViewer>` setup that relies on the two-call shape.

**Recommendation: F1.** F2's benefit is real but the pair has exactly one production caller which is
already correct; F2 changes an interface consumed by sibling-owned test files (§7) for a defect with
no live failure. F1 is the minimal, targeted fix the Bugfix Workflow calls for.

**Deterministic test for F1.** A contract test is possible without contriving a fake failure: assert
by reflection that `IItemViewer.SortConversationByDate` exists with the exact signature and that
`QfcItemController.SetTopicThread` calls both. The existing `ConversationTests.cs:249` test already
asserts the ordering-relevant pair. **The honest position is that F1 admits no new failing test** —
documentation is not observable. The plan should carry a `fail-before-exception` dossier for this
defect rather than a synthetic RED test, and should not state an acceptance condition that cannot
fail (`.claude/rules/plan-acceptance-gates.md` G5/G6). If F2 is chosen instead, the RED test is
`viewer.Verify(v => v.SetConversationItemsSorted(items, SortOrder.Descending), Times.Once())`,
which fails to compile before the fix — an acceptable, if blunt, RED.

#### 5.4.4 Defect 4 — three concurrent marshalling contracts

**Still reproducible: YES**, and quantified in §4. All three seams still exist. Consumer counts:
`UiDispatcher` 4 live sites, `UiSyncContext` 5 live sites (all in `ViewerSetup.cs`), `UiScheduler`
**0 live sites**.

**Recommendation: DEFER, with one carve-out.** See §9.1.

### 5.5 Issue #490 — display and folder contract

#### 5.5.1 Defect 1 — `SetFolderItems` appends rather than sets

**Still reproducible: YES for the append behaviour; NO for the stated mask.**

- `ItemViewer.FolderSearch.cs:20` still forwards to `AddItems`, whose own doc comment
  (`BreadcrumbBridgeCoordinator.cs:130`) says "Appends … (legacy AddRange semantics)".
- The masking `ClearFolderItems()` caller the potential describes **no longer exists**. Issue #438
  replaced the per-keystroke `ClearFolderItems + SetFolderItems + SetFolderSelectedIndex +
  SetFolderDroppedDown` composition with the single `PresentFolderSearchResults` intent
  (`IItemViewer.cs:92-99`, implemented `ItemViewer.FolderSearch.cs:38-39`, backed by
  `BreadcrumbBridgeCoordinator.Search.cs:47-77`, which does its own replace via
  `_router.ReplaceItemsPreservingSession`). The former composition survives only in three comments:
  `QfcItemController.EventHandlers.cs:165`, `IItemViewer.cs:93`,
  `BreadcrumbBridgeCoordinator.Search.cs:26`.

**Current `SetFolderItems` callers — exhaustive (3 sites):**

| Site | Preceding clear? | Exposure |
|---|---|---|
| `QfcItemController.FolderHandling.cs:182` — `_itemViewer.SetFolderItems(_folderHandler.FolderArray);` inside `AssignFolderComboBox()` (`:161-208`) | **No.** The method has no `ClearFolderItems()` and no other clear | Duplicates if `AssignFolderComboBox()` runs twice against the same coordinator generation. It is reachable both from `PopulateFolderComboBox` (`:133-147`, via `_itemViewer.Invoke` at `:141`) and from `PopulateFolderComboBoxAsync` (`:149-159`, via `_itemViewer.UiDispatcher.InvokeAsync` at `:158`) |
| `QfcItemController.MailActions.cs:206` (`MarkItemForDeletion`) | No, but guarded by `if (!_itemViewer.FolderContains("Trash to Delete"))` at `:204` | idempotent by construction — append is intentional here |
| `QfcItemController.MailActions.cs:218` (async variant) | same guard at `:216` | idempotent |

The residual mask is `Cleanup()`: `QfcItemController.ViewerSetup.cs:400`
`(_itemViewer as ItemViewer)?.ResetBreadcrumb();` → `ItemViewer.Breadcrumb.cs:258`
`_breadcrumbLifecycleCoordinator?.Reset()` → `BreadcrumbItemViewerLifecycleCoordinator.cs:195`
`_bridgeCoordinator?.Reset()` → `BreadcrumbBridgeCoordinator.cs:160` `public void Reset() => Clear();`.
So *pooled viewer reuse* is safe; a *second population within one viewer lifetime* is not.

**Minimal fix, and a hard ownership constraint.**
`QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` is **owned by sibling 501**
(`docs/features/active/breadcrumb-coordinator-hub-defects-501/issue.md:43`) and
`QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` is **owned by sibling 488** (same file, `:48`). The fix
must therefore be confined to `ItemViewer.FolderSearch.cs` and to the `IItemViewer` XML doc.

Two options that respect that boundary:

- **G1 (recommended).** Make `SetFolderItems` replace, using only members already public on the
  coordinator:
  ```csharp
  public void SetFolderItems(string[] items)
  {
      BreadcrumbCoordinator?.Clear();
      BreadcrumbCoordinator?.AddItems(items);
  }
  ```
  Risk to weigh: `Clear()` calls `_upgradeLifetime.Invalidate()` (`BreadcrumbBridgeCoordinator.cs:152`),
  which cancels any in-flight suggestion upgrade. At `FolderHandling.cs:182` the very next statement
  block (`:189-192`) issues `SetFolderSuggestions`, which begins a fresh population — so the
  invalidation is benign on the only unguarded call site. At `MailActions.cs:206`/`:218` the
  `FolderContains` guard means the call rarely runs, and when it does the intent *is* to add a single
  row to an existing set — G1 would wipe the folder list there. **G1 is therefore not safe as a blanket
  change.**
- **G2 (recommended instead).** Rename to match behaviour, which is the second half of the potential's
  own acceptance wording ("or is renamed to match its behavior"): rename
  `IItemViewer.SetFolderItems(string[])` → `AddFolderItems(string[])`, update the implementation
  (`ItemViewer.FolderSearch.cs:20`), the three call sites, and any `Mock<IItemViewer>` setups. Then
  separately fix the *actual* duplication risk at `FolderHandling.cs:182` by adding an explicit
  `ClearFolderItems()` immediately before it — restoring the deliberate clear that #438 removed for a
  different code path.

  **`QfcItemController.FolderHandling.cs` is owned by sibling 446** (`quickfiler-bug-family-446/issue.md:63`).
  See §8.2.

**Recommendation: G2, with the `FolderHandling.cs` half deferred if the 446 ownership cannot be
negotiated.** The rename alone closes the contract defect (name matches behaviour) and is entirely
within this feature's files plus the interface. Record the `FolderHandling.cs:182` duplication risk
as an out-of-scope finding if 446 retains ownership.

**Deterministic test.** `Mock<IItemViewer>` cannot observe this (the defect is inside the concrete
viewer). Two honest options:

- Metadata test in `ItemViewerBreadcrumbDropDownContractTests.cs`: assert
  `typeof(IItemViewer).GetMethod("SetFolderItems", new[]{typeof(string[])})` is **null** and
  `GetMethod("AddFolderItems", …)` is non-null. Fails before, passes after. Blunt but falsifiable.
- Behavioural test against `BreadcrumbBridgeCoordinator` directly (not `ItemViewer`): the coordinator
  is host-neutral and already unit-tested — `QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorTests.cs`
  (488 lines, 16 `[TestMethod]`, 12 spare lines — near the ceiling, see §7). This proves `AddItems`
  appends, which documents the rename's justification. It does not prove the viewer forwards
  correctly, because `ItemViewer` cannot be constructed headlessly.

#### 5.5.2 Defect 2 — incompatible threading discipline on `FocusSearch` / `FocusSubject`

**Still reproducible: YES**, exactly as cited (`FolderSearch.cs:79` vs `DisplayState.cs:79`).

Additional facts for the fix:

- `FocusSearch()` is called at `QfcItemController.Navigation.cs:54` (inside `JumpToSearchTextbox`,
  `:51-55`) with **no** surrounding marshal, and is asserted by
  `QuickFiler.Test/Controllers/QfcItemController.NavigationTests.cs:198`
  (`viewer.Verify(v => v.FocusSearch(), Times.Once())`).
- `FocusSubject()` is called at `QfcItemController.MailActions.cs:64`, inside the
  `RightKeyActions["&Expand"]` lambda (`:60-67`).
- `TxtboxSearch.Invoke(...)` on a control with no created handle throws
  `InvalidOperationException: Invoke or BeginInvoke cannot be called on a control until the window
  handle has been created.` — an unconditional `Invoke` with no `InvokeRequired` guard is itself a
  latent defect independent of the deadlock concern the potential names.

**Minimal fix.** Make both members follow one documented contract. The contract that matches every
other `ItemViewer` intent member (all of which are bare forwards) and matches the controller-side
guard convention is: **the viewer forwards; the controller marshals.** That means changing
`FocusSearch()` to a bare `TxtboxSearch.Focus();` and leaving `FocusSubject()` bare, then adding the
`InvokeRequired` guard on the controller side if one is needed. That is a two-file change
(`ItemViewer.FolderSearch.cs` — owned; `QfcItemController.Navigation.cs` — **owned by 444**).

The alternative — make both marshal inside the viewer — is a one-file change but contradicts every
other member of the intent surface and re-imports the no-handle `Invoke` throw into `FocusSubject`.

**Recommendation:** change `FocusSearch()` to the bare form in `ItemViewer.FolderSearch.cs`, document
the contract in the `IItemViewer` XML doc for both members, and record any controller-side guard need
as a cross-feature note against 444 rather than editing `Navigation.cs`.

**Deterministic test.** Metadata/structural only, for the same reason as §5.5.1. The behavioural
assertion "FocusSearch does not marshal" is not observable through `Mock<IItemViewer>`. A
`fail-before-exception` dossier is the honest carrier here; alternatively an IL/reflection assertion
is possible but would be brittle. **Recommend the dossier.**

#### 5.5.3 Defect 3 — `FocusSubject()` targets a `Label` and discards the result

**Still reproducible: YES.** `ItemViewer.DisplayState.cs:79` `public void FocusSubject() => LblSubject.Focus();`
where `LblSubject` is `System.Windows.Forms.Label` (`ItemViewer.cs:244-248`, backing field
`_lblSubject`, declared in `ItemViewer.Designer.cs`). `Label` sets
`ControlStyles.Selectable = false`, so `Focus()` returns `false` and the call is a no-op. The
`bool` return is discarded by the expression-bodied member.

Sole caller: `QfcItemController.MailActions.cs:64`, in `RightKeyActions["&Expand"]`, immediately
before `this.EnumerateConversation()`.

**What was the intended target?** Not determinable from the source. The `&Expand` action expands the
conversation; the plausible intent is to move focus off the folder selector so the expansion keys
work. Do **not** guess a new target. Two defensible minimal fixes:

- **H1 (recommended).** Change `IItemViewer.FocusSubject()` to return `bool`
  (`bool FocusSubject();`), implement as `public bool FocusSubject() => LblSubject.Focus();`, and
  have the sole caller observe it. This makes the failure observable — which is exactly the
  potential's acceptance wording ("its failure is observable") — without inventing a focus target.
  `IItemViewer.cs:54` and `ItemViewer.DisplayState.cs:79` are both in this feature's files. The
  caller `MailActions.cs:64` is **owned by 484**.
- **H2.** Additionally set `LblSubject.TabStop`/`SetStyle(Selectable)` — rejected: that is a
  behaviour change to a WinForms control's selectability with unbounded focus-order consequences,
  well beyond a minimal targeted fix.

**Recommendation: H1**, with the caller-side observation recorded as a cross-feature note if 484
retains `MailActions.cs`.

**Deterministic test.** Fully headless via `Mock<IItemViewer>`: after the signature change,
`viewer.Setup(v => v.FocusSubject()).Returns(false)` and assert the controller's response. Before the
fix the test does not compile (`void` cannot be `.Returns`), which is a valid RED. Additionally a
metadata test asserts `typeof(IItemViewer).GetMethod("FocusSubject").ReturnType == typeof(bool)` —
fails before, passes after, and is falsifiable.

#### 5.5.4 Defect 4 — `FlagTaskDialogResult` as cross-call scratch state

**Still reproducible: YES**, and worse than the potential recorded.

`ItemViewer.Commands.cs:97-101` round-trips `BtnFlagTask.DialogResult`. `BtnFlagTask` is `ButtonSVG`
(`ItemViewer.cs:354-358`) and `SVGControl/ButtonSVG.cs:13` is `public partial class ButtonSVG : Button`,
so the property is `System.Windows.Forms.Button.DialogResult` (`IButtonControl`).

**Additional finding not in the potential: the WinForms side effect is real and the reads are
redundant.**

- Read sites, exhaustive: `MailActions.cs:177` and `:195`. Both are
  `if (_itemViewer.FlagTaskDialogResult == DialogResult.OK)` on the line immediately after
  `_itemViewer.FlagTaskDialogResult = flagTask.Run(modal: true);` (`:176`, `:194`). **Both could be
  a local variable.** No value written elsewhere is ever read.
- Write sites: `MailActions.cs:176`, `:194`, and `ViewerSetup.cs:375` / `:379` (inside
  `AssignControls`, setting `DialogResult.OK` or `DialogResult.Cancel` from `itemInfo.IsTaskFlagSet`).
  **The `ViewerSetup.cs` writes are never read** — `FlagAsTask`/`FlagAsTaskAsync` overwrite before
  reading. They are pure dead state.
- Because `ButtonSVG : Button` implements `IButtonControl`, a non-`None` `DialogResult` on a button
  hosted on a `Form` causes a click to set the form's `DialogResult` and, for a modally-shown form,
  close it. `ItemViewer` is a `UserControl` hosted in `QfcFormViewer`. Whether that path is live
  depends on how `QfcFormViewer` is shown, which was **not** traced in this session. Record it as an
  assessed hazard requiring one Phase 0 confirmation, not as an established defect.

**Minimal fix.** Replace the control-property round-trip with a plain field on `QfcItemController`,
or better, with a local:

```csharp
DialogResult result = flagTask.Run(modal: true);
_itemViewer.FlagTaskDialogResult = result;   // retained only if the property is genuinely needed
if (result == DialogResult.OK) { … }
```

and, if the `ViewerSetup.cs:373-380` writes prove to be dead, delete them. Both files are **owned by
484** (`MailActions.cs`, `ViewerSetup.cs`). The `IItemViewer`/`ItemViewer.Commands.cs` half — removing
`FlagTaskDialogResult` from the interface entirely — is in this feature's files but breaks two
existing assertions (`QfcItemController.ViewerSetupTests.cs:258`, `:283`) which are in a
**484-owned test file**.

**Recommendation.** Given that every write and read site sits in 484-owned files, and given that the
`ViewerSetup.cs` writes are the same statements 484 is editing for #484, **the cleanest disposition
is to keep the property on `IItemViewer` (it is a legitimate presentation projection of the flag
state), fix only the redundant read-back at `MailActions.cs:177`/`:195` if 484 ownership permits, and
record the dead `ViewerSetup.cs:373-380` writes and the `IButtonControl` hazard as out-of-scope
findings.** See §8.2 and §9.3.

**Deterministic test.** Fully headless via `Mock<IItemViewer>`: assert
`viewer.VerifyGet(v => v.FlagTaskDialogResult, Times.Never())` after `FlagAsTask()` with a stubbed
`_flagTasksFactory`. Before the fix that fails (one get). The existing tests at
`ViewerSetupTests.cs:258`/`:283` constrain the setter and must stay green.

#### 5.5.5 Defect 5 — ten ungrouped display projections

**Still reproducible: YES**, count confirmed at exactly ten (`IItemViewer.cs:43-52`). The applying
site is `QfcItemController.ViewerSetup.cs:367-393` inside `AssignControls` (`:358-394`), which sets
all ten (plus `FlagTaskDialogResult`, `ConversationModeChecked`, `EmailCopyChecked`,
`AttachmentsChecked`, `PicturesChecked`) after an `InvokeRequired` re-entry guard at `:361-365`.

**Recommendation: DEFER.** See §9.2.

#### 5.5.6 Nullability observation — `GetSelectedFolder()` `string?` erasure

The potential explicitly says this was not traced. It is traced here.

**The erasure.** `BreadcrumbBridgeCoordinator.cs` is `#nullable enable` (`:1`) and declares
`public string? GetSelectedFolder() => _router.GetSelectedFolder();` (`:190`).
`ItemViewer.FolderSearch.cs` has **no** `#nullable` directive, so its
`public string GetSelectedFolder() => BreadcrumbCoordinator?.GetSelectedFolder();` (`:25`) compiles
with no warning and publishes an un-annotated `string`. `IItemViewer.cs` is also un-annotated
(`:87` `string GetSelectedFolder();`).

**Two independent null sources:**
1. `BreadcrumbCoordinator` is null on a bare viewer (property declared
   `ItemViewer.Breadcrumb.cs:25`, assigned `:59`, **nulled at `:316`**). The `?.` then yields null.
   The file header comment at `ItemViewer.FolderSearch.cs:15-16` records this as intended
   ("On a bare viewer … getters return the legacy empty-combo values") — but for this member the
   "legacy empty-combo value" was `ComboBox.SelectedItem?.ToString()`, i.e. also null, so the
   behaviour is preserved.
2. The router itself returns `string?` when no row is selected.

**Downstream impact — exhaustive (2 consumers, both in `QfcItemController`):**

| Site | Statement | Consequence of null |
|---|---|---|
| `QfcItemController.EventHandlers.cs:215` | `_selectedFolder = _itemViewer.GetSelectedFolder();` inside `CboFolders_SelectedIndexChanged` (`:213-216`) | `_selectedFolder` (`QfcItemController.cs:237`) becomes null; surfaced via `public string SelectedFolder { get => _selectedFolder; }` (`:238-241`) |
| `QfcItemController.FolderHandling.cs:206` | `_selectedFolder = _itemViewer.GetSelectedFolder();` at the tail of `AssignFolderComboBox()` | same |

`SelectedFolder` is a `public` member of an `internal` class. Neither `QfcItemController.cs:237-241`
nor either assignment guards for null. Because both consumer files are un-annotated, the compiler
raises nothing.

**Assessment.** The erasure is a *diagnostic* loss, not a live NullReferenceException: no read of
`_selectedFolder` was found that dereferences it without a check in the files read. A full trace of
every `SelectedFolder` read was **not** performed and is recorded as an open item (§10).

**Recommendation.** Record as an out-of-scope finding. Adding `#nullable enable` to
`ItemViewer.FolderSearch.cs` would opt that file into `CS86xx`-as-error under CI's
`/p:TreatWarningsAsErrors=true` (CLAUDE.md § C#1.3: nullable enforcement is per-file opt-in via the
pragma) and would cascade into `IItemViewer.cs` and both consumer files, none of which this feature
fully owns. That is a nullable-adoption work item, not a bugfix.

---

## 6. Testability analysis — summary matrix

| Defect | Headless test possible? | Seam | New file needed? |
|---|---|---|---|
| #486 D1 check image | **Yes** | `ToolStripMenuItemCb` is a `Component`, not a `Control` — direct construction is legal and does not trip `NoLiveFormInTestAssemblyTests.cs:16` | new `QuickFiler.Test/Viewers/ToolStripMenuItemCbTests.cs` (none exists) |
| #486 D2 divergence | **Yes** | reflection/metadata only | fits in `ItemViewerBreadcrumbDropDownContractTests.cs` (132 lines, 5 methods) |
| #486 D3 `PicturesChanged` | **Yes** | `Mock<IItemViewer>` + `VerifyAdd`; proven technique per 484 spec line 666 | routing per §7 |
| #487 D1 `Console.WriteLine` | **Yes** | reflection metadata-absence | `ItemViewerBreadcrumbDropDownContractTests.cs` |
| #487 D2 unguarded cast | **Yes** (as metadata-absence, given the deletion fix) | reflection | same |
| #489 D1 `ShowMoveOptionsMenu` | **N/A — already fixed** | existing test `SeamDispatcherTests.cs:99` | none |
| #489 D2 `NavigateToString` guard | **Yes** | `Mock<IItemViewer>` with `SetupGet(v => v.InvokeRequired)`; interface declares `InvokeRequired`/`Invoke` at `IItemViewer.cs:135-136` specifically for this | routing per §7 |
| #489 D3 set/sort pair | **No, for the F1 documentation fix** | documentation is not observable | `fail-before-exception` dossier |
| #489 D4 seam consolidation | n/a — deferred | | |
| #490 D1 `SetFolderItems` | **Partly** — metadata test for the rename; behavioural test only against the coordinator, not the viewer | `BreadcrumbBridgeCoordinator` is host-neutral | see §7 for file capacity |
| #490 D2 threading discipline | **No** | not observable through the interface | `fail-before-exception` dossier |
| #490 D3 `FocusSubject` | **Yes** | signature change to `bool` gives a compile-time RED plus a `Mock<IItemViewer>.Returns(false)` behavioural test | routing per §7 |
| #490 D4 `FlagTaskDialogResult` | **Yes** | `Mock<IItemViewer>.VerifyGet(..., Times.Never())` | routing per §7 |
| #490 D5 projections | n/a — deferred | | |

**No defect in this feature requires an STA WinForms test.** The last-resort `*.StaTests.cs`
allowance is not needed. Nothing here needs `WinFormsPumpHost`
(`QuickFiler.Test/TestSupport/WinFormsPumpHost.cs`, 482 lines) either.

**Banned-API compliance.** None of the proposed tests needs `Thread.Sleep`, `Task.Delay`,
`DateTime.Now`, a real wall-clock wait, a temporary file, or a live `BackgroundWorker`. The
`Mock<IUiDispatcher>` synchronous harness already exists at
`QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs:102-137` (`BuildSyncDispatcher`) and
configures `Invoke`, both non-generic `InvokeAsync` overloads, and `BeginInvoke` to run the delegate
inline. **That file is owned by sibling 493** — consume it, do not edit it.

---

## 7. Existing test inventory and file-capacity routing

### 7.1 Files covering `ItemViewer` / `ItemViewerExpanded` / `ToolStripMenuItemCb` / `IItemViewer`

There is **no** dedicated `ItemViewerTests.cs`, `ItemViewerExpandedTests.cs`, or
`ToolStripMenuItemCbTests.cs`. Coverage is indirect, through `Mock<IItemViewer>` in the controller
suites plus one metadata-contract file.

| File | Lines | `[TestMethod]` | Spare to 500 | Note |
|---|---:|---:|---:|---|
| `QuickFiler.Test/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs` | 132 | 5 | 368 | **Best landing zone.** Reflection/metadata only, no instantiation. Existing idiom is exactly what #486 D2 / #487 D1+D2 need |
| `QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs` | 374 | 10 | 126 | natural home for #486 D3 `VerifyAdd`. Contains the headless real-`ItemViewer` fixture 484 cites |
| `QuickFiler.Test/Controllers/QfcItemController.ConversationTests.cs` | 352 | 12 | 148 | owns the set/sort pair assertions (`:249`, `:266`) |
| `QuickFiler.Test/Controllers/QfcItemController.SeamDispatcherTests.cs` | 352 | 14 | 148 | owns `MenuDropDown_ShowsMoveOptionsMenuThroughDispatcher` (`:99`) |
| `QuickFiler.Test/Controllers/QfcItemController.NavigationTests.cs` | 391 | 13 | 109 | asserts `FocusSearch` at `:198` |
| `QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.cs` | 184 | 7 | 316 | natural home for #490 D3/D4 |
| `QuickFiler.Test/Controllers/QfcItemController.EventHandlersTests.cs` | 477 | 16 | **23** | near ceiling |
| `QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs` | 474 | 10 | **26** | near ceiling; owns `FlagTaskDialogResult` assertions `:258`, `:283` and `PicturesChecked` `:262` |
| `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` | 497 | 17 | **3** | **effectively full**; owns `HtmlDarkConverter_WhenWebViewNotInitialized_DoesNotNavigate` (`:483`). Also flagged by 493 as sibling-owned |
| `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs` | 498 | 17 | **2** | **effectively full** |
| `QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorTests.cs` | 488 | 16 | **12** | near ceiling; 501-adjacent |
| `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` | 365 | 1 | 135 | **owned by sibling 493 — consume only** |
| `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` | **500** | 13 | **0** | **PINNED by 468 — do not add any test** (444 spec, line 868) |
| `QuickFiler.Test/NoLiveFormInTestAssemblyTests.cs` | 54 | 1 | 446 | structural guard that constrains every new test file |
| `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs` | 531 | 7 | **over** | pre-existing excess, not this feature's |

### 7.2 Routing recommendation

- New file `QuickFiler.Test/Viewers/ToolStripMenuItemCbTests.cs` for #486 D1 — no existing file covers
  this type at all, and the type is directly constructible.
- `ItemViewerBreadcrumbDropDownContractTests.cs` for every metadata-absence assertion
  (#486 D2, #487 D1, #487 D2, #490 D1 rename, #490 D3 return type). 368 spare lines.
- `QfcItemController.EventWiringTests.cs` for #486 D3.
- `QfcItemController.MailActionsTests.cs` for #490 D3 and #490 D4.
- **#489 D2 must not land in `QfcItemController.FocusAndThemeTests.cs`** (3 spare lines, and 493
  names it sibling-owned). Route to a new file, e.g.
  `QuickFiler.Test/Controllers/QfcItemController.ThemeMarshallingTests.cs`.

### 7.3 `.csproj` `Compile Include` regions — exact current shape

**`QuickFiler/QuickFiler.csproj`.** The `Viewers\` region is **not alphabetical**; it is grouped by
area with `DependentUpon`/`SubType` metadata. The `ItemViewer` partial cluster is lines **414-439**:

```
414  <Compile Include="Viewers\ItemViewer.cs">          (SubType UserControl)
417  <Compile Include="Viewers\ItemViewer.DisplayState.cs">   (DependentUpon ItemViewer.cs)
421  <Compile Include="Viewers\ItemViewer.Commands.cs">       (DependentUpon ItemViewer.cs)
425  <Compile Include="Viewers\ItemViewer.Breadcrumb.cs">     (DependentUpon ItemViewer.cs)
429  <Compile Include="Viewers\ItemViewer.FolderSearch.cs">   (DependentUpon ItemViewer.cs)
433  <Compile Include="Viewers\ItemViewer.WebViewThread.cs">  (DependentUpon ItemViewer.cs)
437  <Compile Include="Viewers\ItemViewer.Designer.cs">       (DependentUpon ItemViewer.cs)
440  <Compile Include="Viewers\ItemViewerExpanded.cs">
443  <Compile Include="Viewers\ItemViewerExpanded.Designer.cs">
458  <Compile Include="Viewers\ToolStripMenuItemCb.cs">       (SubType Component)
461  <Compile Include="Viewers\ToolStripMenuItemCb.Designer.cs">
```

**This feature's region is lines 414-439** — the `ItemViewer.*` partial cluster. A new
`ItemViewer.<Area>.cs` partial belongs immediately after `ItemViewer.WebViewThread.cs` (line 436) and
before `ItemViewer.Designer.cs` (line 437), carrying `<DependentUpon>ItemViewer.cs</DependentUpon>`
and `<SubType>UserControl</SubType>` to match its neighbours. **No `.csproj` edit is required at all
if no new production file is added**, which is the case for every recommended fix in §5.

**`QuickFiler.Test/QuickFiler.Test.csproj`.** Also **not alphabetical**. Two relevant regions:

- `Viewers\` region: lines **61-96** (with one `Controllers\QfcItemControllerBreadcrumbDropDownTests.cs`
  interleaved at `:95`). Current tail neighbours: `:93` `BreadcrumbSubfolderActivationTests.cs`,
  `:94` `BreadcrumbDropDownCoverageThresholdTests.cs`, `:96` `FolderBreadcrumbAssetContractTests.cs`.
  `ItemViewerBreadcrumbDropDownContractTests.cs` sits at `:82`.
- `Controllers\QfcItemController.*` region: lines **139-157**, ending
  `:155` `SeamDispatcherTests.cs`, `:156` `SeamCoreTests.cs`, `:157` `SeamFactoryTests.cs`.

**This feature's region.** For a new `Viewers\ToolStripMenuItemCbTests.cs`, append after line 96
(end of the `Viewers\` block). For a new `Controllers\QfcItemController.ThemeMarshallingTests.cs`,
append after line 157 (end of the `QfcItemController.*` block). Both regions are shared with sibling
children; 501's issue.md (`:59-61`) records the same shared-region hazard and 484's spec (`:561-567`)
records that the ordering is by area and insertion history, superseding an earlier "alphabetical"
claim. **Append at the block tail; do not reorder.**

---

## 8. Sibling-collision map

### 8.1 Ownership as recorded on this branch

| File this feature wants to touch | Owner | Source |
|---|---|---|
| `QuickFiler/Viewers/ItemViewer.cs`, `.DisplayState.cs`, `.Commands.cs`, `.FolderSearch.cs`, `.WebViewThread.cs`, `.Designer.cs` | **489 (this)** | `itemviewer-surface-defects-489/issue.md:29-34` |
| `QuickFiler/Viewers/ItemViewerExpanded.cs`, `.Designer.cs` | **489 (this)** | same |
| `QuickFiler/Viewers/ToolStripMenuItemCb.cs`, `.Designer.cs` | **489 (this)** | same |
| `QuickFiler/Viewers/IItemViewer.cs` | **489 (this)** | same |
| `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` | **488** | `breadcrumb-coordinator-hub-defects-501/issue.md:48` |
| `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` | **501** | `breadcrumb-coordinator-hub-defects-501/issue.md:43` |
| `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs`, `BreadcrumbPopupUiOperations.cs`, `BreadcrumbDropDownHost.cs` | **488** | same, `:48` |
| `QuickFiler/Viewers/WebView2Messenger.cs`, `WebView2BreadcrumbHost.cs` | **476** | same, `:47` |
| `QfcItemController.EventWiring.cs`, `.ViewerSetup.cs`, `.MailActions.cs`, `.FocusAndTheme.cs` | **484** | `qfc-item-controller-defects-484/spec.md:323-327` |
| `QfcItemController.Navigation.cs`, `KbdActions.cs` | **444** | `quickfiler-keyboard-action-defects-444/spec.md:704-711` |
| `QfcItemController.FolderHandling.cs` | **446** | `quickfiler-bug-family-446/issue.md:63-64` |
| `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs`, `.InitializationTests.Part2.cs` | **493** | `quickfiler-test-uithread-dispatcher-493/issue.md:76-78` |
| `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` | sibling-owned (493 names it out of its own scope) | same, `:82-85` |
| `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` | **468** — `[TestMethod]` count frozen | `quickfiler-keyboard-action-defects-444/spec.md:868-870` |

Note also `quickfiler-bug-family-446/issue.md:63-64`: "`QfcItemController.FolderHandling.cs` is ours;
every other `QfcItemController` partial belongs to features 484, 444 or 489." By elimination this
feature's `QfcItemController` partials are `Conversation.cs`, `EventHandlers.cs`,
`Initialization.cs` and `QfcItemController.cs`.

### 8.2 Collisions this feature must resolve before planning

Four recommended fixes land in sibling-owned files:

| Defect | Required file | Owner | Disposition options |
|---|---|---|---|
| #486 D3 (`PicturesChanged` wiring) | `QfcItemController.EventWiring.cs` + `EventHandlers.cs` | 484 (EventWiring) / **489** (EventHandlers) | (a) implement the handler in 489-owned `EventHandlers.cs` and add the one wire line to `EventWiring.cs` as an agreed cross-child edit, coordinating the matching `-=` with 484's `UnwireIntentEvents` (16 → 17); (b) defer the wiring and record the defect as cross-feature. **Recommend (a)** — the defect is user-visible and the diff is two lines |
| #489 D2 (`InvokeRequired` guard) | `QfcItemController.FocusAndTheme.cs` | 484 | 484 changes `:168-179` and `:318-324`; `HtmlDarkConverter` is `:289-301`, textually disjoint. **Recommend the cross-child edit**, anchored on the member name |
| #490 D3 (`FocusSubject` caller) | `QfcItemController.MailActions.cs:64` | 484 | signature change to `bool` forces the caller to change. **Recommend** coordinating with 484, or make the caller a `_ = _itemViewer.FocusSubject();` discard so no semantic change is needed |
| #490 D1 second half (`ClearFolderItems` before `SetFolderItems`) | `QfcItemController.FolderHandling.cs:182` | 446 | **Recommend deferring** and recording as an out-of-scope finding; the rename alone closes the contract defect |

`ItemViewer.Breadcrumb.cs` (488) and `BreadcrumbBridgeCoordinator.cs` (501) must **not** be edited.
Every recommended fix above respects that.

---

## 9. Out-of-scope recommendations

### 9.1 #489 Defect 4 — consolidate the three marshalling seams: **DEFER**

**Recommendation: record as an out-of-scope finding in `spec.md`; open a follow-up issue via the
promotion lifecycle.**

Reasons, evidence-based:

1. **The premise has already shifted.** The dominant controller-side seam is now `IUiDispatcher`
   (16 live sites across `Navigation.cs`, `MailActions.cs`, `Conversation.cs`, `FocusAndTheme.cs`),
   not any of the three `ItemViewer` properties. The remaining `ItemViewer`-seam consumers are 4
   (`UiDispatcher`), 5 (`UiSyncContext`) and 0 (`UiScheduler`) — see §4.
2. **Every consumer sits in a sibling-owned file.** `ViewerSetup.cs` and `Initialization.cs` carry 6
   of the 9 live sites; `ViewerSetup.cs` is 484's, and `Initialization.cs` is this feature's only by
   elimination. `FolderHandling.cs` is 446's. `EfcItemController.cs` is 464's.
   `QfcItemController.ViewerSetup.cs:58`'s `await _itemViewer.UiSyncContext;` is load-bearing for the
   #230 pump work (comment at `:30`) and removing it would reopen a resolved issue.
3. **It is a design change, not a defect fix.** CLAUDE.md's Bugfix Workflow requires a minimal,
   targeted fix preceded by a failing regression test. There is no failing behaviour to reproduce for
   "three seams exist"; the acceptance condition would be structural and would gate an architecture
   change behind a bug feature.
4. **Scope restriction.** `itemviewer-surface-defects-489/issue.md:69-71` requires a deeper design
   problem to be recorded in `spec.md` § Out-of-Scope Findings rather than pulled into scope.

**One carve-out worth taking in scope, if the planner wants a cheap win.** `UiScheduler` has **zero**
live consumers (§4.2). Deleting `IItemViewer.cs:37` and `ItemViewer.cs:65-69` (with its `:27`
capture) is a compile-verified, behaviour-neutral removal entirely inside this feature's files, and it
reduces "three seams" to two. It carries a genuine RED (a metadata test asserting
`typeof(IItemViewer).GetProperty("UiScheduler")` is null). **Recommend taking this carve-out and
deferring the rest.** Note `ItemViewerExpanded.cs:63-67`, `EfcViewer.cs:43`, `QfcItemViewer.cs:61`,
`QfcFormViewer.cs:41` declare unrelated `UiScheduler` members that must not be touched.

### 9.2 #490 Defect 5 — group the ten display projections: **DEFER**

**Recommendation: record as an out-of-scope finding; open a follow-up issue.**

Reasons:

1. **The applying site is not this feature's file.** All ten setters are issued in one block at
   `QfcItemController.ViewerSetup.cs:367-393`, inside `AssignControls` — a **484-owned** file, and
   already behind an `InvokeRequired` re-entry guard (`:361-365`) that makes the whole block a single
   UI-thread turn. The "interrupted partway" failure mode the potential describes is therefore not
   reachable through the only production caller.
2. **The fix is an interface redesign.** Grouping ten members into a transactional construct
   (e.g. `void ApplyDisplayState(ItemDisplayState state)`) changes `IItemViewer`, `ItemViewer.DisplayState.cs`,
   `AssignControls`, and every `Mock<IItemViewer>` `VerifySet` in the suite. `ViewerSetupTests.cs`
   (474 lines, 26 spare, 484-owned) carries several of those assertions.
3. **No live failure is demonstrable.** No RED test can be written that fails today, because the
   single caller applies all ten atomically. Per `.claude/rules/plan-acceptance-gates.md` an
   acceptance condition here would be unfalsifiable.

### 9.3 Additional out-of-scope findings discovered during this research

Record each in `spec.md` § Out-of-Scope Findings and promote per the feature-promotion lifecycle.

| # | Finding | Evidence |
|---|---|---|
| O1 | `FlagTaskDialogResult` writes at `QfcItemController.ViewerSetup.cs:375` and `:379` are never read. Every read (`MailActions.cs:177`, `:195`) is preceded on the immediately previous line by a write. The `AssignControls` writes are dead state | §5.5.4 |
| O2 | Because `ButtonSVG : Button` implements `IButtonControl`, a non-`None` `DialogResult` on `BtnFlagTask` gives the button form-closing semantics when hosted on a modally-shown `Form`. Whether `QfcFormViewer` is shown modally was **not** traced | §5.5.4 |
| O3 | `ItemViewer.FolderSearch.cs:79` calls `TxtboxSearch.Invoke(...)` with **no** `InvokeRequired` guard and no handle guard; on a control whose handle is not yet created this throws `InvalidOperationException` | §5.5.2 |
| O4 | `UtilitiesCS/HelperClasses/ThemeHelpers/ThemeControlGroup.cs:212-229` marshals only when `_controls is not null`; the WebView2 branch (`:289-296`) can therefore invoke `_htmlConverter` off the UI thread. Reached from the EFC path (`EfcItemController.cs:1087` etc.). Out of scope (`UtilitiesCS`) | §5.4.2 |
| O5 | `ItemViewer.Designer.cs` (6224 lines) and `ItemViewerExpanded.Designer.cs` (821 lines) exceed the 500-line ceiling. Pre-existing; not created and not remediated by this feature | §1 |
| O6 | `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs` is 531 lines, over the ceiling. Pre-existing, sibling-adjacent | §7.1 |
| O7 | `GetSelectedFolder()` nullable erasure at `ItemViewer.FolderSearch.cs:25`; adding `#nullable enable` cascades into `IItemViewer.cs`, `QfcItemController.EventHandlers.cs` and `QfcItemController.FolderHandling.cs`, three of which this feature does not fully own | §5.5.6 |
| O8 | `QfcItemController.EventWiring.cs:135` contains a raw `await Task.Delay(newDelay);` in production inside `HandleWebViewInitializedAsync`. Already recorded by 446's research (`quickfiler-bug-family-446/research/2026-08-24T09-50-…:873`) as out of scope there too | read at `:121-137` |

---

## 10. Constraints and open items

### 10.1 Binding constraints for the plan

- **500-line ceiling.** No production or test file added by this feature may exceed 500 lines; no
  pre-existing file may grow past 500 or past its Phase 0 baseline. Current counts in §1 and §7.1.
  `QfcCollectionControllerTests.cs` (500, pinned by 468) must receive no test.
  `QfcItemController.FocusAndThemeTests.cs` (497) and `QfcItemController.FolderHandlingTests.cs`
  (498) can absorb nothing.
- **Coverage.** `ItemViewer` is `[ExcludeFromCodeCoverage]` (`:20`). No acceptance condition may
  claim a coverage delta attributable to any `ItemViewer*.cs` change; per-defect proof is a named
  test. **No new `[ExcludeFromCodeCoverage]` attribute should be introduced.** Note the
  85/75 vs 80/90 threshold conflict recorded by 444's spec (lines 842-848) is pre-existing and
  repository-wide; adopt the stricter of each pair as 444 did.
- **Test framework.** MSTest, Moq, FluentAssertions only. Banned in tests: `Thread.Sleep`,
  `Task.Delay`, real wall-clock waits, `DateTime.Now` outside a clock seam, temporary files, live
  `Form` types in the test assembly (`NoLiveFormInTestAssemblyTests.cs:16`).
- **Toolchain.** Analyzer and nullable builds must use `/t:Rebuild`; the nullable build must **not**
  add `/p:Nullable=enable` (CLAUDE.md § C#1.3).
- **Target framework `net48`.** No `init`, `record`, or `record struct`.
- **`.csproj` regions.** See §7.3. No `.csproj` edit is needed for any recommended production fix;
  two test-file additions require one appended `Compile Include` each, at the tail of their block.

### 10.2 Phase 0 verification gates (must run before the first edit)

1. `dotnet tool run csharpier check .` on the untouched worktree — record the baseline. Determine
   whether `*.Designer.cs` is formatted. **Do not edit either `.Designer.cs` until this is answered**
   (§5.3.1).
2. `nuget restore TaskMaster.sln` and `dotnet tool restore` — 484's spec (lines 723-731) records that
   `packages/` and `.dotnet-sdk/` may be absent in a fresh worktree and that a missing restore
   silently weakens the analyzer set.
3. Re-derive every anchor into `QfcItemController.EventWiring.cs`, `.FocusAndTheme.cs`,
   `.ViewerSetup.cs`, `.MailActions.cs` (484) and `.Navigation.cs` (444) against the actual branch
   head. Every line number in this document for those five files is a **pre-upstream** number.
4. Confirm whether 484 and 444 have executed by grepping for `UnwireEvents` and
   `SyncExpandedRegistrations`. Both returned zero matches on 2026-08-25.

### 10.3 Open items — not verified in this session

| # | Item | Why it matters |
|---|---|---|
| U1 | Whether CSharpier skips `*.Designer.cs` by filename | Gates the #487 fix (§5.3.1) |
| U2 | Whether `QfcFormViewer` is ever shown modally | Determines whether O2 is a live hazard or inert |
| U3 | Every read of `QfcItemController.SelectedFolder` | Determines whether the O7 nullable erasure can produce a live `NullReferenceException` |
| U4 | Whether `AssignFolderComboBox()` can run twice within one viewer lifetime without an intervening `Cleanup()` | Determines the real exposure of #490 D1 (§5.5.1) |
| U5 | The intended focus target of `FocusSubject()` | Not determinable from source; H1 deliberately avoids guessing (§5.5.3) |
| U6 | Whether `ThemeControlGroup._controls` is null for the WebView2 group | Determines whether O4 is live on the EFC path |

---

## 11. Recommended scope summary for `prd-feature` / `atomic-planner`

**In scope, with a deterministic RED test:**

- #486 D1 — delete the `ItemViewerExpanded` check-image handler, its four designer wirings and its
  four constructor calls; `ToolStripMenuItemCb.Checked`'s setter becomes the sole owner.
  New test file `QuickFiler.Test/Viewers/ToolStripMenuItemCbTests.cs`.
- #486 D2 — delete the three dead `ItemViewer.cs` members (`:171-175`, `:177-187`, `:205`).
  Metadata test.
- #486 D3 — add `CbxPictures_CheckedChanged` (489-owned `EventHandlers.cs`) plus one wire line in
  484-owned `EventWiring.cs`; coordinate the matching `-=` with 484. `VerifyAdd` test.
- #487 D1 — delete both `L0v2h2_WebView2_ParentChanged` handlers and their two designer wirings
  (gated on U1). Metadata test.
- #487 D2 — subsumed by #486 D1 / #487 D1 deletions. Metadata test.
- #489 D2 — add the `InvokeRequired` re-entry guard to `HtmlDarkConverter` (484-owned file;
  coordinate). `Mock<IItemViewer>` test with `SetupGet(v => v.InvokeRequired)`.
- #490 D3 — change `FocusSubject()` to return `bool`; discard or observe at the single caller.
  Compile-time RED plus a `Returns(false)` behavioural test.
- #490 D4 — remove the redundant read-back at `MailActions.cs:177`/`:195` (484-owned; coordinate).
  `VerifyGet(..., Times.Never())` test.
- **Carve-out from #489 D4** — delete the zero-consumer `UiScheduler` seam from `IItemViewer.cs:37`
  and `ItemViewer.cs:27`, `:65-69`. Metadata test.

**In scope, carried by a `fail-before-exception` dossier rather than a RED test:**

- #489 D3 — document the set-then-sort atomicity on `IItemViewer.cs:119-120` (F1).
- #490 D2 — make `FocusSearch()` a bare forward and document the one threading contract for both
  focus members.

**In scope as a rename, with a metadata RED test:**

- #490 D1 — rename `SetFolderItems` → `AddFolderItems` across `IItemViewer.cs`,
  `ItemViewer.FolderSearch.cs` and the three call sites; defer the `FolderHandling.cs:182` clear
  (446-owned).

**Closed by citation, no work item:**

- #489 D1 — already fixed by the `IUiDispatcher` seam; standing regression is
  `QfcItemController.SeamDispatcherTests.cs:99`.

**Deferred to a follow-up issue (out-of-scope findings in `spec.md`):**

- #489 D4 (beyond the `UiScheduler` carve-out), #490 D5, and O1 through O8.
