# QfcItemController Exemption Boundary — Cycle-3 Reduced (for maintainer ratification)

- **Timestamp:** 2026-07-02T15-05
- **Issue:** #227 (remediation cycle 3 — targeted residual reduction)
- **Supersedes:** `evidence/other/exemption-boundary.2026-07-02T10-30.md` (the cycle-2, 41-member
  boundary; the 2026-07-02 re-audit found 17 of those 41 actionable — this artifact records their
  removal).
- **Status:** Reduced boundary, re-submitted for maintainer ratification at review.

## Summary of reduction

| Milestone | Exemption count | Basis |
|---|---:|---|
| Cycle-1 (denied) | 103 | Blanket per-partial-file `[ExcludeFromCodeCoverage]` |
| Cycle-2 (partial ratification pending) | 41 | Behavioral seams (Phases 5-7); every residual individually justified |
| Cycle-3 Phase 9 (Tier 1, test-only) | 32 | 9 members de-exempted with zero new production seams |
| Cycle-3 Phase 10a (`FolderPredictor` factory-delegate) | 27 | 5 members de-exempted via a new factory-delegate seam |
| Cycle-3 Phase 10b (`Theme` + `IUiDispatcher` retrofit) | **24 total** = 23 controller members + 1 DI-adapter shim | 3 members de-exempted by extending the existing `IUiDispatcher` seam into `Theme` |

Net this cycle: **41 → 24** (17 members de-exempted, matching the re-audit's finding exactly: 9 + 8 =
17). Every one of the 24 remaining residuals is an individually-named, technically-justified structural
residual with an inline per-member comment (where the comment was removed along with an attribute, the
member is no longer a residual). **No blanket/category exemption remains.**

## De-exempted this cycle (17, now covered by >= 1 passing test)

- **Phase 9 — Tier 1, test-only (9):** `RegisterExpandedActions` (dictionary-membership test,
  `QfcItemController.EventWiringTests.cs`); `JumpToAsync(Control)` (bare handle-less `Control`,
  `QfcItemController.NavigationTests.cs`); `PopulateControls(MailItem,int)` and
  `PopulateControlsAsync` (`Mock<MailItem>`, `QfcItemController.ViewerSetupTests.cs`); `ToggleFocus()`
  and `ToggleFocus(Enums.ToggleState)` (non-executing `Mock<IItemViewer>.Invoke` marshal-verification,
  `QfcItemController.FocusAndThemeTests.cs`); `WpfUiDispatcher`'s forwarding body (live dispatcher on a
  dedicated STA thread, `WpfUiDispatcherTests.cs`); `MailItemActionsAdapter` (attribute removal only —
  full coverage already existed via `MailItemActionsAdapterTests.cs`); `BtnFlagTask_Click` (factory
  sentinel-exception test, `QfcItemController.EventHandlersTests.cs`).
- **Phase 10a — `FolderPredictor` factory-delegate seam (5):** `LoadFolderHandler`,
  `LoadFolderHandlerAsync`, `PopulateFolderComboBox`, `PopulateFolderComboBoxAsync` (all in
  `QfcItemController.FolderHandlingTests.cs`); `TextBoxSearch_TextChanged` (`Mock<IFolderSearchHandler>`,
  `QfcItemController.EventHandlersTests.cs`).
- **Phase 10b — `Theme` + `IUiDispatcher` retrofit (3):** `ToggleFocusAsync(Enums.ToggleState)`,
  `ToggleFocusAsync()`, `ApplyReadEmailFormat` (all in `QfcItemController.SeamDispatcherTests.cs`, using
  `QfcItemControllerTestSupport.BuildDispatchableTheme` with a non-executing `Mock<IUiDispatcher>`).

## Residual set (24) — individually justified

Verified via `grep -rnE "ExcludeFromCodeCoverage\]" QuickFiler/Controllers/QfcItemController*.cs
UtilitiesCS/Threading/WpfUiDispatcher.cs QuickFiler/Viewers/WebView2CoreInitializer.cs
QuickFiler/Interfaces/MailItemActionsAdapter.cs` (count = 24, matches `evidence/qa-gates/p10b-theme-seam-verification.2026-07-02T15-03.md` and re-verified at P11-T7).

### 1. Concrete control-tree orchestration tied to the retained `(ItemViewer)`-cast / no-leaf-interface invariant (12)

