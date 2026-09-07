# Issue #796 — QuickFiler folder drop-down close ordering: research

- Issue: #796
- Worktree: branch `bug/quickfiler-folder-dropdown-closes-on-open-796`, cut from `origin/main` at `c431dc32`
- Date: 2026-09-06
- Scope: OPEN/CLOSE lifecycle and selection-commit ordering only. Row-text projection (archive-root lineage) is owned by a concurrent sibling item and is out of scope here.

Acceptance criteria AC1 through AC6 are reproduced verbatim from `issue.md` in section 9. They are authoritative, are not renumbered, and are not weakened anywhere in this document.

No numeric acceptance criterion is proposed in this artifact. No count, enumeration, or population is asserted as a `spec.md` acceptance criterion, so no `## Numeric Derivation Evidence` section is required.

---

## 1. Citation re-derivation against the current tree

Every citation in the issue's `## Suspected Cause / Notes` section was re-read in this worktree. Paths are repository-relative with forward slashes.

### 1.1 Confirmed exact — symbol exists and line range matches

| Issue citation | Path | Finding |
|---|---|---|
| `FolderBreadcrumb.html:440-442` (`#dropDownButton` posts `selectorToggle`) | `QuickFiler/Resources/FolderBreadcrumb.html` | Confirmed. Lines 440-442 are the `dropDownButton.addEventListener("click", ...)` handler posting `{ type: "selectorToggle" }`. |
| `BreadcrumbBridgeCoordinator.HandleSelectorMessage (:349-358)` | `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` | Toggle branch confirmed at 349-358 (`case BreadcrumbSelectorToggleMessage _:` -> `CancelSelector()` / `OpenSelector()`). See drift note 1.2 (a) on the method declaration line. |
| `FolderBreadcrumbBridgeRouter.OpenSelector` | `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs:185-186` | Confirmed. `public BreadcrumbSelectionTransition OpenSelector() => Mutate(_selectionSession.OpenSelector);` |
| `BreadcrumbDropDownOpenCoordinator.HandleSelectorOpenStateChanged (:178-191)` | `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` | Confirmed exact, 178-191. |
| `BreadcrumbDropDownOpenLifetime.OpenCoreAsync (:215-256)` | `QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs` | Confirmed exact, 215-256. |
| `BreadcrumbDropDownOpenLifetime.Focus.cs:32-51` (`FocusCurrentSurface`) | `QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.Focus.cs` | Confirmed. Method spans 32-51 (expression body closes at 51; file is 53 lines). `_host.FocusPending()` at :40, guarded by `if (takeFocus)` at :39. |
| `BreadcrumbDropDownHost.cs:165-172` (`AutoClose = true`) | `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | Confirmed. `AutoClose = true` at :167 inside the `DropDown` initializer at 165-170; `DropDown.Closed += OnDropDownClosed;` at :171. |
| `BreadcrumbDropDownHost.Open.cs:98-102` | `QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs` | Confirmed. `ShowPopup` at 98-102. See finding 3.1: this member sets `DropDown.AutoClose = takeFocus`, which the issue does not record. |
| `QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs:55` | same | Confirmed. `SetFolderDroppedDownTrue_UsesSameOpenRequestAsMouseSelectorToggle` declared at :55. |
| `FolderBreadcrumbBridgeRouter.cs:478-482` (`ReplaceRowsPreservingSession`) | `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs` | Confirmed exact, 478-482. |
| `BreadcrumbSelectionSession.ReconcileRowsReplaced (:119-147)` | `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs` | Confirmed exact, 119-147. Preserves `IsOpen` (no write to it on any path). |
| `QfcFormController.SetupDisposal.cs:175` | `QuickFiler/Controllers/QfcFormController.SetupDisposal.cs` | Confirmed exact. `_formViewer.FormDeactivated += this.FormViewer_Deactivated;` |
| `QfcFormViewer.cs:207` | `QuickFiler/Viewers/QfcFormViewer.cs` | Confirmed exact. `public void ParkFocusOffWebView2() => this.ActiveControl = _l1v1L2h2_ButtonOK;` |
| `ItemViewer.Breadcrumb.cs:203` (popup focus delegate) | `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` | Confirmed exact. `() => host.ControlHost?.Control.Focus(),` |
| `ItemViewer.Breadcrumb.cs:205` (cancel delegate) | same | Confirmed exact. `() => BreadcrumbCoordinator?.CancelSelector(),` |
| `BreadcrumbDropDownOpenCoordinator.cs:273-288` (`FinishOpenCore`) | same | Confirmed exact, 273-288. Re-checks `_isSelectorOpen()` after the async open. |
| `BreadcrumbDropDownHost.cs:426-437` (`OnDropDownClosed`) | same | Confirmed exact, 426-437. |
| `BreadcrumbDropDownHost.cs:439-455` (`FinishClose`) | same | Confirmed exact, 439-455. `_cancelSelection()` at :450 under `reason == Uncommitted`. |
| `BreadcrumbDropDownHost.cs:452` (asymmetry comment) | same | Confirmed exact. `// Issue #677: only the focus step is gated; the cancel step above always runs.` |
| `QfcItemController.EventHandlers.cs:217-228` (`TextBoxSearch_Leave`) | `QuickFiler/Controllers/QfcItemController.EventHandlers.cs` | Confirmed exact, 217-228. |
| `QfcItemController.EventHandlers.cs:195` (`_searchLeaveHandoffPending = true`) | same | Confirmed exact, inside the `Keys.Down` branch at 192-199. |
| `ItemViewer.Breadcrumb.cs:270-274` (`MayRestoreBreadcrumbFocus`) | same | Confirmed exact, 270-274. |
| `QfcFormControllerDeactivateTests.cs:172` | `QuickFiler.Test/Controllers/QfcFormControllerDeactivateTests.cs` | Confirmed exact. `FormDeactivated_CancelsSelectorOnEveryItemController` declared at :172. |
| `BreadcrumbPendingOpenCloseTests` (`:124`, `:143`) | `QuickFiler.Test/Viewers/BreadcrumbPendingOpenCloseTests.cs` | Confirmed exact. `ToggleAndEscapeWhileOpenIsPending_EachClosesHostExactlyOnce` at :124; `AutomaticSelectorCloseWhileOpenIsPending_ClosesHostExactlyOnce` at :143. See drift note 1.2 (d) on what these tests actually encode. |
| `FolderBreadcrumbBridgeRouter.SearchPresentation.cs:38-55` (`ReplaceItemsPreservingSession`) | `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.SearchPresentation.cs` | Confirmed exact, 38-55. Emits `Handled \| RenderRequired` only — no `OpenStateChanged`, no `SelectionChanged`. |

### 1.2 Citation drift — reported, not silently corrected

(a) **`BreadcrumbBridgeCoordinator.HandleSelectorMessage (:349-358)`.** The cited range is the toggle `case` body, not the method. The method `HandleSelectorMessage` is declared at `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs:343` and ends at :379. Anyone re-reading the citation as a method range will miss the `BreadcrumbSelectorActivationMessage` case at 362-364, which is the row-click commit path and is load-bearing for AC3.

(b) **`QfcFormController.ParkFocusAndCancelSelectors` (`QfcFormController.Deactivate.cs:39-57`).** The symbol exists and the declaration is at :39, but the method body ends at :71, not :57. Lines 58-70 are the per-item boundary `catch` with `logger.Error(...)`. The cited range truncates the method and omits the only existing log call in the file. The file is 73 lines total.

(c) **`BreadcrumbSelectionSession.Open`.** The pipeline description names `BreadcrumbSelectionSession.Open` as the direct callee of `FolderBreadcrumbBridgeRouter.OpenSelector`. The direct callee is `BreadcrumbSelectionSession.OpenSelector` (`UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs:197-202`), which calls `Open()` (`:307-319`). Both symbols exist; there is one hop the citation omits. `OpenSelector` is what supplies the `OpenStateChanged` effect flag (`:200`); `Open()` alone does not.

