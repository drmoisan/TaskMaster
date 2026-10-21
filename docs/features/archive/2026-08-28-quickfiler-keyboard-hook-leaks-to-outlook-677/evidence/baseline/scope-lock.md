# Scope Lock (P0-T10)

Timestamp: 2026-08-28T15-49
Command: N/A (scope confirmation record)
EXIT_CODE: 0

## Spec version and acceptance-criteria source

- Work Mode: `full-bug` (persisted marker in `issue.md`: `- Work Mode: full-bug`).
- AC source: `docs/features/active/2026-08-28-quickfiler-keyboard-hook-leaks-to-outlook-677/spec.md`,
  **version 0.3**, section `## Acceptance Criteria` (AC-1 through AC-10 in bullet order).
  `spec.md` v0.3 is the final scope for this plan.
- `user-story.md` is **intentionally absent**: under `full-bug` mode the acceptance-criteria
  tracking skill resolves `spec.md` only, and `user-story.md` is optional/absent by default.
  Its absence is not a blocker and must not be treated as one.

## In-scope production files (per Decisions D3-D8)

| File | Change | Decision |
|---|---|---|
| `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | add `internal Func<bool> MayTakeFocus`, `FocusAnchorIfPermitted()`, guard `FinishClose` focus step and `FocusPending()` | D3, D4(a)(b) |
| `QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs` | already-open branch schedules `FocusPending` instead of `_focusPending` | D4(c) |
| `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` | assign `host.MayTakeFocus = MayRestoreBreadcrumbFocus;`; add `MayRestoreBreadcrumbFocus()` | D5 |
| `QuickFiler/Interfaces/IQfcFormViewer.cs` | add `FormDeactivated`, `IsWebView2Focused`, `ParkFocusOffWebView2` | D6 |
| `QuickFiler/Viewers/QfcFormViewer.cs` | implement the three additive members | D6 |
| `QuickFiler/Viewers/IItemViewer.cs` | add `void CancelBreadcrumbSelector();` | D7 |
| `QuickFiler/Viewers/ItemViewer.FolderSearch.cs` | implement `CancelBreadcrumbSelector` forwarding | D7 |
| `QuickFiler/Interfaces/IQfcItemController.cs` | add `void CancelBreadcrumbSelector();` | D7 |
| `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` | implement `CancelBreadcrumbSelector` forwarding | D7 |
| `QuickFiler/Controllers/QfcFormController.Deactivate.cs` (**new**) | deactivate handler: park focus, cancel selectors | D7 |
| `QuickFiler/Controllers/QfcFormController.SetupDisposal.cs` | subscribe/unsubscribe `FormDeactivated` | D7 |
| `QuickFiler/QuickFiler.csproj` | `<Compile Include>` for the new partial | D14 |

## In-scope test files

| File | Change |
|---|---|
| `QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.Part3.cs` (**new**) | 8 regression tests (P1-T1) |
| `QuickFiler.Test/Controllers/QfcFormControllerDeactivateTests.cs` (**new**) | 7 regression tests (P1-T2) |
| `QuickFiler.Test/Controllers/QfcItemController.CancelBreadcrumbSelectorTests.cs` (**new**) | 2 regression tests (P1-T3) |
| `QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs` | interface-completion member on `FakeQfcItemController` only (D8 sanctioned structural enabler) |
| `QuickFiler.Test/QuickFiler.Test.csproj` | `<Compile Include>` for the three new test files |

## Interface-implementer sweep (D8), re-verified at execution time

- `IQfcFormViewer` — implemented only by `QuickFiler/Viewers/QfcFormViewer.cs`.
- `IItemViewer` — implemented only by `QuickFiler/Viewers/ItemViewer.cs` (partial type).
- `IQfcItemController` (namespace `QuickFiler.Interfaces`) — implemented by `QfcItemController`
  and by the manual test fake `FakeQfcItemController`
  (`QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs:337`).
- `QuickFiler/Legacy/IQfcItemControllerLegacy.cs` is a different type in namespace
  `QuickFiler.Legacy` and is untouched.

## Out-of-scope invariant

**`QuickFiler/Controllers/KeyboardHandler.cs` is not modified.**

This is spec AC-6 and the Scope & Non-Goals exclusion. The research artifact establishes that
`KeyboardHandler` is ordinary WinForms event wiring strictly confined to QuickFiler's own control
tree and cannot receive events from native Outlook windows, so it is not the defect. P4-T4 gates
the invariant against BASELINE_SHA `361a49b884a4e3fe192bf04bae05151c598398fa`.

Further out-of-scope per `spec.md`: no WebView2 controller-level focus APIs, no rewrite of the
popup off `ToolStripDropDown`, no upstream WebView2Feedback #951 fix, no changes to the
`QuickFiler/Legacy/` tree, and no changes to any non-QuickFiler project.
