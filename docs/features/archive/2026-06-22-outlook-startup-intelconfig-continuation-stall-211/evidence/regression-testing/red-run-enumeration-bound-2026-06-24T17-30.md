# RED Run — Enumeration-Bound Regression (AC10, issue #211)

Timestamp: 2026-06-24T19-22
Command: vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /InIsolation /Tests:ResolvePath_AccessesOnlyFoldersAlongThePath_NotEntireTree
EXIT_CODE: 1

This is the fail-before artifact required by the repository bugfix workflow. The enumeration-bound
assertion was executed against the CURRENT full-`FolderTree`-enumeration behavior via a legacy-
equivalent eager-enumeration harness (`EagerlyEnumerateEntireTree` + `FindByPathOverEagerHarness`),
which reproduces `new FolderTree(Root)` walking the entire default-store folder hierarchy before any
search. The assertion FAILED, confirming the defect.

Output Summary:
- Test Run Failed. Total tests: 1; Failed: 1.
- Failing test: ResolvePath_AccessesOnlyFoldersAlongThePath_NotEntireTree
- Assertion failure (verbatim):
  "Expected counter[0] to be less than or equal to 4 because the navigator must touch only the
   resolution path plus the first-segment BFS frontier, not the entire tree (issue #211 AC10),
   but found 785 (difference of 781)."
- Interpretation: the eager full-tree enumeration (legacy FolderTree behavior) performed 785
  child-folder enumerations on a breadth=5, depth=4 tree, versus the path-bound budget of 4
  (one first-segment BFS enumeration + 3 subsequent direct-child enumerations). Eager count
  (785) >> path-bound count (4), as predicted.
- This RED proves the legacy resolution path enumerates the entire tree, the root cause of the
  ~50,172 ms JunkCertain cold-start stall documented in the delegation. P3-T4 re-points the test
  at the production JunkFolderPathNavigator.ResolvePath; P3-T5 captures the GREEN run.

Note on test runner banner: the FluentAssertions community-license banner is printed by the test
host and is unrelated to the assertion outcome.
