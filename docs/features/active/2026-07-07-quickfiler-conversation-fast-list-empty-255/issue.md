# quickfiler-conversation-fast-list-empty (Issue #255)

- Date captured: 2026-07-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/quickfiler-conversation-fast-list-empty/ (Issue #255)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #255
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/255
- Last Updated: 2026-07-07
- Work Mode: minor-audit

## Summary

In the QuickFiler "Quick File" dialog, when an email item is expanded to show its conversation, the conversation ("fast list") panel displays the message "The fast list is empty" even though the conversation contains multiple items (the conversation count badge shows 8).

## Environment

- OS/version: Windows, Outlook desktop (VSTO add-in)
- Component: QuickFiler item viewer conversation / TopicThread control
- Command/flags used: Open QuickFiler ("Quick File"); expand an email item that belongs to a multi-item conversation
- Data source or fixture: A live conversation with multiple related messages (screenshot shows an 8-item conversation)

## Steps to Reproduce

1. Open the QuickFiler "Quick File" dialog on a mailbox that contains a multi-item conversation.
2. Expand an email item whose conversation count badge shows a non-zero count (e.g., 8).
3. Observe the conversation ("fast list") panel below the item header.

## Expected Behavior

The expanded conversation panel (TopicThread fast list) lists the conversation items (From / Received / In Folder columns populated), consistent with the non-zero conversation count shown on the badge.

## Actual Behavior

The conversation panel shows only the column headers (From, Received, In Folder) and the placeholder message "The fast list is empty", despite the conversation count badge reporting 8 items.

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: See `artifacts/Screenshot 2026-07-07 124120.png`. Item #5 "RE: Carrie next steps" (Shari Ober) shows a conversation count of 8 while the fast list reads "The fast list is empty".

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

The conversation preview is a primary QuickFiler affordance for confirming which related items will be moved together; an empty list undermines confidence in the move.

## Suspected Cause / Notes

The conversation display pipeline populates the count badge from `ConversationResolver.Count.SameFolder` while the fast list is populated via the `SetTopicThread`/`UpdateUI` callback in `QuickFiler/Controllers/QfcItemController.Conversation.cs` → `_itemViewer.SetConversationItems(...)`. Candidate root causes to investigate:

- The async UI publish (`LoadConversationInfoAsync` → `UpdateUI(pair.Expanded)` in `QuickFiler/Helper Classes/ConversationResolver.Loading.cs`) ordering vs. viewer binding refresh.
- A dataframe filter dropping all rows before the list is materialized (e.g., the `SentOn != ""` filter or the same-folder `FilterConversation` filter in `LoadDf`/`LoadDfAsync`).
- The TopicThread control data-source not being refreshed after the list is set.

Files to inspect: `QuickFiler/Helper Classes/ConversationResolver.Loading.cs`, `QuickFiler/Controllers/QfcItemController.Conversation.cs`, and the viewer `SetConversationItems` implementation.

## Acceptance Criteria

- AC1: When an email item with a multi-item conversation is expanded in the QuickFiler item viewer, the conversation ("fast list" / TopicThread) panel is populated with the conversation items instead of showing "The fast list is empty".
- AC2: The number of rows shown in the populated fast list is consistent with the conversation the viewer displays (the placeholder empty message appears only when the resolved conversation list is genuinely empty).
- AC3: The root cause is identified and documented, and a deterministic regression test is added that fails against the pre-fix behavior and passes after the fix, covering the conversation-info/TopicThread population path. The test uses MSTest + Moq + FluentAssertions and does not depend on a live Outlook process or temporary files.
- AC4: The fix is confined to the QuickFiler conversation-display pipeline (no unrelated refactors) and preserves existing behavior for the genuinely-empty conversation case (single-item fallback and Junk E-mail path).
- AC5: The full C# toolchain (CSharpier format, .NET analyzers, nullable type-check, MSTest with coverage) passes, and coverage on changed lines does not regress.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: conversation-info loading / same-folder vs expanded population, TopicThread population callback
- [ ] Integration scenario to retest: expand a multi-item conversation and confirm the fast list is populated
- [ ] Manual verification notes: reproduce with a known multi-item conversation and confirm list rows match the count badge

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
