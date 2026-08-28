# [P0-T1] Policy and Instruction Reads

Timestamp: 2026-08-26T08-26

Task: [P0-T1]
Feature: docs/features/active/quickfiler-bug-family-446
Work Mode: full-bug

## Policy Order

The ten files below were read in the exact order listed, which is the order stated by
`[P0-T1]` and is consistent with `.claude/skills/policy-compliance-order/SKILL.md`
(CLAUDE.md first, then the cross-language rules, then the language-specific rules,
then the supporting skills).

## Files Read (10)

1. `CLAUDE.md`
2. `.claude/rules/general-code-change.md`
3. `.claude/rules/general-unit-test.md`
4. `.claude/rules/csharp.md`
5. `.claude/rules/quality-tiers.md`
6. `.claude/rules/plan-acceptance-gates.md`
7. `.claude/skills/policy-compliance-order/SKILL.md`
8. `.claude/skills/atomic-plan-contract/SKILL.md`
9. `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`
10. `.claude/skills/acceptance-criteria-tracking/SKILL.md`

All ten paths are relative to the workspace root and all ten exist in this worktree.

## Output Summary

All ten files read. Key constraints extracted and in force for this execution:

- C# toolchain order is format -> analyze -> type-check -> test; restart from step 1 on any
  failure or file rewrite (`CLAUDE.md`, `.claude/rules/csharp.md`).
- Format with `dotnet tool run csharpier format .` and verify with `dotnet tool run csharpier check .`;
  never `dotnet format` (`.claude/rules/csharp.md` Toolchain 1).
- Analyzer and nullable gates use `/t:Rebuild`, never `/t:Build`, and never `/p:Nullable=enable`
  (`.claude/rules/csharp.md` Toolchain 2 and 3).
- Tests use MSTest + Moq + FluentAssertions; no external services; no temporary files
  (`.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md` Testing Standards).
- Determinism infrastructure requires `TimeProvider` / `FakeTimeProvider` and bans `Thread.Sleep`,
  `Task.Delay` and wall-clock reads in test code (`.claude/rules/general-unit-test.md`).
- 500-line cap on every production, test and reusable script file; Markdown documentation is exempt
  (`.claude/rules/general-code-change.md` File Size Limit).
- Evidence is written only under `<FEATURE>/evidence/<kind>/`; `artifacts/...` evidence paths are
  forbidden and non-overridable (`.claude/skills/evidence-and-timestamp-conventions/SKILL.md`).
- Acceptance criteria for work mode `full-bug` are tracked in `spec.md` only; check off one at a time
  after verification, never batch, never add criteria
  (`.claude/skills/acceptance-criteria-tracking/SKILL.md`).
- Acceptance conditions must be falsifiable; gate rules G1 through G6 read
  (`.claude/rules/plan-acceptance-gates.md`).

Note on read provenance: `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`,
`.claude/rules/quality-tiers.md` and the four `SKILL.md` files were verified byte-identical
(modulo line endings) to the copies loaded into this session; `CLAUDE.md`, `.claude/rules/csharp.md`
and `.claude/rules/plan-acceptance-gates.md` were read directly from this worktree.
