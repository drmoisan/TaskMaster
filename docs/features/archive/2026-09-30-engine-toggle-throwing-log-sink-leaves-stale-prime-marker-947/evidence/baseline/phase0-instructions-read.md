# Phase 0 Instructions Read (P0-T1)

Timestamp: 2026-10-01T17-32
Task: P0-T1
Issue: #947
Work Mode: minor-audit (AC source: `## Acceptance Criteria` section of issue.md only)

Policy Order: CLAUDE.md -> .claude/rules/general-code-change.md -> .claude/rules/general-unit-test.md -> .claude/rules/csharp.md

Read: CLAUDE.md
Read: .claude/rules/general-code-change.md
Read: .claude/rules/general-unit-test.md
Read: .claude/rules/csharp.md
Read: .claude/rules/plan-acceptance-gates.md
Read: .claude/rules/tonality.md
Read: .claude/skills/acceptance-criteria-tracking/SKILL.md

Output Summary: seven documents read from the item worktree in the order above; the four policy documents were read in the policy-compliance-order sequence. No policy document was modified.

Notes:
- CLAUDE.md is the governing policy where it differs from the cross-language rule summaries (C# coverage floors 80% line / 75% branch; MSTest, Moq, FluentAssertions; CSharpier via `dotnet tool run`; `/t:Rebuild` analyzer and nullable gates without `/p:Nullable=enable`; committed test evidence is a projection, never a raw trx or coverage document).
- Temporary files in tests are prohibited; `Thread.Sleep` and `Task.Delay` are banned in test code.
