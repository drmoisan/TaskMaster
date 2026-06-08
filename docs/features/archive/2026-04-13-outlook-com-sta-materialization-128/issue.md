# outlook-com-sta-materialization (Issue #128)

- Date captured: 2026-04-13
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/outlook-com-sta-materialization/ (Issue #128)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #128
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/128
- Last Updated: 2026-04-14
- Work Mode: minor-audit

## Summary

Unhandled Outlook/Exchange Address Book `COMException` can escape while sender data is
materialized for tokenization because Outlook COM reads are occurring on background worker
threads instead of the main Outlook STA thread.

## Environment

- OS/version: Windows
- Runtime: .NET Framework / VSTO Outlook add-in
- Command/flags used: Background email mining and helper materialization paths
- Data source or fixture: Outlook `MailItem` objects with Exchange-backed sender and recipient metadata

## Steps to Reproduce

1. Process an Outlook `MailItem` through the background mining path in `EmailDataMiner.ToIItemInfo`.
2. Allow the code to call `Task.Run(() => MailItemHelper.FromMailItemAsync(...))`, materializing COM-backed sender and recipient data on a worker thread.
3. Trigger an Exchange-backed sender lookup where `GetSenderName` evaluates `AddressEntryUserType` and then falls back to `sender.Name`.
4. Observe the Outlook/Exchange Address Book `COMException` escaping instead of falling back to `olMail.SenderName`.

## Expected Behavior

Outlook COM-backed sender, recipient, and attachment data needed for tokenization should be
materialized on the calling Outlook STA thread, and sender/recipient helpers should fall back
safely when Exchange directory lookups fail.

## Actual Behavior

The add-in throws `System.Runtime.InteropServices.COMException` while reading Exchange Address
Book data. On the failing path, `EmailDataMiner.ToIItemInfo` moves helper creation to a worker
thread, the Exchange lookup fails, and `GetSenderName` still touches `sender.Name` outside the
guarded block so the exception escapes.

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: `System.Runtime.InteropServices.COMException HResult=0x96A40110 Message=Information was given to the Microsoft Exchange Address Book which requires a newer version of the Address Book. Upgrade Microsoft Exchange.`

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

## Suspected Cause / Notes

Confirmed from code inspection:

- `UtilitiesCS.EmailIntelligence.Bayesian.EmailDataMiner.ToIItemInfo` wraps `MailItemHelper.FromMailItemAsync` in `Task.Run`, which moves COM-backed helper materialization onto a worker thread.
- `UtilitiesCS.OutlookObjects.MailItem.MailItemHelper.MaterializeTokenizationDependencies` already documents the intended safe pattern: force Outlook COM-backed values while still on the caller's Outlook thread.
- `UtilitiesCS.RecipientStatic.GetSenderName` catches Exchange lookup failures but still dereferences `sender.Name` outside the protected block, which lets the COM exception escape.
- Recipient helper fallbacks should follow the same COM-safe pattern so directory lookup failures degrade to mail-item or recipient fallback data.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas
- [x] Integration scenario to retest
- [x] Manual verification notes

- Remove the `Task.Run` wrapper around `MailItemHelper.FromMailItemAsync` in the mining path so Outlook COM reads remain on the calling STA thread.
- Make `GetSenderName` fall back to `olMail.SenderName` without touching `sender.Name` outside a protected block.
- Apply the same defensive fallback approach to recipient name/address helpers when Exchange directory reads fail.

## Acceptance Criteria

- [x] `EmailDataMiner.ToIItemInfo` no longer offloads `MailItemHelper.FromMailItemAsync` to `Task.Run`, so Outlook COM-backed sender/recipient materialization remains on the caller's Outlook STA thread.
- [x] `RecipientStatic.GetSenderName` no longer throws when Exchange Address Book lookup fails; it falls back safely to mail-item sender data without unguarded `sender.Name` access.
- [x] Recipient helper fallbacks use the same defensive pattern for Exchange-backed lookup failures so background tokenization paths degrade safely instead of crashing.
- [x] Regression tests cover the sender/recipient fallback behavior and the helper materialization path implicated by this crash.
- [x] The required C# QA loop passes in order: format, analyzer build, nullable/type-safe build, and MSTest with coverage.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch