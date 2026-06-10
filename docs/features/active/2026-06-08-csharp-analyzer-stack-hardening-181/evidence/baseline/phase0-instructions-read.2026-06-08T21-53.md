# Phase 0 — Policy Instructions Read (Cycle 5, Issue #181)

Timestamp: 2026-06-08T21-53

Policy Order:
1. CLAUDE.md (standing instructions, always loaded)
2. .claude/rules/general-code-change.md (cross-language code change policy)
3. .claude/rules/general-unit-test.md (cross-language unit test policy)
4. .claude/rules/csharp.md (C#-specific standards, analyzer stack #181, banned symbols, DI seams)
5. .claude/rules/ci-workflows.md (pwsh deliberately-failing nested command pattern)
6. .claude/skills/policy-compliance-order/SKILL.md (mandatory reading order, hard constraints)
7. .claude/skills/atomic-plan-contract/SKILL.md (plan format, Phase 0, final QA loop, evidence schema)
8. .claude/skills/evidence-and-timestamp-conventions/SKILL.md (canonical evidence paths, ISO-8601 timestamps)
9. .claude/skills/remediation-handoff-atomic-planner/SKILL.md (remediation trigger and handoff)

Files Read:
- CLAUDE.md
- .claude/rules/general-code-change.md
- .claude/rules/general-unit-test.md
- .claude/rules/csharp.md
- .claude/rules/ci-workflows.md
- .claude/skills/policy-compliance-order/SKILL.md
- .claude/skills/atomic-plan-contract/SKILL.md
- .claude/skills/evidence-and-timestamp-conventions/SKILL.md
- .claude/skills/remediation-handoff-atomic-planner/SKILL.md

Notes:
- CLAUDE.md, general-code-change.md, general-unit-test.md, csharp.md, ci-workflows.md, and the four contract/convention skills (policy-compliance-order, atomic-plan-contract, evidence-and-timestamp-conventions, acceptance-criteria-tracking) were provided in-session and read in full.
- C# toolchain order is mandatory: csharpier -> msbuild analyzers -> msbuild nullable/TreatWarningsAsErrors -> vstest with coverage. Restart from csharpier on any change/failure.
- Banned symbols (BannedApiAnalyzers): DateTime.Now, DateTime.UtcNow, Random.Shared, Thread.Sleep, Task.Delay. Authorized production edits must not introduce any of these.
- Evidence written only under docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/evidence/<kind>/ per canonical conventions.
