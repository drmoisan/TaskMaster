# Phase 0 — Policy Instructions Read

Timestamp: 2026-08-08T16-09

Task: [P0-T1]

Policy Order: The reading order defined by `.claude/skills/policy-compliance-order/SKILL.md`:

1. `CLAUDE.md` (standing instructions, always loaded)
2. `.claude/rules/general-code-change.md` (cross-language code change policy)
3. `.claude/rules/general-unit-test.md` (cross-language unit test policy)
4. Language-specific rules for files in scope — C#: `.claude/rules/csharp.md`

## Files Read (explicit list)

- `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-ad7090ae544fd0fb0\CLAUDE.md`
- `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-ad7090ae544fd0fb0\.claude\rules\general-code-change.md`
- `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-ad7090ae544fd0fb0\.claude\rules\general-unit-test.md`
- `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-ad7090ae544fd0fb0\.claude\rules\csharp.md`

Supporting skills read for this execution:

- `.claude/skills/policy-compliance-order/SKILL.md`
- `.claude/skills/atomic-plan-contract/SKILL.md`
- `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`
- `.claude/skills/acceptance-criteria-tracking/SKILL.md`
- `.claude/rules/tonality.md`
- `.claude/rules/quality-tiers.md`

## Binding Constraints Extracted

- Toolchain order (C#): CSharpier format -> analyzer msbuild -> nullable msbuild -> vstest with
  coverage. Restart at step 1 on any failure or any file rewrite.
- DI seams: introduce the smallest seam. Injectable delegate seam is sanctioned for a single call
  path when a full interface is excessive (`.claude/rules/csharp.md`, "DI Seams", preference 2).
- Prohibited behaviors: weakening assertions, sleeps/retries/timing hacks, broad refactors,
  reporting success without running the toolchain.
- Coverage: repository-wide line coverage >= 80%; any new module/class/method >= 90%; coverage
  regression on changed lines is a blocking finding (`.claude/rules/csharp.md`, Testing Standards).
- Coverage Exclusion Policy (`.claude/rules/general-unit-test.md`): no production file may be
  excluded from coverage measurement.
- No temporary files in tests. No file over 500 lines.

Output Summary: All four policy files in the required order were read, plus the four supporting
skills governing plan format, evidence paths, and AC tracking. No policy document was modified.
