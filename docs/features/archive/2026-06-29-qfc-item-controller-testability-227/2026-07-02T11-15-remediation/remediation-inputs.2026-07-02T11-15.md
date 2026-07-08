# Remediation Inputs — Cycle 3 (Issue #227)

**Generated:** 2026-07-02T11-15 (orchestrator, cycle entry)
**Feature Folder:** `docs/features/active/2026-06-29-qfc-item-controller-testability-227/`
**Base Branch:** `main` (`4611fd60b7d1a782a8024f54cbfd4d28f6d4c264`)
**Head:** `TaskMaster-wt-2026-06-29-09-38` (`0a212191` — cycle-2 delivery + artifact reorganization)
**Trigger:** Maintainer directive (in-session, 2026-07-02): the cycle-2 exit reaudit recorded
`blocking_count == 0` on a 41-member residual boundary, but the maintainer compared 41 against the
seam-redesign research's original ~6-8 irreducible estimate and directed a rigorous independent
re-check of every residual, authorizing a third remediation cycle if warranted.
**Re-audit research:** `artifacts/research/2026-07-02T11-00-qfc-item-controller-residual-reaudit-research.md`
**Updated spec:** `spec.md` v0.4 (Phases 9-10; AC8/AC10 re-opened).

## Cycle scope

The re-audit independently re-verified all 41 delivered residuals against current source (not the
delivered justification comments alone) and found:
- **9 reducible today with zero new production code** — test-only, reusing techniques already proven
  elsewhere in this codebase in the same cycle.
- **8 reducible via two new/extended seams** that mirror patterns cycle-2 already built for
  structurally identical collaborators.
- **24 genuinely irreducible**, cleanly decomposed into five structural categories (see spec.md
  "Redesign scope — cycle 3", item 11).

This is a behavior-preserving testability change; runtime behavior of the QuickFiler item viewer
must not change. No leaf-control interfaces, no `ItemViewer`/Designer change (Option B remains
declined).

### In scope (atomic-planner → atomic-executor → feature-review)

1. **Phase 9 — Tier 1 test-only reductions (9 members, spec Phase 9).** Remove
   `[ExcludeFromCodeCoverage]` and add covering tests for: `RegisterExpandedActions`,
   `JumpToAsync(Control)`, `PopulateControls(MailItem,int)`, `PopulateControlsAsync`, `ToggleFocus()`,
   `ToggleFocus(Enums.ToggleState)`, `WpfUiDispatcher` (the adapter's own forwarding body, via the
   existing `StartRunningDispatcher()` live-dispatcher technique), `MailItemActionsAdapter` (attribute
   removal only — full coverage already exists via `MailItemActionsAdapterTests.cs`),
   `BtnFlagTask_Click` (mirrors its non-exempt sibling `BtnDelItem_Click`).

2. **Phase 10 — Tier 2 new/extended seams (8 members, spec Phase 10).**
   - `FolderPredictor` factory-delegate (mirrors the Phase-6 `EmailFiler`/`FlagTasks`/
     `ConversationResolver` pattern): unblocks `LoadFolderHandler`, `LoadFolderHandlerAsync`,
     `PopulateFolderComboBox`, `PopulateFolderComboBoxAsync`, `TextBoxSearch_TextChanged`.
   - `Theme` + `IUiDispatcher` retrofit (`UtilitiesCS/HelperClasses/ThemeHelpers/Theme.cs`, extends the
     existing `IUiDispatcher` seam into a second class): unblocks `ToggleFocusAsync(Enums.ToggleState)`,
     `ToggleFocusAsync()`, `ApplyReadEmailFormat`.

3. **Reduced boundary artifact.** Write a new `evidence/other/exemption-boundary.<ts>.md` recording
   41 → 24, with the 24 remaining residuals individually re-justified (they were already justified in
   the cycle-2 artifact; carry the reasoning forward and note this is the boundary re-submitted for
   maintainer ratification).

### Explicitly NOT in scope

- The 24 structurally irreducible residuals (spec.md item 11) are NOT touched. In particular:
  `WebView2CoreInitializer`'s adapter body remains exempt (genuine external WebView2-Runtime
  dependency, barred by the External Dependencies rule); the 6 `async void` WinForms-event-signature
  shells remain exempt (framework signature constraint; their extracted cores are already tested);
  the 12 members tied to the retained no-leaf-interface/`(ItemViewer)`-cast invariant; the 2
  `TlpCellSnapShot`-bound members (already a named follow-up, P7-T5); the 3 deliberate virtual
  test-seam methods.
- Leaf-control interfaces / `IList<IButton>` retyping (Option B) remains declined.
- Changing `IQfcItemController`; splitting `QfcCollectionController.cs`; new end-user behavior.

## Constraints

- Legacy non-SDK VSTO/.NET Framework project: `packages.config`, explicit `<Compile Include>` wiring
  (no glob). `Theme.cs` lives in `UtilitiesCS` — confirm its csproj wiring is unaffected by the new
  optional constructor parameter (no new file needed for the retrofit itself).
- Nullable enabled; `/p:TreatWarningsAsErrors=true`; analyzer stack per `.claude/rules/csharp.md`
  (new analyzer diagnostics at `suggestion`).
- 500-line-per-file cap on all production and test files touched.
- Toolchain order per CLAUDE.md: csharpier → analyzers → nullable/TWAE → vstest with coverage.
- Behavior preservation: the `Theme` retrofit must preserve `SetQfcThemeAsync`/`SetMailRead(async:true)`
  dispatch behavior exactly (same thread-marshaling outcome), not merely compile.

## Exit condition for cycle 3

`blocking_count == 0` across the re-audit (`code-review`, `feature-audit`, `policy-audit`), which
requires: the residual exemption count is reduced from 41 to 24 (the 17 Tier 1/Tier 2 members
de-exempted and covered), the two new/extended seams (`FolderPredictor` factory-delegate, `Theme` +
`IUiDispatcher`) are introduced per the DI-seam rule ordering and covered to >= 90%, no changed-line
regression, the affected testable non-exempt denominator remains >= 80%, all files <= 500 lines,
toolchain green, and the reduced 24-member boundary is individually justified and documented for
ratification.
