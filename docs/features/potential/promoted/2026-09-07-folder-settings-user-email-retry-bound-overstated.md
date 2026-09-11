# folder-settings-user-email-retry-bound-overstated (Issue #805)

- Date captured: 2026-09-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/folder-settings-user-email-retry-bound-overstated/ (Issue #805)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #805
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/805
- Last Updated: 2026-09-07
## Summary

The User Email retry added by issue #797 (AC6) is documented in the code comments, the specification prose, and the plan as bounded to "at most once per dialog open", but `PopulateWithCurrent` runs on every store re-selection and a failed retry leaves the address null, so the gate stays open. The real bound is one synchronous Exchange COM lookup on the UI thread per store selection change while the lookup keeps failing. The overstated bound was the stated basis for accepting a synchronous COM read on the UI thread, and that same call chain was captured blocking the UI thread in the original #797 diagnosis. Source: advisory finding CR-1 of the #797 code review, merged in PR #804.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Runtime: .NET Framework 4.8 VSTO Outlook add-in; `main` at `206a3f7e` (PR #804 merge commit)
- Command/flags used: Outlook ribbon -> Settings -> Folder Settings; change the Display Name store selection repeatedly on a mailbox whose Exchange user lookup fails
- Data source or fixture: live Exchange mailbox where `Session.CurrentUser` / `GetExchangeUser()` throws (the 2026-09-06 log shows `COMException: The operation failed.`)

## Steps to Reproduce

1. Use a profile where the Exchange SMTP lookup for the primary store fails (User Email shows "Email address unavailable: ...").
2. Open Settings -> Folder Settings.
3. Change the Display Name selection to another store and back, several times.
4. Observe in the debug log that `GetSmtpAddressFromStore` runs its full COM chain on every re-selection of the failing store, each time blocking the UI thread for the duration of the lookup.

## Expected Behavior

Either the retry is genuinely bounded to once per dialog open, or the three comments and the specification prose state the actual bound. The maintainer-preferred outcome is the first: a per-controller attempted flag set on the first retry and reset in `Launch`, so a persistently failing lookup costs one blocking COM chain per dialog open.

## Actual Behavior

`StoreWrapperController.Display.cs` lines 41-45 gate the retry on `Current.UserEmailAddress is null`. `RefreshUserEmailAddress` assigns null back on failure, and `PopulateWithCurrent` has one production call site, inside `DisplayName_SelectedValueChanged` (`StoreWrapperController.cs` line 169), which fires on every selection change. The comments at `StoreWrapperController.Display.cs` 41-51 and `StoreWrapper.cs` 214-219, plus `spec.md` lines 491-493, Non-Goals item 8, and risk 1 of the #797 feature folder, all claim the once-per-open bound.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: no runtime capture yet; the finding is a code-read result. The original UI-thread block on this chain is recorded in `debug_2026-09-06.log` at 17:35:21 (`ThreadMonitor` stack inside `_ExchangeUser.get_PrimarySmtpAddress()`).

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Unbounded repetition of a synchronous COM call known to block the Outlook UI thread, in exactly the failure case the user is trying to diagnose. Does not violate AC6 as written, so it was correctly left advisory in the review.

## Suspected Cause / Notes

- Review finding: `docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/code-review.2026-09-07T22-40.md`, section "CR-1".
- Related advisory findings from the same review, not filed separately here: CR-2 (explicit save performs file I/O and an unbounded write-lock wait on the UI thread via `SerializeNow`), CR-3 (`SerializeNow` re-arms the single-shot guard early), CR-5 (AC5 double-persistence retained without loud divergence detection). CR-2 shares the UI-thread-blocking theme and may be worth folding into the same fix if scope allows.

## Proposed Fix / Validation Ideas

Acceptance criteria:

- [ ] AC1: A `bool` attempted flag on `StoreWrapperController`, set on the first `RefreshUserEmailAddress` retry and reset in `Launch`, so the retry runs at most once per dialog open per controller instance.
- [ ] AC2: The comments at `StoreWrapperController.Display.cs` 41-51 and `StoreWrapper.cs` 214-219 and the #797 `spec.md` prose state the bound the code actually enforces.
- [ ] AC3: Unit tests: re-selecting a failing store twice in one dialog session triggers exactly one lookup; a new `Launch` permits one more; a successful lookup never retries.

Validation:

- [ ] Unit coverage areas: `PopulateWithCurrent` retry gating with a Moq-stubbed lookup seam; `Launch` reset.
- [ ] Integration scenario to retest: Folder Settings on a failing-lookup profile, cycling store selection.
- [ ] Manual verification notes: confirm one `GetSmtpAddressFromStore` log chain per dialog open.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
