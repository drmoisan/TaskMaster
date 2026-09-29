# P2-T11 AC5 Check-Off

Timestamp: 2026-09-29T09-27
Task: P2-T11
Command: Edit tool on issue.md (AC5 checkbox only); Grep count of `^- \[x\] AC` over issue.md before and after
EXIT_CODE: 0

Evidence read:

- evidence/baseline/p0-t5-format-baseline.2026-09-29T09-01.md: the formatter liveness control passed on this tree (the indentation added to line 272 was removed by the MCP formatter), and the base-tree FORMAT-DRIFT-SET is empty.
- evidence/qa-gates/p1-t7-format-measured.2026-09-29T09-23.md: the measured pass after the change rewrote none of the four Write Set PowerShell files (FORMAT-REWROTE: none).
- evidence/qa-gates/p2-t1-format.iter1.2026-09-29T09-16.md: across a further MCP format run over scripts/vscode and tests/scripts/vscode, HB equals HA for Invoke-MSTestWithCoverage.ps1 (d71b98ec641781702605e6806ca6527726539620) and for Invoke-MSTest.ps1 (9aec072f5255beeb7ff687291221dad4cc72fdd5). A format run after the change therefore leaves both files byte-identical.

Check-off: the AC5 line changed from `- [ ] AC5:` to `- [x] AC5:`; no other character changed. Grep count of `^- \[x\] AC` rose from 4 to 5.

Output Summary: AC5 checked off. Both scripts are formatter-clean under the MCP PoshQC formatter, and the measurement uses raw-byte hashes before and after the run.
