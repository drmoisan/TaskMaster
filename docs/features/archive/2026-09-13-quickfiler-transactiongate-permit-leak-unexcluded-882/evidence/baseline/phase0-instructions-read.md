# Phase 0 Instructions Read (P0-T1)

Timestamp: 2026-09-29T08-50

Policy Order:
1. CLAUDE.md (all sections, including "Committed Test Evidence Format" and "C# Toolchain")
2. .claude/rules/general-code-change.md
3. .claude/rules/general-unit-test.md
4. .claude/rules/csharp.md
5. .claude/rules/tonality.md
6. .claude/rules/plan-acceptance-gates.md
7. .claude/skills/atomic-plan-contract/SKILL.md
8. .claude/skills/evidence-and-timestamp-conventions/SKILL.md
9. .claude/skills/acceptance-criteria-tracking/SKILL.md

Files Read:
1. CLAUDE.md
2. .claude/rules/general-code-change.md
3. .claude/rules/general-unit-test.md
4. .claude/rules/csharp.md
5. .claude/rules/tonality.md
6. .claude/rules/plan-acceptance-gates.md
7. .claude/skills/atomic-plan-contract/SKILL.md
8. .claude/skills/evidence-and-timestamp-conventions/SKILL.md
9. .claude/skills/acceptance-criteria-tracking/SKILL.md
10. docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/spec.md
11. docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/issue.md
12. docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/research/2026-09-28T00-10-transactiongate-research-refresh-research.md

Notes:
- All twelve paths are repository-relative and exist in the worktree tree.
- The spec (version 1.1) is the sole acceptance-criteria source (Work Mode full-bug); issue.md was read for context only, and its superseded runner-behaviour wording is not used as a requirement.
- Key constraints carried forward: committed evidence is projection-only (no raw trx, Cobertura, JaCoCo XML or coverage binary); no absolute host path, account name or host name in any committed text; the parallel test regime (TaskMaster.cli.runsettings, Workers 0, Scope ClassLevel) stays in force; no sleep, retry or DoNotParallelize is introduced.
