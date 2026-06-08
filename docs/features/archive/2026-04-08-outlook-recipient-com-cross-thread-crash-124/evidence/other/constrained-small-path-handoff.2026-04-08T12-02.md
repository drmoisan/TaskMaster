Timestamp: 2026-04-08T12-02
Plan Path: docs/features/active/2026-04-08-outlook-recipient-com-cross-thread-crash-124/plan.2026-04-08T00-00.md
Requirements Source: docs/features/active/2026-04-08-outlook-recipient-com-cross-thread-crash-124/issue.md -> ## Acceptance Criteria

In-scope production files:
- UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.cs
- UtilitiesCS/OutlookObjects/Recipient/RecipientStatic.cs

Targeted test files:
- UtilitiesCS.Test/OutlookObjects/Recipient/RecipientStaticTests.cs
- UtilitiesCS.Test/OutlookObjects/MailItem/MailItemHelperCoreTests.cs

Implementation constraints:
- Stay within the scoped production and test files listed above.
- Satisfy only the acceptance criteria under issue.md -> ## Acceptance Criteria.
- Preserve this exact approved plan path as the controlling small-path plan.
- Return to the unconditional Phase 2 C# QA loop after implementation.

Stop-and-escalate rule:
- If any additional production file is required, stop the small-path route and report the blocker instead of expanding scope.
