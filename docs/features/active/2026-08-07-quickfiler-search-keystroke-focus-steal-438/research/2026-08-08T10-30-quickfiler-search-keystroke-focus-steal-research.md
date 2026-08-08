# Research — QuickFiler Folder-Search Keystroke Focus Steal (Issue #438)

- Issue: #438
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/438
- Feature folder: `docs/features/active/2026-08-07-quickfiler-search-keystroke-focus-steal-438/`
- Requirement source: `issue.md` (work mode: full-bug)
- Timestamp: 2026-08-08T10-30
- Author: task-researcher
- Read-only investigation. No production code or tests were modified.

## Evidence Classification Legend

- **[VERIFIED]** — read directly from a repository file in this session, with `file:line` citation.
- **[INFERRED]** — a conclusion drawn from verified facts plus documented .NET/WinForms behavior. Limits stated.
- **[UNVERIFIED]** — could not be established with the tools available (no build, no debugger, no live Outlook this session).

---

## 1. Root Cause, Confirmed Against Current Source

The issue's 2026-08-07 code-read is accurate, and this session found a second focus-steal mechanism the issue did not record: the per-keystroke `ClearFolderItems()` closes the open selector session, which closes the native popup, and **the close path also transfers focus** (to the collapsed breadcrumb anchor) via `_focusAnchor`. The defect is therefore a composition of three verified behaviors.

### 1.1 The per-keystroke handler

`QfcItemController.TextBoxSearch_TextChanged` (`QuickFiler/Controllers/QfcItemController.EventHandlers.cs:164-178`) **[VERIFIED]** runs on every keystroke (wired at `QfcItemController.EventWiring.cs:77-79`) and unconditionally issues, in order:

1. `_itemViewer.ClearFolderItems()` (line 172)
2. `_itemViewer.SetFolderItems(folders)` (line 173)
3. `_itemViewer.SetFolderSelectedIndex(1)` when `folders.Length >= 2` (lines 175-176)
4. `_itemViewer.SetFolderDroppedDown(true)` (line 177)

No branch leaves focus with the sender.

### 1.2 Focus transfer on open — all three coordinator configurations

`ItemViewer.SetFolderDroppedDown(bool)` forwards to `SetBreadcrumbDropDownState(droppedDown)` (`QuickFiler/Viewers/ItemViewer.FolderSearch.cs:31-32`) **[VERIFIED]**. From there:

- **Bare viewer (no lifecycle coordinator).** `SetBreadcrumbDropDownState` (`QuickFiler/Viewers/ItemViewer.Breadcrumb.cs:223-235`): when `_breadcrumbLifecycleCoordinator == null` and `droppedDown` is true, it calls `FocusBreadcrumb()` (line 229) → `FocusBreadcrumbCore()` (lines 200-209, 211-221) → `_l0vhBreadcrumb_WebView2.Focus()` (line 219). Direct focus steal. **[VERIFIED]**
- **Lifecycle coordinator, no open coordinator.** `BreadcrumbItemViewerLifecycleCoordinator.SetDroppedDown` (`QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs:176-189`): when `_openCoordinator == null` and `droppedDown` is true, it calls `Focus(focus)` (line 183) → posts `FocusBreadcrumbCore` (lines 162-174). Focus steal. **[VERIFIED]**
- **Lifecycle + open coordinator (normal production configuration).** `SetDroppedDown` delegates to `BreadcrumbDropDownOpenCoordinator.SetDroppedDown(true)` (`QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs:100-117`), which calls `_openSelector()`; the resulting `SelectorOpenStateChanged` event reaches `HandleSelectorOpenStateChanged` (lines 119-132) → `RequestOpen()` (lines 84-98) → `OpenCoreAsync` → `BeginOpenCore` → `_host.OpenAsync(...)` (line 195). Two focusing hops then exist:
  - **Fresh open:** `BreadcrumbDropDownOpenLifetime.OpenCoreAsync` (`QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs:214-254`) ends with `FocusCurrentSurface(lease)` (lines 245-247, 287-305), which calls `_host.FocusPending()` (line 294) → `_focusPending` = `() => host.ControlHost?.Control.Focus()` supplied at `ItemViewer.Breadcrumb.cs:164`. The popup WebView2 control receives focus. **[VERIFIED]**
  - **Re-issued open while already open:** `BreadcrumbDropDownHost.OpenAsync` (`QuickFiler/Viewers/BreadcrumbDropDownHost.cs:228-242`): when `OpenState` is already true it executes `_openLifetime.Schedule(_focusPending)` (line 237) — an open request on an open popup is *defined* as "focus the popup again". **[VERIFIED]**

