# C# Coverage Summary

Timestamp: 2026-05-05T14:00:29.3323758-04:00
Baseline Coverage Artifact: `docs/features/active/2026-05-05-outlook-startup-ui-thread-deblock-141/evidence/baseline/csharp-mstest-coverage.2026-05-05T09-21-00.md`
Final Coverage Artifact: `docs/features/active/2026-05-05-outlook-startup-ui-thread-deblock-141/evidence/qa-gates/csharp-mstest-coverage.2026-05-05T14-00-29.md`
Baseline Repo Line Coverage: 78.2220%
Final Repo Line Coverage: 78.3808%
Coverage Delta: +0.1588 percentage points

Changed Production File Line Coverage:
- `TaskMaster/AppGlobals/ApplicationGlobals.cs`: baseline 31.1828%, final 29.5918%
- `TaskMaster/AppGlobals/AppOlObjects.cs`: baseline 23.8267%, final 28.2143%
- `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs`: baseline 93.6306%, final 90.3030%
- `TaskMaster/AppGlobals/AppToDoObjects.cs`: baseline 8.8968%, final 26.1981%

New/Changed-Code Coverage: 61.0619% (69/113 executable changed lines)
Changed-Lines Metric Source: `git diff --unified=0 302dd270faac2985b15399728da47421caab61e0 -- <active production files>` intersected with executable lines present in `coverage/outlook-startup-ui-thread-deblock-141-final.cobertura.xml`.
Coverage Policy Evaluation: Repository-wide line coverage improved from 78.2220% to 78.3808%, so the no-regression repo gate is satisfied. However, the refreshed changed-lines metric is still only 61.0619%, which remains below the validator-ready changed/new-code threshold required for `Coverage Conclusion: PASS`. In addition, two active production files still regressed in per-file line coverage: `TaskMaster/AppGlobals/ApplicationGlobals.cs` declined from 31.1828% to 29.5918% and `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs` declined from 93.6306% to 90.3030%.
Coverage Conclusion: FAIL
