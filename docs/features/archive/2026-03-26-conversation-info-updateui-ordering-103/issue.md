# conversation-info-updateui-ordering (Issue #103)

- Date captured: 2026-03-26
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/conversation-info-updateui-ordering/ (Issue #103)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #103
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/103
- Last Updated: 2026-03-26
- Work Mode: minor-audit

## Summary

`LoadConversationInfoAsync()` in `ConversationResolver` triggers `UpdateUI(ConversationInfo.Expanded)` before the local `pair` variable is assigned back to `ConversationInfo`, causing the lazy getter to call `LoadConversationInfo()` synchronously. When `Count.Expanded == 0` (e.g. for items in Junk E-mail), that synchronous path throws `InvalidOperationException`.

## Environment

- OS/version: Windows 11 / Outlook VSTO add-in
- Python version: N/A
- Command/flags used: Triggered when a mail item in `Junk E-mail` folder is opened and `ConversationResolver` loads conversation info
- Data source or fixture: Mail item with valid `ConversationID` but empty `Df.Expanded` after `FilterConversation` removes all rows for Junk folder

## Steps to Reproduce

1. Open Outlook with the TaskMaster add-in active.
2. Navigate to a mail item in the `Junk E-mail` folder (the item must have a valid ConversationID).
3. Observe that the QuickFiler conversation panel throws an unhandled `InvalidOperationException`.

## Expected Behavior

The conversation panel loads without error, showing either the conversation items or a sensible fallback (single item) when the full conversation cannot be loaded.

## Actual Behavior

```
System.InvalidOperationException
  Message=ConversationInfo cannot be loaded if Df cannot be resolved
  QuickFiler.dll!QuickFiler.Helper_Classes.ConversationResolver.LoadConversationInfo() Line 285
```
The `UpdateUI(ConversationInfo.Expanded)` call in `LoadConversationInfoAsync()` fires the lazy getter before `ConversationInfo = pair` is executed, which re-enters `LoadConversationInfo()` and throws when `Count.Expanded <= 0`.

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet:
```
QuickFiler.dll!QuickFiler.Helper_Classes.ConversationResolver.LoadConversationInfo() Line 285
  at C:\Users\DanMoisan\repos\TaskMaster.worktrees\copilot-worktree-2026-03-19T01-51-14\QuickFiler\Helper Classes\ConversationResolver.cs(285)
```

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

## Suspected Cause / Notes

- Root cause is a read-before-write ordering bug: `LoadConversationInfoAsync()` calls `UpdateUI(ConversationInfo.Expanded)` (which accesses the lazy property getter, triggering synchronous `LoadConversationInfo()`) before it executes `ConversationInfo = pair`.
- Secondary issue: `LoadConversationInfo()` throws instead of returning a safe fallback when `Count.Expanded == 0`. A junk-mail item with a valid ConversationID but all rows filtered out by `FilterConversation` hits this path.

## Proposed Fix / Validation Ideas

- [x] In `LoadConversationInfoAsync()`: assign `ConversationInfo = pair` BEFORE calling `UpdateUI`; pass `pair.Expanded` directly to avoid re-reading the property.
- [x] In `LoadConversationInfo()` (sync path): return a safe fallback `Pair<List<MailItemHelper>>` containing just `[MailHelper]` instead of throwing when `Count.Expanded <= 0`, with a clear error log entry.
- [x] Unit test: verify `LoadConversationInfoAsync` calls `UpdateUI` with the newly assigned pair's Expanded list when `Count.Expanded == 0`.
- [x] Unit test: verify `LoadConversationInfo` no longer throws when `Count.Expanded == 0` but returns single-item fallback.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [x] Move to active fix folder / branch