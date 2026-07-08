# QfcItemController Exemption Boundary — Cycle-2 Reduced (for maintainer ratification)

- **Timestamp:** 2026-07-02T10-30
- **Issue:** #227 (remediation cycle 2 — Option A, seam redesign)
- **Supersedes:** `evidence/other/exemption-boundary.2026-06-29T12-40.md` (the 103-member blanket boundary
  DENIED by the maintainer in `maintainer-decision.2026-07-01.md`).
- **Status:** Reduced boundary, submitted for maintainer ratification at review.

## Summary of reduction

| Milestone | Exemption count | Basis |
|---|---:|---|
| Cycle-1 (denied) | 103 (101 methods + 2 properties) | Blanket per-partial-file `[ExcludeFromCodeCoverage]` |
| After Phase 5 (AC8) | 57 controller members | Removed the no-barrier members and covered them |
| After Phase 6 (AC9) | 42 controller members | Routed bucket-(ii) members through the four seams and covered them |
| After Phase 7 (AC10) | **41 total** = 38 controller members + 3 DI-adapter shims | Removed 4 dead raw-parameter overloads (P7-T1); each residual individually justified |

Net: **103 → 41**, and every one of the 41 is an individually-named, technically-justified
bucket-(iii) residual with an inline per-member comment. **No blanket/category exemption remains.**

## De-exempted members (now covered by >= 1 passing test)

- **Phase 5 (~46 members):** the Initialization ctors + `SaveParameters`; `PopulateControls(MailItemHelper,int)`, `AssignControlsAsync`, `AssignControls`, `Cleanup`; `PopulateConversation(ConversationResolver)`, `RenderConversationCount()`, `SetTopicThread`; the EventWiring registration-membership members; ~8 EventHandlers members; `JumpToFolderDropDown`, `JumpToSearchTextbox`, the parameterless `ToggleExpansion`/`Async`; 12 FocusAndTheme members; `RightKeyActions`/`RightKeyActionsAsync`, `CollapseConversation`, `EnumerateConversation`.
- **Phase 6 (15 members):** `PopulateConversation(int)`, `RenderConversationCountAsync`, `JumpToFolderDropDownAsync`, `MenuDropDown`, `ToggleConversationCheckbox()`/`(ToggleState)`, `ToggleSaveCopyOfMail`, `EnumerateConversationAsync`, `MarkItemForDeletionAsync` (dispatcher seam); `Reply`, `ReplyAll`, `Forward` (dispatcher + `IMailItemActions`); `PopulateConversation()`, `FlagAsTask`, `FlagAsTaskAsync`, `MoveMailAsync` (factory delegates). Plus the extracted testable cores (`BtnPopOutCore`, `BtnReplyCore`, `BtnReplyAllCore`, `BtnForwardCore`, `TxtboxBodyDoubleClickCore`, `HandleWebViewInitializedAsync`) and `WireIntentEvents`, which are new non-exempt methods.

## Residual set (41) — individually justified

The full per-member residual list with technical reasons is in
`evidence/qa-gates/p7r-residual-verification.2026-07-02T10-30.md`. Categorized:

- **Concrete control-tree orchestration/traversal (16):** the 7 `Initialize*`/`Create*` methods,
  `ResolveControlGroups`/`Async`, `WireEvents`/`WireControlTreeEvents`, `RegisterExpandedActions`,
  `InitializeWebViewAsync`, `JumpToAsync(Control)`, plus `PopulateControls(MailItem,int)`/`Async`.
- **Out-of-scope COM/WinForms collaborators (11):** `LoadFolderHandler`/`Async`,
  `PopulateFolderComboBox`/`Async`, `TextBoxSearch_TextChanged` (`FolderPredictor`); `ToggleFocus`×2,
  `ToggleFocusAsync`×2, `ApplyReadEmailFormat` (`Theme` handle-bound); `ToggleExpansionOn`/`Off`
  (`TlpCellSnapShot`).
- **Deliberate virtual test seams (3):** `DoLoadConversationResolverCoreAsync`,
  `ToggleExpansion(ToggleState)`, `ToggleExpansionAsync(ToggleState)`.
- **Thin WinForms-event shells (6):** `BtnFlagTask_Click`, `BtnPopOut_Click`, `BtnReply_Click`,
  `BtnReplyAll_Click`, `BtnForward_Click`, `TxtboxBody_DoubleClick`,
  `WebView2Control_CoreWebView2InitializationCompleted` (7 shells — one overlaps the orchestration list;
  see the per-member artifact for exact placement).
- **DI-adapter shims (3):** `WpfUiDispatcher`, `WebView2CoreInitializer`, `MailItemActionsAdapter`.

Two members (`InitializeWebViewAsync`, `ApplyReadEmailFormat`) were **reclassified from bucket-(ii) to
bucket-(iii)** during Phase 6: their SDK/COM dependency was successfully isolated behind a new seam
(`IWebViewCoreInitializer` / `IMailItemActions`), but a residual concrete-control / handle-bound-Theme
barrier remains that prevents unit execution under Option A. The two synchronous `ToggleFocus()` /
`ToggleFocus(ToggleState)` FocusAndTheme members were already reclassified as justified bucket-(iii)
residuals in Phase 5 (out-of-scope `Theme.SetQfcTheme` handle-bound barrier); they are retained here
with the same per-member reason.

## Deferred follow-up (P7-T5)

- **`TlpCellSnapShot.ApplyState(Control)` seam.** Retyping this method to
  `ApplyState(IContainerControlLocal)` (from the existing `UtilitiesCS.Interfaces.IWinForm` layer, which
  `TlpCellSnapShot` only needs for structural `Controls`/`GetColumn`/`SetColumnSpan` members) would
  unblock `QfcItemController.ToggleExpansionOn` and `ToggleExpansionOff` (residuals #31–#32) in a future
  cycle. This is out of scope for cycle-2 (Option A) and is recorded here as a named follow-up
  recommendation; no scope expansion is performed this cycle.

## Ratification request

This reduced boundary (103 → 41, no blanket/category exemption, each residual individually justified
in source and in the per-member verification artifact) is submitted for maintainer ratification at the
cycle-2 feature review, per the authority-scoped coverage-exception precedent
(`docs/features/active/2026-06-28-qfc-form-viewer-testability-223/maintainer-decision.2026-06-29.md`).
