# Phase 0 — Policy Instructions Read (P0-T1..T5)

Timestamp: 2026-07-08T03-51

Policy Order:
1. CLAUDE.md (standing instructions, always loaded)
2. .claude/rules/general-code-change.md (cross-language code change policy)
3. .claude/rules/general-unit-test.md (cross-language unit test policy)
4. .claude/rules/csharp.md (C#-specific standards)

Files Read (actual paths in the current worktree; the plan's planning-time
`TaskMaster-wt-2026-07-07-13-21` paths do not exist and were superseded per the
orchestrator path correction):
1. C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-aa788d7e018d8924e\CLAUDE.md
2. C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-aa788d7e018d8924e\.claude\rules\general-code-change.md
3. C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-aa788d7e018d8924e\.claude\rules\general-unit-test.md
4. C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-aa788d7e018d8924e\.claude\rules\csharp.md

Output Summary: All four policy documents read start-to-end, no section skipped.
Key constraints captured: csharpier -> analyzers -> nullable/TreatWarningsAsErrors ->
vstest coverage loop; MSTest+Moq+FluentAssertions only; no temp files in tests; net48
has no IsExternalInit (plain readonly struct only); 500-line file limit; repo-wide
testable-denominator line coverage >= 80%, new code >= 90%; COM/VSTO/WinForms coverage
exemption via [ExcludeFromCodeCoverage] / interface-only clarification.
