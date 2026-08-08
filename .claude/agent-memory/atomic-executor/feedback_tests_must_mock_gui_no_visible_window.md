---
name: tests-must-mock-gui-no-visible-window
description: New/changed C# tests must mock GUI elements behind seams; no real window may appear during a test run
metadata:
  type: feedback
---

Any test this agent develops or changes must mock GUI elements behind appropriate seams. No visible window, form, or popup may appear while the test suite runs.

**Why:** The user observed a window popping up during a full `QuickFiler.Test` run (2026-08-08). A visible window means a real WinForms host is being created and pumped instead of being substituted at a seam. Besides being disruptive, it makes tests machine-, focus-, and load-dependent — the same `WinFormsPumpHost`-based tests in `QuickFiler.Test/TestSupport/WinFormsPumpHost.cs` are also the ones that fail with "Invoke or BeginInvoke cannot be called on a control until the window handle has been created" and with 60s `[Timeout]` expiries under machine load.

**How to apply:** In this repo the established headless seams are:
- `Mock<IItemViewer>` at the controller seam.
- `Mock<IBreadcrumbDropDownHost>` plus the headless `ItemViewerDropDownHarness` (constructs a `UserControl` but never shows it, and replaces the show/focus delegates).
- The `BreadcrumbDropDownHostTests` private `Harness`, which injects `showPopup` / `focusPending` / `focusAnchor` as counting delegates so no native popup is ever shown.
- Host-neutral `UtilitiesCS.OutlookObjects.Folder` types (`BreadcrumbSelectionSession`, `FolderBreadcrumbBridgeRouter`, `BreadcrumbStateModel`) which have no UI dependency at all.

Never call `Form.Show`, `Application.Run`, `Control.CreateControl`, or drive a real message pump in a new test. Prefer the host-neutral layer; when a `Control` is unavoidable, inject the show/focus operations as delegates and assert call counts. See [[project_configcontroller_sta_pump_deadlock]] and [[project_uithread_dispatcher_static_swap_race]] for the failure modes this prevents.
