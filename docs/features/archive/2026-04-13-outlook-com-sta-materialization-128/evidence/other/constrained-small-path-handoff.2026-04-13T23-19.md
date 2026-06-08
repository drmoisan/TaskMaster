# Constrained Small-Path Handoff

Timestamp: 2026-04-13T23-19
Plan Path: `c:\Users\DanMoisan\repos\TaskMaster\docs\features\active\2026-04-13-outlook-com-sta-materialization-128\plan.2026-04-13T22-47.md`
Requirements Source: `issue.md` `## Acceptance Criteria` only

## Locked Production Scope

- `UtilitiesCS/EmailIntelligence/Bayesian/EmailDataMiner.cs` (implemented in the physical defining file `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailDataMiner.cs` because that file contains `UtilitiesCS.EmailIntelligence.Bayesian.EmailDataMiner`)
- `UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.cs`
- `UtilitiesCS/OutlookObjects/Recipient/RecipientStatic.cs`

Production File Count: 3

## Locked Test Scope

- `UtilitiesCS.Test/EmailIntelligence/EmailDataMiner_Tests.cs`
- `UtilitiesCS.Test/EmailIntelligence/EmailParsingSorting/MailItemHelperTests.cs`
- `UtilitiesCS.Test/OutlookObjects/Recipient/RecipientStaticSenderResolverTests.cs`

Test File Count: 3

## Downstream Implementation Requirements

- Keep Outlook COM-backed helper materialization on the caller STA thread by removing the `Task.Run` offload around `MailItemHelper.FromMailItemAsync` in the mining path.
- Apply COM-safe sender and recipient fallback guards so Exchange directory failures degrade to safe mail-item or recipient data.
- Add regression coverage only in the locked test homes above.
- Keep this exact plan path as the controlling plan.
- Do not add or depend on `spec.md`, `user-story.md`, or `research.md`.
- Return to Phase 2 for the unconditional final C# QC loop and reduced-audit evidence.

## Stop-And-Escalate Rule

Any required production change outside the three locked production files, or any required test expansion beyond the three locked test homes, ends the constrained small-path route and requires escalation.
