# itemviewer-surface-defects (Spec)

- **Issue:** #489
- **Parent (optional):** epic `quickfiler-bug-family` (#446), wave 2
- **Owner:** drmoisan
- **Last Updated:** 2026-08-25
- **Status:** Draft
- **Version:** 1.0
- **Work Mode:** `full-bug`

> **Acceptance-criteria authority.** This feature's work mode is `full-bug`. Per the
> `acceptance-criteria-tracking` skill, `spec.md` is the **sole** acceptance-criteria source.
> `user-story.md` does not exist for this feature and must not be created. `issue.md`
> § Acceptance Criteria is a pointer to this document, not a second source.

> **Fact base.** Every `file:line` citation in this document is taken from
> `research/2026-08-25T02-15-itemviewer-surface-defects-research.md`, which opened each cited line on
> this branch on 2026-08-25. The four promoted potential documents were captured on 2026-08-07 and
> **their citations are superseded wherever they disagree**. Three of their premises no longer hold;
> see § Superseded Premises.

---

## Context

Four defect issues were filed on 2026-08-07 during preparation research for epic #136 child F14
(issue #456) and deferred out of that child because its non-functional requirement prohibited
behaviour change to observable QuickFiler flows. All four alter observable behaviour on a UI-thread,
menu, or display-contract path in the `ItemViewer` / `ItemViewerExpanded` twin family and its
`IItemViewer` contract.

| Issue | Title | Primary files |
|---|---|---|
| #486 | `itemviewer-move-option-menu-defects` | `ToolStripMenuItemCb.cs`, `ItemViewer.cs`, `ItemViewerExpanded.cs` |
| #487 | `itemviewer-parentchanged-console-and-cast` | `ItemViewer.cs`, `ItemViewerExpanded.cs` (+ their `.Designer.cs`) |
| #489 | `itemviewer-ui-thread-marshalling-divergence` (primary) | `ItemViewer.cs`, `ItemViewer.WebViewThread.cs`, `IItemViewer.cs` |
| #490 | `itemviewer-display-and-folder-contract-defects` | `ItemViewer.FolderSearch.cs`, `ItemViewer.DisplayState.cs`, `ItemViewer.Commands.cs` |

**Observed environment.** `net48`, VSTO/WinForms host, `QuickFiler` project. No live Outlook process
is required to reproduce any of the four by inspection; two of the four are user-visible at runtime.

**Customer impact and severity.**

- **#486 D1 — Medium, user-visible.** The four move-option menu items on the **`ItemViewerExpanded`
  twin only** never display a check mark. The primary QuickFiler `ItemViewer` is unaffected (see
  § Superseded Premises).
- **#486 D3 — Medium, user-visible and silent.** Toggling "Save Pictures" in the QuickFiler move-option
  menu is discarded; the filer uses the settings value captured at viewer-assign time. The menu shows
  the correct *initial* state, which is what makes the failure silent.
- **#487 — Low.** Production `Console.WriteLine` and an unguarded downcast in designer-wired handlers.
- **#489 D2 — Medium-High.** A cross-thread `NavigateToString` is reachable on the theme path when the
  target control has no created handle.
- **#489 D3, #489 D4, #490 D1–D5 — Low to Medium, contract defects.** Names that do not match
  behaviour, an unenforced ordering pair, a discarded `bool`, a redundant control-property read-back,
  and three concurrent marshalling seams.

**First observed.** 2026-08-07 (static analysis during #456 preparation). Re-verified 2026-08-25.

---

## Superseded Premises (read before planning)

Three defect premises stated in the 2026-08-07 potential documents **no longer hold on this branch**.
Any plan or review that reasons from the potential documents alone will be wrong on these three.

### S1 — #489 Defect 1 is already fixed. It carries no work item.

The potential asserts that `QfcItemController._uiDispatcher` "originates from
`System.Windows.Threading.Dispatcher.CurrentDispatcher` captured in the `ItemViewer` constructor",
giving `ShowMoveOptionsMenu` "a different and weaker delivery guarantee than every other UI operation
on the same control". That is false on this branch.

| Potential's claim | Source reality (verified 2026-08-25) |
|---|---|
| `_uiDispatcher` is the `ItemViewer`-captured WPF `Dispatcher` | `QuickFiler/Controllers/QfcItemController.cs:66` — `private UtilitiesCS.Threading.IUiDispatcher _uiDispatcher;` |
| it comes from `ItemViewer.cs:28` | `QuickFiler/Controllers/QfcItemController.Initialization.cs:383` — `_uiDispatcher ??= new UtilitiesCS.Threading.WpfUiDispatcher();` |
| `MenuDropDown` is anomalous | `_uiDispatcher` is the **dominant** pattern: ten sites in `Navigation.cs` alone (`:43`, `:59`, `:83`, `:90`, `:96`, `:102`, `:116`, `:128`, `:197`, `:202`), plus `MailActions.cs:51`, `:186`, `:214`; `Conversation.cs:163`, `:207`; `FocusAndTheme.cs:270` |

`IUiDispatcher` is an injectable seam introduced by the #230 work. `MenuDropDown()` is already covered
by a passing deterministic test:
`QuickFiler.Test/Controllers/QfcItemController.SeamDispatcherTests.cs:99-107`
(`MenuDropDown_ShowsMoveOptionsMenuThroughDispatcher`), which asserts
`dispatcher.Verify(d => d.InvokeAsync(It.IsAny<Action>()), Times.Once())` and
`viewer.Verify(v => v.ShowMoveOptionsMenu(), Times.Once())`.

**Disposition: closed by citation, no work item, no new test.** The CLAUDE.md Bugfix Workflow requires
a failing regression test before a fix. There is no fix here, so a new test would have nothing to
assert against, and `.claude/rules/plan-acceptance-gates.md` would treat a synthesised failing
condition as unfalsifiable. The standing regression is the test named above.

The residual concern the potential raised — that `WpfUiDispatcher` resolves a WPF `Dispatcher` rather
than the WinForms synchronization context — is a different and much broader question that now applies
uniformly to sixteen call sites across four files. It is not a `ShowMoveOptionsMenu` defect and is
recorded in § Out-of-Scope Findings.

### S2 — #490 Defect 1's stated mask was deleted by issue #438.

The potential says the append behaviour is "masked in production only because the caller issues a
preceding `ClearFolderItems()`". **There is now zero production caller of `ClearFolderItems()`.**
Issue #438 replaced the per-keystroke `ClearFolderItems + SetFolderItems + SetFolderSelectedIndex +
SetFolderDroppedDown` composition with the single `PresentFolderSearchResults` intent
(`IItemViewer.cs:92-99`, implemented `ItemViewer.FolderSearch.cs:38-39`, backed by
`BreadcrumbBridgeCoordinator.Search.cs:47-77`). The former composition survives only in three
comments: `QfcItemController.EventHandlers.cs:165`, `IItemViewer.cs:93`,
`BreadcrumbBridgeCoordinator.Search.cs:26`. `ClearFolderItems()` itself remains declared
(`ItemViewer.FolderSearch.cs:41`, `IItemViewer.cs:101`) with no caller.

The append behaviour is real and unchanged: `ItemViewer.FolderSearch.cs:20`
`public void SetFolderItems(string[] items) => BreadcrumbCoordinator?.AddItems(items);`, and
`BreadcrumbBridgeCoordinator.cs:130` documents `AddItems` as "Appends Path B plain rows verbatim and
re-renders (legacy AddRange semantics)". **The real exposure is now
`QfcItemController.FolderHandling.cs:182`**, `_itemViewer.SetFolderItems(_folderHandler.FolderArray);`
inside `AssignFolderComboBox()` (`:161-208`), which has no preceding clear. The other two call sites
(`MailActions.cs:206`, `:218`) are idempotent by construction, guarded by
`if (!_itemViewer.FolderContains("Trash to Delete"))` at `:204` / `:216`.

The residual mask is `Cleanup()`: `QfcItemController.ViewerSetup.cs:400`
`(_itemViewer as ItemViewer)?.ResetBreadcrumb();` → `ItemViewer.Breadcrumb.cs:258` →
`BreadcrumbItemViewerLifecycleCoordinator.cs:195` → `BreadcrumbBridgeCoordinator.cs:160`
`public void Reset() => Clear();`. Pooled viewer *reuse* is therefore safe; a second population
*within one viewer lifetime* is not.

### S3 — #486 Defect 1 affects only the `ItemViewerExpanded` twin.

The potential's severity claim, "the four move-option menu items never display a check mark", is true
for `ItemViewerExpanded` only. `ItemViewer.Designer.cs` (6224 lines) contains **exactly one**
`+= new System` statement in the whole file, at `:256`, and it is the `ParentChanged` wiring — not a
`CheckedChanged` wiring. `EfcViewer` is also unaffected: its equivalent handler and constructor calls
are commented out at `EfcViewer.cs:109-140`.

`ItemViewerExpanded` is production-live: it is `_qfcItemViewerExpandedTemplate` in
`QfcFormViewer.Designer.cs:42` and `:256`, consumed at `QfcFormViewer.cs:189`, `:202-210`. The defect
is real; only its blast radius is narrower than recorded.

---

## Repro & Evidence

All repro chains below are static and were verified by reading the cited lines on 2026-08-25. None
requires a live Outlook process. Frequency is **deterministic** for every item unless noted.

### Issue #486 — move-option menu

#### D1 — the check image is cleared immediately after being set (`ItemViewerExpanded` only)

Verified end-to-end chain:

1. `ToolStripMenuItemCb.Designer.cs:34` — `this.Click += new System.EventHandler(this.ToolStripMenuItemCb_Click);`, run from the constructor at `ToolStripMenuItemCb.cs:15`.
2. `ItemViewerExpanded.Designer.cs:165`/`:175`/`:184`/`:193` set `CheckOnClick = true`. The shadowed setter (`ToolStripMenuItemCb.cs:63-79`) does `base.Click -= …; base.Click += …;`, so exactly one subscription survives. **There is no double-toggle.**
3. A click runs `ToolStripMenuItemCb_Click` (`:53-56`) → `Checked = !Checked`.
4. The shadowed setter (`ToolStripMenuItemCb.cs:32-50`) writes `_checked`, sets `base.Image` correctly (`:40` / `:43`), raises the **shadowed** `CheckedChanged` (`:47`), and calls `base.Invalidate()` (`:48`). **`base.Checked` is never written anywhere in the file.**
5. `ItemViewerExpanded.Designer.cs:171`, `:180`, `:189`, `:198` subscribed that shadowed event to `ItemViewerExpanded.MenuItem_CheckedChanged(object, EventArgs)` (`ItemViewerExpanded.cs:163-167`), which casts to `ToolStripMenuItem` and calls the typed overload (`:169-179`).
6. The typed overload reads `menuItem.Checked`. Because the parameter is typed `ToolStripMenuItem`, this binds to `ToolStripMenuItem.Checked`, which is **always false**, so it takes the `else` branch at `:177`, `menuItem.Image = null;`.

**Expected:** a checked move-option item renders the `CheckBoxChecked` image.
**Actual:** the image the setter just applied is cleared on the same turn.

The four constructor calls at `ItemViewerExpanded.cs:24-27` invoke the same typed overload during
construction.

#### D2 — divergent menu behaviour between the twins

`ItemViewer.cs:171-175` (`MenuItem_CheckedChanged(object sender, EventArgs e)`), `:177-187` (the typed
overload) and `:205` (`private void MoveOptionsMenu_Click(object sender, EventArgs e) { }`) exist with
**no caller and no designer wiring**. Solution-wide grep for `MenuItem_CheckedChanged` and
`MoveOptionsMenu_Click` finds no `ItemViewer` caller, and `ItemViewer.Designer.cs` wires nothing but
`:256`. The two twins therefore carry the same source shape with opposite runtime behaviour, which is
the divergence.

#### D3 — `PicturesChanged` has no production subscriber

- Solution-wide grep for `PicturesChanged` returns exactly two production hits, both declarations: `ItemViewer.Commands.cs:85` (the event, forwarding to `SavePicturesMenuItem.CheckedChanged`) and `IItemViewer.cs:71`. **There is no `+=` anywhere.**
- `WireIntentEvents()` (`QfcItemController.EventWiring.cs:66-94`) wires 16 intent events. `:68` `ConversationModeChanged`, `:92` `EmailCopyChanged`, `:93` `AttachmentsChanged` — no `PicturesChanged`.
- There is **no `CbxPictures_CheckedChanged` handler** on `QfcItemController`. The sibling handlers are `CbxConversation_CheckedChanged` (`EventHandlers.cs:27`), `CbxEmailCopy_CheckedChanged` (`:208`), `CbxAttachments_CheckedChanged` (`:218`).
- `_optionsPictures` (`QfcItemController.cs:57`) is written exactly once, at `ViewerSetup.cs:392` from `_globals.QfSettings.SavePictures`, and read exactly once, at `MailActions.cs:102` (`SavePictures = _optionsPictures,`). It is never refreshed from the menu.
- `PicturesChecked` **is** written at `ViewerSetup.cs:393`, so the menu shows the right initial state.

**Expected:** toggling "Save Pictures" changes what the filer does.
**Actual:** the toggle is discarded silently.

**Correction to the potential's supporting citation.** The potential cites `EfcFormController.cs:389`.
The four EFC wirings are now at `EfcFormController.cs:385-388`, with
`SavePicturesMenuItem.CheckedChanged += SavePictures_CheckedChanged;` at **`:387`**, and handlers at
`:534`, `:539`, `:544`, `:549`. That code wires the raw `_formViewer.SavePicturesMenuItem.CheckedChanged`
on `EfcViewer`, **not** `IItemViewer.PicturesChanged`. The comparison still supports the conclusion —
the EFC path handles all four, the QFC path handles three — but the mechanism differs.
`EfcFormController.cs` changed in PR #605; these line numbers were re-read on 2026-08-25.

### Issue #487 — `Console.WriteLine` and unguarded cast

#### D1 — production `Console.WriteLine`

- `ItemViewer.cs:166` `private void L0v2h2_WebView2_ParentChanged(object sender, EventArgs e)`; `:168` `Console.WriteLine("Parent Changed");` — the entire body. Wired at `ItemViewer.Designer.cs:256`.
- `ItemViewerExpanded.cs:158` the same handler; `:160` the same statement. Wired at `ItemViewerExpanded.Designer.cs:274`.

The literal asserted against in § Acceptance Criteria is `Parent Changed` (exactly that two-word
token, on one line, in both files today).

#### D2 — unguarded downcast in an event handler

`ItemViewer.cs:173` and `ItemViewerExpanded.cs:165` both contain
`var menuItem = (ToolStripMenuItem)sender;` with no type test. Severity is low today because all four
current wirings pass a `ToolStripMenuItemCb` (`ItemViewerExpanded.Designer.cs:36-39` construct all
four as `new QuickFiler.Viewers.ToolStripMenuItemCb()`; fields declared `ToolStripMenuItemCb` at
`:811-814`) and the `ItemViewer` member is dead.

### Issue #489 — UI-thread marshalling

#### D1 — already fixed. See § Superseded Premises S1.

#### D2 — `NavigateToString` unguarded on the theme path

- `QfcItemController.FocusAndTheme.cs:289` `public void HtmlDarkConverter(Enums.ToggleState desiredState)`; `:291` `if (_isWebViewerInitialized)`; `:293` `_itemViewer.NavigateToString(ItemHelper.ToggleDark(desiredState));` — unguarded.
- `ItemViewer.WebViewThread.cs:15` `public void NavigateToString(string html) => L0v2h2_WebView2.NavigateToString(html);` — performs no marshalling of its own.
- Every other forwarder on this surface is guarded: `EventWiring.cs:139-146` (the guarded `NavigateToString` pair), `Conversation.cs:181-185`, `:224-228`, `FolderHandling.cs:139-146`, `:164-168`, `ViewerSetup.cs:361-365`.

**Exposure, traced.** `HtmlDarkConverter` is never called directly. It is passed as an
`Action<Enums.ToggleState>` into `QfcThemeHelper.SetupThemes` at
`QfcItemController.Initialization.cs:177`, `:213`, `:272`, `:305` (`QfcThemeHelper.cs:39` parameter,
forwarded at `:53`, `:91`, `:347` to `Theme.HtmlConverter`), and invoked from
`UtilitiesCS/HelperClasses/ThemeHelpers/Theme.Rendering.cs:126` (`HtmlConverter(HtmlDark);`) and
`ThemeControlGroup.cs:294`. On the QuickFiler path only the first is reachable.
`Theme.SetQfcTheme(bool)` (`Theme.cs:427-445`) marshals in all three branches: `async` →
`_uiDispatcher.InvokeAsync` (`:431`); else `_lblItemNumber.InvokeRequired` → `Invoke` (`:433-436`);
else direct.

**The residual hole.** `Control.InvokeRequired` returns `false` when the control has no created
handle, even off the UI thread. `SetThemeDark(async: false)` / `SetThemeLight(async: false)` are
reachable from `QfcCollectionController.cs:818` and `:822`, and `SetQfcTheme(async: false)` from
`FocusAndTheme.cs:64` and `:120`. If any of those runs on a non-UI thread against a viewer whose
`_lblItemNumber` handle is not yet created, `SetQfcTheme()` executes inline and `NavigateToString` is
called cross-thread. Frequency: **data- and timing-dependent**, not always.

#### D3 — `SetConversationItems` / `SortConversationByDate` atomicity

Both are separate `IItemViewer` members — `IItemViewer.cs:119`
`void SetConversationItems(System.Collections.IList items);` and `:120`
`void SortConversationByDate(SortOrder order);` — separately implemented at
`ItemViewer.WebViewThread.cs:23` and `:25`. Neither the interface, nor the implementation, nor any XML
doc records that calling either alone leaves the list in source order.

The potential cited `IItemViewer.cs:109-110`; those lines are now
`event System.EventHandler SearchTextChanged;` / `event KeyEventHandler SearchKeyDown;`. **Use
`:119-120`.**

Sole production caller: `QfcItemController.Conversation.cs:231` + `:232`, back-to-back inside
`SetTopicThread` (`:221-233`), after the `InvokeRequired` re-entry guard at `:224-228`. Test callers:
`QfcItemController.ConversationTests.cs:261-262`, `:279`, `:341`. No other production implementation
of `IItemViewer` exists.

#### D4 — three concurrent marshalling contracts on one control

All three seams exist on `ItemViewer` and on `IItemViewer`:

| Seam | Declaration | Capture | Interface | Live production consumers |
|---|---|---|---|---|
| `UiSyncContext` | `ItemViewer.cs:59-63` | `:26` | `IItemViewer.cs:38` | **5** — `ViewerSetup.cs:58`, `:264`, `:269`, `:280`, `:285` |
| `UiScheduler` | `ItemViewer.cs:65-69` | `:27` | `IItemViewer.cs:37` | **0** |
| `UiDispatcher` | `ItemViewer.cs:71-75` | `:28` (`Dispatcher.CurrentDispatcher`) | `IItemViewer.cs:36` | **4** — `Initialization.cs:192`, `ViewerSetup.cs:353`, `FolderHandling.cs:158`, `EfcItemController.cs:913`/`:922` |

The only call-site-shaped mention of `IItemViewer.UiScheduler`,
`QfcItemController.ViewerSetup.cs:346`, is commented out. Other types (`EfcViewer.cs:43`,
`QfcItemViewer.cs:61`, `QfcFormViewer.cs:41`, `ItemViewerExpanded.cs:63-67`, `QfcHomeController.cs:438`,
`IQfcFormViewer.cs:16`) declare their **own unrelated** `UiScheduler` members.

### Issue #490 — display and folder contract

#### D1 — `SetFolderItems` appends rather than sets. See § Superseded Premises S2.

#### D2 — incompatible threading discipline on `FocusSearch` / `FocusSubject`

- `ItemViewer.FolderSearch.cs:79` `public void FocusSearch() => TxtboxSearch.Invoke(new Action(() => TxtboxSearch.Focus()));`
- `ItemViewer.DisplayState.cs:79` `public void FocusSubject() => LblSubject.Focus();`

One type, two members, opposite contracts, and neither is documented. `FocusSearch()` is called at
`QfcItemController.Navigation.cs:54` (inside `JumpToSearchTextbox`, `:51-55`) with **no** surrounding
marshal, and is asserted by `QfcItemController.NavigationTests.cs:198`. `FocusSubject()` is called at
`QfcItemController.MailActions.cs:64`, inside the `RightKeyActions["&Expand"]` lambda (`:60-67`).

Additional finding: `TxtboxSearch.Invoke(...)` with no `InvokeRequired` guard and no handle guard
throws `InvalidOperationException: Invoke or BeginInvoke cannot be called on a control until the
window handle has been created` whenever the handle does not yet exist — a latent defect independent
of the deadlock concern the potential names.

#### D3 — `FocusSubject()` targets a `Label` and discards the result

`LblSubject` is declared `public System.Windows.Forms.Label LblSubject` at `ItemViewer.cs:244-248`.
`Label` sets `ControlStyles.Selectable = false`, so `Control.Focus()` returns `false` and the call is
a no-op. `IItemViewer.cs:54` declares `void FocusSubject();`, and the expression-bodied member
discards the `bool`. The failure is therefore unobservable. Sole caller:
`QfcItemController.MailActions.cs:64`, immediately before `this.EnumerateConversation()`.

The intended focus target is **not determinable from the source** and must not be guessed
(research open item U5).

#### D4 — `FlagTaskDialogResult` used as cross-call scratch state

`ItemViewer.Commands.cs:97-101`
`public DialogResult FlagTaskDialogResult { get => BtnFlagTask.DialogResult; set => BtnFlagTask.DialogResult = value; }`.
`BtnFlagTask` is `ButtonSVG` (`ItemViewer.cs:354-358`) and `SVGControl/ButtonSVG.cs:13` is
`public partial class ButtonSVG : Button`, so the backing property is
`System.Windows.Forms.Button.DialogResult` (`IButtonControl`).

**The read-back is redundant.** Read sites, exhaustive: `MailActions.cs:177` and `:195`. Both are
`if (_itemViewer.FlagTaskDialogResult == DialogResult.OK)` on the line immediately after
`_itemViewer.FlagTaskDialogResult = flagTask.Run(modal: true);` (`:176`, `:194`). No value written
elsewhere is ever read back. The writes at `ViewerSetup.cs:375` / `:379` are overwritten before any
read.

#### D5 — ten ungrouped display projections

`IItemViewer.cs:43-52` declares exactly ten independently settable projections: `SenderText`,
`SubjectText`, `BodyText`, `TriageText`, `SentOnText`, `ActionableText`, `ItemNumberText`,
`FolderText`, `ConversationCountText`, `ConversationCountBackColor`. Implemented
`ItemViewer.DisplayState.cs:13-71`; applied in one block at `QfcItemController.ViewerSetup.cs:367-393`
inside `AssignControls` (`:358-394`), behind an `InvokeRequired` re-entry guard at `:361-365`.

---

## Scope & Non-Goals

### In scope, with a deterministic RED test

| Item | Change | Files |
|---|---|---|
| **#486 D1** | Delete `ItemViewerExpanded.MenuItem_CheckedChanged(object, EventArgs)` (`:163-167`) and the typed overload (`:169-179`); delete the four constructor calls (`:24-27`); delete the four designer wirings (`ItemViewerExpanded.Designer.cs:171`, `:180`, `:189`, `:198`). `ToolStripMenuItemCb.Checked`'s setter becomes the sole owner of the check image. | `ItemViewerExpanded.cs`, `ItemViewerExpanded.Designer.cs` |
| **#486 D2** | Delete the three dead `ItemViewer.cs` members: `:171-175`, `:177-187`, `:205`. | `ItemViewer.cs` |
| **#486 D3** | Add `CbxPictures_CheckedChanged` to `QfcItemController.EventHandlers.cs` (`_optionsPictures = _itemViewer.PicturesChecked;`) and one wire line `_itemViewer.PicturesChanged += this.CbxPictures_CheckedChanged;` in `WireIntentEvents()`. | `EventHandlers.cs` (489-owned), `EventWiring.cs` (484-owned — cross-child edit) |
| **#487 D1** | Delete both `L0v2h2_WebView2_ParentChanged` handlers (`ItemViewer.cs:166-169`, `ItemViewerExpanded.cs:158-161`) and their two designer wirings (`ItemViewer.Designer.cs:256`, `ItemViewerExpanded.Designer.cs:274`). **Gated on research open item U1.** | four files |
| **#487 D2** | Subsumed by the #486 D1 and #487 D1 deletions — both unguarded casts live inside deleted members. | — |
| **#489 D2** | Add the `InvokeRequired` re-entry guard to `HtmlDarkConverter`, mirroring `EventWiring.cs:139-146` verbatim. | `FocusAndTheme.cs` (484-owned — cross-child edit) |
| **#490 D3** | Change `IItemViewer.FocusSubject()` to `bool FocusSubject();`; implement as `public bool FocusSubject() => LblSubject.Focus();`; make the sole caller a `_ = _itemViewer.FocusSubject();` discard so no control flow changes. | `IItemViewer.cs:54`, `ItemViewer.DisplayState.cs:79`, `MailActions.cs:64` (484-owned) |
| **#490 D4** | Remove the redundant read-back: hold `flagTask.Run(modal: true)` in a local, assign the property once, and branch on the local. | `MailActions.cs:176-177`, `:194-195` (484-owned) |
| **#489 D4 carve-out** | Delete the zero-consumer `UiScheduler` seam: `IItemViewer.cs:37`, `ItemViewer.cs:27` (the capture) and `:65-69` (the property). | `IItemViewer.cs`, `ItemViewer.cs` |

### In scope, carried by a `fail-before-exception` dossier rather than a RED test

| Item | Change | Why no RED test |
|---|---|---|
| **#489 D3** | Add XML documentation to `IItemViewer.cs:119-120` stating that `SetConversationItems` must be followed by `SortConversationByDate` within the same UI-thread turn, and that calling either alone leaves the list in source order. | Documentation is not observable. The existing `ConversationTests.cs:249` test already pins the ordering-relevant pair. A synthesised failing condition here would be unfalsifiable under `.claude/rules/plan-acceptance-gates.md`. |
| **#490 D2** | Change `FocusSearch()` to the bare forward `public void FocusSearch() => TxtboxSearch.Focus();` and document **one** threading contract for both focus members on `IItemViewer` — *the viewer forwards; the controller marshals*. | "Does not marshal" is not observable through `Mock<IItemViewer>`; the defect is inside the concrete, unconstructible viewer. An IL-shape assertion would be brittle. |

The chosen contract matches every other `ItemViewer` intent member (all bare forwards) and the
controller-side guard convention. The alternative — marshal inside the viewer — would contradict the
whole intent surface and re-import the no-handle `Invoke` throw into `FocusSubject`.

### In scope as a rename, with a metadata RED test

**#490 D1** — rename `IItemViewer.SetFolderItems(string[])` → `AddFolderItems(string[])`, matching the
second arm of the potential's own acceptance wording ("or is renamed to match its behavior"). The
`FolderHandling.cs:182` clear-insertion half is **deferred** (446-owned; see § Out-of-Scope Findings).

The rename's blast radius is materially larger than research §11's phrase "the three call sites"
implies. The exhaustive set, verified 2026-08-25:

| Kind | Sites |
|---|---|
| Declaration | `IItemViewer.cs:80` (plus the comment at `:85`) |
| Implementation | `ItemViewer.FolderSearch.cs:20` |
| Production call sites | `QfcItemController.FolderHandling.cs:182` (**446-owned**), `QfcItemController.MailActions.cs:206`, `:218` (**484-owned**) |
| Test call sites | `QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs:261`; `QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs:170`, `:248`, `:341` (**501-adjacent**); `QfcItemController.SeamDispatcherTests.cs:193`; `QfcItemController.MailActionsTests.cs:66`, `:87`; `QfcItemController.FolderSuggestionsTests.cs:131`, `:159`, `:183`; `QfcItemController.FolderHandlingTests.cs:349`, `:407`, `:433`, `:476` |
| Comments only | `IItemViewer.cs:93`, `QfcItemController.EventHandlers.cs:165`, `BreadcrumbBridgeCoordinator.Search.cs:26` (**501-owned — do not edit**), `QfcItemController.FolderSuggestionsTests.cs:16`, `:130`, `QfcItemController.EventHandlersTests.cs:315` |

**Hard constraint on the rename.** Rename **member invocations only**. Do **not** rename the two
existing test method names `AssignFolderComboBox_RetainsSetFolderItemsAndIndexOneSelection`
(`FolderSuggestionsTests.cs:111`) and
`MarkItemForDeletion_StillAppendsTrashToDeleteViaSetFolderItems`
(`FolderSuggestionsTests.cs:169`). Renaming a test method changes its node ID and would invalidate any
sibling acceptance condition that names it. Leaving them keeps every touched test file line-neutral
and node-ID-stable. Do not edit the `BreadcrumbBridgeCoordinator.Search.cs:26` comment — that file is
501-owned and the stale name inside a historical comment is harmless.

`ClearFolderItems()` is left declared and uncalled; deleting it is not in scope.

### Closed by citation, no work item

**#489 D1** — see § Superseded Premises S1.

### Out of scope / non-goals

- #489 D4 beyond the `UiScheduler` carve-out; #490 D5; research findings O1 through O8. All are listed with evidence pointers in § Out-of-Scope Findings.
- The two `.Designer.cs` files' pre-existing 500-line excess (6224 and 821 lines). Not created by this feature and explicitly not remediated by it.
- Any behaviour change to `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` (**488**) or `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` (**501**). Neither file may be edited.
- Any change to `UtilitiesCS` (`Theme.cs`, `ThemeControlGroup.cs`, `WpfUiDispatcher.cs`).
- Nullable adoption (`#nullable enable`) on any file. See O7.

---

## Upstream contract reconciliation

Both upstream contract tables were read on this branch and **both were found CORRECT and CURRENT**:

- `docs/features/active/qfc-item-controller-defects-484/spec.md` § "Upstream contract (exhaustive) — required by features 464 and 489" (line 329), with its coverage carve-out set at line 704 onward. That carve-out set is deliberately **three** items — (a) the capture-field assignments and lambda adapter inside `[ExcludeFromCodeCoverage] InitializeWebViewAsync`, (b) `DetachWebResourceRequestedHandler`, (c) the default `MoveFailureNotifier` delegate — and this feature neither extends nor contradicts it.
- `docs/features/active/quickfiler-keyboard-action-defects-444/spec.md` § "Upstream contract (exhaustive) — required by features 464 and 489" (line 704).

**There is no table-versus-source disagreement.** 484's statement that `NotifyMoveFailure` marshals
through `_uiDispatcher.Invoke` is consistent with the source: `_uiDispatcher` is
`UtilitiesCS.Threading.IUiDispatcher` (`QfcItemController.cs:66`). **The disagreement is between the
#489 potential document and the source**, and it is recorded in § Superseded Premises S1.

**Neither upstream's production code is on this branch yet.** Solution-wide grep across `QuickFiler/`
for `UnwireEvents`, `UnwireControlTreeEvents`, `UnwireIntentEvents`, `MoveFailureNotifier`,
`TryResolveCidResource`, `DetachWebResourceRequestedHandler` (484) and `SyncExpandedRegistrations`
(444) returned **zero matches** on 2026-08-25. Both siblings are *prepared*, not *executed*.

**Consequence, binding on the plan.** Every line number in either upstream table, and every line
number in this document for a 484- or 444-owned file, is a **pre-change** number.

- **Every acceptance condition must anchor on a member name, never on a line number.**
- Phase 0 must re-derive every anchor into `QfcItemController.EventWiring.cs`, `.FocusAndTheme.cs`, `.ViewerSetup.cs`, `.MailActions.cs` (484) and `.Navigation.cs` (444) against the actual branch head, and record the result in `evidence/baseline/`.

Two specific reconciliations:

1. **`MenuDropDown` is not moved by either upstream.** 444's exhaustive `Navigation.cs` table lists one ADDED member (`SyncExpandedRegistrations`, `private`), zero REMOVED, and two CHANGED (`ToggleExpansion`, `ToggleExpansionAsync`); `MenuDropDown` is in the UNCHANGED list (444 spec line 776). 484 does not touch `Navigation.cs` at all (484 spec lines 323-327). Its **body** is unchanged, but 444 adds a member to the file, so `:83` is not a stable anchor. Anchor on `public async Task MenuDropDown()`.
2. **484 modifies no interface.** 484 spec line 367: "No member is removed. No public member is added. No interface is modified. … `IQfcItemController`, `IItemControler`, and `IItemViewer` are untouched." This feature's three `IItemViewer` changes (`UiScheduler` removal, `FocusSubject` return type, `SetFolderItems` rename) therefore cannot collide with 484's diff, but they **do** change a surface 484's tests mock. Phase 0 must confirm 484 has not landed before assuming that.

---

## Sibling-collision resolution

`ItemViewer.Breadcrumb.cs` (**488**) and `BreadcrumbBridgeCoordinator.cs` (**501**) **must NOT be
edited.** Every disposition below respects that.

### Files this feature owns outright

`QuickFiler/Viewers/ItemViewer.cs`, `.DisplayState.cs`, `.Commands.cs`, `.FolderSearch.cs`,
`.WebViewThread.cs`, `.Designer.cs`; `ItemViewerExpanded.cs` and `.Designer.cs`;
`ToolStripMenuItemCb.cs` and `.Designer.cs`; `IItemViewer.cs`. By elimination
(`quickfiler-bug-family-446/issue.md:63-64`) this feature's `QfcItemController` partials are
`Conversation.cs`, `EventHandlers.cs`, `Initialization.cs` and `QfcItemController.cs`.

### Cross-child edits and their agreed dispositions

Research §8.2 enumerates four. A fifth is added here, discovered while enumerating the #490 D1 rename.

| # | Edit | File | Owner | Disposition |
|---|---|---|---|---|
| 1 | Add one wire line `_itemViewer.PicturesChanged += this.CbxPictures_CheckedChanged;` to `WireIntentEvents()` | `QfcItemController.EventWiring.cs` | **484** | **Proceed as an agreed cross-child edit.** The handler itself lands in 489-owned `EventHandlers.cs`; only the single wire statement is cross-child. 484 adds `UnwireIntentEvents()` with a documented count of **16** intent detachments (484 spec lines 358, 664). Adding a 17th wire obligates a 17th unwire: the plan must record the 16 → 17 hand-off to 484 explicitly, and Phase 0 must re-check whether 484 has landed. Defect is user-visible, diff is two lines. |
| 2 | Add the `InvokeRequired` re-entry guard to `HtmlDarkConverter` | `QfcItemController.FocusAndTheme.cs` | **484** | **Proceed as an agreed cross-child edit.** 484 changes `ToggleNavigation` (`:168-179`) and `ApplyReadEmailFormat` (`:318-324`). `HtmlDarkConverter` is `:289-301`, textually disjoint from both, and is in neither of 484's tables. Anchor on the member name. |
| 3 | `_ = _itemViewer.FocusSubject();` discard at the sole caller | `QfcItemController.MailActions.cs:64` | **484** | **Proceed.** The `void` → `bool` signature change does **not** make this edit compiler-forced: the sole caller is the expression statement `_itemViewer.FocusSubject();`, and a `bool`-returning invocation is a legal expression statement, so the build succeeds with the caller untouched. The discard is adopted deliberately, to make the ignored result explicit; its proof is a `git grep` for the discard form, never the clean build. Zero semantic change, so no 484 assertion is affected. |
| 4 | Insert `ClearFolderItems()` before `SetFolderItems` at `AssignFolderComboBox()` | `QfcItemController.FolderHandling.cs:182` | **446** | **DEFER.** Recorded as an out-of-scope finding. The rename alone closes the contract defect; the clear is a behaviour change in 446's file and belongs to 446 or to a follow-up issue. |
| 5 | **(new)** Rename the `SetFolderItems` invocation to `AddFolderItems` | `QfcItemController.FolderHandling.cs:182` | **446** | **Proceed as a one-token, compiler-forced edit.** Not in research §8.2. A rename that skips this site does not compile. It is textually disjoint from disposition 4 and does not pre-empt it. Same treatment for `MailActions.cs:206`, `:218` (484) and for the fourteen test call sites listed in § Scope, including the 501-adjacent `BreadcrumbDropDownIntegrationTests.cs` and `BreadcrumbSelectorOpenRetryTests.cs`. **Invocation renames only — no test method is renamed.** |

### Test files that are off-limits or capacity-constrained

- `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` — **493-owned.** Consume `BuildSyncDispatcher` (`:102-137`); do not edit.
- `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` — 497 lines, **3 spare**, and 493 names it sibling-owned. **#489 D2's test must not land here.**
- `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs` — 498 lines, **2 spare.** Line-neutral rename edits only.
- `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` — **500 lines, PINNED by 468** (444 spec line 868). **Receives no test and no edit.**
- `QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorTests.cs` — 488 lines, **12 spare**, 501-adjacent. Receives no new test.
- `QuickFiler.Test/Controllers/QfcItemController.EventHandlersTests.cs` (477, 23 spare) and `.ViewerSetupTests.cs` (474, 26 spare) — near ceiling; receive no new test.

---

## Root Cause Analysis

| Defect | Confirmed root cause |
|---|---|
| #486 D1 | A `new`-shadowed `Checked` property whose setter never writes `base.Checked`, combined with a consumer-side handler whose parameter is typed as the **base** `ToolStripMenuItem`. The handler's read therefore binds to the always-false base property and takes the clearing branch. |
| #486 D2 | Dead code retained on one twin after the wiring was removed, producing identical source with opposite runtime behaviour. |
| #486 D3 | An intent event declared on the viewer and the interface but never wired on the QFC controller path; the corresponding controller field is written once at assign time and never refreshed. |
| #487 D1 | Debug instrumentation left in a designer-wired production handler. |
| #487 D2 | An event handler that downcasts `sender` without a type test. |
| #489 D2 | A forwarder that omits the `InvokeRequired` re-entry guard used by every sibling forwarder on the same surface, on a path whose upstream guard (`Control.InvokeRequired`) is vacuously false before handle creation. |
| #489 D3 | A two-call ordering requirement expressed nowhere in the contract. |
| #489 D4 | Three marshalling seams captured in one constructor and published on one interface, with no statement of which is authoritative. |
| #490 D1 | A member named `Set…` implemented as an append, whose masking caller was removed by #438 without renaming the member. |
| #490 D2 | Two focus members on one type with opposite, undocumented threading contracts. |
| #490 D3 | `Control.Focus()`'s `bool` result discarded by an expression-bodied `void` member on a non-selectable control. |
| #490 D4 | A WinForms control property used as a scratch variable across two adjacent statements. |
| #490 D5 | Ten independently settable projections with no transactional grouping. |

---

## Proposed Fix

### Design summary

Three principles govern every fix below.

1. **Delete rather than patch.** #486 D1, #486 D2, #487 D1 and #487 D2 all resolve to one coherent set of deletions. The potential's candidate fix for #486 D1 — `base.Checked = value;` at `ToolStripMenuItemCb.cs:37` — is **rejected**: setting `ToolStripMenuItem.Checked = true` enables WinForms' own check-margin glyph *in addition to* the custom `base.Image`, producing a doubled indicator. `ToolStripMenuItemCb.cs:81-85` deliberately hides `Image` as `private new`, which shows the design intent is that the custom image is the *only* indicator.
2. **The viewer forwards; the controller marshals.** #489 D2 and #490 D2 both restore that single contract rather than inventing a second one.
3. **Do not guess.** #490 D3 makes the failure observable without inventing a focus target (open item U5). #490 D1 renames rather than changing `AddItems` semantics, because a blanket replace would wipe the folder list at the two idempotent `MailActions.cs` call sites (`Clear()` also calls `_upgradeLifetime.Invalidate()`, `BreadcrumbBridgeCoordinator.cs:152`).

### Boundaries and invariants to preserve

- `ToolStripMenuItemCb.Checked`'s setter behaviour (`:32-50`) is already correct and **must not change**. It becomes the sole owner of the check image.
- The `base.Click -= …; base.Click += …;` pattern in the `CheckOnClick` setter (`:63-79`) is load-bearing: it is what prevents a double-toggle. Do not "simplify" it.
- `IItemViewer.UiDispatcher` (4 consumers) and `IItemViewer.UiSyncContext` (5 consumers) **remain**. Only `UiScheduler` is deleted. `ViewerSetup.cs:58`'s `await _itemViewer.UiSyncContext;` is load-bearing for the #230 pump work (comment at `:30`).
- The unrelated `UiScheduler` members on `ItemViewerExpanded.cs:63-67`, `EfcViewer.cs:43`, `QfcItemViewer.cs:61`, `QfcFormViewer.cs:41`, `QfcHomeController.cs:438`, `IQfcFormViewer.cs:16` must not be touched.
- `FlagTaskDialogResult` stays on `IItemViewer`; it is a legitimate presentation projection. Only the redundant read-back is removed. `ViewerSetupTests.cs:258` and `:283` (`VerifySet`) must stay green.
- Deleting a designer-wired handler and its `+=` must happen in the **same change**; deleting the method alone is CS0103.

### Dependencies or blocked work

- **#487 D1 is gated on research open item U1** — whether CSharpier skips `*.Designer.cs` by filename. `.csharpierignore` (15 lines) excludes `**/evidence/**`, `*.cobertura.xml`, `*.coverage`, `*.coveragexml`, `*.trx`, `*.csproj`, `*.props`, `*.targets`. It does **not** list `*.Designer.cs`. Yet `ItemViewer.Designer.cs:256` is roughly 107 columns, above CSharpier's default 100-column print width, and there is no `.csharpierrc` in the repository. Phase 0 must run `dotnet tool run csharpier check .` on the untouched worktree and record the baseline. **Do not edit either `.Designer.cs` until this is answered.**
- Phase 0 must confirm whether 484 and 444 have executed (grep for `UnwireEvents` and `SyncExpandedRegistrations`; both returned zero matches on 2026-08-25).
- `nuget restore TaskMaster.sln` and `dotnet tool restore` are mandatory bootstrap. 484's spec (lines 723-731) records that a missing restore silently weakens the analyzer set because the `.csproj` files import `..\packages\...\*.props` conditionally.

### Implementation strategy

#### Files/modules to change

**489-owned production:**

- `QuickFiler/Viewers/ItemViewer.cs` — delete `:166-169`, `:171-175`, `:177-187`, `:205`, the `_uiScheduler` capture at `:27` and the `UiScheduler` property at `:65-69`.
- `QuickFiler/Viewers/ItemViewer.Designer.cs` — delete the `ParentChanged` wiring at `:256`.
- `QuickFiler/Viewers/ItemViewerExpanded.cs` — delete `:24-27`, `:158-161`, `:163-167`, `:169-179`.
- `QuickFiler/Viewers/ItemViewerExpanded.Designer.cs` — delete `:171`, `:180`, `:189`, `:198`, `:274`.
- `QuickFiler/Viewers/ItemViewer.DisplayState.cs` — `FocusSubject()` returns `bool` (`:79`).
- `QuickFiler/Viewers/ItemViewer.FolderSearch.cs` — rename `SetFolderItems` → `AddFolderItems` (`:20`); `FocusSearch()` becomes a bare forward (`:79`).
- `QuickFiler/Viewers/IItemViewer.cs` — delete `UiScheduler` (`:37`); `bool FocusSubject();` (`:54`); `AddFolderItems` (`:80`, and the comment at `:85`); XML docs on `:119-120` and on both focus members.
- `QuickFiler/Controllers/QfcItemController.EventHandlers.cs` — add `CbxPictures_CheckedChanged`.

**Cross-child production** (per § Sibling-collision resolution): `QfcItemController.EventWiring.cs`,
`.FocusAndTheme.cs`, `.MailActions.cs` (484); `.FolderHandling.cs` (446, one-token rename only).

**Test:** two new files plus the rename-only edits enumerated in § Scope.

**Project files:** `QuickFiler.Test/QuickFiler.Test.csproj` only, four appended `<Compile Include>`
entries. Neither `.csproj` region is alphabetical — both are grouped by area and insertion history
(484 spec `:561-567`). Append at the **tail** of the block; do not reorder.

- `Viewers\ToolStripMenuItemCbTests.cs` → append after the `Viewers\` block (currently ends at line 96).
- `Controllers\QfcItemController.ThemeMarshallingTests.cs` → append after the `Controllers\QfcItemController.*` block (currently ends at line 157, `SeamFactoryTests.cs`).

`QuickFiler/QuickFiler.csproj` needs **no** edit: no new production file is added.

#### Functions/classes impacted

Deleted: `ItemViewer.MenuItem_CheckedChanged` (both overloads), `ItemViewer.MoveOptionsMenu_Click`,
`ItemViewer.L0v2h2_WebView2_ParentChanged`, `ItemViewer.UiScheduler`,
`ItemViewerExpanded.MenuItem_CheckedChanged` (both overloads),
`ItemViewerExpanded.L0v2h2_WebView2_ParentChanged`, `IItemViewer.UiScheduler`.

Added: `QfcItemController.CbxPictures_CheckedChanged`.

Renamed: `IItemViewer.SetFolderItems` → `AddFolderItems`.

Signature-changed: `IItemViewer.FocusSubject()` `void` → `bool`.

Behaviour-changed: `QfcItemController.HtmlDarkConverter` (re-entry guard),
`ItemViewer.FocusSearch()` (bare forward), `QfcItemController.FlagAsTask` / `FlagAsTaskAsync`
(local instead of read-back).

#### Data flow and validation changes

The only data-flow change is #486 D3: `_optionsPictures` becomes a live projection of the menu state
instead of a one-shot snapshot of `_globals.QfSettings.SavePictures`. `PicturesChecked`'s initial
write at `ViewerSetup.cs:393` is unchanged, so the initial state is unaffected.

#### Error handling and logging updates

Two `Console.WriteLine` statements are deleted and **not replaced by a logger**. Routing them to a
logger would introduce an untestable dependency into `[ExcludeFromCodeCoverage]` view code, which is
the worst of the three options considered. No new exception is introduced anywhere.

#### Rollback considerations

Not applicable — no feature flag, no persisted state, no migration. Every change is a source-level
deletion, rename, guard, or signature change, and reverts cleanly by reverting the commit.

### Technical specifications

#### Inputs/outputs and formats

`IItemViewer` surface delta produced by this feature (exhaustive):

| Member | Before | After |
|---|---|---|
| `UiScheduler` | `TaskScheduler UiScheduler { get; }` (`:37`) | **removed** |
| `FocusSubject` | `void FocusSubject();` (`:54`) | `bool FocusSubject();` |
| `SetFolderItems` | `void SetFolderItems(string[] items);` (`:80`) | `void AddFolderItems(string[] items);` |
| `SetConversationItems` / `SortConversationByDate` | undocumented (`:119-120`) | unchanged signatures; XML doc added recording the ordering contract |
| `FocusSearch` / `FocusSubject` | undocumented threading contract | XML doc added: the viewer forwards, the controller marshals |

Everything else on `IItemViewer` is unchanged.

#### Required configuration keys and defaults

None.

#### Backward-compatibility expectations

`IItemViewer` is an internal-surface contract with exactly one production implementation
(`ItemViewer`); `QfcItemViewer.cs` and `QfcItemViewerExpanded.cs` do not implement it. There is no
external consumer, so the three interface changes are safe in-repo breaking changes and every call
site is updated in the same commit.

#### Performance constraints

None. Every change is a deletion, rename, or single added branch. The `InvokeRequired` guard on
`HtmlDarkConverter` adds one property read on the already-correct path.

---

## Assumptions, Constraints, Dependencies

**Assumptions**

- 484 and 444 have not landed at Phase 0 (verified 2026-08-25; Phase 0 must re-verify).
- CSharpier skips `*.Designer.cs` by filename. **Unverified — research open item U1, and a hard Phase 0 gate.**
- `AssignFolderComboBox()` can run more than once within a single viewer lifetime. **Unverified — open item U4.** It affects only the deferred clear-insertion half of #490 D1, not the rename.

**Constraints**

- **Target framework `net48`.** No `init` accessors, no `record`, no `record struct` — `net48` has no `IsExternalInit` and these fail to compile.
- **MSTest + Moq + FluentAssertions only.** Banned in tests: `Thread.Sleep`, `Task.Delay`, real wall-clock waits, `DateTime.Now` outside a clock seam, temporary files, and any live `Form`-derived type in the test assembly (guarded by `QuickFiler.Test/NoLiveFormInTestAssemblyTests.cs:16`).
- **500-line ceiling.** No file this feature adds or grows may exceed 500 lines, and no pre-existing file may grow past its Phase 0 baseline line count. `ItemViewer.Designer.cs` (6224) and `ItemViewerExpanded.Designer.cs` (821) are **already over** the ceiling; that excess is pre-existing, this feature only removes lines from them, and no "must be under 500" criterion is asserted over either.
- **Coverage.** `ItemViewer` carries `[ExcludeFromCodeCoverage]` at `ItemViewer.cs:20`. Because the attribute is not `AllowMultiple`, the other five partials inherit the exemption (recorded at `ItemViewer.DisplayState.cs:8-10`). **No acceptance condition may claim a coverage delta attributable to any `ItemViewer*.cs` change.** `ItemViewerExpanded` and `ToolStripMenuItemCb` carry no attribute and are measurable.
- **Toolchain.** Both msbuild gates use `/t:Rebuild`; the nullable gate must **not** add `/p:Nullable=enable` (CLAUDE.md § C#1.2, § C#1.3).
- The 85/75 versus 80/90 coverage-threshold conflict between `.claude/rules/` and CLAUDE.md is pre-existing and repository-wide. Adopt the stricter of each pair, as 444 did (444 spec lines 842-848).

**External dependencies**

None beyond `nuget restore` and `dotnet tool restore`.

---

## Data / API / Config Impact

- **User-facing changes:** two. The four `ItemViewerExpanded` move-option items begin rendering their check image (#486 D1). Toggling "Save Pictures" in QuickFiler now affects the filing operation (#486 D3).
- **Data or migration considerations:** none.
- **Logging/telemetry updates:** two `Console.WriteLine` statements removed; nothing added.
- **Compatibility notes:** three `IItemViewer` changes, all internal-surface, all call sites updated in the same commit. No CLI flag, config schema, or persisted format changes.

---

## Test Strategy

`ItemViewer` is `[ExcludeFromCodeCoverage]`, so **per-defect proof is a named test**, exactly as 444
records for `QfcCollectionController` (444 spec lines 850-855). A coverage claim over those files
would be an acceptance condition that cannot fail.

**No defect in this feature requires an STA WinForms test.** `ToolStripMenuItemCb` derives from
`ToolStripMenuItem` → `ToolStripDropDownItem` → `ToolStripItem` → `Component`. It is **not** a
`Control`, needs no window handle, and does not trip `NoLiveFormInTestAssemblyTests.cs:16-36`, which
scans only for `System.Windows.Forms.Form`-derived types. `ToolStripItem.Invalidate()` is
null-parent-safe. Nothing here needs `WinFormsPumpHost` or a `*.StaTests.cs` file.

### Why the metadata-absence tests are complete REDs

Several defects are proved by asserting a member is absent via
`GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)`. These are not weaker than a
behavioural test for the deletion cases, because **a leftover designer `+=` referencing a deleted
method is CS0103**. The test proves the method is gone; the compiler proves the wiring is gone. Each
such test fails before the fix (member present) and passes after.

### Per-defect routing

| Defect | Named test(s) | Target file | Why that file |
|---|---|---|---|
| **#486 D1** | `Checked_WhenSetTrue_AssignsCheckedCheckBoxImage` (pin), `Checked_WhenSetFalse_AssignsNullImage` (pin), `Checked_WhenSetTrue_RaisesShadowedCheckedChangedExactlyOnce` (pin), `ToolStripMenuItemCb_IsNotDerivedFromControl` (pin), `ItemViewerExpanded_DeclaresNoMenuItemCheckedChangedHandler` (**RED**) | **NEW** `QuickFiler.Test/Viewers/ToolStripMenuItemCbTests.cs` | No existing file covers this type at all, and the type is directly constructible headlessly. The RED belongs here because the defect's resolution is "the setter becomes the sole owner". |
| **#486 D2** | `ItemViewer_DeclaresNoMenuItemCheckedChangedMembers` (**RED**), `ItemViewer_DeclaresNoMoveOptionsMenuClickHandler` (**RED**) | `QuickFiler.Test/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs` | 132 lines, 5 `[TestMethod]`, **368 spare**. Reflection/metadata only, no instantiation — the existing idiom is exactly what this needs. |
| **#486 D3** | `WireIntentEvents_SubscribesToPicturesChanged` (**RED**, `VerifyAdd(v => v.PicturesChanged += It.IsAny<EventHandler>(), Times.Once())`), `PicturesChanged_WhenRaised_RefreshesOptionsPictures` (**RED**, reflection read of `_optionsPictures` via `QfcItemControllerTestSupport.GetField`) | `QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs` | 374 lines, 10 `[TestMethod]`, **126 spare**. It is the natural home for `WireIntentEvents` assertions and already contains the headless real-`ItemViewer` fixture 484 cites. `VerifyAdd` on this exact mock is a proven technique here (484 spec line 666, `SeamFactoryTests.cs:250-259`). |
| **#487 D1** | `ItemViewer_DeclaresNoParentChangedHandler` (**RED**), `ItemViewerExpanded_DeclaresNoParentChangedHandler` (**RED**) | `ItemViewerBreadcrumbDropDownContractTests.cs` | as above |
| **#487 D2** | Subsumed: `ItemViewer_DeclaresNoMenuItemCheckedChangedMembers`, `ItemViewerExpanded_DeclaresNoMenuItemCheckedChangedHandler`, plus both `ParentChanged` tests — the two unguarded casts live only inside those deleted members. | as above | as above |
| **#489 D1** | **No new test.** Standing regression is the existing `MenuDropDown_ShowsMoveOptionsMenuThroughDispatcher` at `QuickFiler.Test/Controllers/QfcItemController.SeamDispatcherTests.cs:99`, which must stay green. | — | See § Superseded Premises S1. |
| **#489 D2** | `HtmlDarkConverter_WhenInvokeRequired_MarshalsThroughInvoke` (**RED**), `HtmlDarkConverter_WhenInvokeRequired_DoesNotNavigateWithoutMarshalling` (**RED**), `HtmlDarkConverter_WhenNotInvokeRequired_NavigatesDirectly` (pin) | **NEW** `QuickFiler.Test/Controllers/QfcItemController.ThemeMarshallingTests.cs` | **Must not** land in `QfcItemController.FocusAndThemeTests.cs`: that file is 497 lines with **3 spare** and 493 names it sibling-owned. `IItemViewer.cs:135-137` declares `InvokeRequired` and `Invoke(Delegate)` on the interface specifically so guarded routing stays mockable (see the `#pragma warning disable CS0108` rationale at `:134-139`), so `Mock<IItemViewer>` with `SetupGet(v => v.InvokeRequired).Returns(true)` is sufficient. Consume `BuildSyncDispatcher` from the 493-owned `QfcItemController.TestSupport.cs:102-137`; do not edit it. |
| **#489 D3** | **No test — `fail-before-exception` dossier.** | `evidence/regression-testing/` | Documentation is not observable. The existing `SetTopicThread_WhenNotInvokeRequired_SetsItemsAndSortsDescending` (`ConversationTests.cs:249`) already pins the pair and must stay green. |
| **#489 D4 carve-out** | `IItemViewer_DeclaresNoUiSchedulerMember` (**RED**), `IItemViewer_StillDeclaresUiDispatcher` (pin), `IItemViewer_StillDeclaresUiSyncContext` (pin) | `ItemViewerBreadcrumbDropDownContractTests.cs` | as above. The two pins guard against over-deletion of the two seams that still have consumers. |
| **#490 D1** | `IItemViewer_DeclaresAddFolderItemsAndNotSetFolderItems` (**RED**) | `ItemViewerBreadcrumbDropDownContractTests.cs` | Metadata is the only honest carrier: the append behaviour lives inside `BreadcrumbBridgeCoordinator` (501-owned) and `ItemViewer` cannot be constructed headlessly. `BreadcrumbBridgeCoordinatorTests.cs` has **12 spare lines** and is 501-adjacent, so it receives no new test. |
| **#490 D2** | **No test — `fail-before-exception` dossier.** | `evidence/regression-testing/` | "Does not marshal" is not observable through `Mock<IItemViewer>`; an IL-shape assertion would be brittle. `NavigationTests.cs:198` (`viewer.Verify(v => v.FocusSearch(), Times.Once())`) must stay green. |
| **#490 D3** | `IItemViewer_FocusSubjectReturnsBool` (**RED**, `ItemViewerBreadcrumbDropDownContractTests.cs`); `Expand_WhenFocusSubjectReturnsFalse_StillEnumeratesConversation` (**RED** — `Setup(v => v.FocusSubject()).Returns(false)` does not compile against a `void` member) | contract file + `QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.cs` | `MailActionsTests.cs` is 184 lines, 7 `[TestMethod]`, **316 spare** — the natural home for `RightKeyActions` assertions. |
| **#490 D4** | `FlagAsTask_DoesNotReadBackFlagTaskDialogResult` (**RED**, `VerifyGet(v => v.FlagTaskDialogResult, Times.Never())`), `FlagAsTaskAsync_DoesNotReadBackFlagTaskDialogResult` (**RED**) | `QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.cs` | as above. Both fail today (one get each). The setter assertions at `ViewerSetupTests.cs:258`, `:283` must stay green. |

### The two `fail-before-exception` dossiers (explicit)

Two in-scope items admit no failing test and are carried by a dossier instead, written to
`docs/features/active/itemviewer-surface-defects-489/evidence/regression-testing/`:

1. `fail-before-exception-489-d3-set-then-sort.<ISO-8601>.md` — records why documenting the
   `SetConversationItems` → `SortConversationByDate` ordering on `IItemViewer.cs:119-120` cannot
   produce a RED, names the two existing tests that must stay green
   (`ConversationTests.cs:249`, `:266`), and records the rejected alternative F2
   (`SetConversationItemsSorted(IList, SortOrder)`) with the reason: the pair has exactly one
   production caller which is already correct, and F2 would change an interface consumed by
   sibling-owned test files.
2. `fail-before-exception-490-d2-focus-threading.<ISO-8601>.md` — records why "FocusSearch does not
   marshal" is unobservable through `Mock<IItemViewer>`, states the adopted contract (*the viewer
   forwards; the controller marshals*), names `NavigationTests.cs:198` as the test that must stay
   green, and records the residual: `Navigation.cs:54` (444-owned) has no controller-side guard, so
   an off-UI-thread `JumpToSearchTextbox` now silently no-ops instead of throwing.

### Coverage impact and targets

- Repository-wide line coverage must not decrease against the Phase 0 baseline. Any new production member must reach `>= 90%`; no reduction in coverage for changed lines.
- The only new production member is `QfcItemController.CbxPictures_CheckedChanged`, which is fully coverable through `Mock<IItemViewer>` and is covered by `PicturesChanged_WhenRaised_RefreshesOptionsPictures`.
- Deletions inside `[ExcludeFromCodeCoverage]` `ItemViewer*.cs` change no measured line. Deletions inside `ItemViewerExpanded.cs` (measurable, no attribute) remove uncovered lines, which can only help.
- **No new `[ExcludeFromCodeCoverage]` attribute is introduced anywhere by this feature.**
- `coverage.config` at repository root excludes only third-party modules; `QuickFiler` is not assembly-excluded.

### Toolchain commands to run (format → lint → type-check → test)

Bootstrap, once per worktree:

1. `nuget restore TaskMaster.sln`
2. `dotnet tool restore`

Then the four-stage loop, restarting from stage 1 on any failure or auto-fix:

1. `dotnet tool run csharpier format .` (verify with `dotnet tool run csharpier check .`)
2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
4. `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation`

`/InIsolation` is load-bearing and matches CI. `/t:Rebuild` is load-bearing: MSBuild's up-to-date
check does not invalidate on a command-line `/p:` change, so a warm `/t:Build` returns exit 0 having
skipped `CoreCompile` on every project and the gate cannot fail. Non-vacuity is proved by a
`/v:normal` log containing **zero** occurrences of the literal `Skipping target "CoreCompile"`.
`/p:Nullable=enable` must **not** be added (CLAUDE.md § C#1.3).

### Manual validation steps

None required. Every defect is proved by a named test, a compiler error, or a dossier.

---

## Out-of-Scope Findings

Required by `issue.md` § Scope Restrictions. Each entry carries an evidence pointer so it can be
promoted to a follow-up issue through the feature-promotion lifecycle after this feature merges.

| # | Finding | Evidence |
|---|---|---|
| **#489 D4 residual** | Consolidating the remaining two `ItemViewer` marshalling seams. The premise has shifted: the dominant controller-side seam is now `IUiDispatcher` (16 live sites across `Navigation.cs`, `MailActions.cs`, `Conversation.cs`, `FocusAndTheme.cs`), not any `ItemViewer` property. Removing `UiDispatcher` or `UiSyncContext` requires rewriting 4 and 5 call sites, all in sibling-owned files (`ViewerSetup.cs` 484, `FolderHandling.cs` 446, `EfcItemController.cs` 464). It is a design change with no failing behaviour to reproduce, so an acceptance condition would be structural and would gate an architecture change behind a bug feature. | research §4, §9.1 |
| **#490 D5** | Grouping the ten display projections into a transactional construct. The only production caller applies all ten atomically at `QfcItemController.ViewerSetup.cs:367-393` behind an `InvokeRequired` guard at `:361-365`, so the "interrupted partway" failure mode is not reachable and no RED test can be written. The fix is an interface redesign touching `IItemViewer`, `ItemViewer.DisplayState.cs`, `AssignControls`, and every `Mock<IItemViewer>` `VerifySet` in the suite. `ViewerSetup.cs` is 484-owned. | research §9.2 |
| **#490 D1 second half** | Insert an explicit `ClearFolderItems()` before `AddFolderItems` at `QfcItemController.FolderHandling.cs:182`, restoring the deliberate clear that #438 removed for a different code path. Deferred because `FolderHandling.cs` is **446-owned** and the change is behavioural, not mechanical. Gated on open item U4 (whether `AssignFolderComboBox()` can run twice within one viewer lifetime). | research §5.5.1, §8.2 |
| **O1** | `FlagTaskDialogResult` writes at `QfcItemController.ViewerSetup.cs:375` and `:379` are never read — `FlagAsTask`/`FlagAsTaskAsync` overwrite before reading. Pure dead state. `ViewerSetup.cs` is 484-owned and `ViewerSetupTests.cs:258`/`:283` assert those writes. | research §5.5.4 |
| **O2** | Because `ButtonSVG : Button` (`SVGControl/ButtonSVG.cs:13`) implements `IButtonControl`, a non-`None` `DialogResult` on `BtnFlagTask` gives the button form-closing semantics when hosted on a modally-shown `Form`. Whether `QfcFormViewer` is ever shown modally was **not** traced (open item U2). Recorded as an assessed hazard, not an established defect. | research §5.5.4, U2 |
| **O3 (reframed)** | The original O3 — `ItemViewer.FolderSearch.cs:79` calls `TxtboxSearch.Invoke(...)` with no `InvokeRequired` and no handle guard, throwing `InvalidOperationException` before handle creation — is **resolved in scope** by the #490 D2 bare-forward change and must **not** be promoted as written. The residual that remains out of scope is the *other side* of the adopted contract: `QfcItemController.Navigation.cs:54` (**444-owned**) calls `FocusSearch()` with no marshal, so after this feature an off-UI-thread `JumpToSearchTextbox` silently no-ops instead of throwing. Promote the caller-side guard, not the viewer-side throw. | research §5.5.2, §9.3 O3 |
| **O4** | `UtilitiesCS/HelperClasses/ThemeHelpers/ThemeControlGroup.cs:212-229` marshals only when `_controls is not null`; the WebView2 branch (`:289-296`) can therefore invoke `_htmlConverter` off the UI thread. Reached from the EFC path (`EfcItemController.cs:1087`, `:1092`, `:1114`, `:1119`). Out of scope: `UtilitiesCS`. Gated on open item U6. | research §5.4.2, U6 |
| **O5** | `QuickFiler/Viewers/ItemViewer.Designer.cs` (6224 lines) and `ItemViewerExpanded.Designer.cs` (821 lines) exceed the 500-line ceiling. Pre-existing; not created and not remediated by this feature, which only removes lines from both. | research §1 |
| **O6** | `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs` is 531 lines, over the ceiling. Pre-existing and sibling-adjacent. | research §7.1 |
| **O7** | `GetSelectedFolder()` nullable erasure: `BreadcrumbBridgeCoordinator.cs:1` is `#nullable enable` and `:190` declares `public string? GetSelectedFolder()`, but `ItemViewer.FolderSearch.cs:25` publishes an un-annotated `string` and `IItemViewer.cs:87` is likewise un-annotated. Two consumers (`QfcItemController.EventHandlers.cs:215`, `FolderHandling.cs:206`) assign `_selectedFolder` with no null guard. Adding `#nullable enable` would opt the file into `CS86xx`-as-error under CI's `/p:TreatWarningsAsErrors=true` and cascade into three files this feature does not fully own. That is a nullable-adoption work item, not a bugfix. Gated on open item U3. | research §5.5.6, U3 |
| **O8** | `QfcItemController.EventWiring.cs:135` contains a raw `await Task.Delay(newDelay);` in production inside `HandleWebViewInitializedAsync`. Already recorded by 446's research as out of scope there too. | research §9.3 O8 |
| **E1** (discovered during execution) | **Repo-wide stale analyzer HintPaths.** Every one of the sixteen tracked `.csproj` files names `Meziantou.Analyzer.3.0.156` and `Roslynator.Analyzers.4.16.0` in its `<Analyzer Include>` items while its `packages.config` declares `3.0.174` and `4.16.1`. The skew is visible inside a single file: `QuickFiler/QuickFiler.csproj` names `3.0.174` at `:3` and `:579` but `3.0.156` at `:585`, and `4.16.0` at `:586-588`. The state is committed on `origin/main`, so a cold checkout fails `CS0006`; CI is green only because the main checkout's gitignored `packages/` still holds both versions. Repo-wide build configuration, out of scope for this feature; needs its own issue. | `FEATURE/evidence/baseline/phase0-analyzer-build.2026-08-27T23-26.md` |
| **E2** (discovered during execution) | **Dangling `cref` to a removed member.** `QuickFiler.Test/Controllers/QfcItemController.FolderSuggestionsTests.cs:16` carries `<see cref="IItemViewer.SetFolderItems"/>`, naming a member that P8-T5 renamed to `AddFolderItems` and that no longer exists. No `CS1574` is emitted because `QuickFiler.Test.csproj` declares no `DocumentationFile`, so XML documentation generation is off for that project and crefs are never resolved. P8-T7 restricts this feature's edits to that file to invocation renames only, so the comment cannot be corrected here. | `FEATURE/evidence/qa-gates/p10-t12-out-of-scope-completeness.2026-08-28T01-57.md` |
| **E3** (discovered during execution) | **The plan's `$LASTEXITCODE` convention is wrong for a zero-match `git grep`.** `plan.2026-08-25T01-04.md` Â§ Execution conventions states that wrapping a `git grep` in `(... | Measure-Object).Count` makes the pipeline's own exit code `0`. Measured directly at P9-T8: the wrapper changes the value of the PowerShell expression but does not reset `$LASTEXITCODE`, which retains the native `1` that `git grep` returns on a zero-match result. P9-T8 and P10-T15 are the exposed gates; both judge success from `$?` and `$Error.Count` under `$ErrorActionPreference = 'Stop'` and document the residual explicitly. A plan-text correction is out of scope for this feature. | `FEATURE/evidence/regression-testing/p9-t8-txtboxsearch-invoke-after.2026-08-28T01-44.md` |
| **E4** (discovered during execution) | **Stale narrative rows in the spec test matrix.** The Â§ test-matrix rows for #486 D3 (`spec.md:655`) and for #490 D3 and #490 D4 (`spec.md:664`) still name the parent test files with their pre-growth measurements — `QfcItemController.EventWiringTests.cs` at "374 lines, 10 `[TestMethod]`, 126 spare" and `QfcItemController.MailActionsTests.cs` at "184 lines, 7 `[TestMethod]`, 316 spare". Merged siblings 484, 444 and 493 have since taken them to 499 and 498 lines. These are narrative rows, not criteria, and the dated amendment note under Â§ Acceptance Criteria (`spec.md:761-762`) already supersedes them by routing the new tests to `.Part2.cs` continuation files. Recorded so the discrepancy is not read as a live instruction. | `FEATURE/spec.md:761-762` (amendment note); `FEATURE/evidence/qa-gates/p10-t10-csproj-discipline.2026-08-28T01-54.md` |

---

## Acceptance Criteria

**Authority.** This section is the sole acceptance-criteria source for this `full-bug` feature.
Every criterion below is checkable by a named test, by a compiler gate, or by a command, and every
criterion is capable of **failing** if the work is not done. No criterion asserts an absolute
diagnostic or test count over files this feature does not own; all such conditions are phrased as a
comparison against the Phase 0 baseline recorded in
`docs/features/active/itemviewer-surface-defects-489/evidence/baseline/`. No criterion asserts a
"must be under 500 lines" condition over `ItemViewer.Designer.cs` or `ItemViewerExpanded.Designer.cs`,
both of which are already over that ceiling.

**Amendment (2026-08-27).** One criterion under § Scope discipline is amended in place. It
originally read: "`QuickFiler/QuickFiler.csproj` is absent from the diff (no new production file is
added). `QuickFiler.Test/QuickFiler.Test.csproj` gains exactly two `<Compile Include>` entries, each
appended at the tail of its existing block, with no reordering of any pre-existing entry." The
two-entry count became unsatisfiable after merged siblings 484, 444 and 493 grew the two test files
this feature routes new tests into: `QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs`
now measures 499 lines (1 spare against the 500-line ceiling; 374 when the plan was authored) and
`QfcItemController.MailActionsTests.cs` now measures 498 lines (2 spare; 184 at authoring), so the
five new tests cannot be appended to those files. The remedy is this repository's established `PartN`
partial-class continuation convention (precedent: `QfcItemController.InitializationTests.Part2.cs`
and `.Part3.cs`): the new tests land in two new continuation files and the project file gains four
appended entries instead of two. The no-reordering guarantee is unchanged and not weakened.
In a second correction pass on the same date, four criteria (the `WireIntentEvents_SubscribesToPicturesChanged`
and `PicturesChanged_WhenRaised_RefreshesOptionsPictures` pair under Issue #486, and the
`Expand_WhenFocusSubjectReturnsFalse_StillEnumeratesConversation` and the two `FlagAsTask*_DoesNotReadBackFlagTaskDialogResult`
criteria under Issue #490) and one prose line in § Change surface still named the pre-amendment file
locations. Only the file-path token was corrected in each, pointing to the `.Part2.cs` continuation
files above; the test names, node IDs, assertion text, and owning partial class are unchanged, so no
criterion was weakened — each was only made locationally exact.

### Phase 0 baseline (prerequisite for every comparison below)

- [x] A Phase 0 baseline exists under `docs/features/active/itemviewer-surface-defects-489/evidence/baseline/` recording, at minimum: the `dotnet tool run csharpier check .` result on the untouched worktree; the analyzer-build warning count; the nullable-build warning count; the `vstest` passed / failed / skipped counts; the repository-wide line-coverage percentage; the line count of every file this feature will touch; and the repository-wide occurrence count of `[ExcludeFromCodeCoverage]`.
- [x] The Phase 0 baseline answers research open item U1 by recording whether `dotnet tool run csharpier check .` reports `QuickFiler/Viewers/ItemViewer.Designer.cs` as unformatted on the **untouched** worktree. No `.Designer.cs` edit is made before this is recorded.
- [x] The Phase 0 baseline records, from a fresh grep of `QuickFiler/`, whether `UnwireIntentEvents` and `SyncExpandedRegistrations` are present, i.e. whether upstreams 484 and 444 have executed, and re-derives every member anchor this feature needs in `QfcItemController.EventWiring.cs`, `.FocusAndTheme.cs`, `.MailActions.cs`, `.FolderHandling.cs` and `.Navigation.cs` against the actual branch head.

### Issue #486 — move-option menu

- [x] `QuickFiler.Test/Viewers/ToolStripMenuItemCbTests.cs` exists and its test `ItemViewerExpanded_DeclaresNoMenuItemCheckedChangedHandler` passes, proving both `MenuItem_CheckedChanged` overloads are absent from `QuickFiler.ItemViewerExpanded` (namespace `QuickFiler`, per `ItemViewerExpanded.cs:14`; the type is not in `QuickFiler.Viewers`).
- [x] The four constructor calls at `ItemViewerExpanded.cs:24-27` and the four designer wirings at `ItemViewerExpanded.Designer.cs:171`, `:180`, `:189`, `:198` are deleted. Proof: the analyzer build succeeds — a surviving `+=` referencing the deleted method is CS0103 — and the RED test above passes.
- [x] The tests `Checked_WhenSetTrue_AssignsCheckedCheckBoxImage`, `Checked_WhenSetFalse_AssignsNullImage`, `Checked_WhenSetTrue_RaisesShadowedCheckedChangedExactlyOnce` and `ToolStripMenuItemCb_IsNotDerivedFromControl` pass, pinning `ToolStripMenuItemCb.Checked`'s setter as the sole owner of the check image.
- [x] `QuickFiler/Viewers/ToolStripMenuItemCb.cs` contains no assignment to `base.Checked`, and the `base.Click -= …; base.Click += …;` pattern in the `CheckOnClick` setter is unchanged. Proof: `git diff` against the Phase 0 base commit shows no change to `ToolStripMenuItemCb.cs`.
- [x] `ItemViewer_DeclaresNoMenuItemCheckedChangedMembers` and `ItemViewer_DeclaresNoMoveOptionsMenuClickHandler` in `QuickFiler.Test/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs` pass, proving the three dead `ItemViewer.cs` members are deleted.
- [x] `WireIntentEvents_SubscribesToPicturesChanged` in `QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.Part2.cs` passes, asserting `VerifyAdd(v => v.PicturesChanged += It.IsAny<EventHandler>(), Times.Once())` after `WireIntentEvents()`.
- [x] `PicturesChanged_WhenRaised_RefreshesOptionsPictures` in the same file passes, proving `_optionsPictures` follows the menu state rather than remaining the value captured at `ViewerSetup.cs:392`.
- [x] The plan or the executor's handoff record states the `WireIntentEvents` / `UnwireIntentEvents` count change from 16 to 17 and names it as an obligation on upstream 484. Recorded in `evidence/other/`.

### Issue #487 — `Console.WriteLine` and unguarded cast

- [x] `ItemViewer_DeclaresNoParentChangedHandler` and `ItemViewerExpanded_DeclaresNoParentChangedHandler` in `ItemViewerBreadcrumbDropDownContractTests.cs` pass, proving `L0v2h2_WebView2_ParentChanged` is absent from both types.
- [x] `git grep -F -n "Parent Changed" -- QuickFiler/Viewers/` returns **zero** matches. (It returns two today, at `ItemViewer.cs:168` and `ItemViewerExpanded.cs:160`. The asserted token is the exact single-line, non-interpolated literal `Parent Changed`.)
- [x] The two designer wirings at `ItemViewer.Designer.cs:256` and `ItemViewerExpanded.Designer.cs:274` are deleted. Proof: the analyzer build succeeds, which is only possible if both `+=` statements are gone.
- [x] No logger, `Debug.WriteLine`, or replacement diagnostic call is introduced in `ItemViewer.cs` or `ItemViewerExpanded.cs` in place of the deleted `Console.WriteLine` statements. Proof: `git diff` for those two files shows deletions only, no added statement.
- [x] Neither `.Designer.cs` file is reformatted wholesale: each shows only the single deleted wiring line in `git diff --stat` against the Phase 0 base commit, consistent with the U1 answer recorded in the Phase 0 baseline.

### Issue #489 — UI-thread marshalling

- [x] `QuickFiler.Test/Controllers/QfcItemController.ThemeMarshallingTests.cs` exists and `HtmlDarkConverter_WhenInvokeRequired_MarshalsThroughInvoke` passes, asserting `viewer.Verify(v => v.Invoke(It.IsAny<Delegate>()), Times.Once())`.
- [x] `HtmlDarkConverter_WhenInvokeRequired_DoesNotNavigateWithoutMarshalling` and `HtmlDarkConverter_WhenNotInvokeRequired_NavigatesDirectly` in the same file pass.
- [x] No test for #489 D2 is added to `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs`, and that file's line count is unchanged from its Phase 0 baseline (497).
- [x] The dossier `evidence/regression-testing/fail-before-exception-489-d3-set-then-sort.<ISO-8601>.md` exists, records why documenting the set-then-sort ordering admits no RED, and names `SetTopicThread_WhenNotInvokeRequired_SetsItemsAndSortsDescending` (`ConversationTests.cs:249`) and `SetTopicThread_WhenInvokeRequired_MarshalsViaInvoke` (`:266`) as the tests that must stay green.
- [x] `IItemViewer.cs` carries XML documentation on `SetConversationItems` and `SortConversationByDate` stating the same-UI-thread-turn ordering requirement, and both members retain their existing signatures.
- [x] `IItemViewer_DeclaresNoUiSchedulerMember` in `ItemViewerBreadcrumbDropDownContractTests.cs` passes, proving `IItemViewer` no longer declares `UiScheduler`.
- [x] `IItemViewer_StillDeclaresUiDispatcher` and `IItemViewer_StillDeclaresUiSyncContext` in the same file pass, proving the two seams that still have production consumers were not deleted.
- [x] The unrelated `UiScheduler` members on `ItemViewerExpanded.cs:63-67`, `EfcViewer.cs`, `QfcItemViewer.cs`, `QfcFormViewer.cs`, `QfcHomeController.cs` and `IQfcFormViewer.cs` are unchanged. Proof: `git diff --name-only` against the Phase 0 base commit lists none of `QuickFiler/Viewers/EfcViewer.cs`, `QuickFiler/Viewers/QfcItemViewer.cs`, `QuickFiler/Viewers/QfcFormViewer.cs`, `QuickFiler/Controllers/QfcHomeController.cs`, `QuickFiler/Interfaces/IQfcFormViewer.cs`.
- [x] `MenuDropDown_ShowsMoveOptionsMenuThroughDispatcher` (`QuickFiler.Test/Controllers/QfcItemController.SeamDispatcherTests.cs:99`) still passes and is unchanged; it is the standing regression for #489 D1, which is closed by citation with no work item.

### Issue #490 — display and folder contract

- [x] `IItemViewer_DeclaresAddFolderItemsAndNotSetFolderItems` in `ItemViewerBreadcrumbDropDownContractTests.cs` passes, asserting `typeof(IItemViewer).GetMethod("AddFolderItems", new[] { typeof(string[]) })` is non-null and `GetMethod("SetFolderItems", new[] { typeof(string[]) })` is null.
- [x] Every production and test call site of the renamed member is updated: the analyzer build succeeds and the full test run reports zero failures. (A missed site is CS1061.)
- [x] The test methods `AssignFolderComboBox_RetainsSetFolderItemsAndIndexOneSelection` and `MarkItemForDeletion_StillAppendsTrashToDeleteViaSetFolderItems` still exist under those exact names and still pass. Proof: `git grep -F -n "AssignFolderComboBox_RetainsSetFolderItemsAndIndexOneSelection" -- QuickFiler.Test/` returns a match, and likewise for `MarkItemForDeletion_StillAppendsTrashToDeleteViaSetFolderItems`.
- [x] `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Search.cs` is not edited. Proof: it is absent from `git diff --name-only` against the Phase 0 base commit.
- [x] The dossier `evidence/regression-testing/fail-before-exception-490-d2-focus-threading.<ISO-8601>.md` exists, states the adopted contract *the viewer forwards; the controller marshals*, and records the `Navigation.cs:54` caller-side residual as a follow-up.
- [x] `ItemViewer.FolderSearch.cs`'s `FocusSearch()` is a bare forward containing no `Invoke` call, and `IItemViewer.cs` documents one threading contract covering both `FocusSearch` and `FocusSubject`. Proof: `git grep -F -n "TxtboxSearch.Invoke" -- QuickFiler/Viewers/` returns zero matches (it returns one today, at `ItemViewer.FolderSearch.cs:79`).
- [x] `viewer.Verify(v => v.FocusSearch(), Times.Once())` at `QuickFiler.Test/Controllers/QfcItemController.NavigationTests.cs:198` still passes and `QuickFiler/Controllers/QfcItemController.Navigation.cs` is absent from `git diff --name-only`.
- [x] `IItemViewer_FocusSubjectReturnsBool` in `ItemViewerBreadcrumbDropDownContractTests.cs` passes, asserting `typeof(IItemViewer).GetMethod("FocusSubject").ReturnType == typeof(bool)`.
- [x] `Expand_WhenFocusSubjectReturnsFalse_StillEnumeratesConversation` in `QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.Part2.cs` passes. Its `Setup(v => v.FocusSubject()).Returns(false)` does not compile against the pre-change `void` member, which is the RED.
- [x] `LblSubject`'s `TabStop`, `ControlStyles`, and selectability are unchanged, and no new focus target is introduced. Proof: `git diff` for `ItemViewer.DisplayState.cs` shows only the `FocusSubject` return-type change, and `QuickFiler/Viewers/ItemViewer.Designer.cs` shows only the deleted `ParentChanged` wiring.
- [x] `FlagAsTask_DoesNotReadBackFlagTaskDialogResult` and `FlagAsTaskAsync_DoesNotReadBackFlagTaskDialogResult` in `QfcItemController.MailActionsTests.Part2.cs` pass, asserting `VerifyGet(v => v.FlagTaskDialogResult, Times.Never())`.
- [x] `FlagTaskDialogResult` remains declared on `IItemViewer` and on `ItemViewer.Commands.cs`, and the existing setter assertions at `QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs:258` and `:283` still pass unchanged.

### Scope discipline

- [x] `git diff --name-only` against the Phase 0 base commit contains **no** entry for `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` (owned by 488).
- [x] The same diff contains **no** entry for `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` (owned by 501).
- [x] The only sibling-owned **production** files in that diff are `QuickFiler/Controllers/QfcItemController.EventWiring.cs`, `QfcItemController.FocusAndTheme.cs`, `QfcItemController.MailActions.cs` (484) and `QfcItemController.FolderHandling.cs` (446), and each diff is confined to the members named in § Sibling-collision resolution.
- [x] The `QfcItemController.FolderHandling.cs` diff is the one-token `SetFolderItems` → `AddFolderItems` rename at the single call site and nothing else. In particular no `ClearFolderItems()` call is inserted, that half being deferred to 446 or a follow-up issue.
- [x] `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` is absent from the diff; its line count (500) and `[TestMethod]` count (13) are unchanged from the Phase 0 baseline.
- [x] `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` is absent from the diff; `BuildSyncDispatcher` is consumed, not edited.
- [x] No `UtilitiesCS` file appears in the diff.
- [x] `QuickFiler/QuickFiler.csproj` is absent from the diff (no new production file is added). `QuickFiler.Test/QuickFiler.Test.csproj` gains exactly four `<Compile Include>` entries — `Viewers\ToolStripMenuItemCbTests.cs`, `Controllers\QfcItemController.EventWiringTests.Part2.cs`, `Controllers\QfcItemController.ThemeMarshallingTests.cs`, and `Controllers\QfcItemController.MailActionsTests.Part2.cs` — each appended at the tail of its block as that tail stood at insertion time, with no reordering of any pre-existing entry.
- [x] § Out-of-Scope Findings lists, with an evidence pointer for each, the #489 D4 residual, #490 D5, the deferred #490 D1 clear-insertion, and O1 through O8, satisfying `issue.md` § Scope Restrictions.

### File size, toolchain, coverage, and evidence

- [ ] No file this feature adds or grows exceeds 500 lines, and no pre-existing file **outside the enumerated intentional-growth list** grows past its Phase 0 baseline line count. Verified against the per-file line counts recorded in `evidence/baseline/`.
- [ ] `dotnet tool run csharpier check .` reports no unformatted file, or reports exactly the set recorded in the Phase 0 baseline and no additional file.
- [ ] `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` exits 0, and its warning count is **not greater than** the Phase 0 baseline warning count.
- [ ] That analyzer build is proved non-vacuous: its `/v:normal` log contains **zero** occurrences of the literal `Skipping target "CoreCompile"`. The log is stored in `evidence/qa-gates/`.
- [ ] `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` exits 0. The command **does not** include `/p:Nullable=enable` and **does not** use `/t:Build`. Verified by the recorded command line in `evidence/qa-gates/`.
- [ ] The nullable build passing is also the operative `net48` guard: `init` accessors, `record`, and `record struct` fail to compile against `net48`, which has no `IsExternalInit`. No such construct is introduced by this feature.
- [ ] `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation` reports a failed count **not greater than** the Phase 0 baseline failed count; **zero** failures in the two test classes this feature creates (`ToolStripMenuItemCbTests`, `QfcItemController_ThemeMarshallingTests`), which is absolute and satisfiable because both classes exist only after this feature writes them; for the eight pre-existing classes this feature edits (`ItemViewerBreadcrumbDropDownContractTests`, `QfcItemController_EventWiringTests`, `QfcItemController_MailActionsTests`, `BreadcrumbSelectorOpenRetryTests`, `BreadcrumbDropDownIntegrationTests`, `QfcItemController_SeamDispatcherTests`, `QfcItemController_FolderSuggestionsTests`, `QfcItemController_FolderHandlingTests`) a per-class failed count **not greater than** the per-class failed count recorded in the Phase 0 baseline, with every test counted against a non-zero per-class baseline named together with its owning sibling; a skipped count **equal to** the Phase 0 baseline skipped count, which is expected to be `0` because `QuickFiler.Test` carries zero `[Ignore]` attributes and whatever the baseline measured governs; and a passed count **not less than** the Phase 0 baseline passed count. The assembly-wide failed count and the eight per-class counts are relative, not absolute, because `QuickFiler.Test` is co-owned by siblings 468, 484, 446, 493 and 501 and a sibling's pre-existing failure is not this feature's to fix; the absolute gate is the two classes this feature creates.
- [ ] Repository-wide line coverage is **not lower than** the Phase 0 baseline coverage percentage recorded in `evidence/baseline/`.
- [ ] No acceptance condition in this document claims a coverage delta attributable to any `ItemViewer*.cs` change. `QuickFiler/Viewers/ItemViewer.cs:20` still carries `[ExcludeFromCodeCoverage]`, unchanged, and per-defect proof is the named test listed in § Test Strategy.
- [ ] The repository-wide occurrence count of the coverage-exclusion attribute, counted in **both** spellings by `git grep -n -E "\[(System\.Diagnostics\.CodeAnalysis\.)?ExcludeFromCodeCoverage\]" -- "*.cs"`, is **not greater than** the Phase 0 baseline count: no new exclusion attribute is introduced anywhere by this feature, in neither the unqualified `[ExcludeFromCodeCoverage]` form nor the fully-qualified `[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]` form that dominates `QuickFiler/Controllers/QfcItemController.EventHandlers.cs`. A fixed-string count of the unqualified spelling alone would be blind to the form every neighbouring member of that file uses, and that file is the one this feature adds a member to. In particular neither `ItemViewerExpanded` nor `ToolStripMenuItemCb` gains one.
- [ ] The only new production member, `QfcItemController.CbxPictures_CheckedChanged`, reaches `>= 90%` line coverage in the run recorded in `evidence/qa-gates/`.
- [ ] The two new test files contain none of `Thread.Sleep`, `Task.Delay`, or `DateTime.Now`. Proof: `git grep -n -E "Thread\.Sleep|Task\.Delay|DateTime\.Now" -- QuickFiler.Test/Viewers/ToolStripMenuItemCbTests.cs QuickFiler.Test/Controllers/QfcItemController.ThemeMarshallingTests.cs` returns zero matches.
- [ ] No test creates or reads a temporary file, and no test constructs a live `Form`. Proof: the existing structural guard at `QuickFiler.Test/NoLiveFormInTestAssemblyTests.cs:16` passes in the final run.
- [ ] Every test added or edited by this feature uses MSTest attributes, Moq for mocking, and FluentAssertions for assertions.
- [ ] Every evidence artifact this feature produces resolves under `docs/features/active/itemviewer-surface-defects-489/evidence/<kind>/`, using only the canonical kinds `baseline`, `qa-gates`, `regression-testing`, `issue-updates`, and `other`. In particular coverage evidence is written to `evidence/qa-gates/`, **never** to `evidence/coverage/`, and nothing is written to `artifacts/baselines/`, `artifacts/qa/`, `artifacts/coverage/`, or `artifacts/evidence/`.
- [ ] `user-story.md` does not exist in `docs/features/active/itemviewer-surface-defects-489/`. This is a `full-bug` feature and `spec.md` is the sole acceptance-criteria source.

---

## Risks & Mitigations

| Risk | Mitigation |
|---|---|
| A `.Designer.cs` edit triggers a whole-file CSharpier reformat of a 6224-line generated file, making the diff unreviewable and unattributable. | Phase 0 gate: run `dotnet tool run csharpier check .` on the untouched worktree and record the result **before** any `.Designer.cs` edit (open item U1). Acceptance criterion asserts `git diff --stat` shows only the single deleted line per file. |
| Upstream 484 lands mid-flight and moves the anchors this feature edits in `EventWiring.cs`, `FocusAndTheme.cs`, `MailActions.cs`. | Every acceptance condition anchors on a member name, never a line number. Phase 0 re-derives anchors and records whether 484 has executed. |
| The `WireIntentEvents` 16 → 17 change is not mirrored by a 17th `UnwireIntentEvents` detachment once 484 lands, leaking one subscription on `Cleanup()`. | Explicit handoff record in `evidence/other/`, plus an acceptance criterion requiring it. |
| The `SetFolderItems` → `AddFolderItems` rename collides with sibling 501 in `BreadcrumbDropDownIntegrationTests.cs`. | The rename is invocation-only, line-neutral, and node-ID-stable; no test method is renamed. The 501-owned `BreadcrumbBridgeCoordinator.Search.cs` comment is deliberately left stale. |
| Deleting the `ItemViewerExpanded` handler changes rendering in a way the twin's users notice. | That is the point of the fix — the check image begins rendering. The three `ToolStripMenuItemCb` pins assert the resulting image behaviour is the setter's, and the constructor calls are redundant because the constructor already applies the correct initial image at `ToolStripMenuItemCb.cs:16-19`. |
| Removing the `UiScheduler` property leaves an unused `using System.Threading.Tasks;` in `ItemViewer.cs`, which `EnforceCodeStyleInBuild` can raise as IDE0005. | The analyzer gate catches it; remove the `using` only if it becomes genuinely unused (other members may still need it). |
| `FocusSearch()` becoming a bare forward converts a throw into a silent no-op when called off the UI thread. | Recorded explicitly in the #490 D2 dossier and promoted as reframed finding O3. The caller-side guard belongs to 444's `Navigation.cs`. |

---

## Rollout & Follow-up

- **Release/rollout steps:** merge to `epic/quickfiler-bug-family-integration` as a wave-2 child. No runtime migration, no feature flag, no configuration change.
- **Post-fix clean-up tasks:** promote each § Out-of-Scope Findings entry to a follow-up issue through the feature-promotion lifecycle. Promote O3 in its **reframed** form (the `Navigation.cs:54` caller-side guard), not as originally written.
- **Downstream dependent:** feature 488. The three `IItemViewer` changes here (`UiScheduler` removal, `FocusSubject` return type, `SetFolderItems` rename) are the surface delta 488 must plan against.
- **Links:** issues #486, #487, #489, #490; epic #446; `issue.md`; `research/2026-08-25T02-15-itemviewer-surface-defects-research.md`; upstream contracts `qfc-item-controller-defects-484/spec.md` (line 329) and `quickfiler-keyboard-action-defects-444/spec.md` (line 704).
