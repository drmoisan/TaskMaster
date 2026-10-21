# P2-T14 Committed-Evidence-Format and Host-Identifier Scan

Timestamp: 2026-09-29T09-29
Task: P2-T14
Command: Grep tool and Glob tool, patterns as quoted
EXIT_CODE: 0

Scope: the feature folder docs/features/active/2026-09-28-coverage-runner-scoped-threshold-and-format-928 (the plan file, issue.md and the whole evidence tree).

## First pass (fix applied)

- Step (4) initially failed. Four header lines read "MCP payload (transcribed, workspace_root replaced):" and did not name `<repo-root>` on the same line. They were in the P1-T3, P1-T6, P1-T7 and P2-T1 iteration 1 artifacts. Each line was changed to "MCP payload (transcribed, workspace_root replaced by <repo-root>):" (placeholder substitution per D13). No other character of those artifacts changed. The task was then re-run from step (1).

## Second pass (recorded result)

1. Glob with the recursive xml pattern rooted at the feature folder returned no file. Glob with the recursive trx pattern returned no file. No raw test-result or coverage collector document is under the feature folder.
2. Grep, count mode, pattern `(?x) (^|[^A-Za-z]) [A-Za-z] : [\\/]` over the feature folder: 0 matches.
2b. The `hostname` attribute and the `user` property value were read from the JUnit document written by the final P2-T3 run. Each was then searched case-insensitively over the feature folder as a literal (neither value contains a regex metacharacter).
   - HOST-NAME-SCAN: 0 matches
   - ACCOUNT-NAME-SCAN: 0 matches
3. Grep, count mode, pattern `(?xi) [\\/] Users [\\/]` over the feature folder: 0 matches.
4. Grep over the evidence subfolder for the field name: 31 matching lines across 9 files. Grep for lines that also carry `<repo-root>` on the same line: 31 lines across the same 9 files, with equal per-file counts. Every matching line carries the placeholder. The plan file was excluded from this check as the task states.

## Check-off

- The AC7 line in issue.md changed from `- [ ] AC7:` to `- [x] AC7:`; no other character changed.
- Grep count of `^- \[x\] AC` over issue.md: 6 (the P2-T13 value of 5 plus 1).

## Post-write rescan

Steps (2), (2b) and (3) are re-run after this artifact and the check-off are written. The result is appended below only when every count is again 0.

POST-WRITE-RESCAN: 0 matches

Output Summary: PASS. The feature folder contains no xml or trx document. Drive-letter paths, user-profile prefixes, the host name and the account name each return 0 matches, both before and after the write. Every evidence line naming the workspace field carries `<repo-root>`. AC7 is checked off, and the AC count is 6.
