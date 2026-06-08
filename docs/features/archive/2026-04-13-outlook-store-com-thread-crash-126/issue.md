# outlook-store-com-thread-crash (Issue #126)

- Date captured: 2026-04-13
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/outlook-store-com-thread-crash/ (Issue #126)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #126
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/126
- Last Updated: 2026-04-14
- Work Mode: minor-audit

## Summary

Unhandled `COMException` (HRESULT 0xCC540111) thrown during `LoadInboxes()` when lazily evaluating `NamespaceMAPI.Stores.Where(storesWrapper.ShouldIncludeStore)`. The add-in has 2 live Outlook stores but `StoresWrapper.Stores.Count == 1` because store initialization runs off the main STA thread via `Task.Run`, causing COM access failures for additional stores.

## Environment

- OS/version: Windows 10/11
- Runtime: .NET Framework (VSTO Outlook Add-in)
- COM threading: Outlook Object Model requires main STA thread

## Steps to Reproduce

1. Configure Outlook with 2+ mail accounts/stores (e.g., Exchange + Gmail).
2. Start the TaskMaster add-in.
3. `AppOlObjects.LoadStoresAsync()` initializes `StoresWrapper` via `Task.Run` on a background thread.
4. `StoresWrapper.RewireOlObjectsAsync()` calls `Task.Run(() => new StoreWrapper(store).Init())` for each store.
5. The second store fails silently during background COM access, leaving `StoresWrapper.Stores.Count == 1`.
6. Later, `LoadInboxes()` enumerates `NamespaceMAPI.Stores.Where(storesWrapper.ShouldIncludeStore)` and hits the problematic store, throwing `COMException`.

## Expected Behavior

All configured Outlook stores are initialized and enumerated without COM threading violations. `LoadInboxes()` gracefully skips any store that cannot be accessed.

## Actual Behavior

`System.Runtime.InteropServices.COMException` (HRESULT 0xCC540111) thrown during store enumeration. Earlier startup logs show store-processing failures in `StoreWrapper.GetSmtpAddressFromStore()` and `StoresWrapper.RewireOlObjects()`.

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: `System.Runtime.InteropServices.COMException HResult=0xCC540111 Message=Exception from HRESULT: 0xCC540111`

## Impact / Severity

- [x] High
- Users with multiple Outlook stores experience unhandled crashes during add-in startup.

## Suspected Cause / Notes

Root cause: Outlook COM objects are being accessed from background threads via `Task.Run(...)` in:
- `AppOlObjects.LoadStoresAsync()` — wraps entire store-loading in `Task.Run`
- `StoresWrapper.RewireOlObjectsAsync()` — wraps `StoreWrapper.Init()` and `Restore()` in `Task.Run`
- `StoresWrapper.CreateAsync()` — wraps `new StoresWrapper(globals).Init()` in `Task.Run`

Outlook Object Model calls must stay on the main STA thread. Off-thread access fails for some stores, and subsequent on-thread enumeration hits those same stores without protection.

## Acceptance Criteria

- [x] `AppOlObjects.LoadStoresAsync()` no longer wraps Outlook COM access in `Task.Run`; store deserialization and initialization execute on the calling thread.
- [x] `StoresWrapper.RewireOlObjectsAsync()` no longer wraps `StoreWrapper.Init()` or `Restore()` in `Task.Run`; all COM access stays on the calling thread.
- [x] `StoresWrapper.CreateAsync()` no longer wraps `new StoresWrapper(globals).Init()` in `Task.Run`.
- [x] `LoadInboxes()` wraps per-store enumeration (including `ShouldIncludeStore`) in a `try/catch` so that a failing store is logged and skipped rather than crashing the add-in.
- [x] Existing unit tests continue to pass with no regressions.
- [x] Full C# toolchain passes (format, analyzers, nullable/type-check, tests).

## Proposed Fix / Validation Ideas

- [x] Remove `Task.Run` wrapping Outlook COM access in `AppOlObjects.LoadStoresAsync()`, `StoresWrapper.RewireOlObjectsAsync()`, and `StoresWrapper.CreateAsync()`
- [x] Add per-store `try/catch` in `LoadInboxes()` around the `ShouldIncludeStore` filtering/enumeration
- [ ] Unit test coverage for `LoadInboxes` defensive enumeration
- [ ] Unit test coverage for `RewireOlObjectsAsync` without `Task.Run`

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch