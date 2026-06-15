# Phase 0 — Policy Instructions Read

Timestamp: 2026-06-14T08-22

Policy Order:
1. CLAUDE.md (standing instructions, always loaded)
2. .claude/rules/general-code-change.md (cross-language code change policy)
3. .claude/rules/general-unit-test.md (cross-language unit test policy)
4. .claude/rules/csharp.md (C#-specific code + test standards)

Files read:
- CLAUDE.md
- .claude/rules/general-code-change.md
- .claude/rules/general-unit-test.md
- .claude/rules/csharp.md

Additional governing skills reviewed: policy-compliance-order, atomic-plan-contract,
acceptance-criteria-tracking, evidence-and-timestamp-conventions.

Output Summary: All four required policy files read in the prescribed order. Key constraints
confirmed for this feature: test-only change; MSTest + Moq + FluentAssertions; AAA; no temp
files; no external dependencies; >= 90% coverage on new/changed code; full C# toolchain loop
(csharpier -> analyzers -> nullable/TWAE -> MSTest with coverage), restart on any change/failure.
