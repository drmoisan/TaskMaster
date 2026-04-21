# triage-trains-entire-conversation (Issue #137)

- Date captured: 2026-04-21
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/triage-trains-entire-conversation/ (Issue #137)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #137
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/137
- Last Updated: 2026-04-21
- Work Mode: minor-audit

## Summary

When the user clicks a Ribbon triage button (Set A / Set B / Set C) on a single email, `TrainSelectionAsync` iterates over `ActiveExplorer().Selection`. In Outlook's conversation view the Selection can contain the entire conversation thread, causing all conversation emails to be trained and labeled — not just the one the user intended.

## Environment

- OS/version: Windows, Outlook VSTO Add-in
- Python version: N/A (C# project)
- Command/flags used: Ribbon "Triage Set A/B/C" button
- Data source or fixture: Any Outlook inbox with conversation-view grouping enabled

## Steps to Reproduce

1. Enable conversation grouping in the Outlook inbox.
2. Open an email conversation that contains more than one email.
3. Click on a single email in the conversation to select it.
4. Click the "Triage Set A" Ribbon button.

## Expected Behavior

Only the specifically selected email is trained and receives the "Triage" UDF label. Classifier `TotalEmailCount` increments by 1.

## Actual Behavior

All emails visible in the Outlook Selection (potentially the entire conversation thread) are trained and receive the "Triage" UDF. `TotalEmailCount` increments by the number of conversation items in the Selection rather than by 1.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: `Triage_OlLogic.TrainSelectionAsync` iterates `ActiveExplorer().Selection` without filtering to a single conversation item; `TestActionAsync` is called for every item in the selection.

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

## Suspected Cause / Notes

`Triage_OlLogic.TrainSelectionAsync` (`UtilitiesCS/EmailIntelligence/ClassifierGroups/Triage/Triage_OlLogic.cs`):
- Reads `ActiveExplorer().Selection` which in Outlook conversation view can contain the full conversation thread.
- Calls `TestActionAsync(helper, triageId)` (sets "Triage" UDF) and `TrainAsync(helper.Tokens, triageId)` (increments classifier count) for every item in the selection.
- No guard limits training to the item the user explicitly targeted.

Related: `SetUdf` extension method calls `item.Save()` which can trigger Outlook `ItemAdd`/`ItemChange` events, but the `AsyncCondition` (checks for existing "Triage" UDF) should prevent re-classification. Core issue is the selection iteration, not event re-entrancy.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: Add regression test in `Triage_OlLogicTests.cs` verifying that when a Selection contains multiple MailItems from the same conversation, either (a) only one is trained or (b) each is trained independently with `emailCount = 1` per item, and the classifier count matches exactly the count of explicitly selected items — not the conversation size.
- [x] Integration scenario to retest: Single-email triage click in conversation view increments `TotalEmailCount` by 1.
- [x] Manual verification notes: Verify "Triage" UDF is set only on the clicked email, not on sibling conversation items.

## Acceptance Criteria

- [x] AC1: A new regression test exists in `UtilitiesCS.Test/EmailIntelligence/ClassifierGroups/Triage/Triage_OlLogicTests.cs` that creates a mock `Selection` containing two `MailItem` objects (simulating a conversation-view thread) and verifies that `TrainSelectionAsync` increments `TotalEmailCount` by exactly **1** — i.e., only the first/focused item is trained, not all items in the selection.
- [x] AC2: A new regression test verifies that when `TrainSelectionAsync` is called with a two-item `Selection`, the classifier `MatchEmailCount` for the trained label increases by exactly **1** (only the first item contributes), not by 2.
- [x] AC3: The existing test `TrainSelectionAsync_WhenSelectionContainsMailItem_TrainsClassifierWithExpectedLabel` continues to pass (no regression).
- [x] AC4: The full toolchain passes without error: `csharpier format .` → analyzer build → nullable build → test suite with coverage.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [x] Move to active fix folder / branch