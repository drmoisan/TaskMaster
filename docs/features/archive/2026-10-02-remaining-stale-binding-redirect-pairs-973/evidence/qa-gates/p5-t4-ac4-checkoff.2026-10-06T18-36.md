# P5-T4 AC4 check-off

Timestamp: 2026-10-06T18-36
Command: Grep tool over spec.md `^- \[x\] AC4 ` and `^- \[[ x]\] AC[0-9]+ ` (count); Grep `^EXIT_CODE:` over evidence/qa-gates/p2-t*; live Grep `newVersion="(1\.62\.0\.0|10\.0\.0\.7|10\.0\.0\.5|4\.89\.0\.0|8\.22\.0\.0|1\.3\.0\.0)"` over glob */app.config
EXIT_CODE: 0
Output Summary: AC4 met and checked off. The 14 sweep artifacts each read EXIT_CODE 0. P2-T16 shows that every corrected-version file list equals the P0-T4 stale-plus-CURRENT union and that no stale value remains. P3-T12 limits the csproj changes to the five Part C projects, none of them for the 15 pairs. The AC3 run is green. A live Grep for the six stale values over every root-level app.config finds no match.

Artifacts read:
- The 14 sweep artifacts p2-t1 to p2-t14 (Tags, TaskTree, TaskVisualization, QuickFiler, TaskMaster, ToDoModel, VBFunctions.Test, Tags.Test, TaskTree.Test, QuickFiler.Test, TaskMaster.Test, TaskVisualization.Test, ToDoModel.Test, UtilitiesCS.Test), each EXIT_CODE 0, with the redirects corrected in place and no block removed other than ADAL.
- evidence/qa-gates/p2-t16-sweep-verification.2026-10-03T11-23.md (EXIT_CODE 0): every corrected-version list equals the union and every per-file count is 1; the stale check finds no match.
- evidence/qa-gates/p3-t12-UtilitiesCS.Test-csproj.2026-10-03T11-28.md (EXIT_CODE 0): the csproj name-list and porcelain both list exactly the five Part C projects.
- evidence/regression-testing/binding-redirect-gate-pass-after.md (EXIT_CODE 0, suite 16/0).

LIVE-STALE-VALUE-GREP: no match (0 files)
SPEC-LINE: `- [x] AC4 (the 15 pairs corrected in place).` (criterion text unchanged)
