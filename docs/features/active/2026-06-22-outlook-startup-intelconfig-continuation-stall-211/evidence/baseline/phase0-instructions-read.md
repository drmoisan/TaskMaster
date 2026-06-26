# Phase 0 — Instructions Read (Issue #211 PostLoad/LoadInboxes attribution probe)

Timestamp: 2026-06-24T18-30

Policy Order:
1. CLAUDE.md (standing instructions; always loaded)
2. .claude/rules/general-code-change.md (cross-language code change policy)
3. .claude/rules/general-unit-test.md (cross-language unit test policy)
4. .claude/rules/csharp.md (C#-specific code + unit test standards)
5. .claude/skills/atomic-plan-contract/SKILL.md (atomic plan format, Phase 0, final QA loop)
6. .claude/skills/evidence-and-timestamp-conventions/SKILL.md (ISO-8601 timestamps, canonical evidence locations)

Files Read (explicit list, in order):
- CLAUDE.md
- .claude/rules/general-code-change.md
- .claude/rules/general-unit-test.md
- .claude/rules/csharp.md
- .claude/skills/atomic-plan-contract/SKILL.md
- .claude/skills/evidence-and-timestamp-conventions/SKILL.md

Notes:
- Work Mode: full-bug. Diagnosis-only, behavior-preserving instrumentation. No behavior fix.
- C# toolchain order (mandatory): CSharpier -> .NET analyzers -> nullable/TreatWarningsAsErrors -> MSTest with coverage (`/TestCaseFilter:"TestCategory!=LiveOutlook"`). Restart from CSharpier on any change.
- Banned timing APIs: DateTime.Now, DateTime.UtcNow, Random.Shared, Thread.Sleep, Task.Delay. Stopwatch only. Target net48.
- All touched files (production and test) must remain <= 500 lines.
- Evidence under canonical `docs/features/active/2026-06-22-outlook-startup-intelconfig-continuation-stall-211/evidence/<kind>/`.
