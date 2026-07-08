# Phase 0 — Instructions Read (P0-T1)

Timestamp: 2026-06-12T19-45

Policy Order:
1. CLAUDE.md (standing instructions, always loaded)
2. .claude/rules/general-code-change.md (cross-language code change policy)
3. .claude/rules/general-unit-test.md (cross-language unit test policy)
4. Language/domain-specific rules in scope:
   - .claude/rules/csharp.md (C# rules — note: this runsettings-only change touches no `*.cs`; csharp toolchain applies only partially per the plan's "Toolchain applicability" section)
   - .claude/rules/powershell.md (loaded by harness; out of scope — #188 PowerShell files must not be touched)
5. Background skills loaded for this execution:
   - .claude/skills/policy-compliance-order/SKILL.md
   - .claude/skills/atomic-plan-contract/SKILL.md
   - .claude/skills/evidence-and-timestamp-conventions/SKILL.md
   - .claude/skills/acceptance-criteria-tracking/SKILL.md

Files read (explicit list):
- CLAUDE.md
- .claude/rules/general-code-change.md
- .claude/rules/general-unit-test.md
- .claude/rules/csharp.md (via CLAUDE.md embedded C# Code Change Policy and C# Unit Test Policy)
- .claude/skills/policy-compliance-order/SKILL.md
- .claude/skills/atomic-plan-contract/SKILL.md
- .claude/skills/evidence-and-timestamp-conventions/SKILL.md
- .claude/skills/acceptance-criteria-tracking/SKILL.md

Note: CLAUDE.md embeds the four core policies (General Code Change, General Unit Test, C# Code Change, C# Unit Test) directly. The `.claude/rules/*.md` summaries were also auto-loaded by the harness and reviewed.
