# Issue #440 — Breadcrumb Left/Right parent-child navigation: research

- **Issue:** #440
- **Timestamp:** 2026-08-29T00-52
- **Branch read:** `bug/breadcrumb-left-right-arrow-parent-child-navigation-440`
- **Commit read:** `b56400ab663a85b6039139d4548f408821e957ce` (= `origin/main`, = `refs/heads/main`), working tree clean
- **Every line number below is against that commit** unless explicitly attributed to another branch.

---

## HEADLINE — the premise of the delegation brief is stale

**Most of #440 has already been implemented and has landed on `main`.** It was delivered as a
secondary payload of the feature `docs/features/active/breadcrumb-router-navigation-defects-498/`,
whose `spec.md:4` reads: *"**Also closes:** #440, #499."* That spec's acceptance criteria AC-15,
AC-16, AC-17, AC-18, AC-23, AC-24 and AC-28 are all checked `[x]`, and the corresponding code and
tests are present in the tree at `b56400ab`.

The `issue.md` and the seeded `spec.md` in this feature folder both still carry the 2026-08-07
code-read. **Every one of their eight source citations is now wrong** (§1 below). The seeded
`spec.md` is an unpopulated template last touched `2026-08-29T00-22`, i.e. minutes before this
research; nothing in it has been reconciled against the landed work.

**What actually remains is a narrow, concrete residual defect on the Qfc surface only** (§2.3):
`BreadcrumbStateModel.LeftArrow()` is gated so that Left performs the parent-select tree transition
**exactly once**, from the leaf. The Efc surface has no such gate and walks the whole ancestor chain.
The two surfaces therefore do **not** implement the same contract, contradicting both the issue's
Expected Behavior ("Repeated Left presses walk up the ancestor chain until the root is reached") and
#498's own AC-17 claim that "The Qfc surface implements the same Left/Right tree contract".

A secondary residual divergence exists in the Right descent semantics (§2.4).

---

## 1. Ground-truthing every citation in `issue.md` / `spec.md`

`issue.md` §Suspected Cause and `spec.md` §Root Cause Analysis contain the same eight citations.
Verdict per citation, against `b56400ab`:

