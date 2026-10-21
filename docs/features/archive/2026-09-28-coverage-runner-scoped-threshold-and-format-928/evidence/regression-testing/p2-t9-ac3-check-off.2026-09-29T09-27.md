# P2-T9 AC3 Check-Off

Timestamp: 2026-09-29T09-27
Task: P2-T9
Command: Edit tool on issue.md (AC3 checkbox only); Grep count of `^- \[x\] AC` over issue.md before and after
EXIT_CODE: 0

Evidence read:

- evidence/regression-testing/p1-t6-pass-after.2026-09-29T09-21.md: It 12 `throws the line threshold message when the search root is omitted and the line rate is below 80 percent` and It 13 `throws the branch threshold message for a dot search root when the branch rate is below 75 percent` both pass. Both are controls that passed before the fix too.
- evidence/qa-gates/p2-t3-test-coverage.iter1.2026-09-29T09-23.md: the new suite reports 14 tests and 0 failures in the full-population run.
- evidence/qa-gates/p2-t5-file-size-and-untouched-neighbors.2026-09-29T09-26.md: `-lt 80`, `-lt 75`, `required 80% threshold` and `required 75% threshold` each count 1 in the threshold part file, which is absent from the numstat.
- evidence/qa-gates/p2-t6-scope-lock.2026-09-29T09-27.md: neither listing names a path under the GitHub workflows directory.

Check-off: the AC3 line changed from `- [ ] AC3:` to `- [x] AC3:`; no other character changed. Grep count of `^- \[x\] AC` rose from 2 to 3.

Output Summary: AC3 checked off. The unscoped run still throws the existing line and branch threshold messages, the CI workflow is unmodified, and the 80 and 75 literals are unchanged.
