# Phase 0 — Instructions Read

Timestamp: 2026-06-22T21-15

Policy Order:
1. CLAUDE.md (standing instructions)
2. .claude/rules/general-code-change.md (cross-language code change policy)
3. .claude/rules/general-unit-test.md (cross-language unit test policy)
4. .claude/rules/csharp.md (C#-specific rules; files in scope are *.cs)

Files read (in order):
- CLAUDE.md
- .claude/rules/general-code-change.md
- .claude/rules/general-unit-test.md
- .claude/rules/csharp.md
- .claude/skills/atomic-plan-contract/SKILL.md
- .claude/skills/evidence-and-timestamp-conventions/SKILL.md
- docs/features/active/2026-06-18-outlook-startup-intelconfig-deserialize-stall-207/remediation-inputs.2026-06-22T21-15.md

Output Summary: All required policy files and the remediation inputs were read prior to execution.
The work is a test-only change to a single C# file (TaskMaster.Test/AppGlobals/LiveOutlookHookupIntegrationTests.cs);
C# toolchain order applies (CSharpier -> analyzers -> nullable/TWAE -> MSTest). No policy documents modified.
