# disabled-stores-settings-ui (Issue #265)

- Date captured: 2026-07-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/disabled-stores-settings-ui/ (Issue #265)
- Promotion type: feature
- Epic: #260 (store-lockup-resilience)

- Issue: #265
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/265
- Last Updated: 2026-07-07
- Work Mode: full-feature

## Problem / Why

Users need a persistent place to see which stores are disabled and to reenable them, independent
of the transient modeless notification. Today "TaskMaster -> Settings -> Folder Settings" opens
`StoreWrapperController` + `StoreWrapperViewer`, a single-store detail editor (a ComboBox plus
labels) with no list of stores and no enable/disable surface. There is no UI for the existing
exclusion lists either.

## Proposed Behavior

- Add a settings surface (in Folder Settings, or an appropriate adjacent surface) that lists the
  currently disabled stores (session-only and future-sessions, visually distinguished) with a
  per-store "Reenable" action.
- Read the disabled set from `IStoreDisableService` (F1); "Reenable" invokes F3's runtime rehook
  and clears the disablement (persisting when it affects the future-sessions list).
- Follow the existing Controller + `IViewer` seam used by `StoreWrapperController`; use the
  reusable `DgvForm` DataGridView shell (or a new designer following the same pattern) with a
  `DataGridViewButtonColumn` for Reenable wired via `CellContentClick`.

Depends on F1 (disabled model/service), F2 (Folder Settings must actually open — the store model
must be populated), and F3 (reenable rehook). This feature owns the list UI and its wiring only.

## Acceptance Criteria (early draft)

- [ ] Folder Settings (or the chosen surface) shows a list of disabled stores, distinguishing
      session-only from future-sessions disablement.
- [ ] Each row offers a Reenable action that invokes F3 rehook and clears the disablement via F1,
      persisting when the future-sessions list changes; the row updates to reflect the new state.
- [ ] The list reflects the current `IStoreDisableService` state when opened and after a reenable.
- [ ] The UI follows the existing Controller + `IViewer` seam and is unit-testable through that
      interface (Moq), with no live Outlook and no temp files.
- [ ] Full C# toolchain passes; new/changed code meets coverage targets (WinForms designer code
      handled per the repo COM/WinForms coverage exemption).

## Constraints & Risks

- WinForms form-derived and Designer-generated code fall under the repo COM/VSTO/WinForms
  coverage exemption; keep testable logic in the controller behind `IViewer`.
- Depends on F2 so the settings dialog opens with a populated model.
- Reuse existing settings-dialog patterns; do not introduce a new persistence mechanism.
- Decide (in spec) whether to extend the existing `StoreWrapperViewer` or add a sibling
  tab/section; prefer the least-invasive surface consistent with the existing UI.

## Test Conditions to Consider

- [ ] Unit: controller populates the list from the service; Reenable invokes rehook + clears state
      + persists; row state updates.
- [ ] Edge: empty disabled list; reenable failure surfaced without crashing; both scopes present.
- [ ] Integration: open after startup (depends on F2); reenable round-trip reflected in the list.

## Next Step

- [ ] Promote to GitHub issue (feature) via MCP tooling and link to epic #260
