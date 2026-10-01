# P1-T1 Regression test added

Timestamp: 2026-10-01T06-47
Command: Edit insertion of the 24-line Target test source into QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs; then git grep -c -F per token, git diff --numstat 9b3eea58447c264eae6f95a4bfee3bfcec7fb17f over the file, and a [regex]::Matches CRLF/LF count
EXIT_CODE: 0
Output Summary:
- Token DispatchValue_OwnerOnlyOnOwnerThread_FaultsOutsideExecutingCallback: 1 line (exactly 1).
- Token "outside an executing Dispatch callback": 1 line (exactly 1).
- git diff --numstat MERGE-BASE: 24 added, 0 deleted (limit: 0 deleted, at most 40 added).
- Line count 410 (limit 500); CRLF count 410, LF count 410, ReadAllLines 410 (all equal).
- Tokens Thread.Sleep, Task.Delay, [Timeout: 0 occurrences each.
- MERGE-BASE printed by git merge-base origin/main HEAD: 9b3eea58447c264eae6f95a4bfee3bfcec7fb17f (unchanged from the P0-T3 record).
- No test run in this task; the fail-before run is P1-T2.