### 1.3 Focus transfer on close — the mechanism the issue missed

On every keystroke **after** the drop-down is open, step 1 (`ClearFolderItems`) closes the selector session before step 4 reopens it:

- `BreadcrumbBridgeCoordinator.Clear()` (`QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs:150-157`) → `FolderBreadcrumbBridgeRouter.Clear()` (`UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs:168-173`) → `BreadcrumbSelectionSession.ClearSelector()` (`UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs:161-174`), which calls `Cancel()` and reports `OpenStateChanged` when the session was open. **[VERIFIED]**
- That event reaches `BreadcrumbDropDownOpenCoordinator.HandleSelectorOpenStateChanged` (`BreadcrumbDropDownOpenCoordinator.cs:119-132`); since the selector is now closed it runs `CloseCore(ExplicitCommit)` (lines 237-267) → `_host.Close(...)` → `CompleteClose` → `FinishClose` (`BreadcrumbDropDownHost.cs:385-399, 427-437`), which always invokes `_focusAnchor` — wired to `FocusBreadcrumbCore` at `ItemViewer.Breadcrumb.cs:165`. The collapsed breadcrumb WebView2 receives focus, again stealing it from the textbox. **[VERIFIED]**

### 1.4 Net behavior

- **First keystroke** (drop-down closed): session opens, native popup opens, `FocusCurrentSurface` focuses the popup. The caret leaves `TxtboxSearch`; subsequent characters go to the popup. This matches "after roughly one to two characters".
- **Any keystroke typed while the drop-down is open** (user clicked back into the textbox): the popup is closed (focus → collapsed anchor) and reopened (focus → popup) on each keystroke, with visible popup churn.

Any fix that suppresses only the open-side focus (`_focusPending`) still loses focus through the close-side `_focusAnchor`, because the search path closes/reopens the session on every keystroke. **The fix must therefore both (a) stop the per-keystroke close/reopen cycle and (b) make the search-driven open non-focusing.**

---

## 2. The `SetFolderSelectedIndex(1)` Per-Keystroke Call (Question 4)

`SetFolderSelectedIndex(1)` → `BreadcrumbBridgeCoordinator.SelectRow(1)` (`ItemViewer.FolderSearch.cs:27`; `BreadcrumbBridgeCoordinator.cs:175-178`) → `BreadcrumbSelectionSession.SelectRow` (`BreadcrumbSelectionSession.cs:176-183`) **[VERIFIED]**:

- It mutates `_model.SelectRow(index)` directly — the model's selected row is what the collapsed surface renders and what `GetSelectedFolder()` returns (`FolderBreadcrumbBridgeRouter.cs:213-214, 225-235`). So each keystroke immediately makes the collapsed surface display row 1 of the *partial* result set — the user-visible "view jumps to the open folder" symptom.
- It always returns `SelectionChanged | RenderRequired`, so `FolderSelectionChanged` fires (`BreadcrumbBridgeCoordinator.cs:278-293`), and the controller handler `CboFolders_SelectedIndexChanged` (`QfcItemController.EventHandlers.cs:209-212`, wired at `EventWiring.cs:86`) caches `_selectedFolder = GetSelectedFolder()` — **the controller-side selection is committed on every keystroke.**
- Session accounting: on the first keystroke the session is closed at that instant, so `SynchronizeCommittedSelection` (`BreadcrumbSelectionSession.cs:110-116`) also updates `CommittedIdentity` — a genuine commit. On later keystrokes (session open) `CommittedIdentity` is not updated, but the model mutation above still changes what the collapsed surface reports.
- Stale-cache defect: `CancelSelector` effects are `Handled | OpenStateChanged | RenderRequired` **without** `SelectionChanged` (`BreadcrumbSelectionSession.cs:297-304`). After an Escape/uncommitted close the model is restored to the original identity, but no `FolderSelectionChanged` fires, so the controller's `_selectedFolder` keeps the partial-search row 1 value. **[VERIFIED]**

