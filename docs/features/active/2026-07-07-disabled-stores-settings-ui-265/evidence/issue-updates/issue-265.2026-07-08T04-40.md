# Issue #265 — Acceptance Criteria Update Mirror (P8-T4)

Timestamp: 2026-07-08T04-40

PostedAs: unknown
(This is a local mirror of the updated spec.md `## Acceptance Criteria` section. GitHub issue
posting and PR creation are handled by the orchestrator, not by this executor; no `gh` post was
made in this execution.)

## Acceptance Criteria (exact mirror of spec.md after this execution)

- [x] **AC1 — Dedicated surface via additive ribbon button.** A new "Disabled Stores" button in the
      existing Settings menu opens a new dialog backed by `DisabledStoresController` +
      `IDisabledStoresViewer`. The existing Folder Settings and Junk Folder Settings buttons and the
      single-store `StoreWrapperController`/`StoreWrapperViewer` editor are unchanged.
- [x] **AC2 — List reflects service state on open.** On open, the dialog shows one row per entry
      returned by `StoreDisable.GetDisabledStores()`, populated by `DisabledStoresController` from
      that call.
- [x] **AC3 — Scope is visually distinguished.** Session-only rows and future-sessions rows are
      distinguishable: the controller sets `ScopeLabel` and `IsFutureSession` per row, and the
      Designer/cell-formatting layer renders a distinct style for future-sessions rows. Both scopes
      may be present at once, each resolved independently.
- [x] **AC4 — Per-row Reenable routes through F1.** A Reenable action on a row invokes
      `StoreDisable.ReenableAsync(identity)` exactly once with that row's identity, resolved from the
      controller's own list by `DataGridViewCellEventArgs.RowIndex`. F5 does not call F3 directly and
      does not persist state itself.
- [x] **AC5 — Refresh after reenable.** After a Reenable action, the controller unconditionally
      re-fetches `GetDisabledStores()` and rebinds the list, so the displayed rows match the current
      service state after every action.
- [x] **AC6 — Empty list.** When `GetDisabledStores()` returns an empty collection, the dialog opens
      with no rows and no exception.
- [x] **AC7 — Reenable failure is surfaced without crashing.** When `ReenableAsync` throws or
      returns a faulted `Task`, the exception is caught, logged, and surfaced through the `MyBox`
      dialog seam; it does not escape the click handler, and the list is still refreshed from
      `GetDisabledStores()` afterward.
- [x] **AC8 — Controller + IViewer seam, Moq-testable, no live Outlook, no temp files.** All logic
      is unit-tested through `IDisabledStoresViewer` with Moq and a mocked `StoreDisable` service,
      driving clicks via a directly-constructed `DataGridViewCellEventArgs` with no live
      `DataGridView`, no live Outlook, and no temporary files.
- [x] **AC9 — Dialog-open readiness reuse (F2 dependency).** The dialog applies the same readiness
      gate as the single-store editor via a shared readiness helper; the extraction leaves
      `StoreWrapperController.EvaluateLaunchReadiness` behavior unchanged, and existing
      `StoreWrapperController_Tests.*` continue to pass unmodified.
- [x] **AC10 — Toolchain and coverage.** The full C# toolchain passes in order (CSharpier, .NET
      analyzers, nullable analysis with `TreatWarningsAsErrors`, MSTest with coverage);
      `DisabledStoresController.cs` and `DisabledStoreRow.cs` meet the new-code coverage target, and
      WinForms form-derived / Designer-generated files are handled under the repository
      COM/VSTO/WinForms coverage exemption.

All 10 acceptance criteria verified locally. See the evidence-mapping subsection in spec.md and the
`evidence/qa-gates/` + `evidence/regression-testing/` artifacts for backing verification.
