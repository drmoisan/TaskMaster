# C# Coverage Summary

Timestamp: 2026-05-06T14:37:21-04:00
Baseline Coverage Artifact: `docs/features/active/2026-05-05-outlook-startup-ui-thread-deblock-141/evidence/baseline/csharp-mstest-coverage.2026-05-05T09-21-00.md`
Final Coverage Artifact: `docs/features/active/2026-05-05-outlook-startup-ui-thread-deblock-141/evidence/qa-gates/csharp-mstest-coverage.2026-05-06T14-37-21.md`
Baseline Repo Line Coverage: 78.2220%
Final Repo Line Coverage: 76.1316%
Coverage Delta: -2.0904 percentage points

Changed Production File Line Coverage:
- `TaskMaster/AppGlobals/ApplicationGlobals.cs`: baseline 37.9085%, final 59.1195% (+21.2110 pp)
- `TaskMaster/AppGlobals/AppOlObjects.cs`: baseline 24.4898%, final 29.1353% (+4.6455 pp)
- `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs`: baseline 96.2838%, final 96.7273% (+0.4435 pp)
- `TaskMaster/AppGlobals/AppToDoObjects.cs`: baseline 6.9620%, final 46.5779% (+39.6159 pp)

New/Changed-Code Coverage: 76.4706% (78/102 executable changed lines)
Changed-Line Breakdown:
- `TaskMaster/AppGlobals/ApplicationGlobals.cs`: 1/3 executable changed lines covered (33.33%)
- `TaskMaster/AppGlobals/AppOlObjects.cs`: 17/38 executable changed lines covered (44.74%)
- `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs`: 26/26 executable changed lines covered (100.00%)
- `TaskMaster/AppGlobals/AppToDoObjects.cs`: 34/35 executable changed lines covered (97.14%)
Changed-Lines Metric Source: `git diff --unified=0 302dd270faac2985b15399728da47421caab61e0 -- <active production files>` intersected with executable lines present in `coverage/coverage.cobertura.xml`.
Coverage Policy Evaluation: All four touched active production files improved in per-file line coverage relative to the feature-branch baseline. However, the overall repository line rate declined from 78.2220% to 76.1316% (-2.09 pp). This decline is attributable to an increase in tracked lines-valid (+9,022: 203,150 → 212,172) that exceeds the increase in lines-covered (+2,622: 158,908 → 161,530); the additional tracked lines are from files outside the change scope and are not caused by this feature branch. The changed-lines metric for the four in-scope production files is 76.4706% (78/102), which does not satisfy the validator-ready changed/new-code threshold of ≥90%. The principal deficits are in `ApplicationGlobals.cs` (1/3 covered, 33.33%) and `AppOlObjects.cs` (17/38 covered, 44.74%). `StoresWrapper.cs` achieves 100% and `AppToDoObjects.cs` achieves 97.14% on changed lines.
Coverage Conclusion: FAIL
