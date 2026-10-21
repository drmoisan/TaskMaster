Timestamp: 2026-09-03T11-59

Policy Order:
1. CLAUDE.md
2. .claude/rules/general-code-change.md
3. .claude/rules/general-unit-test.md
4. .claude/rules/csharp.md

File-size limit recorded from .claude/rules/general-code-change.md: 500

Threshold Reconciliation: CLAUDE.md (General Unit Test Policy, UT2) states an 80% repository-wide / 90% new-code C# coverage floor. .claude/rules/general-unit-test.md states a uniform 85% line / 75% branch floor across all tiers. CLAUDE.md is rank 1 in the policy-compliance order (per .claude/skills/policy-compliance-order/SKILL.md) and governs the blocking gates in this plan. All four integers: 80, 90, 85, 75.

Requirements Source: spec.md is the sole acceptance-criteria source, 9 criteria (AC1-AC9, spec.md `## Acceptance Criteria` section), per this plan's header. issue.md and the research artifact at docs/features/active/2026-08-31-narrow-fileio2-retryable-exception-set-707/research/2026-09-02T09-15-narrow-fileio2-retryable-exception-set-research.md were also read for context.

Work Mode: full-bug (recorded in issue.md line 12 and spec.md header context).
