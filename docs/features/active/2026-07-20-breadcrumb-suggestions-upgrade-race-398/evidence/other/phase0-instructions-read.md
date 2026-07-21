# Phase 0 — Instructions Read (P0-T1)

Timestamp: 2026-07-20T21-54

Policy Order:
1. CLAUDE.md (standing instructions, always loaded)
2. .claude/rules/general-code-change.md (cross-language code change policy)
3. .claude/rules/general-unit-test.md (cross-language unit test policy)
4. .claude/rules/csharp.md (C# code-change and C# unit-test policy — the in-scope language)

Files read:
- C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-20T12-52\CLAUDE.md
- C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-20T12-52\.claude\rules\general-code-change.md
- C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-20T12-52\.claude\rules\general-unit-test.md
- C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-20T12-52\.claude\rules\csharp.md

Supporting policy skills read for execution context:
- .claude/skills/policy-compliance-order/SKILL.md
- .claude/skills/atomic-plan-contract/SKILL.md
- .claude/skills/acceptance-criteria-tracking/SKILL.md
- .claude/skills/evidence-and-timestamp-conventions/SKILL.md

Output Summary: All four required policy files read in the mandated order. Work is a C# minor-audit bug fix; C# toolchain order is csharpier -> msbuild analyzers -> msbuild nullable -> vstest.console.exe /EnableCodeCoverage, restart on any change. Tests use MSTest + Moq + FluentAssertions with deterministic TaskCompletionSource-gated fakes, no temp files, no wall-clock waits. net48 constraint: no init-only setters, records, or record structs.
