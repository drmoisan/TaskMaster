# P5-T19 AC19 check-off

Timestamp: 2026-10-06T18-43
Command: Grep tool over spec.md `^- \[x\] AC19 ` and `^- \[[ x]\] AC[0-9]+ ` (count); Read of the named artifacts
EXIT_CODE: 0
Output Summary: AC19 met and checked off. The fail-before record holds the six HIT lines in the AC19 list (ExpectedExitCode 1, EXIT_CODE 1). The pass-after record shows 0 hits by both routes. Each of the four single-directive files changed by exactly 0/1, and CategoryClassifierGroup.cs lost exactly two `using` lines. All five files keep equal line and CR counts. P4-T5 and P4-T6 recorded no DIRECTIVE-RESTORED line.

Artifacts read:
- evidence/regression-testing/graph-usings-grep-fail-before.md: ExpectedExitCode 1; EXIT_CODE 1; HIT lines StoreWrapper.cs:7, Triage_OlLogic.cs:10, CategoryClassifierGroup.cs:11 and :12, ManagerAsyncLazy.cs:18, FolderMinimalWrapper.cs:6; GRAPH-USING-HITS 6; GRAPH-USING-GREP-HITS 6.
- evidence/regression-testing/graph-usings-grep-pass-after.md: EXIT_CODE 0; GRAPH-USING-HITS 0; GRAPH-USING-GREP-HITS 0; numstat 0/1 for the four files and 0/2 for CategoryClassifierGroup.cs before Part H; equal line and CR counts.
- evidence/qa-gates/p3-t17-graph-usings-deletions.2026-10-03T11-35.md (EXIT_CODE 0): USINGS-AFTER equals USINGS-BEFORE minus the deleted count, with one hunk per file.
- evidence/other/category-classifier-group-split-census.md (EXIT_CODE 0): `REMOVED-USING-LINES: 2`, and `ADDED-LINES: 1` is the declaration line, so no `using` line is added.
- evidence/qa-gates/msbuild-analyzers.md and msbuild-treatwarningsaserrors.md: `DIRECTIVE-RESTORED: none`.

SPEC-LINE: `- [x] AC19 (six unused Graph directives removed; ...` (criterion text unchanged)