**Conclusion:** yes, this call must change as part of the fix. While the search-driven selector session is open, per-keystroke highlighting must set only the session's *pending* identity (the same mechanism open Up/Down uses — `Move` at `BreadcrumbSelectionSession.cs:400-429` sets `PendingIdentity` when open), publish no `SelectionChanged`, and leave the committed model selection untouched. This directly implements the issue's expected behavior: "updates its rows and its highlighted row without taking focus and without committing a folder selection."

---

## 3. Design Options (Question 2)

The analysis in §1.4 splits the defect into two independent mechanisms, so single-mechanism options are evaluated first and rejected, and the recommendation is the minimal union.

### Option 1 — Non-focusing open intent only (`SetFolderDroppedDown(bool droppedDown, bool takeFocus)` overload)

Thread a `takeFocus` flag from the controller down to the host: controller → `IItemViewer` overload → `ItemViewer.FolderSearch.cs` / `ItemViewer.Breadcrumb.cs` → `BreadcrumbItemViewerLifecycleCoordinator.SetDroppedDown` → `BreadcrumbDropDownOpenCoordinator` (latched intent, because the actual `RequestOpen` arrives via the `SelectorOpenStateChanged` event, not the `SetDroppedDown` call) → `IBreadcrumbDropDownHost.OpenAsync` overload → `BreadcrumbDropDownHost` (suppress `Schedule(_focusPending)` in the already-open branch) → `BreadcrumbDropDownOpenLifetime` (skip the `FocusPending` step of `FocusCurrentSurface`).

- Files changed: ~9 production files. `IItemViewer` changes additively (overload). `IBreadcrumbDropDownHost` (public interface) changes additively.
- Test seam: the existing delegate-counting host harness (`QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.cs:394-443` counts `FocusPendingCount`/`FocusAnchorCount`) plus `Mock<IItemViewer>` at the controller.
- **Rejected as a complete fix:** the per-keystroke `Clear()` still closes the open session, so `FinishClose` still fires `_focusAnchor` (§1.3) and the popup still flickers closed/open per keystroke. The per-keystroke commit (§2) is also unaddressed. Its focus-flag transport is, however, a required *component* of the recommendation.

### Option 2 — Idempotent refresh only (replace rows without closing the session)

Give the search path a row-replacement operation that preserves the open selector session instead of `Clear()`+`AddItems`. The router already has the primitives: `SetItems` replaces plain rows (`FolderBreadcrumbBridgeRouter.cs:119-135`, currently unreachable from the coordinator — no caller wires it) and `ReplaceRowsPreservingSession`/`ReconcileRowsReplaced` (`FolderBreadcrumbBridgeRouter.cs:474-478`; `BreadcrumbSelectionSession.cs:119-147`) already reconciles committed/original/pending identities across an atomic row swap for the suggestions path. A new router transition ("replace plain rows preserving session", emitting `RenderRequired` but no `OpenStateChanged`) plus a coordinator member and viewer intent removes the close/reopen cycle entirely: keystroke N+1 refreshes rows while the popup stays open.

- Files changed: ~6 production files (router, session, bridge coordinator, `IItemViewer`, `ItemViewer.FolderSearch.cs`, controller). No host/lifetime/host-interface change.
- Test seam: host-neutral router/session unit tests (pattern: `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter*Tests.cs`).
- **Rejected as a complete fix:** the *first* keystroke still opens the popup through the normal open pipeline, and `FocusCurrentSurface` (`BreadcrumbDropDownOpenLifetime.cs:287-305`) focuses the popup on that fresh open. Focus is stolen on keystroke one — the primary reported symptom.

### Option 3 — Composite search-presentation intent with an explicit focus intent through the open pipeline (RECOMMENDED)

Combine the minimal parts of Options 1 and 2 behind **one new controller-facing intent member**, and change the per-keystroke highlight to pending-only (§2):

