# Cycle-2 Acceptance-Criteria Traceability (P8-T8)

Timestamp: 2026-07-02T10-45
AC source: `spec.md` (Work Mode: full-feature; `user-story.md` carries no AC checkboxes)

## AC mapping table

| AC | Status | Satisfying tasks | Evidence artifacts |
|---|---|---|---|
| AC5 — affected non-exempt denominator >= 80%; new/extracted (incl. new seam types) >= 90%; no changed-line regression; repo-wide under authority-scoped exception (#197) | PASS | P5-T15, P6-T17, P7-T10, P8-T4, P8-T5 | `qa-gates/final-r2-tests-coverage.2026-07-02T10-45.md`, `regression-testing/coverage-delta-r2.2026-07-02T10-45.md` |
| AC6 — no production file modified/created exceeds 500 lines (incl. new seam files) | PASS | P5-T11, P6-T13, P7-T6, P8-T7 | `qa-gates/final-r2-file-sizes.2026-07-02T10-45.md`, `qa-gates/p6r-file-sizes.2026-07-02T10-17.md`, `qa-gates/p7r-file-sizes.2026-07-02T10-30.md` |
| AC7 — full C# toolchain passes in order (csharpier, analyzers, nullable/TWAE, MSTest+coverage), no regressions | PASS | P5-T12..T15, P6-T14..T17, P7-T7..T10, P8-T1..T4 | `qa-gates/final-r2-csharpier`, `final-r2-analyzers`, `final-r2-nullable`, `final-r2-tests-coverage` (all 2026-07-02T10-45) |
| AC8 — cycle-1 exemption set reduced by de-exempting no-barrier members and covering them; no IItemViewer/mockable-collaborator-exercisable member retains an exemption | PASS | P5-T2..T9, P6-T3/T7/T8, P8-T6 | `qa-gates/final-r2-exemption-delta.2026-07-02T10-45.md`, `qa-gates/p5r-tests-coverage.2026-07-02T09-16.md` |
| AC9 — four behavioral seams introduced per DI-seam ordering, covered >= 90%, behavior preserved; no leaf-control interface layer | PASS | P6-T1..T11 | `qa-gates/p6r-tests-coverage.2026-07-02T10-17.md`, `qa-gates/p6r-file-sizes.2026-07-02T10-17.md` |
| AC10 — any residual `[ExcludeFromCodeCoverage]` individually justified per-member (no blanket/category); reduced boundary documented for maintainer ratification | PASS | P7-T1..T5, P8-T6 | `qa-gates/p7r-residual-verification.2026-07-02T10-30.md`, `other/exemption-boundary.2026-07-02T10-30.md` |

## Reduced exemption boundary

- 103 (cycle-1, denied) -> 41 final (38 `QfcItemController` members + 3 DI-adapter shims), each residual
  individually justified with an inline per-member comment and enumerated in the boundary artifact,
  submitted for maintainer ratification at review. No blanket/category exemption remains.

## Four introduced behavioral seams (AC9)

1. `UtilitiesCS.Threading.IUiDispatcher` + `WpfUiDispatcher` (UI-dispatch seam, ~9 members routed).
2. `QuickFiler.Viewers.IWebViewCoreInitializer` + `WebView2CoreInitializer` (WebView2 core-init adapter).
3. `QuickFiler.Interfaces.IMailItemActions` + `MailItemActionsAdapter` (Outlook COM adapter) plus factory
   delegates for `ConversationResolver`/`FlagTasks`/`EmailFiler`.
4. Thin-delegator extraction of the six `async void` handlers (`BtnPopOut/Reply/ReplyAll/Forward_Click`,
   `TxtboxBody_DoubleClick`, `WebView2Control_CoreWebView2InitializationCompleted`) into testable core
   methods. No leaf-control interface layer (`IButton`/`ILabel`/etc.) was introduced (Option B declined).

## Deferred follow-up

- `TlpCellSnapShot.ApplyState(Control)` -> `ApplyState(IContainerControlLocal)` seam, which would unblock
  `QfcItemController.ToggleExpansionOn`/`ToggleExpansionOff` in a future cycle. Recorded in the boundary
  artifact; not performed this cycle (out of scope, Option A).

## Behavior-preservation confirmation

Runtime behavior of the QuickFiler item viewer is preserved: every edit is a testability refactor,
re-confirmed by the passing-test-count regression gate against the P0-T5 baseline (233 -> 289 -> 328,
0 failures at every phase). Designer code untouched; the `WireEvents` split preserves the net
subscription set and each event's handler order; the `SynchronizationContext` guard placement and the
WebView handler's try/catch are preserved; the COM-adapter substitution was applied atomically.

Output Summary: All six ACs (AC5-AC10) map to completed tasks and >= 1 evidence artifact and are marked
PASS. AC1-AC4 were satisfied in cycle-1. The reduced exemption boundary and the deferred
`TlpCellSnapShot` follow-up are recorded for maintainer ratification.
