# utilitiescs-archive-root-read-and-user-email-retry-801-805 (Issue #812)

- Date captured: 2026-09-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/utilitiescs-archive-root-read-and-user-email-retry-801-805/ (Issue #812)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #812
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/812
- Last Updated: 2026-09-08
## Summary

Consolidates two small `UtilitiesCS` defects filed as #801 and #805 so they ship as one change. (#801) `FolderPredictor.AddRecents` and `AddRecentRows` read the archive root through the `AppOlObjects.ArchiveRootPath` property, whose guard throws `InvalidOperationException` when the root cannot be resolved; with zero suggestions and a non-empty recents list they become the first reader on a path that previously never touched the property, so an unresolvable archive root now surfaces as a throw where the hierarchy provider degrades gracefully. (#805) The User Email retry added by #797 is documented as bounded to once per dialog open, but `PopulateWithCurrent` runs on every store re-selection and a failed retry leaves the address null, so a persistently failing Exchange lookup re-runs the blocking COM chain on the UI thread per selection change.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Runtime: .NET Framework 4.8 VSTO add-in; `main` at `04a54e68`
- Command/flags used: QuickFiler with an unresolvable archive root and a non-empty recents list (#801); Settings -> Folder Settings, cycling the Display Name store selection on a mailbox whose Exchange lookup fails (#805)
- Data source or fixture: live mailbox; the 2026-09-06 log shows `COMException: The operation failed.` from `Session.CurrentUser`

## Steps to Reproduce

1. (#801) Make `ArchiveRootPath` unresolvable (no folder literally named `Archive` under the default store root, or a cross-store mismatch so `ArchiveRootPathGuard` throws). Launch QuickFiler on an item with no classifier suggestions and at least one recent folder. Observe `InvalidOperationException` propagating out of the recents projection instead of the entries rendering as stored.
2. (#805) On a profile where User Email shows "Email address unavailable: ...", open Folder Settings and change the store selection several times. Observe `GetSmtpAddressFromStore` running its full COM chain on every re-selection.

## Expected Behavior

- An unresolvable archive root degrades the recents projection to identity (entries rendered as stored), matching `OutlookFolderHierarchyProvider` and `EfcDataModel.TryGetArchiveRoot`, with one logged warning.
- The User Email retry runs at most once per dialog open per controller instance, and the comments and #797 specification prose state the bound the code enforces.

## Actual Behavior

- `FolderPredictor.cs` `AddRecents` / `AddRecentRows` read `_globals.Ol.ArchiveRootPath` unconditionally; the guard's exception propagates.
- `StoreWrapperController.Display.cs` lines 41-45 gate the retry on `Current.UserEmailAddress is null`; `RefreshUserEmailAddress` assigns null back on failure; `PopulateWithCurrent` is called from `DisplayName_SelectedValueChanged` (`StoreWrapperController.cs` line 169) on every selection change. Comments at `Display.cs` 41-51 and `StoreWrapper.cs` 214-219 and `spec.md` lines 491-493 of the #797 folder claim once-per-open.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: #805 is a code-read finding (CR-1 of `code-review.2026-09-07T22-40.md` in the #797 folder); the original UI-thread block on this chain is at 17:35:21 in `debug_2026-09-06.log` (`ThreadMonitor` inside `_ExchangeUser.get_PrimarySmtpAddress()`). #801 is a code-read finding from the #799 review.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

#801 is a new throw on a previously non-throwing path; #805 is unbounded repetition of a synchronous COM call known to block the Outlook UI thread, in exactly the failure case the user is diagnosing.

## Suspected Cause / Notes

- #801 files: `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` (`AddRecents`, `AddRecentRows`, and the shared projection introduced by #799). Use the same guarded accessor pattern as `EfcDataModel.TryGetArchiveRoot` (`QuickFiler/Controllers/EfcDataModel.cs:280-297`).
- #805 files: `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs`, `StoreWrapperController.Display.cs`, `StoreWrapper.cs`; #797 `spec.md`.
- Related advisory findings in the #797 review not filed separately: CR-2 (`SerializeNow` file I/O and unbounded write-lock wait on the UI thread), CR-3 (single-shot guard re-armed early), CR-5 (AC5 double persistence retained). CR-2 may be folded in if scope allows.
- Superseded issues: #801, #805 (close with a pointer to this issue).

## Proposed Fix / Validation Ideas

Acceptance criteria:

- [ ] AC1: With an unresolvable archive root, the recents projection returns entries unchanged and logs one warning; no exception escapes `AddRecents` / `AddRecentRows`; a unit test covers the throwing-root case with a Moq seam.
- [ ] AC2: A `bool` attempted flag on `StoreWrapperController`, set on the first `RefreshUserEmailAddress` retry and reset in `Launch`, bounds the retry to once per dialog open; tests: two re-selections of a failing store trigger one lookup, a new `Launch` permits one more, a successful lookup never retries.
- [ ] AC3: Comments at `Display.cs` 41-51 and `StoreWrapper.cs` 214-219 and the #797 `spec.md` prose state the enforced bound.

Validation:

- [ ] Unit coverage areas: `FolderPredictor` recents projection with a throwing root accessor; `PopulateWithCurrent` retry gating; `Launch` reset.
- [ ] Integration scenario to retest: QuickFiler on a profile without an `Archive` folder; Folder Settings on a failing-lookup profile.
- [ ] Manual verification notes: one `GetSmtpAddressFromStore` chain per dialog open in the log.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
