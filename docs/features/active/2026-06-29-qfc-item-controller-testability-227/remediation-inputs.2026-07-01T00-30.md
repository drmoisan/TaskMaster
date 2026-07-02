# Remediation Inputs — Cycle 2 (Issue #227)

**Generated:** 2026-07-01T00-30 (orchestrator, cycle entry)
**Feature Folder:** `docs/features/active/2026-06-29-qfc-item-controller-testability-227/`
**Base Branch:** `main` (`4611fd60b7d1a782a8024f54cbfd4d28f6d4c264`)
**Head:** `TaskMaster-wt-2026-06-29-09-38` (`bfc8364b` — cycle-0 audits + cycle-1 R1 evidence)
**Trigger:** Maintainer DENIED R2 exemption-boundary ratification (`maintainer-decision.2026-07-01.md`);
Option A approved.
**Seam-redesign research:** `artifacts/research/2026-07-01T00-00-qfc-item-controller-seam-redesign-research.md`
**Updated spec:** `spec.md` v0.3 (Phases 5–7; AC5 revised; AC8/AC9/AC10 added).

## Cycle scope

Make the 103 cycle-1 `[ExcludeFromCodeCoverage]` members unit-testable through behavioral seams and
exemption removal, rather than exempting them. Drive the exemption set toward zero. This is a
behavior-preserving testability change; runtime behavior of the QuickFiler item viewer must not
change.

### In scope (atomic-planner → atomic-executor → feature-review)

1. **Remove over-broad exemptions (spec Phase 5, AC8).** Remove `[ExcludeFromCodeCoverage]` from the
   ~38 members the seam-redesign research identified as having no genuine barrier (bodies touch only
   the narrowed `IItemViewer` or mockable collaborators), and cover them with MSTest + Moq +
   FluentAssertions tests. Make `_themes` reflection-injectable following the existing `_kbdHandler`
   pattern to unblock the `FocusAndTheme` cluster.

2. **Introduce four behavioral seams (spec Phase 6, AC9)**, DI-seam ordering interface > delegate >
   adapter:
   - `IUiDispatcher` wrapping the static `UiThread.Dispatcher` (`UtilitiesCS/Threading/UiThread.cs`)
     and the `InvokeRequired`/`Invoke`/`BeginInvoke` marshaling.
   - `IWebViewCoreInitializer` adapter over `EnsureCoreWebView2Async` + init-completed handler.
   - `IMailItemActions` adapter over the `MailItem`/`MailItemHelper` boundary + factory delegates for
     `ConversationResolver` / `FlagTasks` / `EmailFiler`.
   - Thin-delegator extraction for the six `async void` UI event handlers.
   Cover the ~40 members these unblock to >= 90% for new/extracted code.

3. **Final residual boundary (spec Phase 7, AC10).** Only the small individually-justified residual
   (research estimate ~6–8) may retain `[ExcludeFromCodeCoverage]`, each with a specific per-member
   technical reason. Record the reduced boundary in a new `evidence/other/exemption-boundary.<ts>.md`
   for maintainer ratification at review.

### Explicitly NOT in scope

- Leaf-control interfaces (`IButton`/`ILabel`/`ICheckBox`/`IComboBox`/`ITextBox`) or `IList<IButton>`
  retyping (Option B, declined — not the actual barrier).
- Changing `IQfcItemController`; splitting `QfcCollectionController.cs`; new end-user behavior.

## Constraints

- Legacy non-SDK VSTO/.NET Framework project: `packages.config`, explicit `<Compile Include>` /
  `<Analyzer Include>` wiring (no glob, no `dotnet restore`). New seam files need explicit csproj
  entries in the correct project.
- Nullable enabled; `/p:TreatWarningsAsErrors=true`; analyzer stack per `.claude/rules/csharp.md`
  (new analyzer diagnostics at `suggestion`).
- 500-line-per-file cap on all production and test files, including new seam and adapter files.
- Toolchain order per CLAUDE.md: csharpier → analyzers → nullable/TWAE → vstest with coverage.
- Behavior preservation: Designer round-trip untouched; event-wiring order preserved; COM adapter
  migration atomic.

## Exit condition for cycle 2

`blocking_count == 0` across the re-audit (`code-review`, `feature-audit`, `policy-audit`), which
requires: AC5, AC6, AC7, AC8, AC9, AC10 met — i.e., exemptions removed where unjustified, the four
seams introduced and covered to >= 90%, the affected testable non-exempt denominator >= 80% with no
changed-line regression, all files <= 500 lines, toolchain green, and the reduced residual boundary
individually justified and documented for ratification.
