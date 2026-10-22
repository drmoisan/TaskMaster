# Phase 0 Instructions Read (Remediation Cycle 1, P0-T1)

Timestamp: 2026-09-29T10-41
Task: P0-T1 (remediation-plan.2026-09-29T10-00.md)
Command: Read tool, one call per file
EXIT_CODE: 0

Policy Order:
1. CLAUDE.md (all sections, including "Committed Test Evidence Format" and UT2)
2. .claude/rules/general-code-change.md
3. .claude/rules/general-unit-test.md
4. .claude/rules/powershell.md
5. .claude/rules/tonality.md
6. .claude/rules/quality-tiers.md
7. .claude/rules/plan-acceptance-gates.md
8. .claude/skills/atomic-plan-contract/SKILL.md
9. .claude/skills/acceptance-criteria-tracking/SKILL.md
10. .claude/skills/evidence-and-timestamp-conventions/SKILL.md
11. docs/features/active/2026-09-28-coverage-runner-scoped-threshold-and-format-928/issue.md
12. docs/features/active/2026-09-28-coverage-runner-scoped-threshold-and-format-928/remediation-inputs.2026-09-29T10-00.md
13. docs/features/active/2026-09-28-coverage-runner-scoped-threshold-and-format-928/policy-audit.2026-09-29T10-00.md
14. docs/features/active/2026-09-28-coverage-runner-scoped-threshold-and-format-928/code-review.2026-09-29T10-00.md
15. docs/features/active/2026-09-28-coverage-runner-scoped-threshold-and-format-928/feature-audit.2026-09-29T10-00.md
16. docs/features/active/2026-09-28-coverage-runner-scoped-threshold-and-format-928/plan.2026-09-28T19-45.md
17. docs/features/active/2026-09-28-coverage-runner-scoped-threshold-and-format-928/remediation-plan.2026-09-29T10-00.md

Files read (all under <repo-root>, in the order above): every file listed in Policy Order was read with the Read tool, one call per file, in the stated order. The full text of items 1 to 7 was also present in the session context loaded from the same worktree.

Output Summary:
- CLAUDE.md UT2 sets the PowerShell line-coverage floor at 80 percent; .claude/rules/general-unit-test.md, .claude/rules/quality-tiers.md and .claude/rules/powershell.md state 85 percent. CLAUDE.md governs as rank 1 in the policy-compliance order. The conflict is recorded here and is not resolved by this plan.
- issue.md: Work Mode minor-audit; section "## Acceptance Criteria" carries AC1 to AC7; AC1 to AC5 and AC7 are checked; AC6 is the only unchecked acceptance criterion.
- remediation-inputs.2026-09-29T10-00.md: R-1 is the only blocking finding; CR-2 (validation attributes and an absolute-path guard on the predicate) is folded into this cycle.
- Promotion candidates P-1, P-2 and P-3 are excluded from this cycle and are owed by the orchestrator through the potential-feature lifecycle.
- CLAUDE.md "Committed Test Evidence Format": no raw coverage collector document and no raw test-platform document may be committed; evidence records projections and derived figures only.