1. **New `IItemViewer` member** (additive), e.g. `void PresentFolderSearchResults(string[] items)`. `TextBoxSearch_TextChanged` reduces to `FindFolder` + this single call. One member is narrower for the controller seam than three new primitives, and it moves the open/highlight sequencing into the coordinator layer that owns the posted-operation queue (sequencing `SetFolderDroppedDown` — which defers through `PostAsync` — against the synchronous `SelectRow` from the controller is otherwise racy by construction).
2. **Coordinator composite** `BreadcrumbBridgeCoordinator.PresentSearchResults(items)`: (a) router `ReplaceItemsPreservingSession(items)` (new transition per Option 2); (b) `OpenSelector()` if the session is closed (existing member, `BreadcrumbBridgeCoordinator.cs:199`); (c) set the session's *pending* identity to the first selectable row (new session transition `HighlightRow`, pending-only, no `SelectionChanged`). Ordering (a)→(b)→(c) guarantees the highlight is applied to an open session so it can never commit; Escape restores the pre-search committed identity per the existing `Cancel` semantics.
3. **Focus intent through the open pipeline** (Option 1 transport, scoped): `BreadcrumbDropDownOpenCoordinator` latches "next native open takes no focus" when the open originates from the search path; the latch is deterministic because `SetDroppedDown`-posted work and `HandleSelectorOpenStateChanged`-posted work execute FIFO on the same `BreadcrumbPopupUiOperations` queue. The flag crosses the host boundary as an additive `IBreadcrumbDropDownHost.OpenAsync(anchor, workingArea, size, bool takeFocus)` overload; `BreadcrumbDropDownHost.OpenAsync` skips `Schedule(_focusPending)` in the already-open branch and passes the flag to `BreadcrumbDropDownOpenLifetime`, which makes the `_host.FocusPending()` call inside `FocusCurrentSurface` conditional (the `LastInitializationException = null` step and the open-result contract are unchanged). The existing 3-parameter `OpenAsync` delegates with `takeFocus: true`, so every existing caller and test keeps its exact semantics.
4. **Bare-viewer / no-open-coordinator branches**: the non-focusing variant of `SetBreadcrumbDropDownState` skips `FocusBreadcrumb()` / `Focus(focus)` in the two fallback branches (`ItemViewer.Breadcrumb.cs:223-235`; `BreadcrumbItemViewerLifecycleCoordinator.cs:176-189`).
5. **Unchanged:** `TextBoxSearch_KeyDown` (`QfcItemController.EventHandlers.cs:180-189`) keeps issuing `SetFolderDroppedDown(true)` + `FocusFolderDropDown()`; `JumpToFolderDropDown`/`Async` (`QfcItemController.Navigation.cs:27-49`) unchanged; mouse toggle unchanged; `AssignFolderComboBox`'s `SetFolderSelectedIndex(1)` on the suggestions path (`QfcItemController.FolderHandling.cs:202`) unchanged and out of scope.

- Files changed (production, estimated 12): `QfcItemController.EventHandlers.cs`; `IItemViewer.cs`; `ItemViewer.FolderSearch.cs`; `ItemViewer.Breadcrumb.cs`; `BreadcrumbItemViewerLifecycleCoordinator.cs`; `BreadcrumbDropDownOpenCoordinator.cs`; `IBreadcrumbDropDownHost.cs`; `BreadcrumbDropDownHost.cs`; `BreadcrumbDropDownOpenLifetime.cs`; `BreadcrumbBridgeCoordinator.cs`; `FolderBreadcrumbBridgeRouter.cs`; `BreadcrumbSelectionSession.cs`. All changes are additive to public/internal contracts; no existing signature is removed or altered.
- `IItemViewer` public contract: one additive member. `IBreadcrumbDropDownHost` public contract: one additive overload (all implementations are in-repo; Moq interface mocks regenerate automatically).
- Test seams: (a) `Mock<IItemViewer>` at the controller (existing pattern, `QfcItemController.EventHandlersTests.cs:314-350`); (b) headless `ItemViewer` + `Mock<IBreadcrumbDropDownHost>` integration harness (existing `ItemViewerDropDownHarness`, `QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs:328-473`); (c) real-host delegate-count harness (`BreadcrumbDropDownHostTests.cs:394-443`, `FocusPendingCount`/`FocusAnchorCount`); (d) host-neutral router/session tests in `UtilitiesCS.Test`.

