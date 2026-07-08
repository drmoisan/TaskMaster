# quickfiler-darkmode-stale-subscription (Issue #251)

- Date captured: 2026-07-06
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/quickfiler-darkmode-stale-subscription/ (Issue #251)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #251
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/251
- Last Updated: 2026-07-07
- Work Mode: minor-audit

## Summary

`QfcCollectionController` subscribes to the global `PropertyChanged` dark-mode notification in `SetupLightDark()` but never unsubscribes in `Cleanup()`/`CleanupAsync()`, which null out `_globals`. A later dark-mode toggle fires `DarkMode_CheckedChanged` on the cleaned-up controller, which dereferences the now-null `_globals`, throwing `NullReferenceException`.

## Environment

- OS/version: Windows (Outlook VSTO add-in host)
- Python version: N/A (C# / .NET Framework VSTO)
- Command/flags used: Runtime; ribbon dark-mode toggle after a QuickFiler collection has been cleaned up
- Data source or fixture: `QuickFiler/Controllers/QfcCollectionController.cs`

## Steps to Reproduce

1. Open a QuickFiler collection so `QfcCollectionController` is constructed; the constructor calls `SetupLightDark(_globals.Ol.DarkMode)`, subscribing `DarkMode_CheckedChanged` to `_globals.Ol.PropertyChanged`.
2. Trigger controller teardown so `Cleanup()` (or `CleanupAsync()`) runs, nulling `_formViewer`, `_globals`, `_parent`, `_itemTlp`, `_itemGroups` without unsubscribing.
3. Toggle dark mode from the ribbon (`RibbonController.ToggleDarkMode()` → `AppOlObjects.DarkMode.set` raises `PropertyChanged("DarkMode")`).
4. The still-subscribed `DarkMode_CheckedChanged` runs on the cleaned-up controller and dereferences `_globals.Ol.DarkMode`.

## Expected Behavior

After a controller is cleaned up, it no longer responds to global dark-mode notifications, and a dark-mode toggle does not raise an exception from the disposed controller.

## Actual Behavior

`System.NullReferenceException: Object reference not set to an instance of an object.` at `QuickFiler.Controllers.QfcCollectionController.DarkMode_CheckedChanged(Object sender, EventArgs e)` (`QfcCollectionController.cs:2121`). Debugger confirms `_globals == null`, `_formViewer == null`, `_parent == null` on the firing instance.

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet:
  ```
  System.NullReferenceException: Object reference not set to an instance of an object.
     at QuickFiler.Controllers.QfcCollectionController.DarkMode_CheckedChanged(Object sender, EventArgs e) in QfcCollectionController.cs:line 2121
  ```

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

Crashes the add-in on a common user action (dark-mode toggle) whenever a QuickFiler collection has been torn down during the session.

## Suspected Cause / Notes

Lifecycle mismatch: subscribe in `SetupLightDark()`, no matching unsubscribe in `Cleanup()`/`CleanupAsync()`, and the handler depends on fields (`_globals`, `_itemGroups`) that cleanup nulls out. The subscription target (`_globals.Ol`) outlives the controller, so the stale handler keeps receiving events.

## Proposed Fix / Validation Ideas

1. Unsubscribe `DarkMode_CheckedChanged` from `_globals.Ol.PropertyChanged` in both `Cleanup()` and `CleanupAsync()` before nulling `_globals` (guard with null-conditional).
2. Add a defensive early return in `DarkMode_CheckedChanged` when the controller has been cleaned up (e.g., `_formViewer is null`).
3. Prefer reading dark-mode state from `sender` (as `IOlObjects`) inside the handler rather than from `_globals`.

- [x] Unit coverage areas: regression test that raises `PropertyChanged` after `Cleanup()`/`CleanupAsync()` and asserts no exception and no theme-change side effect on the cleaned-up controller.
- [ ] Integration scenario to retest: ribbon dark-mode toggle after closing a QuickFiler session.
- [x] Manual verification notes: toggle dark mode after tearing down a QuickFiler collection; confirm no exception.

## Acceptance Criteria

- [x] AC1: A regression test reproduces the defect on the pre-fix code — raising `PropertyChanged("DarkMode")` on the globals dark-mode source after `Cleanup()` throws (or invokes the stale handler) — and passes after the fix.
- [x] AC2: `Cleanup()` unsubscribes `DarkMode_CheckedChanged` from `_globals.Ol.PropertyChanged` before nulling `_globals`.
- [x] AC3: `CleanupAsync()` unsubscribes `DarkMode_CheckedChanged` from `_globals.Ol.PropertyChanged` before nulling `_globals`.
- [x] AC4: `DarkMode_CheckedChanged` no longer throws when invoked on a cleaned-up controller (defensive early return and/or reads state from `sender`), and performs no theme-change side effect in that state.
- [x] AC5: After the fix, raising `PropertyChanged("DarkMode")` following `Cleanup()`/`CleanupAsync()` produces no exception and no call into `SetDarkMode`/`SetLightMode`.
- [x] AC6: No production files other than `QuickFiler/Controllers/QfcCollectionController.cs` are changed; the fix is minimal and targeted.
- [x] AC7: Full C# toolchain passes in order (CSharpier → analyzers → nullable → MSTest) with no regressions; changed-line coverage meets policy.
- [ ] AC8: Required CI checks pass green on the PR head SHA.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [x] Move to active fix folder / branch
