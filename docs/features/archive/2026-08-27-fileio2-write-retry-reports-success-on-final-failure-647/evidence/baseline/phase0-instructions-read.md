# Phase 0 — Instructions Read

Timestamp: 2026-08-31T18-40

Policy Order: The mandatory reading order defined by `policy-compliance-order` and by `CLAUDE.md` section "Policy Compliance Order". Files are read in full, in rank order, and each is recorded below as it is read.

## Files Read

1. CLAUDE.md
2. .claude/rules/general-code-change.md

File Size Limit recorded from `.claude/rules/general-code-change.md`: no production code, test code, or reusable script file may exceed 500 lines. Markdown documentation files are exempt.

3. .claude/rules/general-unit-test.md

Threshold Reconciliation: CLAUDE.md states a repository-wide line coverage floor of 80 and a new-module/class/method floor of 90. `.claude/rules/general-unit-test.md` states a line floor of 85 and a branch floor of 75. CLAUDE.md is rank 1 in the policy order defined by `policy-compliance-order` and by CLAUDE.md's own "Policy Compliance Order" section, and therefore governs the blocking gates in this plan; the 80 and 90 figures are the blocking values, and the 85 and 75 figures are recorded but not blocking here.

4. .claude/rules/csharp.md

## Requirements Sources Read

5. docs/features/active/2026-08-27-fileio2-write-retry-reports-success-on-final-failure-647/issue.md
6. docs/features/active/2026-08-27-fileio2-write-retry-reports-success-on-final-failure-647/spec.md
7. docs/features/active/2026-08-27-fileio2-write-retry-reports-success-on-final-failure-647/research/2026-08-29T08-30-fileio2-write-retry-research.md

Requirements Source: `spec.md` in this feature folder is the sole acceptance-criteria source for issue #647, carrying 21 criteria AC1 through AC21 under its `## Acceptance Criteria` heading. No `user-story.md` exists in this feature folder and none may be created.

Work Mode: full-bug, as recorded on line 12 of `issue.md`.
