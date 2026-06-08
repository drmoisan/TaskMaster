# C# Coverage Summary

Timestamp: 2026-05-05T15:05:19.4841096-04:00
Baseline Coverage Artifact: `docs/features/active/2026-05-05-outlook-startup-ui-thread-deblock-141/evidence/baseline/csharp-mstest-coverage.2026-05-05T09-21-00.md`
Final Coverage Artifact: `docs/features/active/2026-05-05-outlook-startup-ui-thread-deblock-141/evidence/qa-gates/csharp-mstest-coverage.2026-05-05T15-05-19.md`
Baseline Repo Line Coverage: 78.2220%
Final Repo Line Coverage: 78.4021%
Coverage Delta: +0.1801 percentage points

Changed Production File Line Coverage:
- `TaskMaster/AppGlobals/ApplicationGlobals.cs`: baseline 37.9085%, final 36.4780%
- `TaskMaster/AppGlobals/AppOlObjects.cs`: baseline 24.4898%, final 27.8195%
- `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs`: baseline 96.2838%, final 92.7273%
- `TaskMaster/AppGlobals/AppToDoObjects.cs`: baseline 6.9620%, final 24.7126%

New/Changed-Code Coverage: 61.9469% (70/113 executable changed lines)
Changed-Lines Metric Source: `git diff --unified=0 302dd270faac2985b15399728da47421caab61e0 -- <active production files>` intersected with executable lines present in `coverage/outlook-startup-ui-thread-deblock-141-final.cobertura.xml`.
Coverage Policy Evaluation: Repository-wide line coverage did not regress and improved from 78.2220% to 78.4021%. However, the repository-equivalent changed-lines metric for the scoped implementation remains 61.9469%, which still does not satisfy the validator-ready changed/new-code threshold. In addition, two touched active production files still regress in per-file line coverage: `TaskMaster/AppGlobals/ApplicationGlobals.cs` declined from 37.9085% to 36.4780% and `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs` declined from 96.2838% to 92.7273%.
Coverage Conclusion: FAIL
