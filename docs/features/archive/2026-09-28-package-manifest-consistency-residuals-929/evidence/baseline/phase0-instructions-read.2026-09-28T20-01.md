# P0-T2 — Phase 0 instructions read

Timestamp: 2026-09-30T09-12
Command: Read each file below in order from <execution-worktree-root>; line counts by pwsh -NoProfile -Command '@(Get-Content -LiteralPath <path>).Count'
EXIT_CODE: 0
Policy Order: CLAUDE.md, then .claude/rules/general-code-change.md, then .claude/rules/general-unit-test.md, then the language- and domain-specific rules (powershell, csharp, quality-tiers, tonality, ci-workflows, plan-acceptance-gates), then the three skills, as fixed by the policy-compliance-order skill.

Files read (in order, with line counts):

1. CLAUDE.md — 463 lines
2. .claude/rules/general-code-change.md — 80 lines
3. .claude/rules/general-unit-test.md — 105 lines
4. .claude/rules/powershell.md — 97 lines
5. .claude/rules/csharp.md — 96 lines
6. .claude/rules/quality-tiers.md — 51 lines
7. .claude/rules/tonality.md — 80 lines
8. .claude/rules/ci-workflows.md — 42 lines
9. .claude/rules/plan-acceptance-gates.md — 257 lines
10. .claude/skills/atomic-plan-contract/SKILL.md — 245 lines
11. .claude/skills/evidence-and-timestamp-conventions/SKILL.md — 176 lines
12. .claude/skills/acceptance-criteria-tracking/SKILL.md — 104 lines

Output Summary:
- Twelve files read in the order above, each with a non-zero line count.
- issue.md line 12 reads `- Work Mode: minor-audit`.
- issue.md carries the heading `## Acceptance Criteria` (line 40) with exactly 7 lines matching `^- \[ \] AC\d+:` beneath it (AC1 to AC7, lines 44 to 50).
- No spec.md, user-story.md or research.md exists in the feature folder (Test-Path False for each).
- Coverage-floor conflict (convention 10): CLAUDE.md states an 80 percent PowerShell and C# line floor and a four-step toolchain loop; .claude/rules/general-unit-test.md, .claude/rules/quality-tiers.md and .claude/rules/powershell.md state 85 percent and a seven-stage loop. This plan gates on CLAUDE.md and records MEETS-85 as an observation only. The conflict is tracked as open GitHub issue 668.
