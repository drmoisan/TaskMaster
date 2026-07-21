# Phase 0 — Instructions Read (P0-T1)

Timestamp: 2026-07-09T21-56

Policy Order:
1. CLAUDE.md (standing project instructions)
2. .claude/rules/general-code-change.md (cross-language code change policy)
3. .claude/rules/general-unit-test.md (cross-language unit test policy)
4. .claude/rules/csharp.md (C#-specific toolchain and standards)

Files read (in required order):
- C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a89a47769d223ba9c\CLAUDE.md
- C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a89a47769d223ba9c\.claude\rules\general-code-change.md
- C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a89a47769d223ba9c\.claude\rules\general-unit-test.md
- C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a89a47769d223ba9c\.claude\rules\csharp.md

Also read (supporting policy skills provided in session context): policy-compliance-order,
atomic-plan-contract, evidence-and-timestamp-conventions, acceptance-criteria-tracking.

Output Summary: All four policy files read in the required order. Key binding constraints
noted: CSharpier formatting (not dotnet format); analyzer + nullable/TreatWarningsAsErrors
builds; MSTest + Moq + FluentAssertions; 500-line file limit; repo-wide >= 80% line coverage
and >= 90% for new modules; banned APIs (DateTime.Now/UtcNow, Random.Shared, Thread.Sleep,
Task.Delay); interface-seam-preferred DI. No policy conflicts encountered.
