# Phase 0 Instructions Read — Remediation Cycle 1 (#177)

- Timestamp: 2026-06-12T16-02 (UTC)
- Plan: `remediation-plan.2026-06-12T15-54.md`
- Executor: atomic-executor
- Task: [P0-T1]

## Policy Order

Files read in the policy-compliance order defined by `.claude/skills/policy-compliance-order/SKILL.md`:

1. `CLAUDE.md` (standing instructions, all sections)
2. `.claude/rules/general-code-change.md` (cross-language code change policy)
3. `.claude/rules/general-unit-test.md` (cross-language unit test policy)
4. `.claude/rules/csharp.md` (C# language-specific rules)

## Files Read

- `C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-08-12-06\CLAUDE.md`
- `C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-08-12-06\.claude\rules\general-code-change.md`
- `C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-08-12-06\.claude\rules\general-unit-test.md`
- `C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-08-12-06\.claude\rules\csharp.md`

All four policy files were read before any code change. C# toolchain order confirmed:
CSharpier (format) -> analyzer msbuild (lint) -> nullable/TreatWarningsAsErrors msbuild (type-check) -> vstest /EnableCodeCoverage (test); restart from CSharpier on any failure or auto-fix.
