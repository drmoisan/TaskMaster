# C# Coverage Summary

Timestamp: 2026-05-05T13:31:01.4842055-04:00
Baseline Coverage Artifact: `docs/features/active/2026-05-05-outlook-startup-ui-thread-deblock-141/evidence/baseline/csharp-mstest-coverage.2026-05-05T09-21-00.md`
Final Coverage Artifact: `docs/features/active/2026-05-05-outlook-startup-ui-thread-deblock-141/evidence/qa-gates/csharp-mstest-coverage.2026-05-05T13-22-10.md`
Baseline Repo Line Coverage: 78.2220%
Final Repo Line Coverage: 78.2766%
Coverage Delta: +0.0546 percentage points

Changed Production File Line Coverage:
- `TaskMaster/AppGlobals/ApplicationGlobals.cs`: baseline 37.9085%, final 36.4780%
- `TaskMaster/AppGlobals/AppOlObjects.cs`: baseline 24.4898%, final 26.6917%
- `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs`: baseline 96.2838%, final 92.7273%
- `TaskMaster/AppGlobals/AppToDoObjects.cs`: baseline 6.9620%, final 17.4330%

New/Changed-Code Coverage: 38.2550% (57/149 executable changed lines)
Changed-Lines Metric Source: `git diff --unified=0 302dd270faac2985b15399728da47421caab61e0 -- <active production files>` intersected with executable lines present in `coverage/outlook-startup-ui-thread-deblock-141-final.cobertura.xml`.
Coverage Policy Evaluation: Repository-wide line coverage did not regress and improved slightly from 78.2220% to 78.2766%. However, the repository-equivalent changed-lines metric for the scoped implementation is 38.2550%, which does not satisfy the nominal changed/new-code coverage threshold for validator-ready sign-off. In addition, two touched active production files regressed in per-file line coverage: `TaskMaster/AppGlobals/ApplicationGlobals.cs` declined from 37.9085% to 36.4780% and `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs` declined from 96.2838% to 92.7273%.
Coverage Conclusion: FAIL
