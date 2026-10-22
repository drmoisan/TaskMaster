# P0-T1 Policy and feature document read

Timestamp: 2026-09-29T08-51
Policy Order:
1. CLAUDE.md (all sections, including "Committed Test Evidence Format")
2. .claude/rules/general-code-change.md
3. .claude/rules/general-unit-test.md
4. .claude/rules/powershell.md
5. .claude/rules/csharp.md
6. .claude/rules/ci-workflows.md
7. .claude/rules/plan-acceptance-gates.md
8. .claude/rules/tonality.md
9. spec.md (this feature folder)
10. issue.md (this feature folder)
11. research/2026-09-28T19-55-evidence-and-identity-hygiene-sweep-research.md (this feature folder)
12. evidence/other/write-set-inventory.2026-09-28T20-30.md (this feature folder)
Command: file reads of the twelve documents above, in the order listed, from the worktree of branch bug/evidence-and-identity-hygiene-sweep-927 (Read tool; no shell command)
EXIT_CODE: 0
Output Summary:
- All twelve documents were read in the order above.
- AC source: spec.md, AC1 to AC20 (twenty checkbox items under "## Acceptance Criteria"); no user-story.md exists for this item.
- Work mode marker in issue.md reads `full-bug` (line "- Work Mode: full-bug").
- Skills applied: policy-compliance-order, atomic-plan-contract, evidence-and-timestamp-conventions, acceptance-criteria-tracking.
- Notable constraints carried into execution: PowerShell 7, UTF-8 with byte-order mark for new .ps1 files, wrapper seam Invoke-GitExe -GitArgs, no temporary files in tests, 500-line ceiling, placeholders <repo-root>, <user-profile>, <user>, <host> only in artifacts.