(d) **`BreadcrumbPendingOpenCloseTests` encode "close wins over a pending open".** The two cited tests (`:124`, `:143`) do exist at those lines, but what they assert is narrower than the phrase suggests: each asserts that exactly one `IBreadcrumbDropDownHost.Close(expectedReason)` call reaches a mocked host when a close intent arrives while the open task is unresolved. They pin *close idempotency*, not cancel-versus-commit precedence. The tests that literally encode "a close beats a pending open, and cancels" are the two earlier ones in the same file: `CloseWhileFactoryPending_InvalidatesOpenAndRepeatedCloseIsIdempotent` (`:22`) and `CloseWhileReadinessPending_RejectsLateReadyAttachShowAndFocus` (`:55`), which assert `harness.CancelCount.Should().Be(1)` at `:48` and `:79`. Section 6 treats all four.

(e) **Candidate 2 attributed to the click-without-select symptom.** The issue states that native `ToolStripDropDown` auto-close is "the mechanism behind the click-without-select symptom". For the reproduction as written (step 3 types letters, step 4 clicks a row) this is very likely wrong, because a search-driven open sets `DropDown.AutoClose = false` (`QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs:100`). With `AutoClose == false` the framework's auto-dismiss is disabled, so candidate 2 cannot be the first cause on that path. See section 3.1 and section 5.

### 1.3 Citations that could not be checked in this worktree

`docs/features/archive/2026-08-07-quickfiler-search-keystroke-focus-steal-438/research/2026-08-08T10-30-...-research.md:172` is cited with an elided filename. The `2026-08-07-...-438` archive folder exists in this tree, but the exact file and line were not resolved because the citation is not a complete path. Treated as unverified.

---

## 2. The OPEN pipeline and the CLOSE/CANCEL pipeline

Both pipelines are traced end to end below. "sync" means the step runs to completion on the calling stack. "posted" means it is enqueued on the captured UI synchronization boundary (`BreadcrumbUiDispatcher` / `BreadcrumbPopupUiOperations`) and resumes on a later message-pump turn. "await" marks a genuine asynchronous suspension.

### 2.1 OPEN — mouse (arrow click)

| # | Step | File:line | Boundary |
|---|---|---|---|
| 1 | `#dropDownButton` click posts `{type:"selectorToggle"}` | `QuickFiler/Resources/FolderBreadcrumb.html:440-442` | JS, in the collapsed WebView2 |
| 2 | `IWebViewMessenger.MessageReceived` -> `OnMessageReceived` -> `ObserveInboundAsync` -> `DispatchInboundMessageAsync` | `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs:273-306` | async; `IsSelectorMessage` true |
| 3 | `_dispatcher.Dispatch(() => HandleSelectorMessage(json))` | same, `:301` | **posted** (await) |
| 4 | `HandleSelectorMessage` toggle case -> `OpenSelector()` | same, `:343`, `:349-358` | sync on UI boundary |
| 5 | `BreadcrumbBridgeCoordinator.OpenSelector` -> `_router.OpenSelector()` | same, `:149` | sync |
| 6 | `FolderBreadcrumbBridgeRouter.OpenSelector` -> `_selectionSession.OpenSelector` | `UtilitiesCS/.../FolderBreadcrumbBridgeRouter.cs:185-186` | sync |
| 7 | `BreadcrumbSelectionSession.OpenSelector` -> `Open()`; sets `IsOpen = true`; returns `Handled \| OpenStateChanged` | `UtilitiesCS/.../BreadcrumbSelectionSession.cs:197-202`, `:307-319` | sync |
| 8 | `ApplyTransition` -> `_dispatcher.Dispatch(() => PublishTransition(transition))` | `BreadcrumbBridgeCoordinator.cs:186-194` | **posted** |
| 9 | `PublishTransition` raises `SelectorOpenStateChanged` | same, `:239-242` | sync on UI boundary |
| 10 | `BreadcrumbItemViewerLifecycleCoordinator.OnSelectorOpenStateChanged` -> `_openCoordinator.HandleSelectorOpenStateChanged()` | `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs:237-238` | sync |
| 11 | `HandleSelectorOpenStateChanged` -> `_operations.PostAsync(...)` -> `_isSelectorOpen()` true -> `RequestOpen()` | `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs:178-191` | **posted** |
| 12 | `RequestOpen` -> `_currentOpenTask = OpenCoreAsync(_generation)` | same, `:111-125` | sync under `_sync` |
| 13 | `OpenCoreAsync` -> `RunAsync(() => BeginOpenCore(generation))` | same, `:220-231` | **await** |
| 14 | `BeginOpenCore` consumes the no-focus latch (`takeFocus = !_nextOpenTakesNoFocus`), computes anchor/size, calls `_host.OpenAsync(anchor, workingArea, size)` — the 3-parameter overload, i.e. `takeFocus: true` | same, `:239-271` | sync |
| 15 | `BreadcrumbDropDownHost.OpenAsync` -> `OpenWithFocusIntentAsync(..., true)`; not already open -> `_openLifetime.OpenAsync(..., takeFocus: true)` | `QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs:18-22`, `:53-89` | sync |
| 16 | `BreadcrumbDropDownOpenLifetime.OpenAsync` takes a lease, invalidates the previous generation, schedules `OpenCoreAsync` via `RunOnOwnerAsync` | `QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs:44-72` | **posted + await** |
| 17 | `OpenCoreAsync` -> `EnsureSurfaceAsync(lease)` (creates the popup WebView2 on first use; early-returns when `HasInstalledSurface`) | same, `:215-256`, `:290-343` | **await** |
| 18 | `PlaceSurfaceAsync` sizes host/control/dropdown | `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs:188-216` | **await** |
| 19 | `ShowCurrentSurface` -> sets `_host.OpenState = true`, then `_host.ShowPopup(location, takeFocus)` | `BreadcrumbDropDownOpenLifetime.cs:258-279` | **await**, then sync |
| 20 | `ShowPopup` sets `DropDown.AutoClose = takeFocus` and calls `_showPopup` = `ShowOwnedPopup` -> `dropDown.Show(anchor, ...)` | `BreadcrumbDropDownHost.Open.cs:98-102`; `BreadcrumbPopupUiOperations.cs:100-105` | sync |
| 21 | `FocusCurrentSurface(lease, takeFocus)` -> `_host.FocusPending()` | `BreadcrumbDropDownOpenLifetime.Focus.cs:32-51` | **await**, then sync |
| 22 | `FocusPending` evaluates `MayTakeFocus()` (= `ItemViewer.MayRestoreBreadcrumbFocus`) and, if permitted, `host.ControlHost?.Control.Focus()` on the popup's own WebView2 | `BreadcrumbDropDownHost.cs:288-292`; `ItemViewer.Breadcrumb.cs:203`, `:270-274` | sync |
| 23 | `CompleteOpenAsync` resolves the open task; `OpenCoreAsync` then runs `FinishOpenCore(generation, opened)` which re-checks `_isSelectorOpen()` and closes with `ExplicitCommit` if the session was cancelled meanwhile | `BreadcrumbDropDownOpenLifetime.cs:154-186`; `BreadcrumbDropDownOpenCoordinator.cs:229-231`, `:273-288` | **await**, then sync |

