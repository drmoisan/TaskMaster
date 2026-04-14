Timestamp: 2026-04-13T22-58
Work Mode: minor-audit
Acceptance Criteria Section Present: yes
Plan Path: c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-04-13-outlook-com-sta-materialization-128\plan.2026-04-13T22-47.md
SearchScope: docs/features/active/2026-04-13-outlook-com-sta-materialization-128/
SearchPatterns: spec.md, user-story.md, research.md
SearchResult: none
Acceptance Criteria (verbatim):
- [ ] `EmailDataMiner.ToIItemInfo` no longer offloads `MailItemHelper.FromMailItemAsync` to `Task.Run`, so Outlook COM-backed sender/recipient materialization remains on the caller's Outlook STA thread.
- [ ] `RecipientStatic.GetSenderName` no longer throws when Exchange Address Book lookup fails; it falls back safely to mail-item sender data without unguarded `sender.Name` access.
- [ ] Recipient helper fallbacks use the same defensive pattern for Exchange-backed lookup failures so background tokenization paths degrade safely instead of crashing.
- [ ] Regression tests cover the sender/recipient fallback behavior and the helper materialization path implicated by this crash.
- [ ] The required C# QA loop passes in order: format, analyzer build, nullable/type-safe build, and MSTest with coverage.
