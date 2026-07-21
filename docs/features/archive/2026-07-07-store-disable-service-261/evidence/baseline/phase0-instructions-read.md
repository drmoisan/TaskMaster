# Phase 0 — Instructions Read (P0-T1..T5)

Timestamp: 2026-07-07T22-57

Policy Order:
1. CLAUDE.md (standing instructions, position 1)
2. .claude/rules/general-code-change.md (cross-language code change policy, position 2)
3. .claude/rules/general-unit-test.md (cross-language unit test policy, position 3)
4. .claude/rules/csharp.md (C#-specific toolchain and coding standards, position 4)

Files read (start-to-end):
- C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a957d835cc071fcf9\CLAUDE.md
- C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a957d835cc071fcf9\.claude\rules\general-code-change.md
- C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a957d835cc071fcf9\.claude\rules\general-unit-test.md
- C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a957d835cc071fcf9\.claude\rules\csharp.md

Path-correction note:
The plan tasks P0-T1..T4 named policy paths under a stale worktree root
`C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-07-13-21\`. Per the executor's
orchestration directive, the equivalent policy files were read from THIS active
worktree root instead. The C#-specific policy read for P0-T4 is
`.claude\rules\csharp.md` (the C# rule file), consistent with the plan's stated
"C#-specific" intent.

Output Summary: All four policy documents read in full in required order. No section skipped.
Key binding constraints confirmed: csharpier-only formatting (no dotnet format);
4-step C# toolchain (format -> analyzers -> nullable/TreatWarningsAsErrors -> vstest coverage);
MSTest + Moq + FluentAssertions; repo line coverage >= 80% testable denominator, new code >= 90%;
500-line file cap; no temp files in tests; no Thread.Sleep/Task.Delay/real timers (injected clock/timers).
