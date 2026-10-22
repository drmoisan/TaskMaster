# Phase 0 Instructions Read (P0-T1)

Timestamp: 2026-09-29T08-51
Task: P0-T1
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
12. docs/features/active/2026-09-28-coverage-runner-scoped-threshold-and-format-928/plan.2026-09-28T19-45.md

Files read (all under <repo-root>, in the order above): every file listed in Policy Order was read with the Read tool, one call per file, in the stated order. The full text of items 1 to 10 was also present in the session context loaded from the same worktree.

Output Summary:
- CLAUDE.md UT2 sets the PowerShell line-coverage floor at 80 percent; .claude/rules/general-unit-test.md and .claude/rules/quality-tiers.md (and .claude/rules/powershell.md) state 85 percent.
- CLAUDE.md is authority rank 1 in the policy-compliance order, so the 80 percent floor governs AC6, and the CLAUDE.md UT2 new-module target of 90 percent governs the new part file scripts/vscode/Invoke-MSTestWithCoverage.Scope.ps1.
- The 80-versus-85 conflict is recorded here and is not resolved by this plan.
- .claude/rules/powershell.md: toolchain is MCP PoshQC format, analyze, test in that order; per-batch cap of 3 production files and 3 test files; 500-line file ceiling.
- CLAUDE.md "Committed Test Evidence Format": no raw coverage collector document and no raw test-platform document may be committed; evidence records projections and derived figures only.
- issue.md: Work Mode minor-audit; section "## Acceptance Criteria" carries AC1 to AC7, all unchecked.
