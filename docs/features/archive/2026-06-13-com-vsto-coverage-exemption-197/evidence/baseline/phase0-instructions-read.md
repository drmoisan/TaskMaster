# Phase 0 — Policy Read Evidence

Timestamp: 2026-06-13T11-45

Policy Order:
1. CLAUDE.md (standing instructions, always loaded)
2. .claude/rules/general-code-change.md (cross-language code change policy)
3. .claude/rules/general-unit-test.md (cross-language unit test policy)
4. .claude/rules/csharp.md (C#-specific rules; C# files in scope)
5. .claude/skills/atomic-plan-contract/SKILL.md (plan format / Phase 0 / final QA loop rules)
6. .claude/skills/evidence-and-timestamp-conventions/SKILL.md (evidence paths / timestamps)

Files Read:
- CLAUDE.md
- .claude/rules/general-code-change.md
- .claude/rules/general-unit-test.md
- .claude/rules/csharp.md
- .claude/skills/policy-compliance-order/SKILL.md
- .claude/skills/atomic-plan-contract/SKILL.md
- .claude/skills/evidence-and-timestamp-conventions/SKILL.md
- .claude/skills/acceptance-criteria-tracking/SKILL.md

Notes:
- C# toolchain order (per CLAUDE.md C# Toolchain and .claude/rules/csharp.md): csharpier -> msbuild (analyzers) -> msbuild (nullable/TreatWarningsAsErrors) -> vstest with coverage. Restart from csharpier on any file change or failure.
- This feature is attribute/config/doc-only; no production logic or API change.
- Evidence location invariant: all artifacts resolve to docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/evidence/<kind>/.
