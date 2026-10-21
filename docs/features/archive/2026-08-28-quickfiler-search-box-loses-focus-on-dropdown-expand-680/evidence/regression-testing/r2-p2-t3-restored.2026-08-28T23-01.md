Timestamp: 2026-08-28T23-01
Command: git checkout 72b4b7ed -- docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/regression-testing/p2-t3/p2-t3.trx; git hash-object <path>; git rev-parse 72b4b7ed:<path>; (Select-String -Path <path> -Pattern 'outcome="Failed"' -AllMatches).Matches.Count
EXIT_CODE: 0
Output Summary: `git checkout` exited 0 with a single-pathspec restore (git status shows exactly one
modified entry for this path). `git hash-object` on the restored working-tree file equals `git rev-parse
72b4b7ed:<path>` exactly (blob-hash equality confirmed). The `outcome="Failed"` count on the restored
file is 3 (2 <UnitTestResult>-level AC-3 failures plus 1 <ResultSummary outcome="Failed"> run-level
rollup matched by the same pattern), strictly greater than R2_BEFORE_FAILED_COUNT (0, from P0-T5) — the
false-before/true-after pair required by D8.
