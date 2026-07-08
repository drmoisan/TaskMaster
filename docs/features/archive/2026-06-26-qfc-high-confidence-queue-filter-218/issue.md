# qfc-high-confidence-queue-filter (Issue #218)

- Date captured: 2026-06-26
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/qfc-high-confidence-queue-filter/ (Issue #218)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #218
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/218
- Last Updated: 2026-06-27
- Work Mode: minor-audit

## Summary

High-confidence Quick Filer filtering is applied only to the initial GUI load
items instead of the full queue. The filtering must move into
`QfcDatamodel.LoadRemainingEmailsToQueueAsync` so remaining queued messages are
only added when their predicted probability meets the configured threshold.

## Environment

- OS/version: Windows / Outlook VSTO runtime
- .NET version: Repository C# solution toolchain
- Command/flags used: Quick Filer high-confidence launch
- Data source or fixture: Outlook mail items loaded into the Quick Filer queue

## Steps to Reproduce

1. Enable Quick Filer high-confidence mode.
2. Launch Quick Filer with more than the initial visible batch of mail items.
3. Allow remaining emails to load into the queue.

## Expected Behavior

Every mail item added to the Quick Filer queue should pass the high-confidence
prediction threshold when high-confidence mode is enabled. When high-confidence
mode is disabled, queue loading should keep the existing behavior.

## Actual Behavior

Only the initial GUI load path applies the high-confidence filter. Remaining
emails can be added to the queue without running the same prediction threshold
check.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: Not applicable; behavior is visible in queue population logic.

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

## Suspected Cause / Notes

`QfcHomeController.RunAsync` applies high-confidence filtering while loading the
initial GUI items, but `QfcDatamodel.LoadRemainingEmailsToQueueAsync` adds
remaining `MailItem` instances directly to `_masterQueue`.

## Proposed Fix / Validation Ideas

- [ ] Move or duplicate the high-confidence prediction gate into
      `QfcDatamodel.LoadRemainingEmailsToQueueAsync`.
- [ ] Verify high-confidence mode only adds remaining queue items whose
      prediction probability is greater than or equal to
      `Globals.QfSettings.HighConfidenceThreshold`.
- [ ] Verify disabled high-confidence mode preserves the existing queue-add and
      move-monitor hook behavior.
- [ ] Verify the initial GUI load no longer performs queue-scope filtering that
      only applies to the first visible batch.

## Acceptance Criteria

- [x] When high-confidence mode is enabled, `QfcDatamodel.LoadRemainingEmailsToQueueAsync` scores each remaining `MailItem` before adding it to `_masterQueue`.
- [x] When a remaining item score is greater than or equal to `Globals.QfSettings.HighConfidenceThreshold`, the item is added to `_masterQueue` and hooked with `_moveMonitor.HookItem`.
- [x] When a remaining item score is below the configured threshold, the item is not added to `_masterQueue` and is not hooked with `_moveMonitor.HookItem`.
- [x] When high-confidence mode is disabled, remaining `MailItem` queue loading keeps the existing add and hook behavior.
- [x] The GUI initial load path no longer owns the high-confidence filtering decision for only the first visible batch.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