| Member | File | Reason |
|---|---|---|
| `Initialize` (9-arg private) | `QfcItemController.Initialization.cs` | Delegates into `Initialize(bool)`; barrier inherited. |
| `Initialize(bool async)` | `QfcItemController.Initialization.cs` | `ResolveControlGroups((ItemViewer)_itemViewer)`/`SetupThemes` require the concrete `(ItemViewer)` cast, which the retained no-leaf-interface invariant (Option A, P2-T4) deliberately keeps. |
| `InitializeAsync` | `QfcItemController.Initialization.cs` | Same concrete-cast orchestration. |
| `InitializeGraphicsAsync` | `QfcItemController.Initialization.cs` | Same. |
| `InitializeSequentialAsync` | `QfcItemController.Initialization.cs` | Same. |
| `CreateAsync` | `QfcItemController.Initialization.cs` | Static factory; barrier inherited from `InitializeAsync`. |
| `CreateSequentialAsync` | `QfcItemController.Initialization.cs` | Static factory; barrier inherited from `InitializeSequentialAsync`. |
| `InitializeWebViewAsync` | `QfcItemController.ViewerSetup.cs` | WebView2 SDK calls are isolated behind `IWebViewCoreInitializer`, but the residual barrier is `((ItemViewer)_itemViewer).L0v2h2_WebView2` — `IItemViewer` intentionally exposes no raw-control WebView2 accessor per the retained P2-T4 narrowing invariant. |
| `ResolveControlGroups(ItemViewer)` | `QfcItemController.ViewerSetup.cs` | Walks a concrete `ItemViewer`'s Designer-generated `Controls` tree (`GetAllChildren()`) and classifies children by concrete WinForms type. No intent-level substitute without constructing a real `ItemViewer` (declined, Option B). |
| `ResolveControlGroupsAsync(ItemViewer)` | `QfcItemController.ViewerSetup.cs` | Async counterpart of the above; same reason. |
| `WireEvents` | `QfcItemController.EventWiring.cs` | Calling it against `Mock<IItemViewer>.Object` throws `InvalidCastException` inside the `WireControlTreeEvents()` cast it invokes. |
| `WireControlTreeEvents` | `QfcItemController.EventWiring.cs` | `((ItemViewer)_itemViewer).ForAllControls(...)` and the `Buttons`/`MenuItems` concrete-control loops require a live `ItemViewer`. |

### 2. Already-named `TlpCellSnapShot` follow-up (2, P7-T5, out of scope this cycle)

| Member | File | Reason |
|---|---|---|
| `ToggleExpansionOff` | `QfcItemController.Navigation.cs` | `_tlpStates["Compressed"].ApplyState((ItemViewer)_itemViewer)` walks the live control tree via `TlpCellSnapShot.ApplyState(Control)`. Recorded follow-up: retype to `ApplyState(IContainerControlLocal)`. Not pursued this cycle. |
| `ToggleExpansionOn` | `QfcItemController.Navigation.cs` | Same barrier, symmetric method. |

### 3. Deliberate virtual test seams (3)

| Member | File | Reason |
|---|---|---|
| `DoLoadConversationResolverCoreAsync` | `QfcItemController.Conversation.cs` | Deliberate `virtual` override point; production body is intentionally never exercised because tests override it — a testing pattern, not a barrier. |
| `ToggleExpansion(Enums.ToggleState)` | `QfcItemController.Navigation.cs` | `virtual`, made so tests can override the `TlpCellSnapShot`-bound body; tied to the #31/#32 follow-up. |
| `ToggleExpansionAsync(Enums.ToggleState)` | `QfcItemController.Navigation.cs` | Same, async counterpart. |

### 4. `async void` WinForms-event-signature shells, core logic already extracted and tested (6)

| Member | File | Reason |
|---|---|---|
| `BtnPopOut_Click` | `QfcItemController.EventHandlers.cs` | `async void` — the substantive logic is already extracted and tested (`BtnPopOutCore`, not exempt). The remaining shell exists only because WinForms event-handler delegates require a `void`-returning signature that a test cannot `await`. |
| `BtnReply_Click` | `QfcItemController.EventHandlers.cs` | Same shape; core (`BtnReplyCore`) already tested. |
| `BtnReplyAll_Click` | `QfcItemController.EventHandlers.cs` | Same shape; core (`BtnReplyAllCore`) already tested. |
| `BtnForward_Click` | `QfcItemController.EventHandlers.cs` | Same shape; core (`BtnForwardCore`) already tested. |
| `TxtboxBody_DoubleClick` | `QfcItemController.EventHandlers.cs` | Same shape; core (`TxtboxBodyDoubleClickCore`, calls `_mailActions.Display()`) already tested. |
| `WebView2Control_CoreWebView2InitializationCompleted` | `QfcItemController.EventWiring.cs` | Same shape; substantive body extracted to `HandleWebViewInitializedAsync` (already tested). |

### 5. Genuine external-runtime dependency (1)

| Member | File | Reason |
|---|---|---|
| `WebView2CoreInitializer` | `QuickFiler/Viewers/WebView2CoreInitializer.cs` | Forwards to `CoreWebView2Environment.CreateAsync`/`WebView2.EnsureCoreWebView2Async`, which require the installed WebView2 Runtime (a native, versioned browser component/process) to do anything beyond construction. Unlike `Dispatcher` (an in-process .NET type with a proven live-dispatcher test technique, now applied to `WpfUiDispatcher` in Phase 9), there is no in-process, no-external-dependency equivalent for the WebView2 Runtime. Exercising this body would violate `.claude/rules/general-unit-test.md` External Dependencies rule. |

**12 + 2 + 3 + 6 + 1 = 24.**

## Ratification request

This reduced boundary (41 → 24 this cycle; 103 → 24 overall; no blanket/category exemption; each
residual individually justified in source and in this artifact) is re-submitted for maintainer
ratification at the cycle-3 feature review, per the authority-scoped coverage-exception precedent
(`docs/features/active/2026-06-28-qfc-form-viewer-testability-223/maintainer-decision.2026-06-29.md`).
None of the 24 is exempted merely because it currently carries the attribute: 12 are tied to an
explicit, currently-retained design invariant (no leaf-control interfaces, `ItemViewer` stays untouched
Designer code — the maintainer's own Option A scope decision), 6 are a direct structural consequence of
the thin-delegator directive (item 5) itself, 3 are a deliberate, already-validated testing pattern, 2
are an already-named, already-deferred follow-up (P7-T5), and 1 is a genuine external-process dependency
barred by the repo's own unit-test policy.
