# outlook-recipient-com-cross-thread-crash (Issue #124)

- Date captured: 2026-04-08
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/outlook-recipient-com-cross-thread-crash/ (Issue #124)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #124
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/124
- Last Updated: 2026-04-08
- Work Mode: minor-audit

## Summary

Background mail tokenization can crash with an unhandled Outlook COM/MAPI exception while
resolving Exchange recipient names. The failure occurs when recipient/sender Outlook COM
objects are accessed from background `Task.Run` paths and `RecipientStatic.GetRecipientName`
does not provide a COM-safe fallback.

## Environment

- OS/version: Windows
- Python version: N/A
- Command/flags used: Normal Outlook add-in background processing during inbox item handling
- Data source or fixture: Outlook `MailItem` objects with Exchange-backed sender/recipient metadata

## Steps to Reproduce

1. Let the add-in process a new Outlook `MailItem` through `TaskMaster.AppEvents.ProcessMailItemAsync`.
2. Allow `MailItemHelper.FromMailItemAsync` and later tokenization work to run through background `Task.Run` paths.
3. Trigger recipient or sender resolution for an Exchange-backed message where `ExchangeUser.FirstName` or `ExchangeUser.LastName` is read off the Outlook STA thread.
4. Observe the unhandled `System.Runtime.InteropServices.COMException`.

## Expected Behavior

Background mail processing should not access Outlook apartment-threaded COM objects from pool
threads, and recipient/sender name resolution should degrade safely to display-name/address
fallbacks instead of crashing the add-in.

## Actual Behavior

The add-in throws an unhandled Outlook COM exception while resolving Exchange recipient names:

`System.Runtime.InteropServices.COMException (0xD0740106): The operation failed. The messaging interfaces have returned an unknown error. If the problem persists, restart Outlook.`

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: `RecipientStatic.GetRecipientName` reads `ExchangeUser.FirstName` / `LastName` during background tokenization, and related logs show similar COM failures for sender lookup, `CurrentUser`, and store SMTP resolution when Outlook objects are accessed off the main STA thread.

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

## Suspected Cause / Notes

Confirmed root-cause chain from preliminary diagnosis and code inspection:

- `TaskMaster.AppEvents.ProcessMailItemAsync` creates a `MailItemHelper` and then forces `helper.Tokens` inside `Task.Run`.
- `UtilitiesCS.OutlookObjects.MailItem.MailItemHelper.FromMailItemAsync` currently constructs the helper inside `Task.Run`, and its lazy fields defer Outlook COM access for sender, recipients, body, folder, and HTML state.
- `UtilitiesCS.OutlookObjects.Recipient.RecipientStatic.GetRecipientName` reads `ExchangeUser.FirstName` and `LastName` without a `try/catch` fallback, so intermittent off-thread MAPI failures become process-visible exceptions.
- Existing sender helpers already show the intended defensive pattern: catch COM/lookup failures and fall back to `Name`, `Address`, or MAPI property accessors.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas
- [x] Integration scenario to retest
- [x] Manual verification notes

- Materialize sender/recipient/tokenizer-required data on the Outlook/UI thread before any background tokenization path consumes it.
- Add a defensive COM-safe fallback in `GetRecipientName` (and verify `GetRecipientAddress` remains safe) so Exchange directory lookup failures fall back to recipient display name or address data.
- Add regression coverage for the fallback path and for the pre-materialized helper/tokenization path.

## Acceptance Criteria

- [x] `MailItemHelper` no longer relies on background `Task.Run` evaluation of Outlook COM-backed lazy sender/recipient properties during the `ProcessMailItemAsync` tokenization path.
- [x] Exchange recipient-name resolution no longer throws an unhandled COM exception when directory property access fails; it falls back to safe recipient data.
- [x] Regression tests cover the recipient fallback behavior and the helper/tokenization path that previously crossed thread-affinity boundaries.
- [x] The C# QA loop passes in the required order: format, analyzer build, nullable/type-safe build, and MSTest with coverage.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [x] Move to active fix folder / branch