| # | Citation in `issue.md` / `spec.md` | Verdict | Current reality |
|---|---|---|---|
| 1 | `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs:225-250` — `HandleArrowKeyAsync` | **INVALID — wrong file** | The type is now `sealed partial` and split three ways (#498 decision D8, forced by the 500-line limit). `HandleArrowKeyAsync` now lives in `QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs:42-98`. `BreadcrumbBridgeRouter.cs` is now 304 lines and its `:225-250` is `NotifyCoreInitialized` / `ProcessInboundAsync`, unrelated. |
| 2 | same file — `ExpandLeafAsync` | **INVALID — wrong file** | Now `QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs:165-209`. Its early-return now tests the **ACTIVE** segment (`row.ActiveSegment`), not the leaf (`:167-171`), and it keys the single provider call on `row.ActiveSegmentKey` (`:176`, `:185-186`). |
| 3 | `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs:195-216` — `LeftArrow` | **INVALID — wrong lines; description still accurate** | File is now 361 lines. `LeftArrow()` is at `:291-312`. `:195-216` is now the middle of `CollapseAfter`. The described behavior (close leaf expansion, else decrement `CollapsedAfterIndex`, `false` at the root) is unchanged and correct. |
| 4 | `BreadcrumbRow.cs` — `ReExpand`, `CollapsedAfterIndex`, leaf-expansion state | **VALID, relocated** | `CollapsedAfterIndex` property `:112`; `IsCollapsed` `:115`; `IsLeafExpanded` `:118`; `ReExpand()` `:235-244`; `RightArrow()` `:320-339`; `ToggleLeafExpanded()` `:274-283`. |
| 5 | `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs:385-386` — arrow routing | **VALID (± 2 lines)** | File is 489 lines. `ArrowAsync` is `:380-408`; the `RightArrow()`/`LeftArrow()` dispatch is at `:385-388`; the `case ArrowKeyMessage` entry is at `:310`. |
| 6 | `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs:424-455` — `RightArrow`, `LeftArrow` | **INVALID — wrong lines; description now WRONG** | File was split (#498 D8) into `BreadcrumbStateModel.cs` (248 lines) + `BreadcrumbStateModel.Row.cs` (384 lines). `RightArrow()` is `:167-185`, `LeftArrow()` is `:220-246`, and **both now attempt a #440 tree transition first** — see `TryRightTreeTransition` `:193-214`. The claim "Neither reassigns the selected row or node to a parent" is **no longer true**. `_selectedSubfolderIndex` field decl is `:18`; `SelectedSubfolderIndex` `:30`. `TryExpandLeaf` / `TryCollapseLeaf` are members of `BreadcrumbStateRow` in `BreadcrumbStateModel.Row.cs`, not of the model. |
| 7 | `QuickFiler/Resources/FolderBreadcrumb.html:395-404` — `onArrow`, `canRight`, `canLeft` | **PARTLY INVALID — the identifiers no longer exist** | File is 490 lines. `onArrow` is at `:395-405` (line range essentially holds). **`canRight` and `canLeft` were deleted by #498** and replaced by a single `can` at `:399-401`: `row !== null && (row.isSuggestion \|\| row.collapsed \|\| row.leafExpanded \|\| rowHasOpenAffordance(row))`. The `#440` rationale comment is at `:396-397`. `rowHasOpenAffordance` still exists at `:244-246`. `unhandledArrow` post is `:404`. Key binding `:414-428`. |
| 8 | `QuickFiler/Controllers/KeyboardHandler.cs:288-314` — `BreadcrumbArrowFallThrough` | **VALID (± 1 line)** | File is 414 lines. The `#351` comment block is `:288-291`; the method is `:292-315`; Right → `MyBox.ShowDialog("Pop Out Item or Enumerate Conversation?", ..., viewer.Controller.RightKeyActions)` `:304-309`; Left → `viewer.SetFolderDroppedDown(false)` `:313`. |

### Two further items the brief asked about

**`BreadcrumbDocumentAssets` — Efc and Qfc do NOT share one HTML document.** Confirmed by direct
read, not inference:

- **Qfc** uses the compiled resource `QuickFiler/Resources/FolderBreadcrumb.html` (490 lines), a real
  file, reached through `QuickFiler.Properties.Resources.FolderBreadcrumb`.
- **Efc** uses a document *generated in C#* from `UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs`
  (`BridgeJs` string constant) composed with `UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs`.
  Its arrow binding is `BreadcrumbDocumentAssets.cs:102-107`:
  `var map = { ArrowLeft: 'Left', ArrowRight: 'Right', ArrowUp: 'Up', ArrowDown: 'Down' };` followed by
  `post({ type: 'arrowKey', rowId: id || '', key: key });`
  **The Efc document has no client-side gating and no `unhandledArrow` message at all.** All four
  arrows are posted unconditionally. This is a structural divergence from Qfc and is the reason the
  Efc boundary behavior is a silent no-op rather than a fall-through.

**`BreadcrumbBridgeCoordinator` and `ItemViewer` own no arrow *decision*, only transport.**
`QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` re-publishes bridge messages as WinForms-shaped
events: `UnhandledArrow` (`:65`, raised `:322-325`) and the synthetic `FolderArrowKeyDown` (`:68`,
raised `:398-417`, from **both** `ArrowKeyMessage` and `UnhandledArrowMessage`). `ItemViewer`
participates only via `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` and `SetFolderDroppedDown`.
Neither type contains any Left/Right semantics. **They are not in #440's change footprint.**

**`EfcViewer.FolderListBox`** is not a list control. `QuickFiler/Viewers/EfcViewer.cs:85` declares
`internal Microsoft.Web.WebView2.WinForms.WebView2 BreadcrumbWebView => FolderListBox;`, and the
control itself is declared in `QuickFiler/Viewers/EfcViewer.Designer.cs`. It is wired to the router
at `QuickFiler/Controllers/EfcFormController.cs:239` and `:925`. **There is no WinForms `KeyDown`
arrow entry point for Efc breadcrumb arrows** — the only entry point is the WebView2 JS bridge above.
(Negative evidence: grep for `FolderListBox|KeyDown|Arrow` across `EfcViewer.cs` returns exactly one
hit, `:85`; the `KeyDown` handlers at `EfcFormController.cs:405-409` are attached to other controls
and route to `KeyboardHandler`, not to breadcrumb arrows.)

---

## 2. The two arrow pipelines, end to end, at `b56400ab`

### 2.1 Efc pipeline

```
WebView2 keydown (BreadcrumbDocumentAssets.cs:102-107, no gating)
  -> post {type:'arrowKey', rowId, key:'Left'|'Right'|'Up'|'Down'}
  -> IBreadcrumbWebHost.MessageReceived            QuickFiler/Viewers/IBreadcrumbWebHost.cs
  -> BreadcrumbBridgeRouter.OnHostMessageReceived  BreadcrumbBridgeRouter.cs:291-302  (async void boundary)
  -> BreadcrumbBridgeRouter.ProcessInboundAsync    BreadcrumbBridgeRouter.cs:233-289  (case ArrowKey :282-284)
  -> HandleArrowKeyAsync                           BreadcrumbBridgeRouter.Arrows.cs:42-98
       Right -> TryRightTreeTransitionAsync        BreadcrumbBridgeRouter.Arrows.cs:107-140
                  else ReExpand / ExpandLeafAsync  :55-65
       Left  -> row.ActivateSegment(active-1)      :73-80
                  else row.LeftArrow()             :82-85
  -> state on BreadcrumbRow                        UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs
  -> PostRowRender / SelectHierarchyPath           BreadcrumbBridgeRouter.Selection.cs:109-141
  -> BreadcrumbOutboundQueue -> IBreadcrumbWebHost.PostWebMessage
```

### 2.2 Qfc pipeline

```
WebView2 keydown (FolderBreadcrumb.html:414-428) -> onArrow(:395-405), client-side gate `can`
  -> post {type:'arrowKey'|'unhandledArrow', direction:'left'|'right'}
  -> BreadcrumbBridgeCoordinator                   QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs
  -> FolderBreadcrumbBridgeRouter.RouteAsync       FolderBreadcrumbBridgeRouter.cs:310 (case ArrowKeyMessage)
  -> ArrowAsync                                    FolderBreadcrumbBridgeRouter.cs:380-408
       -> _model.RightArrow() / LeftArrow()        BreadcrumbStateModel.cs:167-185 / :220-246
            Right -> TryRightTreeTransition        BreadcrumbStateModel.cs:193-214
            Left  -> row.ActivateSegment(active-1) BreadcrumbStateModel.cs:231-240  [GATED - see 2.3]
       -> if !handled: UnhandledArrowMessage       FolderBreadcrumbBridgeRouter.cs:389-395
       -> else FetchAndAttachSubfoldersAsync       FolderBreadcrumbBridgeRouter.cs:410-...
            keyed on (row.ActiveSegment ?? leaf).Key   :418-420
  -> unhandled path: BreadcrumbBridgeCoordinator.UnhandledArrow (:322-325)
  -> QfcItemController -> IQfcKeyboardHandler.BreadcrumbArrowFallThrough
  -> KeyboardHandler.BreadcrumbArrowFallThrough    KeyboardHandler.cs:292-315
```

### 2.3 THE RESIDUAL DEFECT — Qfc Left performs the tree transition exactly once

`UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs:231-240`:

```csharp
int? activeIndex = row.ActiveSegmentIndex;
if (
    _selectedSubfolderIndex < 0
    && activeIndex.HasValue
    && activeIndex.Value == row.Chain.Count - 1   // <-- LEAF-ANCHORED ONLY
    && row.ActivateSegment(activeIndex.Value - 1)
)
{
    return true;
}
```

Compare `QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs:73-80` (Efc), which has **no**
leaf-anchored condition:

```csharp
if (
    row.ActiveSegmentIndex.HasValue
    && row.ActivateSegment(row.ActiveSegmentIndex.Value - 1)
)
```

`BreadcrumbStateRow.ActivateSegment` (`BreadcrumbStateModel.Row.cs:195-211`) is itself perfectly
capable of walking the chain: it refuses only `!IsSuggestion`, `segmentIndex < 0`,
`segmentIndex >= Chain.Count - 1`, and a no-change index. **The one-step limit is imposed purely by
the `activeIndex.Value == row.Chain.Count - 1` clause in `LeftArrow()`, nothing else.**

This is not an inference. It is codified in a landed test comment,
`UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs:370-371`:

> `// Arrange: the first Left consumes the one available #440 parent-select transition,`
> `// after which nothing remains to collapse and no further tree transition applies.`

**User-visible consequence on Qfc**, traced against a three-segment chain
(`\Inbox` → `\Inbox\Projects` → `\Inbox\Projects\Apollo`), active index starting at 2:

| Press | Qfc actual today | Efc actual today | #440 expected |
|---|---|---|---|
| Left #1 | `ActivateSegment(1)` — parent selected ✓ | `ActivateSegment(1)` ✓ | parent |
| Left #2 | gate fails → `TryCollapseLeaf()` → `false` → **`UnhandledArrowMessage`** → `BreadcrumbArrowFallThrough` → **`SetFolderDroppedDown(false)` closes the QuickFiler folder dropdown** | `ActivateSegment(0)` — root selected ✓ | grandparent / root |
| Left #3 | (dropdown already gone) | refused at root → `row.LeftArrow()` collapse fall-through | no-op / fall-through |

So on Qfc, the second Left **destroys the user's navigation context** instead of walking up one more
level. That is the sharpest statement of the residual defect and is the right thing to reproduce in
a RED test.

The Efc counterpart is explicitly covered and green:
`QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.cs:242-261`
`HandleArrowKey_RepeatedLeft_WalksToRootThenFallsThroughToExistingBehavior` — "two presses walk
2 -> 1 -> 0; the third is refused and falls through".

### 2.4 SECONDARY residual divergence — Right descent commits on Efc, only highlights on Qfc

| | Efc | Qfc |
|---|---|---|
| Descent call | `SelectHierarchyPath(row, child.FullPath)` — `BreadcrumbBridgeRouter.Arrows.cs:138` | `SelectSubfolder(0)` — `BreadcrumbStateModel.cs:212` |
| Effect | `CommitSelection` → sets `SelectedFolderPath`, raises `SelectedFolderPathChanged` (`BreadcrumbBridgeRouter.Selection.cs:109-141`) | sets `_selectedSubfolderIndex = 0` only; no filing-target change |
| Observable | the filing target moves to the child | a highlight moves to the child |

These are different contracts. If the planner intends AC coverage for "both surfaces implement the
same contract", this must be decided explicitly. **Recommendation: leave as-is and record it.** The
Qfc surface has a separate committed/original/pending selector session
(`UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs`) that owns commitment, and #498
decision D1 explicitly ratified that #440 does **not** write that session (AC-20, `[x]`). Making Qfc
Right commit a filing target would breach that ratified boundary and pull #400's selector session
back into scope.

### 2.5 A shared limit, present on BOTH surfaces (report, do not necessarily fix)

Right descends exactly one level and then stalls on both surfaces. After the descent, the active
segment index is unchanged, so a further Right re-runs the identical descent
(`GetActiveChild(0)` → same child). Neither surface can descend two levels with Right alone. Moving
*within* a level is the job of Up/Down, which on Qfc are `selectorKey` messages
(`FolderBreadcrumb.html:407-418`) owned by the #400 selector session and out of #440's scope per D1.
The issue's stated composition ("Left to move up, Right to open the level below, Up/Down to move
within a level") is therefore satisfied only for a single descent. Flag this to the maintainer; do
not silently expand scope.

### 2.6 What is shared vs. duplicated

The brief states "the two surfaces already share `BreadcrumbRow`". **This is false.** Verified:

- Efc uses `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs` (`BreadcrumbRow`, 361 lines).
- Qfc uses `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.Row.cs` (`BreadcrumbStateRow`, 384 lines).

These are two distinct types with parallel but non-identical members
(`ActivateSegment`, `ActiveSegmentIndex`, `GetActiveChild` exist on both with slightly different
guards; `BreadcrumbRow` additionally requires an attached `_segmentKeys` entry at
`BreadcrumbRow.cs:157`, which `BreadcrumbStateRow` does not because `FolderBreadcrumbSegment`
carries `Key` intrinsically). The only genuinely shared types are `FolderBreadcrumbSegment`,
`FolderTreeNodeKey`, and `IFolderHierarchyProvider`.

**Consequence for planning:** the `spec.md` design direction "Share the transition logic between
`BreadcrumbStateModel` (Qfc) and `BreadcrumbBridgeRouter` (Efc) rather than implementing it twice"
is a **larger refactor than #440 needs**, and #498 already decided against it. #498 decision D9
ratified expressing each surface's transitions through its own already-landed active-segment seams.
**Recommendation: do not unify. The residual fix is a guard change in one method.**

---

## 3. The child-retrieval seam — `IFolderHierarchyProvider`

`UtilitiesCS/OutlookObjects/Folder/IFolderHierarchyProvider.cs`, 65 lines, three members:

```csharp
Task<IReadOnlyList<FolderBreadcrumbSegment>> GetAncestorChainAsync(FolderTreeNodeKey leafKey, CancellationToken ct);   // :31-34
Task<IReadOnlyList<FolderBreadcrumbSegment>> GetImmediateSubfoldersAsync(FolderTreeNodeKey segmentKey, CancellationToken ct); // :46-49
Task<FolderTreeNodeKey?> ResolveLeafKeyAsync(string folderPath, CancellationToken ct);  // :60-63
```

- **All async** (`Task`-returning). The interface XML doc (`:12-18`) records that the async shape
  exists only because *snapshot acquisition* is async; the ancestor walk and children projection are
  synchronous underneath.
- **Sole implementation:** `UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs`.
- **Its only dependency is `IOutlookFolderTreeService`, an interface — not COM.** Stated at
  `IFolderHierarchyProvider.cs:13-14` and confirmed by the existing test suites.
- **How each router obtains it:** constructor injection in both cases —
  `BreadcrumbBridgeRouter(IFolderHierarchyProvider provider, ...)` at
  `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs:41-56`, and
  `FolderBreadcrumbBridgeRouter(IFolderHierarchyProvider provider)` at
  `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs:19-22`.
- **Fully injectable for MSTest + Moq.** Already mocked with `MockBehavior.Strict` in the landed
  #440 tests, e.g. `ParentSubfolderProviderMock()` at
  `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs:418`.
- **No live-Outlook dependency on the arrow path.** Negative evidence: grep for
  `STAThread|STA|Outlook.Application|RequiresLiveOutlook|Ignore]` across
  `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.cs` returns **no matches**.
- **No new member is needed.** #498 AC-16 and AC-30 explicitly forbid adding one, and forbid
  reintroducing the retired two-call `ResolveLeafKeyAsync` → `GetImmediateSubfoldersAsync` pattern on
  the expansion path.

---

## 4. What "selected node" means today — the concept already exists

The `spec.md` design direction proposes *introducing* an explicit selected-node concept. **It was
already introduced by #498.** Current per-surface state:

| Concept | Efc (`BreadcrumbRow`) | Qfc (`BreadcrumbStateRow` / `BreadcrumbStateModel`) |
|---|---|---|
| Selected row | `_selectedRowId` (`BreadcrumbBridgeRouter.cs:33`) | `_selectedIndex` / `SelectRow(int)` (`BreadcrumbStateModel.cs:120-133`) |
| **Selected node** | `ActiveSegmentIndex` (`BreadcrumbRow.cs:94`), `ActiveSegment` `:97`, `ActiveSegmentKey` `:101-105` | `ActiveSegmentIndex` (`BreadcrumbStateModel.Row.cs:173-174`), `ActiveSegment` `:177-178` |
| Node mutator | `ActivateSegment(int)` (`BreadcrumbRow.cs:151-172`) | `ActivateSegment(int)` (`BreadcrumbStateModel.Row.cs:195-211`) |
| Child projection | `GetActiveChild(int)` (`BreadcrumbRow.cs:175-188`) | `GetActiveChild(int)` (`BreadcrumbStateModel.Row.cs:217-225`) |
| Selected child | n/a (descent commits a path instead) | `_selectedSubfolderIndex` (`BreadcrumbStateModel.cs:18`, public `:30`) |
| Display-collapse (separate) | `CollapsedAfterIndex` / `IsLeafExpanded` | `CollapsedAfterIndex` / `LeafExpanded` |
| JS-side | none — Efc doc posts unconditionally | `row.isSuggestion \|\| row.collapsed \|\| row.leafExpanded \|\| rowHasOpenAffordance(row)` (`FolderBreadcrumb.html:399-401`) |

The selected-node concept is correctly **distinct** from both the selected row and the
display-collapse state on both surfaces, exactly as the issue asked. It lives **per-surface**, on the
two parallel row types.

### Options and file-level cost

| Option | Change footprint | Assessment |
|---|---|---|
| **A. Relax the Qfc leaf-anchored guard (RECOMMENDED)** | 1 production file: `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs` (`:231-240`, drop `activeIndex.Value == row.Chain.Count - 1`, keep `_selectedSubfolderIndex < 0`). 2 test files updated + 1-2 tests added. | Minimal, surgical, brings Qfc to byte-equivalent semantics with the landed Efc branch. No new type, no new interface member, no HTML change, no file-size pressure. |
| **B. Hoist a shared transition helper into a new host-neutral type** | New file under `UtilitiesCS/OutlookObjects/Folder/`, plus edits to `BreadcrumbStateModel.cs`, `BreadcrumbBridgeRouter.Arrows.cs`, and adapters over two non-identical row types. | Rejected. The two row types differ in their key-attachment invariant (§2.6); a shared helper needs an adapter interface over both, which is a refactor with no defect-fixing content. #498 D9 already decided against it. |
| **C. Unify `BreadcrumbRow` and `BreadcrumbStateRow`** | Very large; touches every breadcrumb test file. | Rejected outright. Out of proportion to a Medium-severity keyboard defect, and would collide head-on with the two in-flight branches (§7). |

**Recommendation: Option A.**

---

## 5. The two open planning decisions

Both boundaries were already decided under #498 decision **D2**, and the decisions are locked by
landed acceptance criteria AC-23 (`[x]`) and AC-24 (`[x]`). The evidence below both confirms the
landed behavior and gives the reasoning to **retain** it.

### 5.1 Root-level Left — no-op vs. fall-through

**What Efc does today at that boundary:** `ActivateSegment` refuses (`BreadcrumbRow.cs:154-159`,
`segmentIndex < 0`), so the Left branch falls through to `row.LeftArrow()`
(`BreadcrumbBridgeRouter.Arrows.cs:82-85`), which collapses the trailing segment and finally returns
`false` at the root (`BreadcrumbRow.cs:304-308`). When it returns `false`, `PostRowRender` is not
called and **nothing is emitted at all** — the Efc bridge document has no `unhandledArrow` message
(§1, `BreadcrumbDocumentAssets.cs:102-107`). Efc root-Left is therefore a **silent no-op** and
structurally cannot be anything else.

**What Qfc does today:** falls through to `TryCollapseLeaf()`; on `false`,
`FolderBreadcrumbBridgeRouter.cs:389-395` emits `UnhandledArrowMessage`, which reaches
`KeyboardHandler.BreadcrumbArrowFallThrough` and calls `viewer.SetFolderDroppedDown(false)`
(`KeyboardHandler.cs:313`).

> **RECOMMENDATION: retain the fall-through on Qfc; retain the silent no-op on Efc.**
>
> **Reasoning.** (a) The asymmetry is intrinsic, not accidental: Qfc's breadcrumb is a *drop-down*
> that has a close gesture; Efc's is the whole form and has none. (b) `SetFolderDroppedDown(false)`
> is the keyboard-only way to dismiss the QuickFiler folder drop-down; suppressing it would strand
> keyboard users inside an open drop-down. (c) It is already ratified by #498 AC-24 and is asserted
> by a landed test at the interface seam,
> `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.cs:415`
> `Boundary_QfcUnhandledArrow_StillReachesBreadcrumbArrowFallThrough`.
>
> **User-visible consequence:** unchanged from today for Efc. For Qfc it *improves*: today the
> drop-down closes on the **second** Left (§2.3); after the fix it closes only after the user has
> actually walked to the root, which is the intended "one more Left past the top" gesture.

### 5.2 Childless-node Right — no-op vs. fall-through

**What Efc does today:** `TryRightTreeTransitionAsync` returns `false` when
`row.ActiveSegment?.HasSubfolders != true` (`BreadcrumbBridgeRouter.Arrows.cs:115`); the legacy path
then calls `ExpandLeafAsync`, which early-returns on the same condition
(`BreadcrumbBridgeRouter.Arrows.cs:167-171`). No message, no render — **silent no-op**.

**What Qfc does today:** `TryRightTreeTransition` returns `false`, `TryExpandLeaf()` returns `false`,
so `UnhandledArrowMessage` → `BreadcrumbArrowFallThrough` → `MyBox.ShowDialog("Pop Out Item or
Enumerate Conversation?", ..., viewer.Controller.RightKeyActions)` (`KeyboardHandler.cs:304-309`).

> **RECOMMENDATION: retain the fall-through on Qfc; retain the silent no-op on Efc.**
>
> **Reasoning.** `BreadcrumbArrowFallThrough`'s Right branch is the **only** keyboard entry point to
> the Pop Out / Enumerate Conversation dialog. Removing or re-gating it deletes a user-visible
> feature, which `issue.md:77` itself declares out of scope: *"Removing it would be a user-visible
> change to the Pop Out / Enumerate Conversation entry point, which is out of scope for this bug
> unless the maintainer decides otherwise."* No maintainer decision to the contrary exists in the
> tree (searched `docs/features/active/**/spec.md` and `docs/features/archive/**/spec.md` for
> "Pop Out"; only the out-of-scope statements and #498 AC-24's preservation clause appear).
>
> **User-visible consequence:** unchanged. Right on a genuinely childless node still offers the
> dialog on Qfc and is still inert on Efc.

**Net:** neither open decision requires a behavior change. Both should be recorded in `spec.md` as
*already decided and preserved*, with AC-23/AC-24 cited, so a reviewer does not reopen them.

---

## 6. Reconciliation with issue #400

**Where #400 landed:** `docs/features/archive/2026-07-21-quickfiler-folder-selector-dropdown-400/`
(archived; the pre-#498 branches in §7 still carry it under `docs/features/active/`, which is one
independent confirmation that they are stale).

**The reconciliation has already been performed and does not need redoing.** #498's spec contains a
reviewer-findable section `#400 AC-9 supersession record` at
`docs/features/active/breadcrumb-router-navigation-defects-498/spec.md:304-311`, and the execution
evidence is at
`docs/features/active/breadcrumb-router-navigation-defects-498/evidence/qa-gates/p7-t7-ac21-supersession-record.md`.
Its disposition table:

| Clause of #400 AC-9 (`spec.md:247`) | Disposition |
|---|---|
| "Left and Right preserve the existing breadcrumb **expand, collapse** ... behavior" | **RETRACTED IN PART** — only for rows whose resolved chain has >1 segment, and only to the extent a tree transition is attempted first |
| "... and **unhandled-key** behavior ..." | **PRESERVED** |
| "... do not mutate the committed/original/pending **selector session**" | **PRESERVED** |
| #400 AC-5 through AC-8 (Up/Down/Enter/Escape) | **PRESERVED** |

**Action for #440's spec: cite this record; do not author a second supersession.** The residual fix
in §2.3 falls entirely inside the already-retracted clause (it is the same retraction, applied one
step further up the chain), so it needs no new retraction.

### Tests asserting the CURRENT Left/Right semantics that the residual fix would break

This is the load-bearing list. Two tests will go RED under Option A, both on Qfc, both because they
were written against the one-step limit. Traces are against a three-segment chain
(`ThreeSegmentChain()` at `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelTests.cs:29-37`;
`ModelWithSuggestion()` at `:39-45`), active index starting at 2.

1. **`UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs:368`
   `Route_LeftArrow_NothingToCollapse_ReportsUnhandledLeft`** — **WILL BREAK.** It sends two Lefts
   and asserts the second yields `UnhandledArrowMessage(Left)`. Under the fix the second Left becomes
   `ActivateSegment(0)` and is handled. Its Arrange comment (`:370-371`) explicitly names the
   one-step limit and must be rewritten. *Repair:* drive to the root first (three Lefts on a
   three-segment chain), then assert the unhandled report on the next press.

2. **`UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs:61`
   `Arrows_RightExpandsThenLeftCollapses_UnhandledWhenNothingChanges`** — **WILL BREAK** at `:76`
   (`model.LeftArrow().Should().BeFalse()`). Trace: Right → `TryExpandLeaf` true; Right → false;
   Left #1 → `ActivateSegment(1)` returns **true** and incidentally clears `LeafExpanded`, so `:74`
   and `:75` still pass today; Left #2 → gate fails today so `:76` passes, but under the fix
   `ActivateSegment(0)` returns **true**. *Repair:* either extend the sequence to the root, or
   re-point the "nothing changes" assertion at a single-segment row.

**Tests verified NOT to break** (checked individually, not assumed):

- `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbSelectionSessionTests.cs:175`
  `BreadcrumbLeftAndRightTransitions_DoNotMutateSelectorSession` — safe. Its row is a **single**
  segment (`:180-183`), so no tree transition is ever reachable.
- `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs:125`
  `LeftArrow_WithSubfolderSelected_ResetsSubfolderSelectionAndCollapses` — safe, provided the
  `_selectedSubfolderIndex < 0` clause is **retained**. Removing that clause too would break it.
  **The fix must drop only the leaf-anchored clause.**
- `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelTests.cs:329`
  `LeftArrow_QfcMultiSegmentRow_SelectsParentNode` — safe (single Left).
- `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelTests.cs:376`
  `ArrowKey_QfcSingleSegmentRow_TakesPreExistingCollapsePath` — safe (single segment).
- `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs:94`
  `Arrows_WithNoSelection_AreUnhandled` — safe (no selection).
- `QuickFiler.Test/Viewers/FolderBreadcrumbAssetContractTests.cs:360`
  `LeftAndRightBreadcrumbMessages_RemainSupported` — safe. It asserts only the four message-shape
  strings (`:363-366`); Option A changes no HTML. #498 AC-19 requires this file stay out of the diff.
- All Efc tests in `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.cs:182-427` — safe;
  Option A does not touch the Efc surface.

---

## 7. Concurrent in-flight work (MANDATORY finding)

**Tooling note:** no Bash/shell tool was available in this session, so `git log` and `git diff` could
not be executed. Equivalent evidence was obtained by reading git metadata and the branches' live
worktrees directly. Every claim below is a direct file read, not a diff.

**Both branches exist and are checked out in live worktrees:**

| Branch | SHA (`.git/packed-refs`) | Live worktree |
|---|---|---|
| `feature/quickfiler-breadcrumb-bridge-coverage-r2` | `09dfac9ab69859805cccd337e822a7dc54c5cc67` | `.claude/worktrees/agent-aca320624821a4ad1` |
| `feature/quickfiler-per-file-coverage-capstone-r2` | `b5316040e357d41d88b2d622a5c1030c3f4e7771` | `.claude/worktrees/agent-a24c84de174a27784` |

### 7.1 Both branches are cut from a base far older than `origin/main`

Direct file-existence evidence, identical for both worktrees:

- `QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs` — **ABSENT.** So is
  `BreadcrumbBridgeRouter.Selection.cs`. `BreadcrumbBridgeRouter.cs` is still the pre-split
  monolith (the #495 spec baseline table records it at **450** lines; `main` now has 304 + 211 + 209
  across three partials).
- Grep for `440` across all `*.cs` in `agent-aca320624821a4ad1` — **no files found.** Neither branch
  contains any part of the #440 implementation.
- Test files present on `main` but **absent** on both branches:
  `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs`,
  `BreadcrumbBridgeRouterIssue614Tests.cs`,
  `BreadcrumbBridgeRouterQueueTests.Part2.cs`,
  `BreadcrumbBridgeRouterTests.Selection.cs`,
  `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterReplaceItemsTests.cs`.
- Feature-folder skew: both branches still carry
  `docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/` (archived on `main`) and
  neither carries `docs/features/active/breadcrumb-router-navigation-defects-498/`,
  `.../2026-08-07-efcviewer-missing-lineage-and-segment-navigation-439/`, or this #440 folder.

**Conclusion: these branches predate #439 (PR #605), #498, #499, #614 and the entire #440
implementation.**

### 7.2 What each branch touches in #440's scope

**`feature/quickfiler-breadcrumb-bridge-coverage-r2` = issue #495**
(`docs/features/active/2026-08-08-quickfiler-breadcrumb-bridge-coverage-495/spec.md` on that branch).
Its declared target (`spec.md:40`, `:100`) is to take
`QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` from 97.87% line / 92.22% branch to **100% / 100%**.
Its tests live in `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterQueueTests.cs` (`spec.md:214`)
and `BreadcrumbBridgeRouterTests.cs`. `spec.md:222` names `BreadcrumbBridgeRouter` **and
`BreadcrumbRow`**; `spec.md:235` explicitly distinguishes `FolderBreadcrumbBridgeRouter` as a
different type.

> **Direct collision with #440.** It rewrites/extends the exact two Efc test files that hold the
> landed #440 Efc tests, against a version of `BreadcrumbBridgeRouter.cs` that no longer exists.
> Its coverage numbers (450-line single file, `:288`, `:372`, `:426`, `:434` J5/J6 branch targets in
> `spec.md:133`) are all computed against the pre-split file and are **void**.

**`feature/quickfiler-per-file-coverage-capstone-r2` = issue #497**
(`docs/features/active/2026-08-08-quickfiler-per-file-coverage-capstone-497/spec.md`). Grep for
`BreadcrumbBridgeRouter|BreadcrumbRow|BreadcrumbStateModel|FolderBreadcrumb|KeyboardHandler.cs` in
that spec returns **no matches**. Per `spec.md:27-28` it *"owns no production files, adds no coverage,
and remediates nothing. Its deliverable is evidence."* It re-derives the denominator from
`QuickFiler/QuickFiler.csproj` at execution time (`spec.md:35-36`).

> **No direct file collision with #440.** Indirect only: it is a verification gate over the epic and
> its ledger, and its own baseline is stale. If it runs against `main` it will re-derive the
> denominator correctly and will simply report the three new partial files as unledgered.

### 7.3 Explicit statement for the planner

**Files #440's plan must expect to be reshaped:**

| File | Reshaped by | Risk to #440 |
|---|---|---|
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` | #495 (r2 branch), against the pre-split 450-line form | **HIGH** — but #440 Option A does not touch it |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs` | #495 has no concept of this file; a naive merge deletes the #440 Efc transitions | **HIGH** |
| `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.cs` | #495 | **HIGH** — holds all five landed #440 Efc tests |
| `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterQueueTests.cs` | #495 | MEDIUM |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs` | #495 names it in scope (`spec.md:222`) | MEDIUM |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs` | **neither branch** | **LOW — this is #440's only production file under Option A** |
| `UtilitiesCS.Test/.../BreadcrumbStateModelSequenceTests.cs`, `FolderBreadcrumbBridgeRouterTests.cs` | **neither branch** | **LOW** |

**How this should change #440's approach — three concrete instructions:**

1. **Take Option A and nothing wider.** Its entire production footprint is
   `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs`, plus two test files, **none of which
   either in-flight branch touches.** Options B and C would put #440 squarely inside #495's blast
   radius. This is the single strongest argument for Option A.
2. **Do not rebase onto, cherry-pick from, or merge either branch.** Both are pre-#439/#498/#614.
   Per the repository's recorded fan-in hazard, an older-base branch **deletes** what `main` gained
   meanwhile with no merge conflict. Merging `feature/quickfiler-breadcrumb-bridge-coverage-r2` as-is
   would silently revert the #440 Efc implementation, the #439 lineage fix, and the #614 stem
   guards.
3. **Flag #495 as requiring a rebuild on `main`, not a conflict resolution**, and say so in #440's
   spec Risks section. Its coverage baseline, its line-number citations, and its whole file model are
   void. #440 should not attempt to fix this, but must not be planned as though #495 will land
   cleanly beside it.

---

## 8. Existing test inventory for these surfaces

All paths repository-relative, at `b56400ab`. Line counts in parentheses.

**Efc arrow / row surface — `QuickFiler.Test/`:**
- `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.cs` (462) — **holds the landed #440 Efc tests**
  at `:207`, `:242`, `:270`, `:305`, `:344`, `:376`, and the Qfc boundary test `:415`.
- `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.Selection.cs` (—) — Up/Down/selection arrows `:24`, `:131`, `:144`, `:205`, `:227`.
- `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterQueueTests.cs` / `.Part2.cs` — outbound queue + the `async void` host-event seam.
- `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs`, `...Issue614Tests.cs`.
- `QuickFiler.Test/Viewers/FolderBreadcrumbAssetContractTests.cs` (405) — Qfc HTML resource contract, `:360`.
- `QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorTests.cs` — `:260`, `:275`, `:290`.
- `QuickFiler.Test/Viewers/BreadcrumbSelectorCoordinatorTests.cs` — `:124`.
- `QuickFiler.Test/Controllers/QfcItemControllerBreadcrumbDropDownTests.cs` — `:156` fall-through routing.

**Qfc arrow / row surface — `UtilitiesCS.Test/`:**
- `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelTests.cs` (454) — **primary home for #440 Qfc state tests**; shared helpers `ThreeSegmentChain` `:29`, `ModelWithSuggestion` `:39`; #440 tests `:329`, `:352`, `:376`, `:419`, `:435`.
- `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs` — **partial of the same class** `BreadcrumbStateModelTests` (`:16`); sequence tests `:61`, `:80`, `:94`, `:125`.
- `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs` (495) — **primary home for #440 Qfc router tests**; `:348`, `:368`, `:447`, `:478`; strict provider mock `:418`.
- `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbRowStateTests.cs` (379) — Efc `BreadcrumbRow` state machine; `:223`, `:235`, `:246`, `:256`, `:269`, `:281`.
- `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSelectorTests.cs` — #400 selector session (regression-only for #440).
- `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbSelectionSessionTests.cs` — `:175`.
- `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterEdgeTests.cs`, `...InFlightTests.cs`, `...ReplaceItemsTests.cs`, `FolderBreadcrumbRouterSelectionConcurrencyTests.cs`.
- `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbMessageCodecTests.cs`, `BreadcrumbBridgeMessagesTests.cs` — arrow message round-trips.
- `UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTests.cs` — the D5 suffix fallback.

**Not to be confused with the above** (different, older state machines; not in #440's scope):
`UtilitiesCS.Test/OutlookObjects/Folder/FolderTreeStateModelTests.cs` (`:189`, `:201`, `:214`, `:232`, `:249`) and
`UtilitiesCS.Test/OutlookObjects/Folder/FolderSuggestionTreeStateTests.cs` (`:89`-`:164`). Both carry
`RightArrow`/`LeftArrow` test names but exercise `FolderTreeStateModel` / `FolderSuggestionTreeState`.

**Live-Outlook / STA requirements: NONE on the #440 path.** Negative evidence: grep for
`STAThread|STA|Outlook.Application|RequiresLiveOutlook|Ignore]` in
`QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.cs` returned no matches;
`BreadcrumbStateModelSequenceTests.cs:13-15` states "Deterministic; no Outlook, WebView2, timers, or
temp files." All four files the planner needs are pure Moq + FluentAssertions + MSTest.
(Caveat outside #440's scope: `QuickFiler.Test/Viewers/BreadcrumbDropDown*` and
`WebView2BreadcrumbHost*` construct real WebView2 controls and are WinForms-bound; #440 needs none of
them. Repo convention runs `QuickFiler.Test` serially in CI.)

**Placement guidance:** add the residual-defect tests to the two files that already own the Qfc
contract — `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelTests.cs` (state level) and
`UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs` (router level).
**Invent no new test file.** Watch file size: `FolderBreadcrumbBridgeRouterTests.cs` is at 495/500
and `BreadcrumbStateModelTests.cs` at 454/500, so the router-level test may force a partial split —
the precedent already exists (`BreadcrumbStateModelSequenceTests.cs` is a partial of
`BreadcrumbStateModelTests`).

---

## 9. Issue #439 lineage dependency — DISCHARGED on both surfaces

`issue.md:81` says parent selection is meaningful only once rows carry a resolved multi-segment
ancestor chain, and asks whether #440 must be scoped to rows whose chain already resolves.

**Both halves of that dependency are satisfied on `main`. No scoping restriction is required.**

- **Efc:** #439 landed. `docs/features/active/2026-08-07-efcviewer-missing-lineage-and-segment-navigation-439/`
  holds a complete audit trail (`plan.2026-08-24T17-30.md`, `code-review.2026-08-24T22-20.md`,
  `feature-audit`, `policy-audit`, `remediation-plan.2026-08-24T22-25.md`) and the code is present:
  `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs` exists, and
  `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbHtmlRendererTests.cs:42`
  `Issue439ResolvedLineageUsesUnicodeArrowSeparators` asserts the resolved-lineage rendering.
  #498 `spec.md:57` records the fix at feature commit `c39db103` (PR #605).
- **Qfc:** the analogous chain resolution was delivered by #498 decision **D5** as an explicit
  prerequisite for #440's Qfc half. `UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs`
  now falls back to `ResolveByUniqueSuffix` (`:79`, `:90-111`) when the exact match misses, accepting
  the suffix match **only when unique**. `FolderBreadcrumbBridgeRouter.cs:52-71` consumes it
  (`ResolveLeafKeyAsync` → `GetAncestorChainAsync`) and builds a multi-segment `BreadcrumbStateRow`
  while preserving the presented filing target (`:63-69`, decision D7). Covered by
  `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs:110-118`
  `SetSuggestionsAsync_StrictProvider_ResolvesMultiSegmentChain`, whose doc comment states the chain
  "is what gives Left and Right a parent to navigate to". #498 AC-7 through AC-11 and AC-14 are all
  `[x]`.

**Carry-forward warning (from prior research on this cluster, still current).** #498 decision D7
deliberately kept the Qfc filing target on the **presented** path rather than the newly-resolved full
Outlook path (`FolderBreadcrumbBridgeRouter.cs:63-64`). This exists because the suggestion
probability is joined on the presented text and the filing target is consumed as
`DestinationOlStem`. **#440 must not "improve" the filing target to use `ActiveSegment.FullPath`;**
doing so silently drops every suggestion percentage and changes the filing destination. Option A
touches none of this — a further reason to prefer it.

---

## Automation Feasibility

**The residual fix and its verification are fully automatable. No human interaction is required.**

| Activity | Automatable | Basis |
|---|---|---|
| Reproduce the defect (RED test) | **Yes** | `BreadcrumbStateModel` is pure and host-neutral; `IFolderHierarchyProvider` is constructor-injected and already mocked with `MockBehavior.Strict`. No Outlook, no WebView2, no STA, no temp files. |
| Implement Option A | **Yes** | One boolean clause in one method. |
| `dotnet tool run csharpier format .` / `check .` | Yes | Standard. |
| `msbuild ... /p:EnableNETAnalyzers` and `... /p:TreatWarningsAsErrors` | Yes | Standard. Do **not** add `/p:Nullable=enable` (diverges from CI and cannot pass). |
| `vstest.console.exe ... /EnableCodeCoverage` | Yes | Add `/InIsolation` and exclude `\.claude\` worktree paths from assembly discovery, per repo convention. |
| Repair the two RED-turning tests (§6) | Yes | Both are pure model/router tests. |
| Confirm #400 selector contract unchanged | Yes | `BreadcrumbStateModelSelectorTests.cs`, `BreadcrumbSelectionSessionTests.cs`, `FolderBreadcrumbAssetContractTests.cs` all run headless. |

**Requires a human, but only as optional confirmation, not as a gate:**

- **Live-Outlook manual verification of the end-to-end keyboard gesture** — pressing Left twice in a
  real QuickFiler drop-down against a real mail store and observing that the drop-down no longer
  closes. This cannot be automated: it needs a live `Microsoft.Office.Interop.Outlook.Application`,
  a real WebView2 render, and real focus. It is **confirmatory only**; the state-machine and router
  tests fully determine correctness of the change, and `issue.md:96` already lists this as a manual
  note rather than an acceptance gate.
- **Maintainer ratification** is *not* needed for the boundary decisions (§5): both were already
  ratified under #498 AC-23/AC-24. A maintainer decision *would* be required only if the planner
  chose to change the Pop Out / Enumerate Conversation entry point or to unify the Right descent
  semantics (§2.4) — this research recommends neither.

---

## Recommended `spec.md` shape (summary for the planner)

1. **Rewrite §Root Cause Analysis wholesale.** All eight inherited citations are stale (§1); six are
   flatly wrong about current behavior.
2. **Restate the defect narrowly:** Qfc Left performs the parent-select transition once, then closes
   the folder drop-down; Efc walks the chain. `BreadcrumbStateModel.cs:235`.
3. **In scope:** `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs`;
   `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelTests.cs`;
   `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs`;
   `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs`.
4. **Out of scope (state explicitly):** all Efc files; `FolderBreadcrumb.html`; `KeyboardHandler.cs`;
   `IFolderHierarchyProvider.cs`; `BreadcrumbSelectionSession.cs`;
   `BreadcrumbBridgeCoordinator.cs`; `ItemViewer*`; the Right-descent commit semantics (§2.4); the
   single-level Right descent limit (§2.5); the D7 filing-target form (§9).
5. **ACs must include:** repeated Left walks Qfc to the root; Left at the Qfc root still reaches
   `BreadcrumbArrowFallThrough`; Right on a childless Qfc node still reaches it; the two named tests
   are updated with recorded rationale; `FolderBreadcrumbAssetContractTests.cs` stays out of the
   diff; the #400 AC-9 supersession record is **cited, not re-authored**.
6. **Risks:** the §7 stale-branch collision, stated as "rebuild #495 on `main`, never merge as-is".
