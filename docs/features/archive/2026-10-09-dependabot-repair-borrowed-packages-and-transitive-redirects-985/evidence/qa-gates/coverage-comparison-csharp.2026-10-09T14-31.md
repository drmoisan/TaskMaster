# C# Coverage Comparison (P5-T6)

Timestamp: 2026-10-09T14-31
Command: comparison of FEATURE/evidence/baseline/cs-test.2026-10-09T14-11.md (P0-T13) and FEATURE/evidence/qa-gates/cs-test.2026-10-09T14-31.md (P5-T5); git diff --numstat BASE-SHA -- '*.cs'; git status --porcelain -- '*.cs'
EXIT_CODE: 0
Output Summary:
- Baseline line coverage: 85.41 (56496/66143); final line coverage: 85.41 (56495/66143); delta 0.00 percentage points (one covered line fewer, within run-to-run variation; no C# source changed)
- Baseline branch coverage: 79.83 (13709/17173); final branch coverage: 79.83 (13709/17173); delta 0.00
- Runner floors (line 80, branch 75) met on both runs.
- `git diff --numstat BASE-SHA -- '*.cs'`: empty
- `git status --porcelain -- '*.cs'`: empty
- New or changed C# code coverage: not applicable (no C# source line changed, proved by the two empty outputs above).
- Result: PASS; no scope violation.
