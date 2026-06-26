# Phase 0 — Instructions Read (#211 Phase 3.1 UI-heartbeat + GC probe)

Timestamp: 2026-06-23T18-40

Policy Order:
1. CLAUDE.md (standing instructions, always loaded)
2. .claude/rules/general-code-change.md (cross-language code change policy)
3. .claude/rules/general-unit-test.md (cross-language unit test policy)
4. .claude/rules/csharp.md (C#-specific code + test standards)
5. .claude/skills/atomic-plan-contract/SKILL.md (atomic plan format, Phase 0, final QA loop rules)
6. .claude/skills/evidence-and-timestamp-conventions/SKILL.md (canonical evidence paths + timestamp format)

Files Read (explicit list):
- CLAUDE.md
- .claude/rules/general-code-change.md
- .claude/rules/general-unit-test.md
- .claude/rules/csharp.md
- .claude/skills/atomic-plan-contract/SKILL.md
- .claude/skills/evidence-and-timestamp-conventions/SKILL.md

Notes:
- Work mode for this plan: full-bug (diagnosis-only, behavior-preserving instrumentation).
- AC source per plan: AC11/AC12/AC13 listed in plan `## Acceptance Criteria (this increment)`.
- Toolchain order (mandatory, restart from step 1 on any change/failure): CSharpier -> .NET analyzers -> nullable/TWAE -> MSTest with coverage (`/TestCaseFilter:"TestCategory!=LiveOutlook"`).
- Banned APIs (BannedSymbols.txt): DateTime.Now, DateTime.UtcNow, Random.Shared, Thread.Sleep, Task.Delay. Stopwatch only for interval timing.
- Evidence root (canonical, non-overridable): docs/features/active/2026-06-22-outlook-startup-intelconfig-continuation-stall-211/evidence/<kind>/.