**Ordering substance.** Step 22 (the Win32 focus move onto the popup's top-level window) happens *before* step 23 (`FinishOpenCore`). If the focus move deactivates the QuickFiler form, the resulting `Form.Deactivate` handler runs on the same UI thread and can cancel the session while the open task is still resolving. `FinishOpenCore` then observes `_isSelectorOpen() == false` and issues `CloseCore(ExplicitCommit)` itself. That is a second, independent close path fed by the first.

### 2.2 OPEN — keyboard (Down in the search box)

Identical from step 11 onward. The entry differs:

1. `QfcItemController.TextBoxSearch_KeyDown`, `Keys.Down` branch — `QuickFiler/Controllers/QfcItemController.EventHandlers.cs:190-199`. Three synchronous statements in order: `_itemViewer.SetFolderDroppedDown(true)`; `_searchLeaveHandoffPending = true`; `_itemViewer.FocusFolderDropDown()`.
2. `ItemViewer.SetFolderDroppedDown` -> `SetBreadcrumbDropDownState(true)` -> `_breadcrumbLifecycleCoordinator.SetDroppedDown(true, FocusBreadcrumbCore)` — `QuickFiler/Viewers/ItemViewer.FolderSearch.cs:31-32`; `ItemViewer.Breadcrumb.cs:288-300`.
3. `BreadcrumbItemViewerLifecycleCoordinator.SetDroppedDown` -> `_openCoordinator.SetDroppedDown(true)` — `BreadcrumbItemViewerLifecycleCoordinator.cs:192-205`.
4. `BreadcrumbDropDownOpenCoordinator.SetDroppedDown` **posts** `_openSelector()` (= `BreadcrumbBridgeCoordinator.OpenSelector`) — `BreadcrumbDropDownOpenCoordinator.cs:159-176`. The open reaches `RequestOpen` through the `SelectorOpenStateChanged` event, exactly as in the mouse path.
5. `FocusFolderDropDown()` -> `FocusBreadcrumb()` -> posted `FocusBreadcrumbCore()` -> `_l0vhBreadcrumb_WebView2.Focus()` — `ItemViewer.FolderSearch.cs:47`; `ItemViewer.Breadcrumb.cs:246-255`, `:276-286`. This focuses the **collapsed anchor** on the same form, which raises `Leave` on the search textbox — the reason the #680 latch exists.

No no-focus latch is set on either gesture path, so `BeginOpenCore` computes `takeFocus == true` for both. `BreadcrumbSelectorOpenRetryTests.SetFolderDroppedDownTrue_UsesSameOpenRequestAsMouseSelectorToggle` (`QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs:55`) pins that the two produce the same open request, which is consistent with mouse and keyboard failing identically.

### 2.3 OPEN — search typing (the path that works)

`QfcItemController.TextBoxSearch_TextChanged` (`QuickFiler/Controllers/QfcItemController.EventHandlers.cs:173-182`) -> `ItemViewer.PresentFolderSearchResults` -> `PresentBreadcrumbSearchResults` -> `BreadcrumbItemViewerLifecycleCoordinator.PresentSearchResults` (in `BreadcrumbItemViewerLifecycleCoordinator.Search.cs`), which latches `LatchNextOpenTakesNoFocus()` (`BreadcrumbDropDownOpenCoordinator.cs:139-147`) so `BeginOpenCore` computes `takeFocus == false`. Consequences, both verified in source:

- `ShowPopup` sets `DropDown.AutoClose = false` (`BreadcrumbDropDownHost.Open.cs:100`).
- `FocusCurrentSurface` skips `_host.FocusPending()` (`BreadcrumbDropDownOpenLifetime.Focus.cs:39-40`).

Row-set refreshes route through `ReplaceItemsPreservingSession` (`UtilitiesCS/.../FolderBreadcrumbBridgeRouter.SearchPresentation.cs:38-55`) and `ReplaceRowsPreservingSession` -> `ReconcileRowsReplaced` (`FolderBreadcrumbBridgeRouter.cs:478-482`; `BreadcrumbSelectionSession.cs:119-147`), neither of which writes `IsOpen` or emits `OpenStateChanged`. The issue's claim that async suggestion decoration is not the cause is confirmed.

### 2.4 CLOSE/CANCEL — every route into `FinishClose`

`BreadcrumbDropDownHost.FinishClose(reason)` (`QuickFiler/Viewers/BreadcrumbDropDownHost.cs:439-455`) is the single completion point. It runs three operations through `CompleteAll`:

1. `DropDown.AutoClose = true` (restore default, #680).
2. `if (reason == Uncommitted) _cancelSelection();` — `_cancelSelection` is `() => BreadcrumbCoordinator?.CancelSelector()` (`ItemViewer.Breadcrumb.cs:205`).
3. `FocusAnchorIfPermitted()` — gated by `MayTakeFocus()`; the cancel above is not gated (comment at `:452`).

Four routes reach it:

- **R1, native auto-close.** WinForms raises `ToolStripDropDown.Closed` -> `OnDropDownClosed` (`:426-437`). Guards `_disposed`, `_programmaticClose`, `!OpenState`; then `_openLifetime.InvalidateAndSchedule(...)` — **posted** — re-checks the same three guards and calls `FinishClose(Uncommitted)`. This is the only route that supplies `Uncommitted` without a caller choosing it.
- **R2, programmatic close.** `Close(reason)` (`:247-257`) -> `InvalidateAndSchedule(() => CompleteClose(reason, true))` — **posted** -> `CompleteClose` (`:397-411`) sets `_programmaticClose`, calls `CloseNative()` (`:413-424`), then `FinishClose(reason)`.
- **R3, open-failure rollback.** `RestoreAfterOpenFailure` (`:457-469`) -> `FinishClose(Uncommitted)`.
- **R4, dispose / reset.** `DisposeCoreAsync` (`:329-352`) and `ResetCoreAsync` (`:303-327`) -> `CompleteClose(Uncommitted, true)`.

The coordinator's side of R2 is `BreadcrumbDropDownOpenCoordinator.CloseCore` (`:324-359`), reached from `SetDroppedDown(false)` (`:174`), `HandleSelectorOpenStateChanged` when the session reports closed (`:189`, reason `ExplicitCommit`), and `FinishOpenCore` (`:284`, reason `ExplicitCommit`).

### 2.5 CANCEL originating outside the popup

`QfcFormController.FormViewer_Deactivated` -> `ParkFocusAndCancelSelectors` (`QuickFiler/Controllers/QfcFormController.Deactivate.cs:26-27`, `:39-71`), wired at `QfcFormController.SetupDisposal.cs:175`, runs entirely **synchronously** inside the WinForms `Form.Deactivate` event:

1. `if (_formViewer?.IsWebView2Focused == true) _formViewer.ParkFocusOffWebView2();` (`:41-44`). `ParkFocusOffWebView2` is `this.ActiveControl = _l1v1L2h2_ButtonOK` (`QfcFormViewer.cs:207`), a synchronous WinForms active-control change that raises `Leave` on the previously active control before returning.
2. `foreach (QfcItemGroup group in groups) group.ItemController?.CancelBreadcrumbSelector();` (`:52-70`), each in its own `try`/`catch` with `logger.Error`.
3. `CancelBreadcrumbSelector` -> `ItemViewer.CancelBreadcrumbSelector` (`QuickFiler/Viewers/ItemViewer.FolderSearch.cs:43`) -> `BreadcrumbCoordinator.CancelSelector()` -> session `Cancel()` -> `Handled \| OpenStateChanged \| RenderRequired` -> posted `PublishTransition` -> `SelectorOpenStateChanged` -> `HandleSelectorOpenStateChanged` -> `_isSelectorOpen()` false -> `CloseCore(ExplicitCommit)` -> `_host.Close(ExplicitCommit)` -> R2.

Note the reason: a deactivation-driven cancel closes the host with `ExplicitCommit`, so `FinishClose` does **not** call `_cancelSelection()` a second time. The session was already cancelled at step 3.

### 2.6 The row-click commit path (AC3)

1. Expanded selectable row registers `click` -> `post({ type: "selectorActivate", identity: row.identity })` — `QuickFiler/Resources/FolderBreadcrumb.html:289-291`. `click` fires on **mouseup**, not mousedown.
2. `HandleSelectorMessage` -> `case BreadcrumbSelectorActivationMessage activation: ActivateSelector(activation.Identity)` — `BreadcrumbBridgeCoordinator.cs:362-364`, `:173-174`.
3. `FolderBreadcrumbBridgeRouter.ActivateSelector` (`:194-195`) -> `BreadcrumbSelectionSession.ActivateSelector` (`:238-259`) -> `Activate(identity)` (`:353-375`).
4. **When the session is open**, `Activate` sets `PendingIdentity` and calls `CommitPending()` (`:361-364`), which commits and ends the session; the effect set includes `SelectionChanged` and `OpenStateChanged`, so the host closes with `ExplicitCommit` and `FinishClose` performs no cancel. This is the correct behavior AC3 asks for.
5. **When the session is already closed**, `Activate` takes the branch at `:367-374`: it still selects the row in the model and updates `CommittedIdentity`. `ActivateSelector` returns `Handled \| RenderRequired \| SelectionChanged` (no `OpenStateChanged`, since `closed` is false).

Step 5 is important: a late `selectorActivate` on a closed session would still change the selection. The reported symptom is that the selection does **not** change. The most parsimonious reading, given step 1, is that the `selectorActivate` message is never produced at all — the popup is dismissed on **mousedown**, so no `mouseup` and therefore no `click` is delivered to the page. That is a hypothesis about browser/window behavior, not a code-verified fact; it is exactly the kind of thing the AC6 instrumentation must settle, and section 4 states the discriminating evidence.

---

## 3. Two structural findings the issue does not record

### 3.1 `AutoClose` is not constant — it tracks `takeFocus`

`QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs:98-102`:

```csharp
internal void ShowPopup(Point location, bool takeFocus)
{
    DropDown.AutoClose = takeFocus;
    _showPopup(DropDown, Anchor, location);
}
```

The constructor's `AutoClose = true` (`BreadcrumbDropDownHost.cs:167`) is only the initial value; `FinishClose` restores it to `true` (`:446`); and every show overwrites it with the gesture's focus intent. This maps one-to-one onto the reported symptom asymmetry:

| Gesture | `takeFocus` | `AutoClose` at show | `FocusPending()` called | Reported behavior |
|---|---|---|---|---|
| Arrow click | true | true | yes | flashes closed |
| Down in search box | true | true | yes | flashes closed |
| Typing (search) | false | false | no | stays open |

Both candidate 1 and candidate 2 are gated by `takeFocus` in the same direction, so this asymmetry does **not** discriminate between them. It does, however, refute candidate 2 as the first cause of the *click-without-select* symptom in the reproduction as written (step 3 types, so the popup was shown with `AutoClose == false`).

### 3.2 `ParkFocusOffWebView2` can synchronously trigger candidate 3

`ParkFocusAndCancelSelectors` parks focus (`Deactivate.cs:41-44`) **before** the cancel loop (`:52-70`). Parking is `this.ActiveControl = _l1v1L2h2_ButtonOK`, which synchronously raises `Leave` on the outgoing active control. If the search textbox is the outgoing control, `QfcItemController.TextBoxSearch_Leave` (`EventHandlers.cs:217-228`) runs *inside* the parking assignment and, with `_searchLeaveHandoffPending == false`, issues `SetFolderDroppedDown(false)` — a close with reason `Uncommitted`, ahead of the deactivate handler's own cancel loop.

However, `IsWebView2Focused` (`QfcFormViewer.cs:190-201`) walks the `ActiveControl` chain and returns true only when the leaf is a `WebView2`. When the caret is in the search textbox, the leaf is a `TextBox`, so parking is skipped and this chain does not fire. The chain is therefore live only when a WebView2 is the active leaf — which is the arrow-click case, where the outgoing control is the collapsed breadcrumb WebView2, not the search textbox. In other words: on current code, candidate 3 is **not** reachable through candidate 1 on either reproduction path. This is a derivation from source, not an observation.

---

## 4. Discriminating evidence for the three candidates, and what AC6 must log

### 4.1 What would confirm or refute each candidate as the *first* cause

**Candidate 1 — `QfcFormController.ParkFocusAndCancelSelectors` (form deactivation).**

- *Confirms:* an entry log line from `ParkFocusAndCancelSelectors` appears in the log **before** any line from `OnDropDownClosed`, and the subsequent `OnDropDownClosed` line reports `CloseReason=CloseCalled` and `ProgrammaticClose=True` (i.e. the native close was our own `CloseNative`, downstream of the cancel).
- *Refutes:* `ParkFocusAndCancelSelectors` is never entered during the flash, or it is entered but reports `Groups=0` / `Cancelled=0`, or it is entered strictly after `OnDropDownClosed`.
- Additional discriminator available with no new plumbing: log `Form.ActiveForm == null` at entry. `ToolStripDropDown` is not a `Form`, so if the popup itself owns activation, `Form.ActiveForm` is null rather than naming another window. A null `ActiveForm` at deactivation is evidence of a self-inflicted deactivation; a non-null `ActiveForm` naming a foreign form is evidence of a genuine one. (`Form.ActiveForm` returning null for a non-Form active window is framework behavior, asserted from background knowledge, not verified in this session.)

**Candidate 2 — native `ToolStripDropDown` auto-close.**

- *Confirms:* an `OnDropDownClosed` line appears **first**, with `CloseReason` equal to `AppFocusChange` or `AppClicked` and `ProgrammaticClose=False`, `OpenState=True`, `AutoClose=True`.
- *Refutes:* `CloseReason=CloseCalled` (our own `CloseNative` did it), or the line does not appear at all before `ParkFocusAndCancelSelectors`, or `AutoClose=False` at the moment it fires.
- `ToolStripDropDownClosedEventArgs.CloseReason` is currently **discarded**: the `e` parameter of `OnDropDownClosed` (`BreadcrumbDropDownHost.cs:426`) is never read. It is the single most discriminating value available and costs nothing to record.

**Candidate 3 — `QfcItemController.TextBoxSearch_Leave`.**

- *Confirms:* a `TextBoxSearch_Leave` line reporting `HandoffPending=False, DropDownOpen=True` immediately precedes the close.
- *Refutes:* no such line, or `HandoffPending=True` (the Down-arrow handoff consumed it correctly).
- Section 3.2 predicts this candidate is unreachable on both reproduction paths. AC6 names only two instrumentation sites, so this candidate would be confirmed or refuted only indirectly — by whether the observed ordering leaves room for a third close. Adding a third temporary log line at `EventHandlers.cs:217` would settle it directly and is recommended even though AC6 does not require it.

### 4.2 Logging facility, exact call form, and correlation identifier

**`QfcFormController.ParkFocusAndCancelSelectors` — facility present, no new field needed.**

`QfcFormController` is a partial class with a static log4net field already in scope:

```csharp
// QuickFiler/Controllers/QfcFormController.cs:21-23
private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
    System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
);
```

It is already used in the target file at `QfcFormController.Deactivate.cs:64` (`logger.Error(...)`). The exact call form used by the nearest sibling partial for structured stage logging is:

```csharp
// QuickFiler/Controllers/QfcFormController.EventHandlers.cs:31
logger.Debug($"Cancel teardown stage completed. Stage={stage}");
// QuickFiler/Controllers/QfcFormController.EventHandlers.cs:129
logger.Info($"Cancel teardown starting. AlreadyCancelled={already}");
```

Interpolated string, sentence prefix, then `Key=Value` pairs. AC6 instrumentation should match that shape.

**`BreadcrumbDropDownHost.OnDropDownClosed` — no logger exists; one must be added.**

`QuickFiler/Viewers/BreadcrumbDropDownHost.cs` contains no logging of any kind. Its only failure sink is `_uiOperations.Report(exception)` (`:485`), which routes to `BreadcrumbUiDispatcher`'s error sink. The immediate neighbours in the same directory declare a static log4net field with this exact shape:

```csharp
// QuickFiler/Viewers/BreadcrumbUiDispatcher.cs:17-19
private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
    typeof(BreadcrumbUiDispatcher)
);
// QuickFiler/Viewers/BreadcrumbWebViewSurfaceFactory.cs:21-23 uses the same typeof(...) form
```

Note the field name differs by neighbourhood: `QuickFiler/Controllers` uses `logger`, `QuickFiler/Viewers` uses `log`.

**Blocking constraint:** `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` is **498 lines** against the repository's 500-line ceiling. A logger field plus two or three log statements cannot fit. The instrumentation must go into a new partial-class part (the file already has a precedent for this: `BreadcrumbDropDownHost.Open.cs` exists specifically to keep the main part under the ceiling — see its class comment at `BreadcrumbDropDownHost.Open.cs:8-14`). That means `OnDropDownClosed` itself has to move, or delegate to a diagnostics helper declared in the new part.

**Correlation identifier: none exists that both sites can carry.** Verified:

- Form-controller side: `IQfcItemController.ItemNumber` exists (`QuickFiler/Interfaces/IQfcItemController.cs:45`), a 1-based item index reachable from `group.ItemController` inside the cancel loop. Usable immediately.
- Host side: the host knows only `Anchor` and its own instance. `Anchor.Name` is the Designer constant `"L0vhBreadcrumb_WebView2"` for every item viewer (`QuickFiler/Viewers/ItemViewer.Designer.cs:206`), so it does **not** discriminate between items. `BreadcrumbDropDownOpenLease.Generation` (`BreadcrumbDropDownOpenLifetime.cs:18`) and `BreadcrumbDropDownOpenCoordinator._generation` (`:27`) are per-host monotonic counters, but neither is exposed on `IBreadcrumbDropDownHost` and neither is visible to the form controller.
- Consequently, correlating the two sites today requires either (a) an ordinal surrogate such as `System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this)` logged at both ends plus a third line from `ItemViewer` tying the two together, or (b) threading `ItemNumber` (or a `Func<string>` owner descriptor) into `BreadcrumbDropDownHost` at construction in `ItemViewer.Breadcrumb.cs:198-207`. Option (b) is a constructor-arity change and would disturb the reflection-based constructor binding that `BreadcrumbDropDownHost.cs:210-214` explicitly warns about; a settable `internal` property assigned right after construction (the pattern already used for `MayTakeFocus` at `ItemViewer.Breadcrumb.cs:212`) avoids that.

**Sequence source.** No application-level sequence number is needed. The log4net conversion pattern is `%date [%thread] %-5level %logger [%property{NDC}] - %message%newline` (`TaskMaster/log4net.config:30`), which yields millisecond timestamps and the thread name. Both instrumentation sites run on the single Outlook UI thread (`[VSTA_Main]` in the observed log), and log4net appends in call order, so **file order is the ordering** for same-thread lines. Millisecond timestamps alone would be insufficient — the whole flash occurs inside a fraction of a second and several of these steps can share a millisecond — so any analysis must rely on file order, not on comparing timestamps.

**Minimum useful line content.**

- `ParkFocusAndCancelSelectors` entry: method name, `WebView2Focused=`, `ActiveFormIsThisForm=` (or `ActiveFormNull=`), `Groups=<count>`; and per item, `ItemNumber=<n>`, `SelectorWasOpen=<bool>`.
- `OnDropDownClosed` entry: method name, `CloseReason=<e.CloseReason>`, `ProgrammaticClose=<_programmaticClose>`, `OpenState=<OpenState>`, `AutoClose=<DropDown.AutoClose>`, `Disposed=<_disposed>`, `PendingClose=<_openLifetime.IsPendingClose>`. Log at entry, before the guard returns, so a suppressed close is still visible.

---

## 5. The self-inflicted-versus-genuine deactivation seam

**What AC2 needs to distinguish.** At the moment `Form.Deactivate` fires, the handler must answer: did activation move to this add-in's own breadcrumb popup (suppress the cancel), or to any other window (cancel, preserving the #677 contract)?

**What is currently injectable.**

- `IQfcFormViewer` (`QuickFiler/Interfaces/IQfcFormViewer.cs`) exposes `FormDeactivated` (`:57`), `IsWebView2Focused` (`:64`), `ParkFocusOffWebView2()` (`:70`). All three are already mocked with Moq in `QfcFormControllerDeactivateTests`, and the event is delivered with `Mock.Raise`. Adding a fourth member here is a pure interface extension with an existing test pattern.
- `_groups` is injected by private-field reflection in the same test class (`QfcFormControllerDeactivateTests.cs:72-92`), so the item-controller fan-out is fully controllable.
- `IBreadcrumbDropDownHost` (`QuickFiler/Viewers/IBreadcrumbDropDownHost.cs:19-67`) exposes `IsOpen`. `BreadcrumbDropDownHost.MayTakeFocus` is an `internal Func<bool>` settable property on the concrete type (`BreadcrumbDropDownHost.cs:216`) — an existing, proven predicate seam that tests already drive.
- `BreadcrumbPendingOpenCloseTests.PendingHostHarness` (`:197-274`) constructs a real `BreadcrumbDropDownHost` headlessly with delegate seams for focus-pending, focus-anchor, cancel, and show, under an inline synchronization context. No window is created and no WebView2 is initialized.

**What is not injectable.**

- `Form.ActiveForm` is a WinForms static, read directly and privately in `ItemViewer.MayRestoreBreadcrumbFocus` (`ItemViewer.Breadcrumb.cs:270-274`). `ItemViewer` is `[ExcludeFromCodeCoverage]` (`ItemViewer.cs:20`) and the method is `private`. No test can drive it.
- `Control.Focus()`, `ToolStripDropDown.Show`, `ToolStripDropDown.AutoClose`, and `ToolStripDropDownClosedEventArgs.CloseReason` are framework state. `CloseReason` is supplied by the framework and cannot be produced in a headless test without showing a real dropdown; a test can only assert on how the handler *branches* given a reason it was handed.
- `QfcFormViewer.ParkFocusOffWebView2` and `IsWebView2Focused` (`QfcFormViewer.cs:190-207`) read real `ActiveControl` chains on a real Form.

**Minimum seam.** A boolean intent reported by the viewer at deactivation time, consumed by `ParkFocusAndCancelSelectors`:

```
IQfcFormViewer:
    /// True when the window that took activation is a breadcrumb popup owned by this form.
    bool DeactivationIsSelfInflicted { get; }
```

The production implementation lives in `QfcFormViewer` and is the only place that touches non-injectable state (`Form.ActiveForm == null` combined with "one of this form's item viewers reports an open drop-down", the latter reachable through the already-existing `IItemViewer.IsFolderDropDownOpen` at `QuickFiler/Viewers/IItemViewer.cs` and `ItemViewer.FolderSearch.cs:93`). `QfcFormController` then reads a single mockable boolean, and every AC2 branch is unit-testable with `Mock<IQfcFormViewer>` and no window.

An alternative with a smaller interface footprint is a `Func<bool>` property on `QfcFormController` assigned by the same wiring that assigns `host.MayTakeFocus` — matching the `MayTakeFocus` precedent exactly. The interface-member form is preferred because the deactivate suite already mocks `IQfcFormViewer` and would need no new construction seam.

**Residual that no unit test can cover.** Whether the popup taking focus actually produces a `Form.Deactivate` on this thread, and whether `Form.ActiveForm` is null at that moment, is Win32/WinForms runtime behavior. It is the INFERRED premise the issue flags and cannot be turned into a deterministic test in this repository. AC6's instrumentation is the mechanism that converts it from inferred to observed; it is not a substitute for a test, and the resulting fix must still be unit-tested at the managed seam.

---

## 6. The two existing test files that pin the current contract

### 6.1 `QuickFiler.Test/Controllers/QfcFormControllerDeactivateTests.cs` (248 lines)

Class `QfcFormControllerDeactivateTests`. Headless: `Mock<IQfcFormViewer>`, `Mock<IQfcItemController>`, `_groups` injected by reflection, event delivered with `Mock.Raise`. Six test methods:

| Line | Method | Currently asserts |
|---|---|---|
| 96 | `RegisterFormEventHandlers_SubscribesFormDeactivated` | the handler is wired during `RegisterFormEventHandlers` |
| 113 | `UnregisterFormEventHandlers_UnsubscribesFormDeactivated` | the handler is unwired during `UnregisterFormEventHandlers` |
| 134 | `FormDeactivated_WebView2Focused_ParksFocusOnce` | `IsWebView2Focused == true` -> `ParkFocusOffWebView2()` `Times.Once()` |
| 153 | `FormDeactivated_NoWebView2Focus_DoesNotPark` | `IsWebView2Focused == false` -> `ParkFocusOffWebView2()` `Times.Never()` |
| **172** | **`FormDeactivated_CancelsSelectorOnEveryItemController`** | with two injected item controllers, **each** receives `CancelBreadcrumbSelector()` `Times.Once()` (assertions at `:185-186`) |
| 194 | `FormDeactivated_NullGroupsOrNullItemGroups_DoesNotThrow` | null `_groups` and null `ItemGroups` are both non-throwing |
| 227 | `FormDeactivated_ItemCancelThrows_DoesNotPropagateAndContinues` | a throwing item does not propagate and the next item is still cancelled |

**Which assertion changes under AC2.** Only `FormDeactivated_CancelsSelectorOnEveryItemController` (`:172`). Today it raises `FormDeactivated` against a bare `Mock<IQfcFormViewer>` and asserts the cancel is unconditional. Under AC2 the cancel becomes conditional on the new self-inflicted seam.

**The deliberate update — not a weakening.** Split the single unconditional assertion into a matched pair, keeping the same method name for the genuine case so the #677 contract remains visibly pinned:

1. Keep `FormDeactivated_CancelsSelectorOnEveryItemController` at its current name and its current `Times.Once()` assertions on both controllers, and add one Arrange line setting the new seam to report a **genuine** deactivation. Because Moq's default `bool` return is `false`, choosing `false` to mean "genuine / not self-inflicted" makes the existing Arrange block correct without modification and keeps the diff to a doc-comment amendment. That is the recommended polarity.
2. Add a new sibling test — for example `FormDeactivated_SelfInflictedByOwnPopup_DoesNotCancelAnySelector` — that sets the seam to report self-inflicted and asserts `CancelBreadcrumbSelector()` `Times.Never()` on both controllers. This is the AC2 fail-before test.
3. Leave `FormDeactivated_WebView2Focused_ParksFocusOnce` (`:134`) unchanged only if the fix keeps focus parking unconditional. If parking is also suppressed for a self-inflicted deactivation, that test needs the same explicit genuine-case Arrange line and a paired negative. Decide this explicitly; do not let it change by default.

The file is 248 lines, so both additions fit under the 500-line ceiling without a split.

### 6.2 `QuickFiler.Test/Viewers/BreadcrumbPendingOpenCloseTests.cs` (380 lines)

Class `BreadcrumbPendingOpenCloseTests`. Five test methods:

| Line | Method | Currently asserts |
|---|---|---|
| 22 | `CloseWhileFactoryPending_InvalidatesOpenAndRepeatedCloseIsIdempotent` | a `Close(Uncommitted)` while the surface factory is unresolved completes the open task as `false` without waiting; `ShowCount == 0`; `FocusPendingCount == 0`; first close `true`, repeat `false`; **`CancelCount == 1` (`:48`)**; `FocusAnchorCount == 1` (`:49`); `IsOpen == false` |
| 55 | `CloseWhileReadinessPending_RejectsLateReadyAttachShowAndFocus` | same shape for a close during document readiness; `ReadyEventCount == 0`; **`CancelCount == 1` (`:79`)**; `FocusAnchorCount == 1` (`:80`); `PopupMessenger` null |
| 86 | `CloseCanceledFactory_AllowsOneFreshReopenWithoutLateMutation` | after a cancelled first attempt a fresh open succeeds exactly once; `ShowCount == 1`, `FocusPendingCount == 1`, `CancelCount == 1`, `FocusAnchorCount == 1`; the stale surface and messenger are disposed exactly once and the current ones are not |
| **124** | **`ToggleAndEscapeWhileOpenIsPending_EachClosesHostExactlyOnce`** | through a headless `ItemViewer` with a `Mock<IBreadcrumbDropDownHost>`: `SetBreadcrumbDropDownState(false)` produces exactly one `Close(Uncommitted)`; `HandleSelectorKey(Escape)` produces exactly one `Close(ExplicitCommit)` |
| **143** | **`AutomaticSelectorCloseWhileOpenIsPending_ClosesHostExactlyOnce`** | `BreadcrumbCoordinator.CancelSelector()` produces exactly one `Close(ExplicitCommit)` |

**Which assertion changes under the new contract.** The five tests exercise *explicitly requested* closes, never a native auto-close and never a form deactivation, so none of them is contradicted by AC1, AC2 or AC4. Two assertions are at risk depending on the fix shape:

- **`CancelCount == 1` at `:48` and `:79`.** These count invocations of the `cancelSelection` delegate supplied to the host constructor, reached through `FinishClose(Uncommitted)`. If the AC3 fix makes `FinishClose` consult a pending-commit latch and suppress the cancel while a commit is in flight, these two must be reasoned about explicitly: in both tests no commit is in flight, so the correct post-fix value remains `1` and the assertion must be **kept unchanged** as the guard proving the suppression is scoped, not global. If a proposed fix makes either of these `0`, that is evidence the suppression is too broad — treat it as a design signal, not as a test to update.
- **`FocusAnchorCount == 1` at `:49`, `:80`, `:114`.** These count the `focusAnchor` delegate, which `FinishClose` invokes through `FocusAnchorIfPermitted`. The harness leaves `MayTakeFocus` at its `() => true` default (`BreadcrumbDropDownHost.cs:216`), so they stay `1` unless the fix changes the default. Do not change the default.
- **`:124` and `:143`.** Under AC1 these remain correct as written: an explicit toggle-off and an explicit `CancelSelector()` must still close the host exactly once. The deliberate update here is **no change**, plus a new sibling test asserting the converse — that a *native* close arriving while an activation-commit is pending does not cancel (the AC3 fail-before test). That sibling needs a `ToolStripDropDownClosedEventArgs`-driven entry, which the existing `PendingHostHarness` cannot produce because it never shows a real dropdown; see section 8 for the automation limit.

The file is 380 lines. One or two added tests fit; a third harness would likely require a new file.

### 6.3 Adjacent test files that will feel the change

`QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.cs` is **499** lines and `QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs` is **500** lines. Both are at or one line from the ceiling. Any new host-level or integration-level test must go into a new file, which in a non-SDK-style project also means a new `<Compile Include>` entry.

---

## 7. Candidate write set

Non-SDK-style projects (explicit `<Compile Include>` required for every added or removed `.cs`): **`QuickFiler`**, **`QuickFiler.Test`**, **`UtilitiesCS`**, **`UtilitiesCS.Test`** — all four declare `<Project ToolsVersion="15.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">`. On the write set below, only `QuickFiler` and `QuickFiler.Test` gain files, so only those two `.csproj` files must be edited.

### Production

- `QuickFiler/Controllers/QfcFormController.Deactivate.cs` — AC6 instrumentation of `ParkFocusAndCancelSelectors`; AC2 self-inflicted guard around the cancel loop. 73 lines, ample room.
- `QuickFiler/Interfaces/IQfcFormViewer.cs` — AC2: declare the self-inflicted-deactivation seam. 72 lines.
- `QuickFiler/Viewers/QfcFormViewer.cs` — AC2: production implementation of the seam (the only site that reads `Form.ActiveForm`-class state on the form side). 293 lines.
- `QuickFiler/Viewers/BreadcrumbDropDownHost.Diagnostics.cs` — **NEW**. AC6 instrumentation of `OnDropDownClosed` plus the `log4net.ILog` field. Required because `BreadcrumbDropDownHost.cs` is 498/500 lines and cannot absorb them; follows the existing `BreadcrumbDropDownHost.Open.cs` partial-split precedent.
- `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` — AC3: commit-before-cancel ordering in `FinishClose`; move or delegate `OnDropDownClosed` to the new diagnostics part. Any net line growth must be offset by the move.
- `QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs` — AC1/AC3: if the fix changes the `AutoClose = takeFocus` policy in `ShowPopup`, or adds a pending-commit latch set at open time. 107 lines.
- `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` — AC2: assign the new self-inflicted/popup-owns-activation state alongside the existing `host.MayTakeFocus = MayRestoreBreadcrumbFocus` at `:212`. 456 lines; watch the ceiling.
- `QuickFiler/Controllers/QfcItemController.EventHandlers.cs` — AC4: extend the `_searchLeaveHandoffPending` latch to the mouse open path. 263 lines.
- `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` — AC3, only if the commit-before-cancel ordering has to be enforced at the coordinator rather than the host. 395 lines.
- `QuickFiler/Resources/FolderBreadcrumb.html` — AC3, only if the row-activation handler must move from `click` (mouseup) to `pointerdown`/`mousedown` so the message is produced before dismissal. **Contention note:** this file is also the breadcrumb page the concurrent sibling item renders row text into. The change contemplated here is confined to the row event listener at `:289-291` and touches no projection logic, but the file is shared and the contention is recorded here rather than hidden.
- `QuickFiler/QuickFiler.csproj` — **required**: add a `<Compile Include="Viewers\BreadcrumbDropDownHost.Diagnostics.cs" />` entry.

### Test

- `QuickFiler.Test/Controllers/QfcFormControllerDeactivateTests.cs` — AC2: deliberate update of `FormDeactivated_CancelsSelectorOnEveryItemController` plus the new self-inflicted negative test. 248 lines.
- `QuickFiler.Test/Viewers/BreadcrumbPendingOpenCloseTests.cs` — AC1/AC3: keep the existing five, add the pending-commit-versus-native-close guard if it fits. 380 lines.
- `QuickFiler.Test/Viewers/BreadcrumbDropDownCloseOrderingTests.cs` — **NEW**. AC3 commit-before-cancel ordering at the host seam; `BreadcrumbDropDownHostTests.cs` (499) and `BreadcrumbDropDownIntegrationTests.cs` (500) have no room.
- `QuickFiler.Test/Controllers/QfcItemController.SearchLeaveLatchTests.cs` — **NEW**, or extend `QuickFiler.Test/Controllers/QfcItemController.EventHandlersTests.cs` (477 lines, 23 lines of headroom). AC4 mouse-path latch. Prefer the new file; 23 lines is not enough for an Arrange-Act-Assert pair with doc comments.
- `QuickFiler.Test/QuickFiler.Test.csproj` — **required**: add `<Compile Include>` entries for each new test file above.

### Explicitly not in the write set

These were considered and are stated without backticks because no change is expected: UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs, UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.SearchPresentation.cs, UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs, UtilitiesCS/UtilitiesCS.csproj, UtilitiesCS.Test/UtilitiesCS.Test.csproj, QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs, QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs, QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs, QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.Focus.cs, QuickFiler/Viewers/IBreadcrumbDropDownHost.cs. The AC5 regression guard is satisfied by leaving the session-preserving replacement path untouched, which is why the UtilitiesCS router and session files must stay out of the diff.

---

## 8. Runtime log evidence

Source: `<taskmaster-repo>/TaskMaster/bin/Debug/logs/debug_2026-09-06.log`, read only, outside this worktree. Not modified and not copied into the repository.

### 8.1 What the log does show

- Four complete QuickFiler sessions on 2026-09-06, each ending with a `QfcHomeController` "Home cleanup complete; ribbon release callback invoked." line at 17:35:42, 19:08:03, 19:56:14 and 20:15:31.
- Per-item load telemetry from `QuickFiler.Controllers.QfcItemController` ("Probability debug [QfcItemController.LoadFolderHandlerAsync ...] ... TopScore=") and from `QuickFiler.Controllers.QfcDatamodel`, `QfcStreamingDequeueConfidenceGate`, `QfcCollectionController`, `QuickFiler.Helper_Classes.ConversationResolver`.
- The #791 Cancel-teardown stage lines from `QuickFiler.Controllers.QfcFormController` at the end of each session (`Cancel teardown starting. AlreadyCancelled=False`, then `Stage=cancel-token`, `reset-keyboard`, `park-focus`, `unregister-handlers`, `hide-form`, `quiesce-loader`, `groups-cleanup`).
- Many ERROR and WARN lines, none of them from the QuickFiler item-view drop-down. The recurring ones are `UtilitiesCS.EmailIntelligence.ImageStripper` ("Failed to initialise tesseract engine"), `UtilitiesCS.HelperClasses.SegmentStopWatch` ("SegmentStopWatch created on UI thread 1"), and `QuickFiler.Viewers.WebView2BreadcrumbHost` / `QuickFiler.Controllers.EfcFormController` ("Breadcrumb CoreWebView2 initialization failed ... 0x8007139F").

### 8.2 The `WebView2BreadcrumbHost` errors are a different component

`WebView2BreadcrumbHost` is referenced in production only from `QuickFiler/Viewers/WebView2BreadcrumbHost.cs`, `QuickFiler/Viewers/IBreadcrumbWebHost.cs`, `QuickFiler/Viewers/EfcViewer.cs` and `QuickFiler/Controllers/EfcFormController.cs`. The logged stack frames confirm the caller is `EfcFormController.InitializeBreadcrumbHostAsync`. This is the EmailFilerControl breadcrumb, not the QuickFiler `ItemViewer` popup pipeline (`BreadcrumbDropDownHost`). These errors are unrelated to #796 and are the subject of a separate promoted entry (`docs/features/potential/promoted/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state.md`).

### 8.3 What the log does not show — the AC6 justification

Searching the whole file for log4net logger names of the QuickFiler item-view drop-down pipeline returns **nothing**:

- `QuickFiler.Viewers.BreadcrumbDropDownHost` — zero lines (the type declares no logger at all).
- `QuickFiler.Viewers.BreadcrumbUiDispatcher`, `BreadcrumbCollapsedSurfaceController`, `BreadcrumbWebViewSurfaceFactory`, `BreadcrumbMessengerHub`, `ItemViewer`, `QfcFormViewer` — zero lines each. The only `QuickFiler.Viewers.*` logger that appears anywhere in the file is `WebView2BreadcrumbHost`.
- `QuickFiler.Controllers.QfcFormController` appears only for the Cancel-teardown stage lines; nothing from `ParkFocusAndCancelSelectors`, which logs only on the per-item exception path (`QfcFormController.Deactivate.cs:64`) and therefore emitted nothing.

The most direct evidence is the interaction window itself. In the fourth session the last load-time line is at `20:02:28,953` and the next line in the entire file is `20:15:31,320` (`Cancel teardown starting`). That is a thirteen-minute window of live user interaction with the QuickFiler form — the window in which the reproduction gestures occur — containing **zero** log lines of any level.

**Verdict.** The issue's claim is confirmed: the close is a normal, non-exceptional code path and produces no diagnostic of any kind. The many ERROR/WARN lines in the file all belong to other components and none of them is emitted by the drop-down open or close pipeline. Runtime instrumentation is therefore the only way to establish the ordering, which is exactly what AC6 requires as the first implementation step.

---

## 9. Acceptance criteria, verbatim

- [ ] AC1: Opening the list by arrow click or by Down in the search box leaves it open until Escape, Left, a second arrow click, an item selection, or selection of a different QfcItem.
- [ ] AC2: A deactivation of the QuickFiler form caused by the popup taking focus does not cancel the selector session; a deactivation caused by any other window still does (the #677 contract is preserved for genuine deactivation).
- [ ] AC3: A mouse click on a row in the open list selects that row and closes the list; the selection is committed before any auto-close cancel runs.
- [ ] AC4: The #680 leave-handoff latch covers the mouse open path as well as the Down-arrow path.
- [ ] AC5: Row-set refreshes while open (search, late decoration) continue not to close the list (#438 AC-3 regression guard).
- [ ] AC6: The first implementation step instruments `ParkFocusAndCancelSelectors` and `OnDropDownClosed` with debug log lines so the runtime ordering is confirmed before the fix is chosen.

---

## 10. Candidate status summary

| Candidate | Status | Basis |
|---|---|---|
| 1 — `QfcFormController.ParkFocusAndCancelSelectors` on form deactivation | **Downstream code CONFIRMED; first-cause status INFERRED** | The wiring (`SetupDisposal.cs:175`), the unconditional cancel loop (`Deactivate.cs:52-70`), the popup focus call (`ItemViewer.Breadcrumb.cs:203`), the absence of any self-inflicted latch, and the `FinishOpenCore` re-check (`BreadcrumbDropDownOpenCoordinator.cs:273-288`) are all verified in source. That focusing the popup's WebView2 actually deactivates the QuickFiler form is Win32/WinForms runtime behavior and remains unverified. Strongest candidate on both reproduction paths; on the search path (step 4) it is the only one of the three not already excluded by code. |
| 2 — native `ToolStripDropDown` auto-close -> `OnDropDownClosed` -> `FinishClose(Uncommitted)` | **Code path CONFIRMED; first-cause status INFERRED for gesture opens, LIKELY REFUTED for the search path** | `OnDropDownClosed` (`:426-437`) and the unconditional `_cancelSelection()` under `Uncommitted` (`:449-450`) are verified. For gesture opens `AutoClose == true` (`Open.cs:100` with `takeFocus == true`), so the framework auto-close is armed. For a search-driven open `AutoClose == false`, which disables the framework auto-dismiss, so this candidate cannot be the first cause of the step-4 click-without-select symptom as the issue states. `e.CloseReason` is currently discarded and is the value that settles this. |
| 3 — `QfcItemController.TextBoxSearch_Leave` | **Code path CONFIRMED; LIKELY NOT REACHED on either reproduction path** | The handler (`EventHandlers.cs:217-228`) and the Down-only latch (`:195`) are verified. On the arrow-click path the search textbox never holds focus, so no `Leave` occurs. On the search path `IsWebView2Focused` is false (the leaf is a `TextBox`), so `ParkFocusOffWebView2` is skipped and the synchronous `ActiveControl`-change route into `Leave` does not fire; and a click into the popup moves activation to a different top-level window rather than to another control on the same form, which does not raise `Leave`. The AC4 gap (no mouse-path latch) is real and must still be closed, but this candidate is not the first cause. |

Recommended sequencing: land AC6 instrumentation first (it is a two-site, log-only change with the new-partial-file constraint noted in section 7), reproduce, read the ordering off file order in a single-thread log, then choose between the AC2 latch and the AC3 commit-before-cancel ordering as the primary fix. Both will very likely be needed; the log determines which one is the *first* cause and therefore which one carries the fail-before regression test for AC1.

---

## Automation Feasibility

Manual verification of this defect requires a live Outlook process, a real WebView2 surface, a real `ToolStripDropDown`, and human mouse and keyboard gestures. None of that is automatable in this repository: the test policy forbids external processes, and no window may be shown.

### Automatable as MSTest / Moq / FluentAssertions unit tests

| Requirement | Automatable | Seam |
|---|---|---|
| AC2 — cancel suppressed on self-inflicted deactivation, still fires on genuine deactivation | **Yes, fully** | `Mock<IQfcFormViewer>` + `Mock.Raise(x => x.FormDeactivated += null, ...)` + reflection-injected `_groups`, exactly as `QfcFormControllerDeactivateTests` already does. Both branches of the new seam are drivable. |
| AC3 — commit runs before any auto-close cancel | **Partially** | The *branching* is testable at the `BreadcrumbDropDownHost` seam using `PendingHostHarness`-style delegate counters: assert the cancel delegate is not invoked while a commit latch is set, and is invoked when it is not. What is **not** testable is producing a genuine framework `ToolStripDropDownClosedEventArgs` with `CloseReason == AppFocusChange`; a test can only invoke the handler with a constructed args value, which proves the branch, not the framework's choice of reason. |
| AC4 — mouse-path leave latch | **Yes, fully** | `QfcItemController` with a `Mock<IItemViewer>`; drive the mouse open path, then raise the search-box `Leave` and assert `SetFolderDroppedDown(false)` is not called. `IItemViewer.IsFolderDropDownOpen` and `SearchLeave` are already interface members. |
| AC5 — refresh while open does not close | **Yes, fully, and already covered** | `ReplaceItemsPreservingSession` emits no `OpenStateChanged` (`FolderBreadcrumbBridgeRouter.SearchPresentation.cs:38-55`); existing UtilitiesCS.Test router suites pin this. A `Mock<IBreadcrumbDropDownHost>` assertion of `Close(It.IsAny<...>())` `Times.Never()` across a refresh is the regression guard at the viewer level. |
| AC6 — instrumentation exists at the two named sites | **Partially** | That a log statement exists can be pinned structurally (a source-text or reflection assertion, as the repository has done elsewhere for declaration-only seams). That the emitted *ordering* is what the fix assumes cannot be asserted without the live host. |
| AC1 — list stays open on gesture open | **Partially** | Every managed step is assertable: the open task resolves `true`, `host.IsOpen` stays `true`, no `Close` reaches the mocked host, and the session reports `IsSelectorOpen == true`. What is not assertable is that no *framework* close occurs, because no framework dropdown is shown. |

### Requires a human, and the recommended response for each

1. **The popup taking focus deactivates the QuickFiler form (candidate 1's premise).** Not automatable — Win32 activation on a live UI thread. *Recommended response:* AC6 instrumentation plus a one-time manual observation recorded as a runbook step and an evidence artifact under the feature's `evidence/` tree. Do not attempt a synthetic test; record the observation as the confirmation of the INFERRED premise.
2. **`ToolStripDropDownClosedEventArgs.CloseReason` produced by the framework for this gesture.** Not automatable. *Recommended response:* capture it in the AC6 log line and cite the log excerpt as evidence in the fix's spec. This converts an inference into an observation without adding a merge gate that cannot pass.
3. **A row click in the expanded WebView2 produces (or fails to produce) a `selectorActivate` message.** Not automatable — requires a real WebView2 and a real mouse gesture. *Recommended response:* extend the existing #438 runbook (`docs/features/archive/2026-08-07-quickfiler-search-keystroke-focus-steal-438/runbooks/verify-search-focus-retention.runbook.md`) with the arrow-click and row-click gestures, as the issue's Validation section already proposes, and record the result as a manual-verification evidence artifact. If the instrumentation shows the message never arrives, the HTML `click` -> `pointerdown` change becomes the AC3 fix and its own managed-side assertion (that a `pointerdown`-sourced `selectorActivate` commits) is unit-testable at the coordinator.
4. **WinForms modal menu mode retargeting keyboard while `AutoClose == true`.** Not automatable; this is the documented #680 residual. *Recommended response:* carry forward the #680 precedent — assert the managed precondition (`DropDown.AutoClose` at the moment `_showPopup` runs) rather than the framework consequence, and document the framework consequence as a manual check.
5. **Selecting a different QfcItem while the list is open (AC1's last clause).** Partially automatable at the collection-controller seam with mocks; the visual outcome is not. *Recommended response:* unit-test the managed close intent, manual-verify the visual outcome, and record both.

General recommendation: follow the precedent set by #400 and #438 — a documented, maintainer-sanctioned manual-verification exception with a runbook and an evidence artifact, rather than a merge gate that no automated suite can satisfy. Every AC retains at least one automatable managed-seam assertion, so no acceptance criterion is left with manual verification as its only evidence.