**Rejected alternatives (summary).** Option 1 alone: leaves the close-side `_focusAnchor` steal and per-keystroke popup churn/commit. Option 2 alone: leaves the first-open `FocusCurrentSurface` steal. Also rejected: (i) removing focus from the open pipeline entirely and making callers focus explicitly — breaks #400 AC-13 for mouse-toggle opens, whose only popup-focus path is `_focusPending` (the Down-arrow `FocusFolderDropDown()` focuses the collapsed anchor, not the popup: `ItemViewer.FolderSearch.cs:36` → `FocusBreadcrumb`); (ii) a viewer-owned "suppress next focus" latch around the existing delegates without a pipeline parameter — implicit cross-layer state, non-deterministic against in-flight opens, contrary to the interface-seam-first rule in `.claude/rules/csharp.md`; (iii) controller-side guard "only open when not already open" — the controller has no open-state getter on `IItemViewer`, and it still focuses on the first open.

---

## 4. Interaction With Issue #400 (Question 3)

Source: `docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/spec.md` (acceptance criteria at lines 239-257) **[VERIFIED]**. The constraining ACs and the recommendation's effect:

| #400 AC | Constraint | Option 3 effect |
|---|---|---|
| AC-3 | Button activation and `SetFolderDroppedDown(true)` open the native popup | **Preserved.** `SetFolderDroppedDown(bool)` is unchanged (signature pinned by `ItemViewerBreadcrumbDropDownContractTests.cs:77-100`); the search path uses a new additive intent. |
| AC-5 | Closed Up/Down commit immediately | **Preserved.** Session `Move` semantics untouched. |
| AC-6 | Open snapshots `original`; open Up/Down change only `pending` | **Preserved and reused.** The search highlight uses the same pending-only mechanism, so it composes with open-arrow navigation. |
| AC-7 | Enter/mouse activation commit once, close, return focus | **Preserved.** Commit paths untouched; focus-return-on-close (`FinishClose`) untouched. |
| AC-8 | Escape/uncommitted close restores the opening identity, returns focus | **Preserved.** The search path stops *triggering* uncommitted closes per keystroke; the close behavior itself is unchanged. Note: with the fix, Escape during search restores the identity committed before the search session opened — the correct #438 expected behavior. |
| AC-9 | Left/Right breadcrumb behavior | **Preserved.** No arrow routing touched. |
| AC-12 | One render per surface per state update; one transition per inbound event | **Preserved; must be pinned.** The new `ReplaceItemsPreservingSession` must emit exactly one render per surface per keystroke (the current Clear+AddItems emits at least two). |
| AC-13 | "Focus enters the pending option on open" | **Deliberately scoped.** This is the only #400 criterion in tension with #438. #438 carves out *search-driven* opens as non-focusing; explicit-gesture opens (mouse toggle, Down arrow, JumpToFolderDropDown) keep focus-on-open because the plain `SetFolderDroppedDown(true)` / 3-parameter `OpenAsync` default to `takeFocus: true`. The plan should record this as a sanctioned, gesture-scoped qualification of #400 AC-13, not a regression. |
| AC-14 | Lazy popup creation, reuse, disposal | **Preserved/improved.** The popup is no longer closed and recreated-shown per keystroke. |
| AC-15 | Deterministic edge behavior, no throw/leak | **Preserved.** Empty result sets and single-row (banner-only) results must remain deterministic no-ops for the highlight step (`OpenSelector` already refuses to open with no selectable rows, `BreadcrumbSelectionSession.cs:307-319`). |

Down-arrow contract (explicit requirement of this research task): `TextBoxSearch_KeyDown` continues to issue both `SetFolderDroppedDown(true)` and `FocusFolderDropDown()` (`QfcItemController.EventHandlers.cs:180-189`), pinned by `QfcItemController.EventHandlersTests.cs:355-371` — unchanged and kept passing.

---

## 5. Existing Test Spec (Question 5)

### 5.1 Tests that are part of the spec and must keep passing

