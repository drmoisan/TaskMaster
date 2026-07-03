# QfcItemController Exemption Boundary — Cycle-5 Reduced (for maintainer ratification)

- **Timestamp:** 2026-07-02T17-00
- **Issue:** #227 (remediation cycle 5 — targeted residual reduction)
- **Supersedes:** `evidence/other/exemption-boundary.2026-07-02T15-05.md` (the cycle-3, 24-member boundary).
- **Status:** Reduced boundary, submitted for maintainer ratification alongside the prior 24.

## Summary of reduction

| Milestone | Exemption count | Basis |
|---|---:|---|
| Cycle-3 Phase 10b (prior boundary) | 24 | See `evidence/other/exemption-boundary.2026-07-02T15-05.md` |
| Cycle-5 (this cycle) | **19** | 5 members de-exempted: `ResolveControlGroups(ItemViewer)`, `WireControlTreeEvents()`, `WireEvents()` (R1+R3 — headless real-`ItemViewer` construction) and `ToggleExpansionOff`/`ToggleExpansionOn` (R2 — `TlpCellSnapShot`/`IContainerControlLocal` retrofit) |

Net this cycle: **24 → 19** (5 members de-exempted, matching the plan's target exactly).

## De-exempted this cycle (5, now covered by ≥ 1 passing test exercising genuine behavior)

- **R1+R3 — headless real-`ItemViewer` construction (3):** `ResolveControlGroups(ItemViewer)` and `WireControlTreeEvents()` are exercised end-to-end against a real, headless `new QuickFiler.ItemViewer()` (constructed after installing/restoring a `SynchronizationContext`, mirroring `ProgressPane_Tests.cs`'s try/finally pattern), with zero mocking of the control tree; outcomes are verified by raising the real protected `Control.OnPreviewKeyDown`/`OnKeyDown`/`OnMouseEnter` methods via reflection (`QfcItemController.ViewerSetupTests.cs`, `QfcItemController.EventWiringTests.cs`). `WireEvents()` reuses the same headless-`ItemViewer` fixture to prove both `WireControlTreeEvents()` and `WireIntentEvents()` ran in sequence.
- **R2 — `TlpCellSnapShot`/`IContainerControlLocal` retrofit (2):** `ToggleExpansionOff`/`ToggleExpansionOn` no longer carry a concrete `(ItemViewer)` cast; `TlpCellSnapShot.ApplyState`/`TlpCellSnapShotList.ApplyState` were retyped from `Control` to `IContainerControlLocal` (already implemented implicitly by `ItemViewer : UserControl` with zero forwarders needed — confirmed by a live compiler run, see `evidence/other/p2-t3-containercontrol-accessibility-groundtruth.2026-07-02T17-00.md`), and `IItemViewer` now extends `IContainerControlLocal` so `Mock<IItemViewer>` satisfies it automatically. Tests use a bare `Control` host with named children and a `Mock<IItemViewer>` (no real `ItemViewer` required), proving `ApplyState`'s real `Find`/style-copy/`Enabled`/`Visible` restore logic (`TlpCellSnapShotTests.cs`, `QfcItemController.NavigationTests.cs`).

## Residual set (19) — individually justified

Verified via `grep -rnE "ExcludeFromCodeCoverage\]" QuickFiler/Controllers/QfcItemController*.cs UtilitiesCS/Threading/WpfUiDispatcher.cs QuickFiler/Viewers/WebView2CoreInitializer.cs QuickFiler/Interfaces/MailItemActionsAdapter.cs` (count = 19, matches `evidence/qa-gates/final-residual-and-file-size-verification.2026-07-02T17-00.md`).

### 1. Concrete control-tree orchestration tied to the retained `(ItemViewer)`-cast / no-leaf-interface invariant (9, down from 12)

| Member | File | Reason |
|---|---|---|
| `Initialize` (9-arg private) | `QfcItemController.Initialization.cs` | Delegates into `Initialize(bool)`; barrier inherited. |
| `Initialize(bool async)` | `QfcItemController.Initialization.cs` | `SetupThemes`/orchestration requires the concrete `(ItemViewer)` cast for the WebView-init dispatch path; barrier is the unbuilt WinForms message-pump seam for `UiSyncContext`-awaiting paths (research §1.3), not headless construction. |
| `InitializeAsync` | `QfcItemController.Initialization.cs` | Same concrete-cast orchestration; awaits `ResolveControlGroupsAsync`'s `UiSyncContext` continuation. |
| `InitializeGraphicsAsync` | `QfcItemController.Initialization.cs` | Same. |
| `InitializeSequentialAsync` | `QfcItemController.Initialization.cs` | Same. |
| `CreateAsync` | `QfcItemController.Initialization.cs` | Static factory; barrier inherited from `InitializeAsync`. |
| `CreateSequentialAsync` | `QfcItemController.Initialization.cs` | Static factory; barrier inherited from `InitializeSequentialAsync`. |
| `InitializeWebViewAsync` | `QfcItemController.ViewerSetup.cs` | WebView2 SDK calls are isolated behind `IWebViewCoreInitializer`, but the residual barrier is `((ItemViewer)_itemViewer).L0v2h2_WebView2` — `IItemViewer` intentionally exposes no raw-control WebView2 accessor. Unaffected by the R1/R2 retrofits this cycle. |
| `ResolveControlGroupsAsync(ItemViewer)` | `QfcItemController.ViewerSetup.cs` | Async counterpart of `ResolveControlGroups`; does `await itemViewer.UiSyncContext` — the `WindowsFormsSynchronizationContext`-deadlock hazard documented in `AsyncSerialization_Tests.cs`. Requires the unbuilt WinForms message-pump test seam, not headless construction alone. |

**Cycle-5 removed from this bucket:** `ResolveControlGroups(ItemViewer)`, `WireControlTreeEvents`, `WireEvents` (all 3 confirmed testable via headless real-`ItemViewer` construction with no new production seam).

### 2. `TlpCellSnapShot` follow-up (0, down from 2 — fully resolved this cycle)

Both `ToggleExpansionOff` and `ToggleExpansionOn` are de-exempted this cycle via the `IContainerControlLocal` retrofit. This category is now empty.

### 3. Deliberate virtual test seams (3, unchanged)

| Member | File | Reason |
|---|---|---|
| `DoLoadConversationResolverCoreAsync` | `QfcItemController.Conversation.cs` | Deliberate `virtual` override point; production body is intentionally never exercised because tests override it — a testing pattern, not a barrier. |
| `ToggleExpansion(Enums.ToggleState)` | `QfcItemController.Navigation.cs` | `virtual`, made so tests can override the state-taking body (now de-exempted at the leaf via R2; the parent `virtual` dispatcher remains a deliberate test seam per its own design). |
| `ToggleExpansionAsync(Enums.ToggleState)` | `QfcItemController.Navigation.cs` | Same, async counterpart. |

### 4. `async void` WinForms-event-signature shells, core logic already extracted and tested (6, unchanged)

| Member | File | Reason |
|---|---|---|
| `BtnPopOut_Click` | `QfcItemController.EventHandlers.cs` | `async void` shell; core (`BtnPopOutCore`) already tested. |
| `BtnReply_Click` | `QfcItemController.EventHandlers.cs` | Same shape; core (`BtnReplyCore`) already tested. |
| `BtnReplyAll_Click` | `QfcItemController.EventHandlers.cs` | Same shape; core (`BtnReplyAllCore`) already tested. |
| `BtnForward_Click` | `QfcItemController.EventHandlers.cs` | Same shape; core (`BtnForwardCore`) already tested. |
| `TxtboxBody_DoubleClick` | `QfcItemController.EventHandlers.cs` | Same shape; core (`TxtboxBodyDoubleClickCore`) already tested. |
| `WebView2Control_CoreWebView2InitializationCompleted` | `QfcItemController.EventWiring.cs` | Same shape; substantive body extracted to `HandleWebViewInitializedAsync` (already tested). |

### 5. Genuine external-runtime dependency (1, unchanged)

| Member | File | Reason |
|---|---|---|
| `WebView2CoreInitializer` | `QuickFiler/Viewers/WebView2CoreInitializer.cs` | Forwards to `CoreWebView2Environment.CreateAsync`/`WebView2.EnsureCoreWebView2Async`, which require the installed WebView2 Runtime (a native, versioned browser component/process). Exercising this body would violate `.claude/rules/general-unit-test.md` External Dependencies rule. |

**9 + 0 + 3 + 6 + 1 = 19.**

## Ratification request

This reduced boundary (24 → 19 this cycle; 103 → 19 overall since cycle-1; no blanket/category exemption; each residual individually justified in source and in this artifact) is submitted for maintainer ratification alongside the prior 24, per the authority-scoped coverage-exception precedent (`docs/features/active/2026-06-28-qfc-form-viewer-testability-223/maintainer-decision.2026-06-29.md`). None of the 19 is exempted merely because it currently carries the attribute: 9 are tied to the unbuilt WinForms-message-pump test-infrastructure gap for `UiSyncContext`-awaiting async paths (tracked as a separate follow-up issue per the remediation-inputs scope note), 6 are a direct structural consequence of the thin-delegator directive, 3 are a deliberate, already-validated testing pattern, and 1 is a genuine external-process dependency barred by the repo's own unit-test policy.
