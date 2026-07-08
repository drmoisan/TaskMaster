# Phase 0 — Instructions Read (AC10 direct-path navigation, issue #211)

Timestamp: 2026-06-24T19-06

Policy Order:
1. CLAUDE.md (standing instructions, always loaded)
2. .claude/rules/general-code-change.md (cross-language code change policy)
3. .claude/rules/general-unit-test.md (cross-language unit test policy)
4. .claude/rules/csharp.md (C#-specific code + unit test standards)

Files Read (explicit list):
- c:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-18-10-03\CLAUDE.md
- c:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-18-10-03\.claude\rules\general-code-change.md
- c:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-18-10-03\.claude\rules\general-unit-test.md
- c:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-18-10-03\.claude\rules\csharp.md

Supporting skills also read for this execution:
- .claude\skills\policy-compliance-order\SKILL.md
- .claude\skills\atomic-plan-contract\SKILL.md
- .claude\skills\evidence-and-timestamp-conventions\SKILL.md
- .claude\skills\acceptance-criteria-tracking\SKILL.md

Key constraints confirmed for this increment:
- Bugfix workflow: failing regression test FIRST (red), then minimal targeted fix (green).
- C# toolchain order: CSharpier -> analyzers -> nullable/TWAE -> MSTest+coverage; restart on any change.
- net48; banned APIs (DateTime.Now/UtcNow, Random.Shared, Thread.Sleep, Task.Delay) NOT introduced.
- MSTest + Moq + FluentAssertions; deterministic (no live COM/timer/network/filesystem; no temp files).
- New-code coverage >= 90%; no repo-wide regression; all touched files (production AND test) <= 500 lines.
- Do NOT modify FolderTree.cs; keep diagnostic instrumentation intact.
- Evidence under canonical docs/features/.../evidence/<kind>/ only.
