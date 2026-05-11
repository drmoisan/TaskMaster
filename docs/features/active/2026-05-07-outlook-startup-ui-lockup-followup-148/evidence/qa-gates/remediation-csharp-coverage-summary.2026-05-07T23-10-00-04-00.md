# Remediation C# Coverage Summary

Timestamp: 2026-05-07T23:10:00-04:00
Baseline Coverage Artifact: evidence/baseline/csharp-mstest-coverage.2026-05-07T21-44-13-04-00.md
Final Coverage Artifact: evidence/qa-gates/remediation-csharp-mstest-coverage.2026-05-07T23-09-50-04-00.md
Baseline Repo Line Coverage: 21.82
Final Repo Line Coverage: 76.6499
Coverage Delta: 54.8299
Per-File Coverage:
- TaskMaster/AppGlobals/AppEvents.cs | baseline: 4.60 | final: 77.30
- QuickFiler/Controllers/EfcHomeController.cs | baseline: 6.35 | final: 15.87
- QuickFiler/Controllers/EfcDataModel.cs | baseline: 0.00 | final: 48.30
- QuickFiler/Helper Classes/ConversationResolver.cs | baseline: 18.73 | final: 89.18
- UtilitiesCS/Extensions/DfDeedle.cs | baseline: 62.07 | final: 96.28
- UtilitiesCS/OutlookObjects/Conversation/ConversationHelper.cs | baseline: 81.13 | final: 90.36
- UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.cs | baseline: 45.75 | final: 96.32
- UtilitiesCS/OutlookObjects/Table/OlTableExtensions.cs | baseline: 92.20 | final: 95.95
New/Changed-Code Coverage: 90.989
Coverage Policy Evaluation:
- Repository >= 80%: FAIL
- Repository No-Regression vs baseline: PASS
- New/Changed-Code Coverage >= 90%: PASS
- Repository composite gate (>= 80% or valid no-regression baseline): PASS
Coverage Conclusion: PASS
Notes:
- The remediation inputs require `Coverage Conclusion: PASS` when repository coverage meets the repo gate or satisfies a valid no-regression baseline and changed/new-code coverage is `>= 90%`.
- The repository-wide line coverage remains below `80%`, but it improved materially from `21.82%` to `76.6499%` and therefore satisfies the no-regression branch-level condition for this remediation cycle.
- The changed/new-code coverage figure `90.989` is the verified remediation-cycle aggregate across the eight tracked production files.