- `QuickFiler.Test/Controllers/QfcItemController.EventHandlersTests.cs:355-371` (Down arrow drops down + focuses) and `:373-388` (non-Down key does nothing). **[VERIFIED]**
- `QuickFiler.Test/Controllers/QfcItemController.NavigationTests.cs:159-181` (`JumpToFolderDropDown` focuses + drops down) and `QfcItemController.SeamDispatcherTests.cs:94-95` (async variant). **[VERIFIED]**
- `QuickFiler.Test/Controllers/QfcItemController.FolderSuggestionsTests.cs:110-166` and `QfcItemController.FolderHandlingTests.cs:434-494` — pin `SetFolderItems`/`SetFolderSelectedIndex(1)`/`SetFolderSelectedItem` on the **suggestions** path (`AssignFolderComboBox`), which this fix does not touch. **[VERIFIED]**
- `QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs` — all cases: `SetFolderDroppedDown(true)` opens once with screen bounds (42-62), `false` closes uncommitted (89-107), native close restores original + one focus return (110-137), reset/pooled reuse (227-261), init failure (264-294), disposal (297-312). The 3-parameter `OpenAsync` and default focusing semantics are unchanged, so these remain green. **[VERIFIED]**
- `QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.cs` — `FocusPendingCount == 1` on a default open (:103) and `== 2` after close/reopen (:162): both exercise the default `takeFocus: true` path and remain valid.
- `QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs:55-76` — keyboard `SetFolderDroppedDown(true)` and mouse toggle produce the identical open request; unchanged for the explicit-gesture path.
- `QuickFiler.Test/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs:77-100` — pins the `SetFolderDroppedDown(bool)` signature and folder event types; additive members keep it green.
- `BreadcrumbDropDownOpenCoordinatorTests(+Part2)`, `BreadcrumbDropDownLifecycle*Tests`, `BreadcrumbPendingOpenCloseTests`, `BreadcrumbItemViewerLifecycleCoordinatorTests`, hub/dispatch/placement tests — default-path semantics unchanged.
- `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbSelectionSessionTests.cs`, `FolderBreadcrumbBridgeRouter*Tests.cs`, `BreadcrumbSelectionMapTests.cs` — existing transitions untouched; new transitions are additive.

### 5.2 Tests that encode the defective behavior and must change (explicit call-out)

- `QuickFiler.Test/Controllers/QfcItemController.EventHandlersTests.cs:313-350` — `TextBoxSearch_TextChanged_UsesInjectedFolderSearchHandler_PopulatesAndSelectsFolder` verifies exactly the defective sequence: `ClearFolderItems()` once, `SetFolderItems` once, `SetFolderSelectedIndex(1)` once, `SetFolderDroppedDown(true)` once. **Justification for change:** the handler's intent composition is the defect under repair; the durable behavior this test protects (wildcard `FindFolder` query from `SearchText`, population of the drop-down, row-1 highlight, drop-down shown) must be re-asserted against the new intent member (`PresentFolderSearchResults` receiving the `FindFolder` result), with added negative assertions (`SetFolderDroppedDown` never, `FocusFolderDropDown` never, `SetFolderSelectedIndex` never). No other existing test pins the search-path composition. **[VERIFIED — grep across `QuickFiler.Test` for `SetFolderDroppedDown|SetFolderSelectedIndex|FocusFolderDropDown` returned no other search-path assertions]**

---

## 6. Recommended Approach (Question 6)

**Recommendation: Option 3** (composite `PresentFolderSearchResults` intent + session-preserving row replacement + pending-only highlight + `takeFocus` intent threaded through `OpenAsync`). Rationale:

1. It is the only enumerated option that removes **both** verified focus-steal mechanisms (§1.2 open-side `_focusPending`, §1.3 close-side `_focusAnchor`) and the per-keystroke commit (§2).
2. It reuses existing, tested primitives: `ReconcileRowsReplaced` for session-preserving row swaps, `PendingIdentity` for non-committing highlight, the FIFO `BreadcrumbPopupUiOperations` queue for deterministic intent latching.
3. Every contract change is additive; every #400 acceptance criterion except the deliberately scoped AC-13 is preserved, and existing gesture-path tests keep passing unmodified.
4. It matches `.claude/rules/csharp.md` seam preference (narrow interface members over delegates or latent state) and the issue's own proposed design direction.

