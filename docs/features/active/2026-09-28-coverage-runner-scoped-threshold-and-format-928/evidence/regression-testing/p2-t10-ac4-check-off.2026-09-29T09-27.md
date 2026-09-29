# P2-T10 AC4 Check-Off

Timestamp: 2026-09-29T09-27
Task: P2-T10
Command: Grep count of `\.PARAMETER SearchRoot` over scripts/vscode/Invoke-MSTestWithCoverage.ps1; Edit tool on issue.md (AC4 checkbox only); Grep count of `^- \[x\] AC` over issue.md before and after
EXIT_CODE: 0

Evidence read:

- evidence/regression-testing/p1-t3-expect-fail.2026-09-29T09-14.md: It 14 `documents the scoped-run behavior on the SearchRoot parameter` failed before the fix on the `$key.Count` assertion (no SearchRoot help entry).
- evidence/regression-testing/p1-t6-pass-after.2026-09-29T09-21.md: It 14 passes after the fix.
- evidence/qa-gates/p2-t3-test-coverage.iter1.2026-09-29T09-23.md: the new suite reports 14 tests and 0 failures in the full-population run.
- evidence/regression-testing/p1-t5-entry-point-edit.2026-09-29T09-19.md: `.PARAMETER SearchRoot` count 1. Re-checked on the current committed file: count 1. The help paragraph (entry point lines 284 to 291, shown in the P2-T3 hunk at +278,14) defines a scoped run and states that the threshold assertions are skipped on a scoped run with one warning. It also states that an unscoped run keeps enforcing the 80 and 75 percent floors.

Check-off: the AC4 line changed from `- [ ] AC4:` to `- [x] AC4:`; no other character changed. Grep count of `^- \[x\] AC` rose from 3 to 4.

Output Summary: AC4 checked off. The comment-based help of Invoke-MSTestWithCoverageMain documents the scoped-run behavior and the definition of a scoped run. It 14 fails before the fix and passes after it.
