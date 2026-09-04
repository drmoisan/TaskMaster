# Phase 0 — Instructions Read

Timestamp: 2026-09-03T11-50

Policy Order: `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/powershell.md`, `.claude/rules/quality-tiers.md`

## Files read (all eight, in order)

1. `CLAUDE.md`
2. `.claude/rules/general-code-change.md`
3. `.claude/rules/general-unit-test.md`
4. `.claude/rules/powershell.md`
5. `.claude/rules/quality-tiers.md`
6. `.claude/skills/atomic-plan-contract/SKILL.md`
7. `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`
8. `.claude/skills/acceptance-criteria-tracking/SKILL.md`

All eight paths were confirmed present in this worktree before reading:

```
EXISTS CLAUDE.md True LINES 447
EXISTS .claude/rules/general-code-change.md True LINES 80
EXISTS .claude/rules/general-unit-test.md True LINES 105
EXISTS .claude/rules/powershell.md True LINES 97
EXISTS .claude/rules/quality-tiers.md True LINES 51
EXISTS .claude/skills/atomic-plan-contract/SKILL.md True LINES 245
EXISTS .claude/skills/evidence-and-timestamp-conventions/SKILL.md True LINES 176
EXISTS .claude/skills/acceptance-criteria-tracking/SKILL.md True LINES 104
```

## Coverage thresholds this plan is gated on ([P0-T2])

POWERSHELL COVERAGE GATE: line >= 85 percent, no branch gate

Sources, quoted from the two currently-in-force rule files:

- `.claude/rules/quality-tiers.md` line 33: `- Line coverage: >= 85%.`
- `.claude/rules/quality-tiers.md` line 34: `- Branch coverage: >= 75% for languages whose coverage tooling measures branch coverage. PowerShell (Pester) and bash (kcov) are exempt from this threshold because neither tool measures branch coverage; no branch-coverage gate applies to them.`
- `.claude/rules/general-unit-test.md` line 23: `- **Line coverage must remain >= 85% across all tiers (T1-T4).**`
- `.claude/rules/general-unit-test.md` line 24: PowerShell (Pester) is an exception to the `>= 75%` branch threshold because Pester measures no branch coverage in any output format, so only the line threshold applies.
- `.claude/rules/powershell.md` line 63: `- Line coverage must remain >= 85% across all tiers (T1-T4) per `.claude/rules/quality-tiers.md`.`

POLICY CONFLICT NOTED: `CLAUDE.md`, which is position 1 in the policy read order, states a repository-wide line floor of 80 percent and a new-module target of 90 percent, while `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md` state 85 percent line coverage and 75 percent branch coverage; this plan applies the stricter 85 percent figure, and this plan's blocking coverage condition is post-change greater than or equal to baseline rather than an absolute floor claim, because the position of the baseline against any floor predates this item.