**Estimated change size:** 12 production files (§3 Option 3 list; all under the 500-line ceiling today — largest touched is `BreadcrumbDropDownOpenLifetime.cs` at 477 lines, which needs watching), 1 modified test file (`QfcItemController.EventHandlersTests.cs`), and approximately 4 new test files/case groups (controller regression; open-coordinator/host non-focusing open; router/session replace+highlight; ItemViewer harness end-to-end). Both `QuickFiler.Test` and `UtilitiesCS.Test` are legacy non-SDK projects — new files need explicit `<Compile Include>` entries.

**Primary regression test (fails before, passes after):** at the controller seam, with `Mock<IFolderSearchHandler>` and `Mock<IItemViewer>` (exact arrangement of `EventHandlersTests.cs:314-343`), invoke `TextBoxSearch_TextChanged` and assert `viewer.Verify(v => v.SetFolderDroppedDown(It.IsAny<bool>()), Times.Never())` and `viewer.Verify(v => v.FocusFolderDropDown(), Times.Never())` alongside the new presentation intent being issued once. This fails today at runtime (the handler calls `SetFolderDroppedDown(true)` at `EventHandlers.cs:177`) and passes after the fix — a deterministic, compile-clean fail-before.

**Supporting regressions:**
- Pipeline-level (real `BreadcrumbDropDownHost` harness with counted delegates): a search-driven open invokes `FocusPending` zero times and `FocusAnchor` zero times, while a default open still invokes `FocusPending` once (protects #400 AC-13 for gestures).
- Open-coordinator level: two consecutive search refreshes produce exactly one `OpenAsync` and zero `Close` calls on the host (fails today: the second refresh closes and reopens).
- Session level: `HighlightRow` on an open session changes only `PendingIdentity`, publishes no `SelectionChanged`, and `Cancel` restores the pre-search committed identity; row replacement while open preserves the session via `ReconcileRowsReplaced`.
- Controller cache: after Escape-cancel, `_selectedFolder` must not retain a mid-search highlight (guards the stale-cache defect in §2).

**Risks and mitigations:**
- `ToolStripDropDown` with `AutoClose = true` (`BreadcrumbDropDownHost.cs:165-170`) is expected to stay open while the user types in the same form (no deactivation, no outside click) **[INFERRED — WinForms behavior; not provable in a unit test]**; if runtime behavior differs, the popup would auto-close per keystroke and `OnDropDownClosed` (`BreadcrumbDropDownHost.cs:414-425`) would cancel the session. The harness-level tests cannot observe this; see §8.
- WebView2 native focus grab during first popup surface creation **[UNVERIFIED]** — the managed pipeline no longer requests focus, but CoreWebView2 controller creation can move Win32 focus independently of managed code. Only a live session can confirm; see §8.
- Keystroke ordering: `PresentFolderSearchResults` executes router mutations synchronously and native-open work through the posted queue, in keystroke order; a later keystroke's replace correctly supersedes an in-flight open's row count (popup height uses `_rowCount` at open time, `BreadcrumbDropDownOpenCoordinator.cs:192-194` — an already-open popup keeps its height until reopened; acceptable, note in plan).

---

## 7. Out of Scope, Confirmed: EfcViewer Search Path (Question 7)

**Not the same defect.** `EfcFormController.SearchText_TextChanged` (`QuickFiler/Controllers/EfcFormController.cs:556-559`) → `BindFolderRows` (:873-883) → `BindBreadcrumbRowsAsync` (:886-903) → `BreadcrumbBridgeRouter.BindRowsAsync` (`QuickFiler/Controllers/BreadcrumbBridgeRouter.cs:74-116`) → `DeliverDocument()` re-renders a **persistently visible** WebView2 via NavigateToString. **[VERIFIED]** There is no popup, no open intent, and no managed focus call anywhere in that path: the only focus-adjacent member, `FocusSearchRequested` (`BreadcrumbBridgeRouter.cs:268-269`, consumed at `EfcFormController.cs:851`), fires solely on an inbound "Up from top row" page message, and `SearchText_DownArrow` (`EfcFormController.cs:404-413`) is an explicit gesture. Whether `NavigateToString` re-delivery itself moves Win32 focus at runtime is **[UNVERIFIED]**; if a user reproduces focus loss in the EFC search box, it is a separate defect with a different mechanism and should be promoted as its own issue. Do not fix it under #438.

---

## 8. Automation Feasibility

**Almost all acceptance criteria for this fix are satisfiable by automated MSTest coverage alone; one residual check is only observable in a live Outlook session.**

- Every focus transfer in the defective pipeline is a **managed delegate invocation** (`_focusPending`, `_focusAnchor`, `FocusBreadcrumbCore`) injected through constructor seams, and the repository already has delegate-counting harnesses for exactly these (`BreadcrumbDropDownHostTests` `FocusPendingCount`/`FocusAnchorCount`), a headless-`ItemViewer` + mocked-host integration harness (`ItemViewerDropDownHarness`), and `Mock<IItemViewer>` controller tests. The issue's stated validation areas — refresh intent without focus intent, refresh-only open not scheduling `_focusPending`, Down-arrow still focusing, full search string reaching `SearchText` with rows tracking the complete query — are all assertable deterministically at these seams with MSTest + Moq + FluentAssertions and no temporary files. Headless `ItemViewer` construction in unit tests is an established, safe pattern in this suite.
- **Residual human check (identify explicitly):** the issue's "Manual verification notes" item — *"type an eight-character folder name at normal speed and confirm the caret never leaves the textbox"* — cannot be fully discharged by unit tests, because two native behaviors are outside the managed seam: (a) whether CoreWebView2 popup-surface creation grabs Win32 focus on its own during the first search-driven open, and (b) whether `ToolStripDropDown.AutoClose` keeps the non-activated popup open while the user keeps typing in the same window. If the orchestrator adopts that manual note as an acceptance criterion, that single criterion requires a human typing into a live Outlook session; every other criterion in `issue.md`'s validation list is automatable. Recommended resolution: mirror the #400 precedent (spec lines 60, 84, 235: user-operated validation is not required delivery evidence) — treat the live-Outlook typing check as a documented post-fix verification note / scope exception, with the automated seam-level assertions as the merge gate.

---

## 9. Requirements Mapping to Design (state model summary)

Search-session state (all host-neutral, all existing types):

- `BreadcrumbSelectionSession`: `{IsOpen, CommittedIdentity, OriginalIdentity, PendingIdentity}` — gains one transition `HighlightRow(index)`: requires `IsOpen`; sets `PendingIdentity` to the identity of the first selectable row at/after `index`; effects `Handled | RenderRequired` (no `SelectionChanged`, no `OpenStateChanged`).
- `FolderBreadcrumbBridgeRouter`: gains `ReplaceItemsPreservingSession(items)` = generation bump + `_model.ReplaceRows(plainRows)` + `ReconcileRowsReplaced()`; returns a transition with `RenderRequired` only. (Note: the existing unused `SetItems` at :119-135 clears the model without session reconciliation and should not be reused as-is.)
- `BreadcrumbBridgeCoordinator`: gains `PresentSearchResults(items)` = replace → open-if-closed → highlight; posts one render per surface.
- `BreadcrumbDropDownOpenCoordinator` + host + open lifetime: `takeFocus` intent, default `true`; `false` only for search-driven opens; latched on the FIFO operations queue.
- Controller: `TextBoxSearch_TextChanged` = `FindFolder` + `PresentFolderSearchResults(folders)`; `TextBoxSearch_KeyDown` unchanged.

Transitions per keystroke (fixed behavior): closed → `[replace rows] → [open session] → [pending=row1] → [native open, no focus]`; open → `[replace rows, session preserved] → [pending=row1] → no native churn`. Explicit gestures unchanged: Down/toggle → focusing open; Enter/click → commit+close+focus return; Escape/outside → cancel+close+focus return.

Coverage: router/session/coordinator changes are measurable and target >= 90% (new members); `ItemViewer` partials remain `[ExcludeFromCodeCoverage]` thin forwarding (`ItemViewer.FolderSearch.cs:9-17` comment); the controller handler is already covered via the seam tests.
