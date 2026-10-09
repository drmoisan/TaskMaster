# Phase 0 Instructions Read (Issue #985)

Timestamp: 2026-10-09T14-04

Policy Order: CLAUDE.md, then .claude/rules/general-code-change.md, then .claude/rules/general-unit-test.md, then the domain rules (.claude/rules/quality-tiers.md, .claude/rules/powershell.md, .claude/rules/csharp.md, .claude/rules/tonality.md, .claude/rules/plan-acceptance-gates.md), then the skills (atomic-plan-contract, evidence-and-timestamp-conventions, acceptance-criteria-tracking), then the feature documents (issue.md, spec.md, research).

Files Read:
1. CLAUDE.md
2. .claude/rules/general-code-change.md
3. .claude/rules/general-unit-test.md
4. .claude/rules/quality-tiers.md
5. .claude/rules/powershell.md
6. .claude/rules/csharp.md
7. .claude/rules/tonality.md
8. .claude/rules/plan-acceptance-gates.md
9. .claude/skills/atomic-plan-contract/SKILL.md
10. .claude/skills/evidence-and-timestamp-conventions/SKILL.md
11. .claude/skills/acceptance-criteria-tracking/SKILL.md
12. docs/features/active/2026-10-09-dependabot-repair-borrowed-packages-and-transitive-redirects-985/issue.md
13. docs/features/active/2026-10-09-dependabot-repair-borrowed-packages-and-transitive-redirects-985/spec.md
14. docs/features/active/2026-10-09-dependabot-repair-borrowed-packages-and-transitive-redirects-985/research/research.2026-10-09T14-10.md

Notes:
- Work Mode (issue.md): full-bug; AC source is spec.md only (AC1 to AC7).
- Coverage floors applied per plan D6: CLAUDE.md UT2 (line 80, new code 90) takes precedence over the 85 figure in quality-tiers.md, which is recorded as an observation.
