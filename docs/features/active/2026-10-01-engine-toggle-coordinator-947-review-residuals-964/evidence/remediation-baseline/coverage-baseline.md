# Coverage baseline (P0-T4)

Timestamp: 2026-10-03T09-23 (host clock read at correction; the label first written was composed, not read)
Command: Grep tool -n over evidence/qa-gates/coverage-final.md for five committed-evidence patterns; Grep tool counts over coverage/final-964.cobertura.xml (explicit file path) for the Messages class node with branch-rate 0.5 and with branch-rate 1.
EXIT_CODE: 0
Output Summary: each committed-evidence pattern has exactly one matching line (lines 13, 37, 91, 93, 94 of coverage-final.md); raw document read gives 1 for the 0.5 pattern and 0 for the 1 pattern.

BASELINE-COORD-BRANCHES: 43/44 (97.73 percent)
BASELINE-COORD-LINES: 203/203
BASELINE-MESSAGES-LINES: 42/42
BASELINE-FIRST-PARTY: - First-party coverage: lines 56629/65881 (85.96%), branches 13683/17082 (80.10%)
BASELINE-TEST-TOTAL: 7388
BASELINE-MESSAGES-BRANCH-RATE: 0.5

Raw-document read (coverage/final-964.cobertura.xml, git-ignored, not committed): pattern with branch-rate 0.5 = 1, pattern with branch-rate 1 = 0.
